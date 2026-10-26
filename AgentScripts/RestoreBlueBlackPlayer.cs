using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using PhoViet;
public static class RestoreBlueBlackPlayer
{
 const string Art="Assets/PhoViet/Art/Bikes/MeshyRiders/",Temp="Assets/PhoViet/Editor/PetshopRestoreTemporary.unity";
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode first");
  var main=SceneManager.GetActiveScene();var g=main.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<PhoVietGame>(true)).Single();
  if(main.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Wrong active scene");
  if(g.rider.GetComponent<PlayerCarVisual>()==null)throw new Exception("Expected current Petshop player before restoration");
  if(File.Exists(Temp))throw new Exception("Temporary scene already exists; preserve it and investigate");
  if(!File.Exists(".tools/PhoViet-before-restore-blue-bike-memory.unity"))EditorSceneManager.SaveScene(main,".tools/PhoViet-before-restore-blue-bike-memory.unity",true);
  var fingerprints=g.vehicles.Select(v=>EditorJsonUtility.ToJson(v.model.GetComponent<BikeVisualAnimator>()??(UnityEngine.Object)v.model.GetComponent<CarVisualAnimator>())).ToArray();string police=EditorJsonUtility.ToJson(g.policeBike.GetComponent<BikeVisualAnimator>());
  var texturePath=Art+"PV_HeroScooter_BlueBlack_Palette.png";AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
  var tex=(TextureImporter)AssetImporter.GetAtPath(texturePath);var source=(TextureImporter)AssetImporter.GetAtPath(Art+"PV_HeroScooter_Palette.png");var settings=new TextureImporterSettings();source.ReadTextureSettings(settings);tex.SetTextureSettings(settings);
  tex.maxTextureSize=source.maxTextureSize;tex.textureCompression=source.textureCompression;tex.compressionQuality=source.compressionQuality;
  foreach(string platform in new[]{"WebGL","Standalone","iPhone","Android"})tex.SetPlatformTextureSettings(source.GetPlatformTextureSettings(platform));tex.SaveAndReimport();
  string matPath=Art+"PV_HeroScooter_BlueBlack.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
  if(mat==null){mat=new Material(AssetDatabase.LoadAssetAtPath<Material>(Art+"PV_HeroScooter.mat"));AssetDatabase.CreateAsset(mat,matPath);}
  mat.name="PV_HeroScooter_BlueBlack_Atlas";mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture>(texturePath);EditorUtility.SetDirty(mat);
  Scene backup=default;
  try
  {
   File.Copy(".tools/PhoViet-before-petshop-player-memory.unity",Temp);AssetDatabase.ImportAsset(Temp,ImportAssetOptions.ForceSynchronousImport);backup=EditorSceneManager.OpenScene(Temp,OpenSceneMode.Additive);
   var old=backup.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<PhoVietGame>(true)).Single();
   if(old.rider.GetComponent<BikeVisualAnimator>()==null)throw new Exception("Backup does not contain original motorcycle player");
   var previous=g.rider;var restored=UnityEngine.Object.Instantiate(old.rider.gameObject,g.transform).transform;restored.name=old.rider.name;restored.localPosition=new Vector3(PhoVietGame.LaneX[2],0,0);restored.localRotation=Quaternion.identity;restored.gameObject.SetActive(true);
   Func<Transform,Transform> map=t=>t==null?null:t==old.rider?restored:restored.Find(AnimationUtility.CalculateTransformPath(t,old.rider));
   g.rider=restored;g.leftKickLeg=map(old.leftKickLeg);g.rightKickLeg=map(old.rightKickLeg);
   var lamp=g.daylight.vehicles.Single(v=>v.root==previous);var oldLamp=old.daylight.vehicles.Single(v=>v.root==old.rider);var used=new HashSet<Renderer>();
   lamp.root=restored;lamp.head=map(oldLamp.head);
   var halos=new List<DayNightLighting.Halo>();
   foreach(var h in oldLamp.halos){var glow=lamp.halos.First(x=>x.rear==h.rear&&!used.Contains(x.glow)).glow;used.Add(glow);halos.Add(new DayNightLighting.Halo{source=map(h.source),glow=glow,rear=h.rear,size=h.size});}
   foreach(var h in lamp.halos)if(h.glow!=null&&!used.Contains(h.glow))UnityEngine.Object.DestroyImmediate(h.glow.gameObject);
   lamp.halos=halos.ToArray();if(lamp.head==null||lamp.halos.Any(h=>h.source==null))throw new Exception("Lamp source mapping failed");
   foreach(var skin in restored.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.sharedMaterial=mat;
   UnityEngine.Object.DestroyImmediate(previous.gameObject);
   g.daylight.emissionMaterials=g.daylight.emissionMaterials.Where(m=>m!=null&&!AssetDatabase.GetAssetPath(m).StartsWith("Assets/PhoViet/Art/PlayerCar/",StringComparison.Ordinal)).Concat(new[]{mat}).Distinct().ToArray();
   g.daylight.emissionUsers=g.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&(g.daylight.emissionMaterials.Contains(m)||m==g.daylight.streetLens))).ToArray();
   for(int i=0;i<fingerprints.Length;i++)if(fingerprints[i]!=EditorJsonUtility.ToJson(g.vehicles[i].model.GetComponent<BikeVisualAnimator>()??(UnityEngine.Object)g.vehicles[i].model.GetComponent<CarVisualAnimator>()))throw new Exception("NPC changed: "+i);
   if(police!=EditorJsonUtility.ToJson(g.policeBike.GetComponent<BikeVisualAnimator>()))throw new Exception("Police changed");
   if(g.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must remain locked");
   SceneManager.SetActiveScene(main);EditorUtility.SetDirty(g);EditorUtility.SetDirty(g.daylight);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(main);EditorSceneManager.SaveScene(main);
   var result=new{success=true,restored=true,blueBlack=true,npcPreserved=fingerprints.Length,policePreserved=true,jobsLocked=true,halos=lamp.halos.Length};File.WriteAllText("Captures/hero-blue-black-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{if(backup.IsValid())EditorSceneManager.CloseScene(backup,true);SceneManager.SetActiveScene(main);if(File.Exists(Temp))AssetDatabase.DeleteAsset(Temp);}
 }
}
