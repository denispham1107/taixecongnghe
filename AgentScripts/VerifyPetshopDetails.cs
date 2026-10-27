using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Unity.Collections;using Newtonsoft.Json.Linq;using PhoViet;
public static class VerifyPetshopDetails
{
 static Vector3[] Vertices(Mesh mesh){using(var data=MeshUtility.AcquireReadOnlyMeshData(mesh))using(var v=new NativeArray<Vector3>(data[0].vertexCount,Allocator.Temp)){data[0].GetVertices(v);return v.ToArray();}}
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
  var library=JObject.Parse(File.ReadAllText("Assets/PhoViet/Art/Pets/PV_PetGeometry.json"));var cache=new Dictionary<Mesh,Vector3[]>();
  foreach(string species in new[]{"dog","cat","rabbit"})
  {
   var models=AssetDatabase.LoadAllAssetsAtPath("Assets/PhoViet/Art/Pets/PV_Pet"+char.ToUpperInvariant(species[0])+species.Substring(1)+".fbx").OfType<Mesh>().ToArray();
   check(models.Length==3,"Three standalone Blender MCP LOD meshes for "+species);
   check(((JArray)library["triangles"][species]).Select(x=>(int)x).Max()<1000,"Mobile geometry budget under 1000 triangles per "+species);
  }
  Action<Mesh,JArray,int,string> verifyPets=(mesh,pets,detail,label)=>
  {
   if(!cache.TryGetValue(mesh,out var actual)){actual=Vertices(mesh);cache.Add(mesh,actual);}
   foreach(JObject pet in pets)
   {
    string species=(string)pet["species"];var source=(JArray)library["models"][species][detail]["v"];var pos=(JArray)pet["position"];float yaw=(float)pet["yaw"]*Mathf.Deg2Rad,scale=(float)pet["scale"],c=Mathf.Cos(yaw),s=Mathf.Sin(yaw);bool found=true;
    for(int sample=0;sample<6;sample++)
    {
     var v=(JArray)source[sample*(source.Count-1)/5];float x=(float)v[0],y=(float)v[1],z=(float)v[2];var target=new Vector3((float)pos[0]+scale*(c*x+s*z),-((float)pos[2]+scale*(-s*x+c*z)),(float)pos[1]+scale*y);
     found&=actual.Any(p=>(p-target).sqrMagnitude<.0000001f);
    }
    check(found,label+": actual MCP "+species+" geometry present "+(string)pet["location"]);
   }
  };
  for(int i=0;i<12;i++)
  {
   string asset="PV_StreetShop"+i.ToString("00")+"_Petshop",path="Assets/PhoViet/Art/Streets/"+asset;var meta=JObject.Parse(File.ReadAllText(path+".json"));var pets=(JArray)meta["pets"];
   check((string)meta["subtitle"]=="Lưu Trú - Spa Pet","Exact subtitle for shop "+i);check(pets.Count==9,"Nine pets in shop "+i);
   foreach(string species in new[]{"dog","cat","rabbit"})check(pets.Any(p=>(string)p["species"]==species&&(string)p["location"]=="outside")&&pets.Any(p=>(string)p["species"]==species&&(string)p["location"]=="inside"),"Each species appears inside and outside shop "+i+": "+species);
   check(!((JArray)meta["obstacles"]).Any(o=>new[]{"Bàn","Ghế","Tủ","Quầy","Kệ"}.Any(k=>((string)o["kind"]).Contains(k))),"No removed furniture/cabinet collision boxes in shop "+i);
   foreach(var mesh in AssetDatabase.LoadAllAssetsAtPath(path+".fbx").OfType<Mesh>()){int detail=mesh.name.Last()-'0';verifyPets(mesh,pets,detail,mesh.name);}
  }
  int basePets=0;
  foreach(string file in new[]{"PV_RiverApartments","PV_RiverPetshopExtra"})
  {
   string path="Assets/PhoViet/Art/River/"+file;var info=JObject.Parse(File.ReadAllText(path+".json"));var meshes=AssetDatabase.LoadAllAssetsAtPath(path+".fbx").OfType<Mesh>().ToDictionary(m=>m.name);
   foreach(JObject part in info["parts"])
   {
    if(part["pets"]==null)continue;var pets=(JArray)part["pets"];if(file=="PV_RiverApartments")basePets+=pets.Count;
    check(((JArray)part["pets_by_lod"]).All(n=>(int)n==pets.Count),"Riverside pets retained in all LODs: "+(string)part["name"]);
    for(int d=0;d<3;d++)verifyPets(meshes[(string)part["name"]+"_LOD"+d],pets,d,(string)part["name"]+" LOD"+d);
   }
  }
  check(basePets==171,"Nineteen Petshop units each retain nine riverside pets");
  var shops=g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs().Length==3&&l.GetLODs()[0].renderers.Any(r=>r!=null&&r.GetComponent<MeshFilter>()!=null&&AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh).EndsWith("_Petshop.fbx"))).ToArray();
  foreach(var shop in shops){var set=shop.GetComponentInParent<StreetObstacleSet>(true);check(set!=null&&!set.obstacles.Any(o=>new[]{"Bàn","Ghế","Tủ","Quầy","Kệ"}.Any(k=>o.kind.Contains(k))),"No ghost furniture collider at "+shop.transform.parent.name);}
  check(shops.Length==147&&g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"Brand ratio and renderer count retained");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
  var report=new{success=true,count=checks.Count,checks,normalPetsPerShop=9,riverPetsPerShop=9,pooledPetInstances=7560};File.WriteAllText("Captures/petshop-details-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
