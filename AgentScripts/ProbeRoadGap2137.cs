using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
// 21.138: what covers the road beyond the next junction at each graphics level, with an image of the same frame.
public static class ProbeRoadGap2137
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static string Describe(Renderer r){var f=r.GetComponent<MeshFilter>();var m=r.sharedMaterial;return r.name+"|"+(f!=null&&f.sharedMesh!=null?f.sharedMesh.name:"")+"|"+(m!=null?m.name+"/"+m.shader.name:"");}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;bool dino=g.dinosaursEnabled,tank=g.tankBarrageEnabled;var rows=new List<object>();
  QualitySettings.SetQualityLevel(3,true);QualitySettings.lodBias=1;QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.shadowCascades=0;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.pixelLightCount=2;
  g.forceTouchGraphics=true;typeof(PhoVietGame).GetField("graphicsInitialized",F).SetValue(g,false);typeof(PhoVietGame).GetMethod("InitializeGraphicsSettings",F).Invoke(g,null);
  var rt=new RenderTexture(844,390,24);var tex=new Texture2D(844,390,TextureFormat.RGB24,false);
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=false;
   var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));
   foreach(int level in new[]{0,1,2})
   {
    g.SetGraphicsQuality(level);g.ResetRide();g.StartRide();g.daylight.SetCycleTime(8);
    foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    for(int i=0;i<40&&g.Distance<42;i++){tick(1/30f);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}}
    g.daylight.UpdateLamps();typeof(PhoVietGame).GetMethod("UpdateTileCulling",F).Invoke(g,new object[]{true});
    var old=g.rideCamera.targetTexture;g.rideCamera.targetTexture=rt;g.rideCamera.Render();g.rideCamera.targetTexture=old;var a=RenderTexture.active;RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,844,390),0,0);tex.Apply();RenderTexture.active=a;File.WriteAllBytes("Captures/roadgap-level"+level+".png",tex.EncodeToPNG());
    var cam=g.rideCamera;var planes=GeometryUtility.CalculateFrustumPlanes(cam);
    // Ground-level things along the road ahead.
    var ahead=new List<object>();
    foreach(var row in g.roadRows){if(!row.root.gameObject.activeInHierarchy)continue;foreach(var t in row.tiles){if(!t.gameObject.activeInHierarchy)continue;foreach(var r in t.GetComponentsInChildren<Renderer>(true)){var b=r.bounds;if(b.extents.y>.6f||b.extents.z<8||Mathf.Abs(b.center.x)>6||b.center.y>.6f)continue;Vector3 c=cam.transform.InverseTransformPoint(b.center);ahead.Add(new{what=Describe(r),z=Math.Round(c.z),near=Math.Round(c.z-b.extents.z),far=Math.Round(c.z+b.extents.z),on=r.gameObject.activeInHierarchy&&r.enabled,lod=r.GetComponentInParent<LODGroup>()!=null?r.GetComponentInParent<LODGroup>().name:"",visible=r.isVisible});}}}
    rows.Add(new{level,distance=Math.Round(g.Distance),junction=Math.Round(g.NextJunctionDistance),far=cam.farClipPlane,lodBias=QualitySettings.lodBias,fogStart=RenderSettings.fogStartDistance,fogEnd=RenderSettings.fogEndDistance,fogColor=ColorUtility.ToHtmlStringRGB(RenderSettings.fogColor),tiles=g.ActiveRoadTiles,ahead});
   }
   File.WriteAllText("Captures/roadgap-probe.json",Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented));return "ok";
  }
  finally{g.forceTouchGraphics=false;g.dinosaursEnabled=dino;g.tankBarrageEnabled=tank;g.SetGraphicsQuality(0);g.ResetRide();Time.timeScale=time;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
}
