using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 // Phản chiếu phẳng thật cho mặt sông (25/10/2026, huongdan.md 21.45): một camera gương lật qua mặt nước,
 // chỉ chạy khi có khối cầu/sông trong tầm nhìn, độ phân giải thấp (mặc định 42% màn hình, tối đa 768 px, xa 120 m),
 // không bóng đổ/đèn điểm để nhẹ cho WebGL. Chỉ vẽ lớp ReflectLayer (mô hình cầu/kè/thuyền/đèn cầu và các tòa nhà phố, do
 // PhoVietRiverUpgrade gán) cùng bầu trời: vỉa hè, cây, đèn tín hiệu, mặt đường, xe… gần như không thấy từ mặt nước mà tốn draw call.
 // Kết quả vào texture toàn cục _RiverReflectionTex cho shader River Water Mirror.
 [RequireComponent(typeof(Camera))]
 public sealed class RiverReflection:MonoBehaviour
 {
  public PhoVietGame game;
  [Range(.2f,1f)] public float resolution=.42f;
  public int maxSize=768;
  public float farClip=120f,clipOffset=.05f;
  public int maxLod=0;          // ép LOD thô hơn trong ảnh phản chiếu (QualitySettings.maximumLODLevel)
  public const int ReflectLayer=8;   // "PV_RiverReflect"
  public bool allLayers;        // true: phản chiếu mọi thứ (chỉ để so sánh/đo)
  [Min(1)] public int interval=1;   // render ảnh phản chiếu mỗi N lần camera vẽ (giữ ảnh cũ ở các lần còn lại)
  int counter;
  public bool noShadows=true,lowerLod=true,noPixelLights=true;   // chỉnh QualitySettings tạm thời trong lúc render gương
  Camera view,mirror;RenderTexture target;
  readonly Plane[] frustum=new Plane[6];
  readonly Dictionary<RiverBlock,Renderer> water=new Dictionary<RiverBlock,Renderer>();
  static bool rendering;
  static readonly int TexId=Shader.PropertyToID("_RiverReflectionTex"),OnId=Shader.PropertyToID("_RiverReflectionOn"),ReflectingId=Shader.PropertyToID("_RiverReflecting");
  void OnEnable(){view=GetComponent<Camera>();}
  void OnDisable()
  {
   Shader.SetGlobalFloat(OnId,0);
   if(target!=null){target.Release();Destroy(target);target=null;}
   if(mirror!=null){Destroy(mirror.gameObject);mirror=null;}
  }
  // Cao độ mặt nước (thế giới) của khối cầu đầu tiên nằm trong khung nhìn.
  bool VisibleWater(out float height)
  {
   height=0;if(game==null||!game.riversEnabled)return false;
   GeometryUtility.CalculateFrustumPlanes(view,frustum);
   foreach(var row in game.roadRows)foreach(var tile in row.tiles)
   {
    var block=tile.GetComponent<RiverBlock>();if(block==null||!block.IsBridge||!tile.gameObject.activeInHierarchy)continue;
    if(!water.TryGetValue(block,out var r)||r==null)
    {
     r=null;foreach(var c in block.bridge.GetComponentsInChildren<Renderer>(true))if(c.name.StartsWith("PV_RiverBridge_LOD0",System.StringComparison.Ordinal)){r=c;break;}
     water[block]=r;
    }
    if(r==null||!GeometryUtility.TestPlanesAABB(frustum,r.bounds))continue;
    height=tile.TransformPoint(new Vector3(0,RiverBlock.WaterY,0)).y;return true;
   }
   return false;
  }
  void Ensure(int w,int h)
  {
   if(target==null||target.width!=w||target.height!=h)
   {
    if(target!=null){target.Release();Destroy(target);}
    target=new RenderTexture(w,h,16,RenderTextureFormat.Default){name="River reflection",hideFlags=HideFlags.DontSave,useMipMap=false,autoGenerateMips=false};
   }
   if(mirror==null)
   {
    var go=new GameObject("River reflection camera"){hideFlags=HideFlags.HideAndDontSave};
    mirror=go.AddComponent<Camera>();mirror.enabled=false;
   }
  }
  void OnPreCull()
  {
   if(rendering||view==null)return;
   if(!VisibleWater(out float h)){Shader.SetGlobalFloat(OnId,0);counter=0;return;}
   if(target!=null&&interval>1&&(counter++%interval)!=0){Shader.SetGlobalTexture(TexId,target);Shader.SetGlobalFloat(OnId,1);return;}
   float scale=Mathf.Min(resolution,maxSize/(float)Mathf.Max(view.pixelWidth,view.pixelHeight));
   int w=Mathf.Max(64,Mathf.RoundToInt(view.pixelWidth*scale)),ht=Mathf.Max(64,Mathf.RoundToInt(view.pixelHeight*scale));
   Ensure(w,ht);
   mirror.CopyFrom(view);
   mirror.enabled=false;mirror.targetTexture=target;mirror.allowHDR=false;mirror.allowMSAA=false;mirror.useOcclusionCulling=false;
   mirror.depthTextureMode=DepthTextureMode.None;mirror.renderingPath=RenderingPath.Forward;mirror.farClipPlane=Mathf.Min(view.farClipPlane,farClip);
   if(!allLayers)mirror.cullingMask=view.cullingMask&(1<<ReflectLayer);
   var reflection=Reflection(new Vector4(0,1,0,-h));
   mirror.worldToCameraMatrix=view.worldToCameraMatrix*reflection;
   mirror.projectionMatrix=view.CalculateObliqueMatrix(CameraSpacePlane(mirror.worldToCameraMatrix,new Vector3(0,h+clipOffset,0),Vector3.up));
   // Loại vật theo phối cảnh thường giới hạn farClip (ma trận xiên làm méo mặt phẳng xa nên không loại được gì).
   mirror.cullingMatrix=Matrix4x4.Perspective(view.fieldOfView,view.aspect,view.nearClipPlane,mirror.farClipPlane)*mirror.worldToCameraMatrix;
   mirror.transform.position=reflection.MultiplyPoint(view.transform.position);
   var e=view.transform.eulerAngles;mirror.transform.eulerAngles=new Vector3(-e.x,e.y,e.z);
   int lights=QualitySettings.pixelLightCount,maxLevel=QualitySettings.maximumLODLevel;float shadows=QualitySettings.shadowDistance,lod=QualitySettings.lodBias;
   rendering=true;
   try
   {
    if(noPixelLights)QualitySettings.pixelLightCount=0;if(noShadows)QualitySettings.shadowDistance=0;if(lowerLod){QualitySettings.lodBias=lod*.5f;QualitySettings.maximumLODLevel=Mathf.Max(maxLevel,maxLod);}
    Shader.SetGlobalFloat(ReflectingId,1);GL.invertCulling=true;
    mirror.Render();
   }
   finally
   {
    GL.invertCulling=false;Shader.SetGlobalFloat(ReflectingId,0);
    if(noPixelLights)QualitySettings.pixelLightCount=lights;if(noShadows)QualitySettings.shadowDistance=shadows;if(lowerLod){QualitySettings.lodBias=lod;QualitySettings.maximumLODLevel=maxLevel;}
    rendering=false;
   }
   Shader.SetGlobalTexture(TexId,target);Shader.SetGlobalFloat(OnId,1);
  }
  static Vector4 CameraSpacePlane(Matrix4x4 worldToCamera,Vector3 pos,Vector3 normal)
  {
   Vector3 p=worldToCamera.MultiplyPoint(pos),n=worldToCamera.MultiplyVector(normal).normalized;
   return new Vector4(n.x,n.y,n.z,-Vector3.Dot(p,n));
  }
  static Matrix4x4 Reflection(Vector4 p)
  {
   var m=Matrix4x4.identity;
   m.m00=1-2*p.x*p.x;m.m01=-2*p.x*p.y;m.m02=-2*p.x*p.z;m.m03=-2*p.w*p.x;
   m.m10=-2*p.y*p.x;m.m11=1-2*p.y*p.y;m.m12=-2*p.y*p.z;m.m13=-2*p.w*p.y;
   m.m20=-2*p.z*p.x;m.m21=-2*p.z*p.y;m.m22=1-2*p.z*p.z;m.m23=-2*p.w*p.z;
   return m;
  }
 }
}
