using UnityEngine;
using UnityEngine.Rendering;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  Transform markerGlow;TextMesh markerLabel;Material markerGlowMaterial,markerLabelMaterial;
  static readonly Color WaypointGreen=new Color(.16f,1f,.12f,1);
  public bool JobMarkerVisible=>jobMarker!=null&&jobMarker.gameObject.activeSelf;
  public string JobMarkerLabel=>markerLabel==null?"":markerLabel.text;
  Material WaypointMaterial(Color color)
  {
   var material=new Material(Resources.Load<Shader>("PhoVietWaypoint"));material.color=color;return material;
  }
  Transform MarkerMesh(string name,Vector3[] vertices,int[] triangles,Color color,float scale,float depth)
  {
   var part=StreetArt.Group(name,jobMarker,new Vector3(0,.85f,depth));part.localScale=Vector3.one*scale;
   var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateBounds();
   part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=part.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=WaypointMaterial(color);
   renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return part;
  }
  void CreateJobMarker()
  {
   jobMarker=StreetArt.Group("Mũi tên xanh • Điểm hẹn",transform);
   var arrow=new[]{new Vector3(-.28f,.85f,0),new Vector3(.28f,.85f,0),new Vector3(.28f,-.05f,0),new Vector3(.68f,-.05f,0),new Vector3(0,-.85f,0),new Vector3(-.68f,-.05f,0),new Vector3(-.28f,-.05f,0)};
   var faces=new[]{0,1,2,0,2,6,6,3,4,6,4,5};
   markerGlow=MarkerMesh("Quầng sáng xanh",arrow,faces,new Color(.12f,1f,.2f,.22f),1.45f,.04f);
   markerGlowMaterial=markerGlow.GetComponent<Renderer>().sharedMaterial;
   MarkerMesh("Viền tương phản đậm",arrow,faces,new Color(.02f,.14f,.1f,.96f),1.2f,.03f);
   MarkerMesh("Viền sáng",arrow,faces,Color.white,1.1f,.02f);
   MarkerMesh("Mũi tên xanh lá",arrow,faces,WaypointGreen,1,0);
   var quad=new[]{new Vector3(-1.35f,1.02f,0),new Vector3(1.35f,1.02f,0),new Vector3(1.35f,1.48f,0),new Vector3(-1.35f,1.48f,0)};
   MarkerMesh("Nền nhãn điểm hẹn",quad,new[]{0,1,2,0,2,3},new Color(.02f,.14f,.1f,.96f),1,-.01f);
   StreetArt.Text(jobMarker,"Nhãn điểm hẹn","",new Vector3(0,2.1f,-.03f),.32f,Color.white);
   markerLabel=jobMarker.GetComponentInChildren<TextMesh>();
   markerLabelMaterial=WaypointMaterial(Color.white);markerLabelMaterial.renderQueue=4001;
   markerLabel.GetComponent<Renderer>().sharedMaterial=markerLabelMaterial;
  }
  void UpdateJobMarker()
  {
   if(!jobsEnabled||ActiveJob==null||rideCamera==null||State==RideState.Ready||State==RideState.Jailed||State==RideState.GameOver||State==RideState.Arrested)
   {if(jobMarker!=null)jobMarker.gameObject.SetActive(false);return;}
   var target=CurrentWorkTarget;
   // Convert the stable city address into the current scrolling road frame.
   // This also works during a turn; the camera has not yet rebased that frame.
   Vector2 delta=target.Point+target.CurbDirection*7.6f-NavigationPosition;
   var local=new Vector3(Vector2.Dot(delta,CityNavigation.Directions[(navHeading+1)%4]),.45f,Vector2.Dot(delta,CityNavigation.Directions[navHeading]));
   Vector3 world=transform.TransformPoint(local),view=rideCamera.WorldToViewportPoint(world);
   bool visible=view.z>rideCamera.nearClipPlane&&view.z<150&&view.x>=0&&view.x<=1&&view.y>=0&&view.y<=1;
   if(!visible){if(jobMarker!=null)jobMarker.gameObject.SetActive(false);return;}
   if(jobMarker==null)CreateJobMarker();jobMarker.gameObject.SetActive(true);
   jobMarker.localPosition=local;jobMarker.rotation=rideCamera.transform.rotation;
   // A readable size at distance, with no light sources or bloom cost on phones.
   float size=Mathf.Clamp(view.z*.032f,.75f,3.5f);jobMarker.localScale=Vector3.one*size;
   float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*4);
   markerGlow.localScale=Vector3.one*(1.4f+pulse*.12f);markerGlowMaterial.color=new Color(.12f,1f,.2f,.18f+pulse*.14f);
   markerLabel.text=JobPhase==WorkPhase.Pickup||JobPhase==WorkPhase.Boarding?"ĐÓN KHÁCH":"TRẢ KHÁCH";
   if(StreetArt.Font!=null)markerLabelMaterial.mainTexture=StreetArt.Font.material.mainTexture;
  }
 }
}
