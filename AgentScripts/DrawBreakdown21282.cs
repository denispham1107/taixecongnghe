using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
// 21.282: (Play) renderer đang được camera chính thấy (frustum + LOD), gom theo shader / instancing / MaterialPropertyBlock / đổ bóng.
public static class DrawBreakdown21282
{
 public static object Main()
 {
  var cam=Camera.main; var planes=GeometryUtility.CalculateFrustumPlanes(cam);
  var rows=new Dictionary<string,int>(); int total=0, shadowCasters=0, mpb=0;
  var lodVisible=new HashSet<Renderer>(); var inLod=new HashSet<Renderer>();
  foreach(var lg in UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude)){
   if(!lg.enabled)continue; var lods=lg.GetLODs(); if(lods.Length==0)continue;
   float size=lg.size*Mathf.Max(lg.transform.lossyScale.x,Mathf.Max(lg.transform.lossyScale.y,lg.transform.lossyScale.z));
   float dist=Vector3.Distance(cam.transform.position,lg.transform.TransformPoint(lg.localReferencePoint));
   float h=size/(2f*dist*Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad))*QualitySettings.lodBias; // xấp xỉ chiều cao màn hình
   int pick=-1; for(int i=0;i<lods.Length;i++){ if(h>=lods[i].screenRelativeTransitionHeight){pick=i;break;} }
   for(int i=0;i<lods.Length;i++) foreach(var r in lods[i].renderers){ if(!r)continue; inLod.Add(r); if(i==pick)lodVisible.Add(r);} }
  var mb=new MaterialPropertyBlock(); var pairs=new Dictionary<(Mesh,Material),int>(); var src=new Dictionary<Mesh,string>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)){
   if(!r.enabled||r is ParticleSystemRenderer||r is TrailRenderer||r is LineRenderer)continue;
   if(inLod.Contains(r)&&!lodVisible.Contains(r))continue;
   if(!GeometryUtility.TestPlanesAABB(planes,r.bounds))continue;
   if(Vector3.Distance(cam.transform.position,r.bounds.ClosestPoint(cam.transform.position))>cam.farClipPlane)continue;
   bool hasMpb=r.HasPropertyBlock(); if(hasMpb)mpb++;
   var mats=r.sharedMaterials; int subs=r is MeshRenderer&&r.GetComponent<MeshFilter>()&&r.GetComponent<MeshFilter>().sharedMesh?r.GetComponent<MeshFilter>().sharedMesh.subMeshCount:1;
   var mfm=r.GetComponent<MeshFilter>()?r.GetComponent<MeshFilter>().sharedMesh:null;
   for(int i=0;i<mats.Length&&i<Math.Max(1,subs);i++){var m=mats[i]; if(!m)continue; total++; if(mfm){pairs[(mfm,m)]=(pairs.TryGetValue((mfm,m),out var pc)?pc:0)+1; if(!src.ContainsKey(mfm))src[mfm]=UnityEditor.AssetDatabase.GetAssetPath(mfm).Split('/').Last()+"|"+r.transform.root.name;}
    var k=m.shader.name+(m.enableInstancing?" [inst]":" [no-inst]")+(hasMpb?" [MPB]":"");
    rows[k]=(rows.TryGetValue(k,out var o)?o:0)+1;}
   if(r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.Off)shadowCasters++;
  }
  var singles=pairs.Where(x=>x.Value==1).GroupBy(x=>src[x.Key.Item1]).Select(g=>(g.Key,g.Count())).OrderByDescending(x=>x.Item2).Take(15).Select(x=>x.Item2+" "+x.Key).ToArray();
  int nSingle=pairs.Count(x=>x.Value==1), nGroups=pairs.Count(x=>x.Value>1);
  return new{nSingle,nGroups,singles,cam=cam.transform.position.ToString(),far=cam.farClipPlane,total,shadowCasters,mpb,rows=rows.OrderByDescending(x=>x.Value).Take(25).Select(x=>x.Value+"  "+x.Key).ToArray()};
 }
}
