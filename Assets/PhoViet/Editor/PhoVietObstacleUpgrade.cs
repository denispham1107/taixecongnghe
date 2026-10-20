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
   if(Application.isPlaying)throw new Exception("Exit Play Mode");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();int sets=0,boxes=0;
   foreach(var row in g.roadRows)foreach(var tile in row.tiles)
   {
    var props=new List<StreetObstacleSet.Obstacle>();
    Action<string,Vector3,Vector3> add=(k,p,s)=>props.Add(new StreetObstacleSet.Obstacle(k,p,s));
    foreach(float side in new[]{-1f,1f})
    {
     add("Xe máy đậu",new Vector3(side*9.2f,1,21),new Vector3(1.35f,1.5f,1.6f));
     add("Xe đẩy bán hàng",new Vector3(side*8.5f,1.2f,43),new Vector3(1.6f,2.4f,1.9f));
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
      int seed=Array.IndexOf(VietnamStreet.Shops,t.name.Substring("Blender • ".Length));if(seed<0)continue;
      float height=8.4f+(seed%4)*2.65f;
      put("Nhà cửa",new Vector3(0,height/2,0),new Vector3(7.7f,height,7.8f));
      put("Bảng hiệu",new Vector3(0,3.86f,-4.01f),new Vector3(7.1f,1.28f,.24f));
      put("Mái hiên",new Vector3(0,2.93f,-4.7f),new Vector3(7.1f,.18f,1.3f));
      if(seed==3||seed==4||seed==11)
      {foreach(float x in new[]{-2.45f,1.8f})put("Kệ tạp hóa",new Vector3(x,1.1f,-4.3f),new Vector3(1.8f,1.8f,.65f));}
      else if(seed==5||seed==9)
      {put("Tủ dụng cụ sửa xe",new Vector3(-2.2f,.88f,-4.45f),new Vector3(1.35f,1.15f,.65f));put("Chồng lốp xe",new Vector3(1.6f,.75f,-4.45f),new Vector3(1,.95f,1));}
      else
      {
       put("Quầy thức ăn",new Vector3(1.8f,1,-4.36f),new Vector3(2.2f,1.4f,.8f));
       foreach(float x in new[]{-2.2f,-.5f})
       {put("Bàn ăn",new Vector3(x,.65f,-4.95f),new Vector3(1.15f,.75f,1.1f));foreach(float z in new[]{-5.65f,-4.2f})put("Ghế nhựa",new Vector3(x,.5f,z),new Vector3(.42f,.43f,.42f));}
      }
     }
     else if(t.name.StartsWith("PV_StreetTree",StringComparison.Ordinal)&&t.parent==tile)
     {put("Thân cây",new Vector3(.08f,2.7f,0),new Vector3(.38f,5.4f,.38f));put("Bồn gốc cây",new Vector3(0,.25f,0),new Vector3(1.45f,.5f,1.45f));}
     else continue;
     Assign(t,list);sets++;boxes+=list.Count;
    }
   }
   EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);return new{success=true,sets,boxes};
  }
  static void Assign(Transform t,List<StreetObstacleSet.Obstacle> list)
  {var set=t.GetComponent<StreetObstacleSet>()??t.gameObject.AddComponent<StreetObstacleSet>();set.obstacles=list.ToArray();EditorUtility.SetDirty(set);}
 }
}
