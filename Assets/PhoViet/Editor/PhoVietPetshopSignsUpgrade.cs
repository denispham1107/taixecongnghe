using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Random=System.Random;
namespace PhoViet.Editor
{
 public static class PhoVietPetshopSignsUpgrade
 {
  public const string Brand="Cù Lao Petshop";
  const string Shops="Assets/PhoViet/Art/Streets/",River="Assets/PhoViet/Art/River/";
  [Serializable] sealed class SignInfo {public int river_units,river_base_brands,river_extra_brands,river_high_blocks;public string river_extra_part;}
  public static bool BrandedSeed(int seed)=>((seed%10)+10)%10<3;
  static Dictionary<string,Mesh> Meshes(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().ToDictionary(m=>m.name.Replace("_PetshopExtra",""));
  static void Assign(MeshFilter filter,Mesh mesh)
  {
   if(mesh==null)throw new Exception("Missing replacement mesh");if(filter.sharedMesh==mesh)return;
   filter.sharedMesh=mesh;EditorUtility.SetDirty(filter);PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
  }
  static void Shuffle<T>(IList<T> list,Random random){for(int i=list.Count-1;i>0;i--){int j=random.Next(i+1);var t=list[i];list[i]=list[j];list[j]=t;}}
  [MenuItem("Phố Việt/Art/Apply Cù Lao Petshop signs")]
  public static object Install()
  {
   if(Application.isPlaying)throw new Exception("Edit Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
   if(g==null||g.jobsEnabled||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Correct scene/jobs locked required");
   if(g.gameObject.scene.isDirty)throw new Exception("Save or inspect existing scene edits first");
   if(!File.Exists(".tools/PhoViet-before-petshop-pets.unity"))EditorSceneManager.SaveScene(g.gameObject.scene,".tools/PhoViet-before-petshop-pets.unity",true);
   var spec=JsonUtility.FromJson<SignInfo>(File.ReadAllText(River+"PV_PetshopSigns.json"));
   int rendererCount=g.scenery.GetComponentsInChildren<Renderer>(true).Length;
   var originalObstacles=g.scenery.GetComponentsInChildren<StreetObstacleSet>(true).ToDictionary(s=>s,s=>JsonUtility.ToJson(s));var brandedSets=new HashSet<StreetObstacleSet>();
   var tiles=g.roadRows.SelectMany(r=>r.tiles).ToList();if(tiles.Count!=35)throw new Exception("Expected 35 pooled street blocks");
   var highStreet=Enumerable.Range(0,35).ToList();Shuffle(highStreet,new Random(72026));var streetExtra=new HashSet<int>(highStreet.Take(7));
   var highRiver=Enumerable.Range(0,35).ToList();Shuffle(highRiver,new Random(72027));var riverExtra=new HashSet<int>(highRiver.Take(spec.river_high_blocks));
   var sources=new Dictionary<string,Dictionary<string,Mesh>>();
   // Validate every asset before modifying any scene reference.
   for(int i=0;i<12;i++)foreach(bool branded in new[]{false,true})
   {
    string asset="PV_StreetShop"+i.ToString("00")+(branded?"_Petshop":""),path=Shops+asset+".fbx";var meshes=Meshes(path);
    for(int d=0;d<3;d++)if(!meshes.ContainsKey(asset+"_LOD"+d))throw new Exception("Missing "+asset+" LOD"+d);sources.Add(path,meshes);
   }
   foreach(string file in new[]{"PV_RiverApartments.fbx","PV_RiverPetshopExtra.fbx"})
   {
    var meshes=Meshes(River+file);for(int d=0;d<3;d++)if(!meshes.ContainsKey(spec.river_extra_part+"_LOD"+d))throw new Exception("Missing river sign variant LOD"+d);sources.Add(River+file,meshes);
   }
   int shops=0,brands=0,rivers=0,riverBrands=0;var samples=new List<object>();
   for(int tileIndex=0;tileIndex<tiles.Count;tileIndex++)
   {
    var tile=tiles[tileIndex];var groups=tile.GetComponentsInChildren<LODGroup>(true).Where(l=>l.GetLODs().Length==3&&l.GetLODs()[0].renderers.Any(r=>r!=null&&r.GetComponent<MeshFilter>()!=null&&r.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("PV_StreetShop",StringComparison.Ordinal))).ToArray();
    if(groups.Length!=14)throw new Exception("Expected 14 shop facades per block, found "+groups.Length);
    var selected=new HashSet<LODGroup>();var rng=new Random(72026+tileIndex*37);
    foreach(int side in new[]{-1,1})
    {
     var half=groups.Where(l=>Mathf.Sign(tile.InverseTransformPoint(l.transform.position).x)==side).ToList();if(half.Count!=7)throw new Exception("Expected seven shops per side");
     Shuffle(half,rng);int count=2+(streetExtra.Contains(tileIndex)&&side==(tileIndex%2==0?-1:1)?1:0);foreach(var l in half.Take(count))selected.Add(l);
    }
    foreach(var lod in groups)
    {
     var levels=lod.GetLODs();var name=levels[0].renderers[0].GetComponent<MeshFilter>().sharedMesh.name;int index=int.Parse(name.Substring("PV_StreetShop".Length,2));bool branded=selected.Contains(lod);
     string asset="PV_StreetShop"+index.ToString("00")+(branded?"_Petshop":""),path=Shops+asset+".fbx";
     if(!sources.TryGetValue(path,out var source)){source=Meshes(path);sources.Add(path,source);}
     for(int d=0;d<3;d++)foreach(var r in levels[d].renderers)Assign(r.GetComponent<MeshFilter>(),source[asset+"_LOD"+d]);
     if(branded)
     {
      var set=lod.GetComponentInParent<StreetObstacleSet>(true);if(set==null||set.transform==tile)throw new Exception("Missing shop obstacle set");
      var metadata=StreetArtMetadata.Load(asset);set.obstacles=metadata.obstacles.Select(o=>new StreetObstacleSet.Obstacle(o.kind,new Vector3(o.center[0],o.center[1],o.center[2]),new Vector3(o.size[0],o.size[1],o.size[2]))).ToArray();
      EditorUtility.SetDirty(set);PrefabUtility.RecordPrefabInstancePropertyModifications(set);brandedSets.Add(set);
     }
     shops++;if(branded){brands++;if(samples.Count<2)samples.Add(new{tile=tileIndex,shop=index,position=lod.transform.position.ToString("F3")});}
    }
    var river=tile.GetComponent<RiverBlock>();var apt=river.bridge.transform.Find("Căn hộ ven sông • Blender");if(apt==null)throw new Exception("Missing riverside apartments");
    var part=apt.Find(spec.river_extra_part);if(part==null)throw new Exception("Missing alternate apartment part");
    string riverPath=River+(riverExtra.Contains(tileIndex)?"PV_RiverPetshopExtra.fbx":"PV_RiverApartments.fbx");
    if(!sources.TryGetValue(riverPath,out var riverMeshes)){riverMeshes=Meshes(riverPath);sources.Add(riverPath,riverMeshes);}
    foreach(var mf in part.GetComponentsInChildren<MeshFilter>(true))
    {
     string mesh=mf.sharedMesh.name.Replace("_PetshopExtra","");if(!mesh.StartsWith(spec.river_extra_part+"_LOD",StringComparison.Ordinal))throw new Exception("Unexpected apartment mesh "+mesh);
     Assign(mf,riverMeshes[mesh]); // Includes the LOD2 shadow-only proxy; no additional draw objects.
    }
    rivers+=spec.river_units;riverBrands+=spec.river_base_brands+(riverExtra.Contains(tileIndex)?1:0);
   }
   if(shops!=490||brands!=147||riverBrands!=(int)Math.Round(rivers*.3))throw new Exception("Brand proportions did not balance");
   if(rendererCount!=g.scenery.GetComponentsInChildren<Renderer>(true).Length||originalObstacles.Any(p=>!brandedSets.Contains(p.Key)&&p.Value!=JsonUtility.ToJson(p.Key)))throw new Exception("Unexpected renderer/non-Petshop collision changes");
   EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();
   var report=new{success=true,shops,brands,rivers,riverBrands,riverSigns=rivers*2,riverBrandedSigns=riverBrands*2,riverRatio=riverBrands/(float)rivers,rendererCount,updatedPetshopObstacles=brandedSets.Count,extraRiverBlocks=spec.river_high_blocks,allThreeLods=true,samples,jobs=g.jobsEnabled};
   File.WriteAllText("Captures/petshop-sign-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
 }
}
