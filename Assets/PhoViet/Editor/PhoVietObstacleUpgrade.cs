using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PhoViet.Editor
{
 public static class PhoVietObstacleUpgrade
 {
  public static object Install()
  {
   if(Application.isPlaying)throw new Exception("Exit Play Mode");cache.Clear();var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();int sets=0,boxes=0;
   foreach(var row in g.roadRows)foreach(var tile in row.tiles)
   {
    var props=new List<StreetObstacleSet.Obstacle>();
    Action<string,Vector3,Vector3> add=(k,p,s)=>props.Add(new StreetObstacleSet.Obstacle(k,p,s));
    foreach(float side in new[]{-1f,1f})
    {
     add("Xe máy đậu",new Vector3(side*9.2f,1,21),new Vector3(1.35f,1.5f,1.6f));
     add("Cột đèn đường",new Vector3(side*7.6f,3.4f,28),new Vector3(.25f,6.8f,.25f));
     add("Cột điện",new Vector3(side*10.1f,4.4f,56),new Vector3(.3f,8.8f,.3f));
    }
    Assign(tile,props);sets++;boxes+=props.Count;
    foreach(var t in tile.GetComponentsInChildren<Transform>(true))
    {
     var list=new List<StreetObstacleSet.Obstacle>();
     Action<string,Vector3,Vector3> put=(k,p,s)=>list.Add(new StreetObstacleSet.Obstacle(k,p,s));
     if(t.name.StartsWith("Blender • ",StringComparison.Ordinal))
     {
      var model=t.Cast<Transform>().FirstOrDefault(c=>c.name.StartsWith("PV_StreetShop",StringComparison.Ordinal));if(model==null)continue;
      // Hộp vật cản do pho_viet_saigon_streets.py ghi cùng FBX, khớp hình học shop đang render.
      foreach(var o in Metadata(model.name).obstacles)put(o.kind,new Vector3(o.center[0],o.center[1],o.center[2]),new Vector3(o.size[0],o.size[1],o.size[2]));
     }
     else if(t.name.StartsWith("PV_StreetTree",StringComparison.Ordinal)&&t.parent==tile)
     {foreach(var o in Metadata(t.name).obstacles)put(o.kind,new Vector3(o.center[0],o.center[1],o.center[2]),new Vector3(o.size[0],o.size[1],o.size[2]));}
     else continue;
     Assign(t,list);sets++;boxes+=list.Count;
    }
   }
   EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);return new{success=true,sets,boxes};
  }
  static readonly Dictionary<string,StreetArtMetadata> cache=new Dictionary<string,StreetArtMetadata>();
  static StreetArtMetadata Metadata(string asset)
  {
   if(cache.TryGetValue(asset,out var m))return m;
   m=StreetArtMetadata.Load(asset);if(m.obstacles==null||m.obstacles.Length==0)throw new Exception(asset+": no obstacle boxes");cache[asset]=m;return m;
  }
  static void Assign(Transform t,List<StreetObstacleSet.Obstacle> list)
  {var set=t.GetComponent<StreetObstacleSet>()??t.gameObject.AddComponent<StreetObstacleSet>();set.obstacles=list.ToArray();EditorUtility.SetDirty(set);}
 }
 // JSON do pho_viet_saigon_streets.py ghi cạnh mỗi FBX phố. Đặt trong assembly Editor để
 // JsonUtility đọc được mảng lớp lồng (assembly tạm của run_script không đọc được).
 [Serializable] public sealed class StreetArtMetadata
 {
  [Serializable] public sealed class Box{public string kind;public float[] center;public float[] size;}
  public string name;public string generator;public int[] lod_triangles;public int[] leaf_counts;public int[] palette_grid;public float height;public Box[] obstacles;
  public static StreetArtMetadata Load(string asset)
  {
   var text=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/PhoViet/Art/Streets/"+asset+".json");if(text==null)throw new Exception("Missing street metadata "+asset);
   return JsonUtility.FromJson<StreetArtMetadata>(text.text);
  }
 }
}
