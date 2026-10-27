using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Unity.Collections;using PhoViet;
public static class VerifyPetshopSigns
{
 sealed class Data {public Vector3[] vertices;public Vector2[] uv;}
 static Data Read(Mesh mesh)
 {
  using(var list=MeshUtility.AcquireReadOnlyMeshData(mesh))using(var v=new NativeArray<Vector3>(list[0].vertexCount,Allocator.Temp))using(var uv=new NativeArray<Vector2>(list[0].vertexCount,Allocator.Temp))
  {list[0].GetVertices(v);list[0].GetUVs(0,uv);return new Data{vertices=v.ToArray(),uv=uv.ToArray()};}
 }
 static int Points(Data data,float plane,float lo,float hi,float lowY,float highY,Vector2? color=null)
 {
  int n=0;for(int i=0;i<data.vertices.Length;i++){var p=data.vertices[i];if(Mathf.Abs(p.y-plane)<.001f&&p.x>=lo-.001f&&p.x<=hi+.001f&&p.z>=lowY&&p.z<=highY&&(!color.HasValue||Vector2.Distance(data.uv[i],color.Value)<.0001f))n++;}return n;
 }
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  var info=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/PV_PetshopSigns.json"));
  Vector2 white=new Vector2(.0625f,.1875f);var samples=new List<object>();
  for(int shop=0;shop<12;shop++)
  {
   string prefix="PV_StreetShop"+shop.ToString("00")+"_Petshop";var meshes=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/Streets/"+prefix+".fbx").OfType<Mesh>().ToArray();
   check(meshes.Length==3,"Three real Petshop LODs for facade "+shop);
   foreach(var mesh in meshes)
   {
    var data=Read(mesh);check(!mesh.isReadable&&Points(data,4.118f,-3.7f,3.7f,3.35f,4.85f,white)>30,"White vector Petshop letters retained in "+mesh.name);
    check(Points(data,4.115f,-3.7f,3.7f,3.35f,4.85f,new Vector2(.6875f,.4375f))>=4,"Blue atlas board retained in "+mesh.name);
    string original="PV_StreetShop"+shop.ToString("00");var baseline=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/Streets/"+original+".fbx").OfType<Mesh>().Single(m=>m.name==mesh.name.Replace("_Petshop",""));
    var originalUpper=Read(baseline).vertices.Where(v=>v.z>8.1f).ToArray(); var currentUpper=new HashSet<Vector3>(data.vertices.Where(v=>v.z>8.1f)); check(originalUpper.Length>50&&originalUpper.All(v=>currentUpper.Contains(v)),"Upper building geometry unchanged in "+mesh.name);
   }
  }
  var baseMeshes=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/River/PV_RiverApartments.fbx").OfType<Mesh>().ToDictionary(m=>m.name);
  var cache=new Dictionary<Mesh,Data>();int unit=0;var units=(Newtonsoft.Json.Linq.JArray)info["river_order"];
  foreach(Newtonsoft.Json.Linq.JArray row in units)
  {
   float x0=(float)row[0],x1=(float)row[1],zf=(float)row[2],f=(float)row[3],cx=(x0+x1)*.5f;
   var near=baseMeshes.Values.Where(m=>m.name.EndsWith("_LOD0")&&m.bounds.min.x<=cx&&m.bounds.max.x>=cx&&m.bounds.min.y<=-zf+.01f&&m.bounds.max.y>=-zf-.01f).ToArray();
   check(near.Length==1,"Unique riverside facade for sign "+unit);string part=near[0].name.Replace("_LOD0","");
   for(int d=0;d<3;d++)
   {
    var mesh=baseMeshes[part+"_LOD"+d];if(!cache.TryGetValue(mesh,out var data)){data=Read(mesh);cache.Add(mesh,data);}
    check(Points(data,-(zf+f*.21f),x0+.1f,x1-.1f,2.98f,3.72f)>6,"Actual lettering geometry in river sign "+unit+" LOD"+d);
    check(Points(data,-(zf-f*3-f*.14f),x0+.1f,x1-.1f,2.98f,3.72f)>6,"River-facing lettering remains in sign "+unit+" LOD"+d);
    if(unit<(int)info["river_base_brands"])check(Points(data,-(zf+f*.21f),x0+.1f,x1-.1f,2.98f,3.72f,white)>6,"Petshop lettering is white in river sign "+unit+" LOD"+d+" part="+part+" key="+row.ToString(Newtonsoft.Json.Formatting.None)+" white="+Points(data,-(zf+f*.21f),x0+.1f,x1-.1f,2.98f,3.72f,white));
   }
   if(unit<2)samples.Add(new{part,x0,x1,zf,f});unit++;
  }
  var extraMeshes=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/River/PV_RiverPetshopExtra.fbx").OfType<Mesh>().ToArray();var extraKey=(Newtonsoft.Json.Linq.JArray)units[(int)info["river_base_brands"]];
  foreach(var mesh in extraMeshes){var data=Read(mesh);float a=(float)extraKey[0],b=(float)extraKey[1],z=(float)extraKey[2],f=(float)extraKey[3];check(Points(data,-(z+f*.21f),a+.1f,b-.1f,2.98f,3.72f,white)>6&&Points(data,-(z-f*3-f*.14f),a+.1f,b-.1f,2.98f,3.72f,white)>6,"Extra Petshop business has white letters on both elevations in "+mesh.name);}
  var all=g.scenery.GetComponentsInChildren<MeshFilter>(true);var street=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs().Length==3&&l.GetLODs()[0].renderers.Any(r=>r!=null&&r.GetComponent<MeshFilter>()!=null&&r.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("PV_StreetShop"))).ToArray();
  int branded=street.Count(l=>AssetDatabase.GetAssetPath(l.GetLODs()[0].renderers[0].GetComponent<MeshFilter>().sharedMesh).EndsWith("_Petshop.fbx"));
  check(street.Length==490&&branded==147,"Exactly 147 of 490 pooled street facades use Cù Lao Petshop");
  int high=0;
  foreach(var tile in g.roadRows.SelectMany(r=>r.tiles))
  {
   var apt=tile.GetComponent<RiverBlock>().bridge.transform.Find("Căn hộ ven sông • Blender");string part=(string)info["river_extra_part"];
   var meshes=apt.Find(part).GetComponentsInChildren<MeshFilter>(true);bool extra=AssetDatabase.GetAssetPath(meshes[0].sharedMesh).EndsWith("PV_RiverPetshopExtra.fbx");if(extra)high++;
   check(meshes.Length==4&&meshes.All(m=>AssetDatabase.GetAssetPath(m.sharedMesh).EndsWith(extra?"PV_RiverPetshopExtra.fbx":"PV_RiverApartments.fbx")),"Visible LODs and shadow proxy share the correct variant in "+tile.name);
  }
  check(high==28&&35*19+high==693&&unit*35==2310,"Exactly 693 of 2310 riverside signs use Cù Lao Petshop");
  var install=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Captures/petshop-sign-install.json"));check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==(int)install["rendererCount"],"No additional render objects or sign text components");
  check(street.All(l=>Mathf.Abs(l.GetLODs()[0].screenRelativeTransitionHeight-.45f)<.001f&&Mathf.Abs(l.GetLODs()[1].screenRelativeTransitionHeight-.175f)<.001f)&&QualitySettings.antiAliasing==4&&QualitySettings.shadowDistance==100,"Distance LOD, antialiasing and shadows retained");
  check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
  var report=new{success=true,count=checks.Count,checks,streetShops=490,streetBrands=branded,riverUnits=unit,riverBrands=693,riverSamples=samples};File.WriteAllText("Captures/petshop-sign-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
