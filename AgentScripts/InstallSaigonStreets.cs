using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PhoViet;
// Cài bộ phố Sài Gòn (pho_viet_saigon_streets.py) vào scene đang mở mà không dựng lại các dãy đường:
// reimport 16 FBX cùng GUID, đổi tên đối tượng shop theo VietnamStreet.Shops mới, tính lại bounds LOD,
// rồi chạy lại PhoVietObstacleUpgrade (đọc hộp vật cản từ JSON Blender) và lưu scene.
public static class InstallSaigonStreets
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Exit Play Mode");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null||g.roadRows.Count!=5)throw new Exception("PhoViet scene required");
  const string folder="Assets/PhoViet/Art/Streets/";
  AssetDatabase.ImportAsset(folder+"PV_StreetPalette.png",ImportAssetOptions.ForceUpdate);
  foreach(var name in Enumerable.Range(0,12).Select(i=>"PV_StreetShop"+i.ToString("00")).Concat(new[]{"PV_StreetTree0","PV_StreetTree1","PV_StreetTree2","PV_StreetPavement"}))
  {AssetDatabase.ImportAsset(folder+name+".fbx",ImportAssetOptions.ForceUpdate);AssetDatabase.ImportAsset(folder+name+".json",ImportAssetOptions.ForceUpdate);}
  AssetDatabase.ImportAsset(folder+"catalogue.json",ImportAssetOptions.ForceUpdate);
  // Tên shop: model con PV_StreetShopNN quyết định chỉ số, không suy từ tên cũ.
  int renamed=0,lods=0;
  foreach(var t in g.scenery.GetComponentsInChildren<Transform>(true))
  {
   if(!t.name.StartsWith("Blender • ",StringComparison.Ordinal))continue;
   var model=t.Cast<Transform>().FirstOrDefault(c=>c.name.StartsWith("PV_StreetShop",StringComparison.Ordinal));if(model==null)continue;
   int index=int.Parse(model.name.Substring("PV_StreetShop".Length,2));string wanted="Blender • "+VietnamStreet.Shops[index];
   if(t.name!=wanted){t.name=wanted;renamed++;}
  }
  int glassRenderers=0;
  foreach(var l in g.scenery.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.StartsWith("PV_Street",StringComparison.Ordinal)))
  {
   foreach(var r in l.GetComponentsInChildren<MeshRenderer>(true)){var mats=PhoViet.Editor.PhoVietStreetUpgrade.Materials(r);r.sharedMaterials=mats;if(mats.Length>1)glassRenderers++;}
   l.RecalculateBounds();EditorUtility.SetDirty(l);lods++;
  }
  AssetDatabase.SaveAssets();
  var obstacles=PhoViet.Editor.PhoVietObstacleUpgrade.Install();
  EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  return new{success=true,renamed,lods,glassRenderers,obstacles};
 }
}
