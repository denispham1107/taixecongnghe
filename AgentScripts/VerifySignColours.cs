using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Unity.Collections;using Newtonsoft.Json.Linq;using PhoViet;
public static class VerifySignColours
{
 sealed class Data {public Vector3[] vertices;public Vector2[] uv;}
 static Data Read(Mesh m){using(var d=MeshUtility.AcquireReadOnlyMeshData(m))using(var v=new NativeArray<Vector3>(d[0].vertexCount,Allocator.Temp))using(var u=new NativeArray<Vector2>(d[0].vertexCount,Allocator.Temp)){d[0].GetVertices(v);d[0].GetUVs(0,u);return new Data{vertices=v.ToArray(),uv=u.ToArray()};}}
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  var spec=JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/PV_PetshopSigns.json"));var palette=(JObject)spec["board_palette"];var cache=new Dictionary<Mesh,Data>();
  Func<Mesh,float,float,float,float,float,string,bool> board=(mesh,plane,lo,hi,low,high,col)=>
  {
   if(!cache.TryGetValue(mesh,out var data)){data=Read(mesh);cache.Add(mesh,data);}var a=palette[col]["uv"];var uv=new Vector2((float)a[0],(float)a[1]);int points=0;
   for(int i=0;i<data.vertices.Length;i++){var v=data.vertices[i];if(Mathf.Abs(v.y-plane)<.001f&&v.x>=lo-.002f&&v.x<=hi+.002f&&v.z>=low-.002f&&v.z<=high+.002f&&Vector2.Distance(data.uv[i],uv)<.0001f)points++;}return points>=4;
  };
  var colours=new Dictionary<string,string>();
  for(int i=0;i<12;i++)foreach(bool pet in new[]{false,true})
  {
   string name="PV_StreetShop"+i.ToString("00")+(pet?"_Petshop":""),path="Assets/PhoViet/Art/Streets/"+name;var meta=JObject.Parse(File.ReadAllText(path+".json"));string colour=(string)meta["board"];colours[name]=colour;
   check(pet?colour=="blue":colour!="blue"&&colour!="navy"&&colour!="awnBlue","Reserved blue / distinct ordinary business: "+name);
   foreach(var mesh in AssetDatabase.LoadAllAssetsAtPath(path+".fbx").OfType<Mesh>())
   {
    check(board(mesh,4.115f,-3.68f,3.68f,3.42f,4.78f,colour),"Actual board UV colour in "+mesh.name);
    if(!pet&&(i==4||i==7||i==10))
    {
     var data=cache[mesh];var u=palette[colour]["uv"];var uv=new Vector2((float)u[0],(float)u[1]);int n=0;
     for(int k=0;k<data.vertices.Length;k++){var v=data.vertices[k];if((Mathf.Abs(v.x-3.47f)<.001f||Mathf.Abs(v.x-3.63f)<.001f)&&v.z>6.26f&&v.z<7.94f&&Vector2.Distance(data.uv[k],uv)<.0001f)n++;}
     check(n>=8,"Side blade board matches its business colour in "+mesh.name);
    }
   }
  }
  check(colours.Where(k=>!k.Key.EndsWith("_Petshop")).Select(k=>k.Value).Distinct().Count()==12,"Twelve distinct ordinary business colours");
  var riverData=new Dictionary<string,JObject>();foreach(string file in new[]{"PV_RiverApartments","PV_RiverPetshopExtra"})riverData[file]=JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/River/"+file+".json"));
  int streetBrands=0,riverBrands=0,streetPairs=0,riverPairs=0;
  foreach(var tile in g.roadRows.SelectMany(r=>r.tiles))
  {
   var shops=tile.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs().Length==3&&l.GetLODs()[0].renderers.Any(r=>r!=null&&r.GetComponent<MeshFilter>()!=null&&r.GetComponent<MeshFilter>().sharedMesh!=null&&r.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("PV_StreetShop"))).ToArray();check(shops.Length==14,"Fourteen shops at "+tile.name);
   foreach(int side in new[]{-1,1})
   {
    string previous=null;foreach(var shop in shops.Where(l=>Mathf.Sign(tile.InverseTransformPoint(l.transform.position).x)==side).OrderBy(l=>tile.InverseTransformPoint(l.transform.position).z))
    {
     string name=shop.GetLODs()[0].renderers[0].GetComponent<MeshFilter>().sharedMesh.name.Replace("_LOD0","");string colour=colours[name];if(colour=="blue")streetBrands++;
     if(previous!=null){check(colour!=previous,"Adjacent street boards differ at "+tile.name+" / "+side+" / "+name);streetPairs++;}previous=colour;
    }
   }
   var apt=tile.GetComponent<RiverBlock>().bridge.transform.Find("Căn hộ ven sông • Blender");var signs=new List<JObject>();
   foreach(Transform part in apt)
   {
    var filters=part.GetComponentsInChildren<MeshFilter>(true);check(filters.Length==4&&filters.All(m=>m.sharedMesh!=null),"Every apartment LOD/shadow reference resolves at "+part.name);
    string file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(filters[0].sharedMesh));var info=((JArray)riverData[file]["parts"]).OfType<JObject>().Single(p=>((string)p["name"]).Replace("_PetshopExtra","")==part.name);var rows=((JArray)info["shop_signs"]).OfType<JObject>().ToArray();signs.AddRange(rows);
    foreach(var row in rows)
    {
     float a=(float)row["x0"],b=(float)row["x1"],z=(float)row["zf"],f=(float)row["f"],y=(float)row["base"];string colour=(string)row["board"];bool pet=(bool)row["petshop"];if(pet)riverBrands++;
     check(pet?colour=="blue":colour!="blue"&&colour!="navy"&&colour!="awnBlue","Riverside blue reserved for Petshop");
     foreach(var mesh in filters.Select(m=>m.sharedMesh).Distinct())check(board(mesh,-(z+f*.20f),a+.2f,b-.2f,y+2.79f,y+3.51f,colour)&&board(mesh,-(z-f*3-f*.12f),a+.2f,b-.2f,y+2.79f,y+3.51f,colour),"Both actual riverside board faces / LOD colour in "+mesh.name);
    }
   }
   check(signs.Count==66,"All sixty-six riverside businesses accounted for");
   foreach(var line in signs.GroupBy(s=>new{z=(float)s["zf"],f=(float)s["f"]}))
   {
    JObject previous=null;foreach(var row in line.OrderBy(s=>(float)s["x0"]))
    {if(previous!=null&&Mathf.Abs((float)previous["x1"]-(float)row["x0"])<.01f){check((string)previous["board"]!=(string)row["board"],"Adjacent riverside boards differ at "+tile.name);riverPairs++;}previous=row;}
   }
  }
  check(streetBrands==147&&riverBrands==693,"Petshop frequency remains exactly thirty percent in both environments");check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"No additional render objects");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
  var report=new{success=true,count=checks.Count,streetPairs,riverPairs,streetBrands,riverBrands,checks};File.WriteAllText("Captures/sign-colours-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
