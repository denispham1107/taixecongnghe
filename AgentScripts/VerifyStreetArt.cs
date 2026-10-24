using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
using Meta=PhoViet.Editor.StreetArtMetadata;
public static class VerifyStreetArt
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
  Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  var all=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
  check(all.Length==1260,"Every block in all five road rows has the complete Blender kit");
  check(all.Select(l=>AssetDatabase.GetAssetPath(l.GetComponentInChildren<MeshFilter>(true).sharedMesh)).Distinct().Count()==16,"Twelve shops, three trees and pavement are instanced from sixteen shared Blender FBX assets");
  // Bộ phố Sài Gòn (24/10/2026): metadata, ngân sách tam giác và palette 8x8 từ pho_viet_saigon_streets.py.
  var assets=Enumerable.Range(0,12).Select(i=>"PV_StreetShop"+i.ToString("00")).Concat(new[]{"PV_StreetTree0","PV_StreetTree1","PV_StreetTree2","PV_StreetPavement"}).ToArray();
  var metas=new Dictionary<string,Meta>();
  foreach(var a in assets)
  {
   var m=Meta.Load(a);metas[a]=m;
   check(m.generator=="pho_viet_saigon_streets.py"&&m.palette_grid!=null&&m.palette_grid.SequenceEqual(new[]{8,8}),a+": exported by the Saigon street generator with the 8x8 palette grid");
   int budget=a.Contains("Shop")?27879:a.Contains("Tree")?30076:15500;
   check(m.lod_triangles.Length==3&&m.lod_triangles[0]<=budget&&m.lod_triangles[0]>m.lod_triangles[1]&&m.lod_triangles[1]>m.lod_triangles[2],a+": three decreasing LODs within the previous WebGL triangle budget");
   var meshes=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/Streets/"+a+".fbx").OfType<Mesh>().OrderBy(x=>x.name).ToArray();
   check(meshes.Length==3&&Enumerable.Range(0,3).All(i=>Mathf.Abs(meshes[i].triangles.Length/3-m.lod_triangles[i])<=m.lod_triangles[i]*.02f+4),a+": imported Unity meshes match the Blender LOD triangle counts");
   check(m.obstacles!=null&&(a=="PV_StreetPavement"||m.obstacles.Length>0)&&m.obstacles.All(o=>o.size.All(v=>v>0)),a+": light obstacle boxes are exported with the geometry (flat pavement has none)");
  }
  check(metas.Values.Where(m=>m.name.Contains("Tree")).All(m=>m.leaf_counts.Length==3&&m.leaf_counts[2]>=500&&m.leaf_counts[0]>m.leaf_counts[1]&&m.leaf_counts[1]>m.leaf_counts[2]),"Street trees keep individual leaves at every LOD, no round foliage blobs");
  var palette=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Art/Streets/PV_StreetPalette.png");
  check(palette.width==128&&palette.height==128&&palette.filterMode==FilterMode.Point&&palette.mipmapCount==1,"Street palette stays a 128px point-sampled atlas without mip bleeding");
  foreach(var row in g.roadRows)
  {
   check(row.tiles.Count==7,"Seven connected blocks retained on "+row.root.name);
   foreach(var b in row.tiles)
   {
    var lods=b.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)).ToArray();
    check(lods.Length==36,b.name+": fourteen shopfronts, six trees and sixteen pavement segments");
    var shops=b.Cast<Transform>().Where(t=>t.name.StartsWith("Blender • ",StringComparison.Ordinal)).ToArray();
    check(shops.Length==14,b.name+": both street sides use detailed shop fronts");
    check(shops.All(t=>Mathf.Abs(Vector3.Dot(t.localRotation*Vector3.forward,Vector3.right))>.99f),b.name+": shop facades and integral wall-mounted signs face their own road at ninety degrees");
    check(shops.All(t=>{var model=t.Cast<Transform>().First(c=>c.name.StartsWith("PV_StreetShop",StringComparison.Ordinal));return t.name=="Blender • "+VietnamStreet.Shops[int.Parse(model.name.Substring(13,2))];}),b.name+": storefront object names match the lettering on each Saigon sign");
    check(shops.All(t=>{var model=t.Cast<Transform>().First(c=>c.name.StartsWith("PV_StreetShop",StringComparison.Ordinal));var set=t.GetComponent<StreetObstacleSet>();return set!=null&&set.obstacles.Length==metas[model.name].obstacles.Length;}),b.name+": every shop collision set matches its exported props, awning, sign and building");
    // Kích thước LOD tính bằng mét; nhà cao tầng phía sau có thể cao ~37 m nhưng không phải lỗi scale cm.
    check(lods.All(l=>l.size>4&&l.size<45),b.name+": model dimensions remain in metres after FBX conversion");
    check(lods.All(l=>l.GetLODs().Length==3&&l.GetLODs().All(d=>d.renderers.Length==1&&d.renderers[0]!=null)),b.name+": three complete distance LODs for every scenery instance");
   }
  }
  check(all.SelectMany(l=>l.GetComponentsInChildren<MeshRenderer>(true)).All(r=>r.sharedMaterial.name=="PV_StreetAtlas"),"All scenery uses one shared atlas material");
  var shopRenderers=all.Where(l=>l.name.StartsWith("PV_StreetShop",StringComparison.Ordinal)).SelectMany(l=>l.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
  check(shopRenderers.All(r=>{var m=r.GetComponent<MeshFilter>().sharedMesh;bool repair=r.name.StartsWith("PV_StreetShop05",StringComparison.Ordinal);return repair?m.subMeshCount==1:m.subMeshCount==2&&r.sharedMaterials.Length==2&&r.sharedMaterials[1].name=="PV_StreetGlass"&&r.sharedMaterials[1].renderQueue>=3000;}),"Every glazed shopfront renders its glass submesh with the transparent PV_StreetGlass (repair shop stays open)");
  check(g.GetComponentsInChildren<BikeVisualAnimator>(true).Length==33,"Detailed motorcycles and riders retained");
  check(g.GetComponentsInChildren<CarVisualAnimator>(true).Length==11,"Detailed four-wheel traffic retained");
  check(g.trafficLights.Count==420,"All junction traffic lamps retained");
  check(!g.jobsEnabled&&!g.JobsAvailable,"Ride jobs remain locked while the street art changes");
  // Cột đèn đường cong + cột tín hiệu cần vươn (yêu cầu 24/10 lần hai) và mây trời.
  var signals=g.scenery.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Đèn giao thông").ToArray();
  check(signals.Length==140&&signals.All(t=>t.Find("Blender • cột đèn tín hiệu")!=null&&t.Find("Blender • cột đèn tín hiệu").GetComponentInChildren<LODGroup>(true).GetLODs().Length==3),"Every junction has the Blender cantilever signal pole with three LODs");
  check(signals.All(t=>{var h=t.Find("Hộp đèn").localPosition;return Mathf.Abs(Mathf.Abs(h.x)-3f)<.01f&&Mathf.Sign(h.x)==-Mathf.Sign(t.localPosition.x)&&Mathf.Abs(h.y-6.82f)<.01f;}),"Runtime red/amber/green bulbs hang under the new arm, facing approaching traffic");
  check(g.trafficLights.All(r=>r!=null&&r.transform.parent.name=="Hộp đèn"),"Traffic light colour renderers still drive the moved signal heads");
  check(g.roadRows.SelectMany(r=>r.tiles).SelectMany(b=>b.Cast<Transform>()).Where(t=>t.name.StartsWith("Geometry • ",StringComparison.Ordinal)).All(t=>{var m=t.GetComponent<MeshFilter>().sharedMesh;var v=m.vertices;var tr=m.triangles;for(int i=0;i<tr.Length;i+=3){var p=v[tr[i]];if(Mathf.Abs(p.x)>5.8f&&Mathf.Abs(p.x)<7.85f&&p.y>.5f&&p.y<5.1f&&(Mathf.Abs(p.z-61)<.5f||Mathf.Abs(p.z-78)<.5f))return false;}return true;}),"Old box-shaped signal posts are removed from the combined block meshes");
  check(g.roadRows.SelectMany(r=>r.tiles).SelectMany(b=>b.Cast<Transform>()).Where(t=>t.name.StartsWith("Geometry • ",StringComparison.Ordinal)).All(t=>{var m=t.GetComponent<MeshFilter>().sharedMesh;var v=m.vertices;var tr=m.triangles;for(int i=0;i<tr.Length;i+=3){var p=v[tr[i]];if(Mathf.Abs(Mathf.Abs(p.x)-8.15f)<.45f&&p.y>.9f&&p.y<1.9f&&new[]{15f,33f,51f}.Any(z=>Mathf.Abs(p.z-z)<.45f))return false;}return true;})&&!g.scenery.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Người đi bộ"),"Standing sidewalk people are removed from every block (user request 24/10)");
  check(!g.scenery.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Xe bánh mì")&&g.roadRows.SelectMany(r=>r.tiles).SelectMany(b=>b.Cast<Transform>()).Where(t=>t.name.StartsWith("Geometry • ",StringComparison.Ordinal)).All(t=>{var m=t.GetComponent<MeshFilter>().sharedMesh;var v=m.vertices;var tr=m.triangles;for(int i=0;i<tr.Length;i+=3){var p=v[tr[i]];if(Mathf.Abs(Mathf.Abs(p.x)-8.5f)<.4f&&p.y>.6f&&p.y<2.2f&&Mathf.Abs(p.z-43)<.8f)return false;}return true;})&&g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).All(s=>s.obstacles.All(o=>o.kind!="Xe đẩy bán hàng")),"Old bread cart no longer stands in front of the Bún Bò Huế food cabinet (user request 24/10)");
  var lampUnits=g.scenery.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Đèn LED • ",StringComparison.Ordinal)).ToArray();
  check(lampUnits.Length==70&&lampUnits.All(u=>(u.Find("Tâm quang học LED").localPosition-new Vector3(2.35f,8.48f,0)).sqrMagnitude<1e-4f),"Street-lamp optical centres follow the curved Blender luminaire");
  check(lampUnits.All(u=>{var b=u.Find("Blender • đèn đường LED").GetComponent<MeshRenderer>().bounds;var head=u.Find("Tâm quang học LED").position;return b.max.y>8.4f&&b.Contains(head+Vector3.up*.08f);}),"Curved lamp geometry actually reaches its optical centre");
  var sky=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Lighting/PV_DayNightSky.mat");
  check(sky.shader.name=="PhoViet/Day Night Sky"&&sky.shader.FindPropertyIndex("_CloudCover")>=0&&!ShaderUtil.ShaderHasError(sky.shader),"Sky shader renders procedural clouds without compile errors");
  return new{success=true,count=checks.Count,checks};
 }
}
