using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
// Cột đèn đường cong + cột đèn tín hiệu cần vươn theo ảnh mẫu Sài Gòn (pho_viet_saigon_streets.py, build_saigon_fixtures).
// Không dựng lại road rows/đèn ngày–đêm: giữ 70 bộ "Đèn LED", 420 bóng "Tín hiệu"; chỉ
// - gỡ tam giác cột/cần/hộp tín hiệu cũ khỏi mesh gộp "Geometry • …" của từng khối (lưu mesh mới trong Generated),
// - gắn PV_TrafficSignal vào mỗi nhóm "Đèn giao thông" và dời "Hộp đèn" tới hộp treo mới,
// - dời "Tâm quang học LED" và vệt sáng mặt đường theo bầu đèn mới, rồi lưu scene.
public static class InstallSaigonStreetFixtures
{
 public const float LampReach=2.35f,LampHeight=8.48f,SignalReach=3.0f,SignalHeadY=6.82f;
 const string SignalPath="Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx",LampPath="Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.fbx";
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.roadRows.Count!=5)throw new Exception("PhoViet scene required");
  AssetDatabase.ImportAsset(SignalPath,ImportAssetOptions.ForceUpdate);AssetDatabase.ImportAsset(LampPath,ImportAssetOptions.ForceUpdate);
  var signal=AssetDatabase.LoadAssetAtPath<GameObject>(SignalPath);var lamp=AssetDatabase.LoadAssetAtPath<GameObject>(LampPath);
  var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
  var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Lighting/PV_StreetLEDMetal.mat");
  metal.color=Hex("8C959C");metal.SetFloat("_Metallic",.35f);metal.SetFloat("_Glossiness",.45f);EditorUtility.SetDirty(metal);
  var cleaned=new Dictionary<Mesh,Mesh>();int removed=0,signals=0,lamps=0;
  foreach(var row in g.roadRows)foreach(var block in row.tiles)
  {
   foreach(var filter in block.GetComponentsInChildren<MeshFilter>(true))
   {
    if(filter.transform.parent!=block||!filter.name.StartsWith("Geometry • ",StringComparison.Ordinal)||filter.sharedMesh==null)continue;
    if(!cleaned.TryGetValue(filter.sharedMesh,out var clean)){clean=RemoveOldSignals(filter.sharedMesh,ref removed);cleaned.Add(filter.sharedMesh,clean);}
    filter.sharedMesh=clean;
   }
   foreach(Transform group in block.Cast<Transform>().Where(t=>t.name=="Đèn giao thông").ToArray())
   {
    float toRoad=group.localPosition.x<0?1:-1;
    var old=group.Find("Blender • cột đèn tín hiệu");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
    var holder=new GameObject("Blender • cột đèn tín hiệu").transform;holder.SetParent(group,false);holder.localRotation=Quaternion.Euler(0,toRoad>0?0:180,0);
    var instance=(GameObject)PrefabUtility.InstantiatePrefab(signal,holder);instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=signal.transform.localRotation;instance.transform.localScale=signal.transform.localScale;
    var renderers=instance.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r=>r.name.Contains("_LOD2")?2:r.name.Contains("_LOD1")?1:0).ToArray();
    if(renderers.Length!=3)throw new Exception("PV_TrafficSignal needs three LODs");
    foreach(var r in renderers)r.sharedMaterial=atlas;
    var lod=instance.GetComponent<LODGroup>();if(lod==null)lod=instance.AddComponent<LODGroup>();
    lod.SetLODs(new[]{new LOD(.12f,new Renderer[]{renderers[0]}),new LOD(.04f,new Renderer[]{renderers[1]}),new LOD(.004f,new Renderer[]{renderers[2]})});lod.RecalculateBounds();
    var head=group.Find("Hộp đèn");if(head==null)throw new Exception("Signal head missing");
    head.localPosition=new Vector3(toRoad*SignalReach,SignalHeadY,0);signals++;
   }
   var lighting=block.Find("Chiếu sáng phố • Blender");if(lighting==null)throw new Exception(block.name+": street lighting missing");
   foreach(Transform unit in lighting.Cast<Transform>().Where(t=>t.name.StartsWith("Đèn LED • ",StringComparison.Ordinal)))
   {
    float side=unit.localPosition.x<0?-1:1;
    var model=unit.Find("Blender • đèn đường LED");model.localRotation=lamp.transform.localRotation;model.localPosition=Vector3.zero;
    var lodGroup=model.GetComponent<LODGroup>();if(lodGroup!=null)lodGroup.RecalculateBounds();
    unit.Find("Tâm quang học LED").localPosition=new Vector3(LampReach,LampHeight,0);
    var pool=lighting.Cast<Transform>().First(t=>t.name=="Vệt sáng đèn đường"&&Mathf.Sign(t.localPosition.x)==side);
    pool.localPosition=new Vector3(side*(7.6f-LampReach),pool.localPosition.y,pool.localPosition.z);lamps++;
   }
  }
  EditorUtility.SetDirty(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
  return new{success=true,signals,lamps,removedTriangles=removed,cleanedMeshes=cleaned.Count(p=>p.Key!=p.Value)};
 }
 // Cột/cần/hộp tín hiệu cũ của VietnamStreet.Signal ở x=±7,65, z=61/78 (cột tới 4,9 m, hộp ở x=±6,15).
 static Mesh RemoveOldSignals(Mesh source,ref int removed)
 {
  var v=source.vertices;var tris=source.triangles;var keep=new List<int>();int skipped=0;
  Func<Vector3,bool> inside=p=>Mathf.Abs(p.x)>5.8f&&Mathf.Abs(p.x)<7.85f&&p.y>.08f&&p.y<5.12f&&(Mathf.Abs(p.z-61)<.5f||Mathf.Abs(p.z-78)<.5f);
  for(int i=0;i<tris.Length;i+=3)
  {
   if(inside(v[tris[i]])&&inside(v[tris[i+1]])&&inside(v[tris[i+2]])){skipped++;continue;}
   keep.Add(tris[i]);keep.Add(tris[i+1]);keep.Add(tris[i+2]);
  }
  if(skipped==0)return source;removed+=skipped;
  string path="Assets/PhoViet/Generated/PV_NoOldSignals_"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source))+".asset";
  var clean=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(clean==null){clean=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(clean,path);}
  clean.name=source.name;clean.triangles=keep.ToArray();clean.RecalculateBounds();EditorUtility.SetDirty(clean);return clean;
 }
 static Color Hex(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
}
