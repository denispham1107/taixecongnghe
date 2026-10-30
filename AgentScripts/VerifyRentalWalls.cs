using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;using Newtonsoft.Json.Linq;
public static class VerifyRentalWalls
{
 static int checks;static void Check(bool b,string m){checks++;if(!b)throw new Exception(m);}
 public static object Main()
 {
  checks=0;var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  var before=JArray.Parse(File.ReadAllText("Captures/rental-walls-meshes-before.json"));
  var paths=AssetDatabase.FindAssets("t:Model",new[]{"Assets/PhoViet/Art/Streets","Assets/PhoViet/Art/River"}).Select(AssetDatabase.GUIDToAssetPath).Where(PhoViet.Editor.PhoVietWallFinish.IsBuilding).ToArray();
  Check(paths.Length==25,"25 building models");int meshes=0,wallVertices=0,glassFaces=0;var rows=new List<object>();
  foreach(var path in paths)foreach(var filter in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<MeshFilter>(true))
  {
   var m=filter.sharedMesh;var uv=m.uv;var materials=filter.GetComponent<Renderer>().sharedMaterials;int marked=0;meshes++;
   for(int i=0;i<uv.Length;i++)
   {
    float f=Mathf.Repeat(uv[i].x*8,1);if(Mathf.Abs(f-.75f)<.001f)marked++;
    else if(Mathf.Abs(f-.5f)>.001f)throw new Exception("Invalid marker "+m.name);
   }
   Check(marked>0,"Plaster coverage "+m.name);wallVertices+=marked;
   Check(uv.Length==m.vertexCount,"UV count "+m.name);
   var clone=UnityEngine.Object.Instantiate(m);try{PhoViet.Editor.PhoVietWallFinish.MarkPlaster(clone,materials);Check(uv.SequenceEqual(clone.uv),"Idempotent importer "+m.name);}finally{UnityEngine.Object.DestroyImmediate(clone);}
   if(!path.Contains("RiverApartments"))
   {var old=before.Single(x=>(string)x["path"]==path&&(string)x["name"]==m.name);Check(m.vertexCount==(int)old["vertices"],"Street topology "+m.name);}
   for(int s=0;s<m.subMeshCount;s++)
   {
    var ids=m.GetTriangles(s);bool glass=s==1;bool room=s==2;
    bool valid=true;foreach(int v in ids)if(Mathf.Repeat(uv[v].x*8,1)>.7f)
    {int colour=Mathf.FloorToInt(uv[v].x*8)+8*Mathf.FloorToInt(uv[v].y*8);if(glass||!(colour>=0&&colour<=9||colour==21||colour==22||room&&colour==19))valid=false;}
    Check(valid,"Excluded surface "+m.name+"/"+materials[s].name);if(glass)glassFaces+=ids.Length/3;
   }
   rows.Add(new{name=m.name,vertices=m.vertexCount,plasterVertices=marked});
  }
  Check(meshes==144,"144 building meshes");Check(glassFaces>0,"Glass slot explicitly checked (import materials are Default-Material)");
  int otherMeshes=0;foreach(var row in before.Where(x=>!PhoViet.Editor.PhoVietWallFinish.IsBuilding((string)x["path"])))
  {
   var m=AssetDatabase.LoadAllAssetsAtPath((string)row["path"]).OfType<Mesh>().Single(x=>x.name==(string)row["name"]);
   Check(m.vertexCount==(int)row["vertices"],"Other topology "+m.name);
   Check(m.uv.All(v=>Mathf.Repeat(v.x*8,1)<.7f),"Other unmarked "+m.name);otherMeshes++;
  }
  Check(otherMeshes==27,"27 non-building atlas meshes");
  foreach(string name in new[]{"PhoViet/Street Wall Finish","PhoViet/Apartment Room"})
  {var sh=Shader.Find(name);Check(sh!=null&&sh.isSupported,"Supported "+name);var messages=ShaderUtil.GetShaderMessages(sh);Check(!messages.Any(m=>m.severity.ToString()=="Error"),"Shader compile "+name);}
  var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");var roomMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/River/PV_RiverRoomLight.mat");
  Check(atlas.shader.name=="PhoViet/Street Wall Finish","Atlas shader");Check(Mathf.Abs(atlas.GetFloat("_Glossiness")-.12f)<.0001f,"Original prop smoothness");
  foreach(var m in new[]{atlas,roomMat}){Check(m.GetFloat("_WallSmoothness")==.035f,"Wall matte");Check(m.GetFloat("_StuccoContrast")==.10f&&m.GetFloat("_StuccoStrength")==.20f,"Fine relief");Check(m.GetTexture("_StuccoDetail")!=null,"Detail bound");}
  Check(Mathf.Abs(roomMat.GetFloat("_Glow")-1.15f)<.0001f,"Night glow");Check(Vector4.Distance(roomMat.GetColor("_Warm"),new Vector4(1,.86f,.66f,1))<.0001f,"Night tint");
  var glassMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetGlass.mat");Check(glassMat.shader.name=="Standard"&&Mathf.Abs(glassMat.GetFloat("_Glossiness")-.92f)<.0001f&&Mathf.Abs(glassMat.GetFloat("_Metallic")-.05f)<.0001f,"Preserve glass");
  const string texturePath="Assets/PhoViet/Art/Streets/PV_WallStuccoDetail.png";var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);var ti=(TextureImporter)AssetImporter.GetAtPath(texturePath);
  Check(tex.width==1024&&tex.height==1024&&tex.mipmapCount==11,"Detail resolution/mips");Check(!ti.sRGBTexture&&!ti.alphaIsTransparency&&ti.mipmapEnabled,"Linear packed detail");Check(ti.wrapMode==TextureWrapMode.Repeat&&ti.filterMode==FilterMode.Trilinear,"No seam/distance alias");
  Check(tex.format==TextureFormat.DXT5&&!tex.isReadable,"Compressed non-readable detail");Check(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex)<3*1024*1024,"Total Editor texture allocation <3MiB (includes Editor overhead)");
  Check(g.scenery.GetComponentsInChildren<Renderer>(true).Length==10983,"No new renderers");Check(g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).Sum(s=>s.obstacles.Length)==7670,"Collision unchanged");var groups=g.scenery.GetComponentsInChildren<LODGroup>(true);Check(groups.Length==2779&&!groups.Any(l=>l.GetLODs().Any(d=>d.renderers.Any(r=>r==null))),"LOD refs");Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs lock");
  var result=new{success=true,checks,buildingModels=paths.Length,meshes,otherMeshes,wallVertices,glassFaces,textureMemory=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex),rows};File.WriteAllText("Captures/rental-walls-unity-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.checks,meshes,otherMeshes,wallVertices,glassFaces,result.textureMemory};
 }
}
