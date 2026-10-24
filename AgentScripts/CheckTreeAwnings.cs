using System.Linq;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Đếm đỉnh lưới cây (LOD0) nằm trong khối mái hiên của hai tiệm bên cạnh (huongdan.md 21.43).
public static class CheckTreeAwnings
{
 public static object Main()
 {
  var g=Object.FindAnyObjectByType<PhoVietGame>();var tile=g.roadRows[0].tiles[1];var report=new List<string>();int total=0;
  foreach(Transform tree in tile)
  {
   if(!tree.name.StartsWith("PV_StreetTree"))continue;
   var mf=tree.GetComponentsInChildren<MeshFilter>(true).First(f=>f.name.Contains("LOD0"));var verts=mf.sharedMesh.vertices;int inside=0;float minY=99;
   foreach(Transform shop in tile)
   {
    if(!shop.name.StartsWith("Blender • "))continue;
    foreach(var v in verts)
    {
     var w=mf.transform.TransformPoint(v);var l=shop.InverseTransformPoint(w);
     // Mái hiên (pho_viet_saigon_streets.awning, hệ tác giả: x dọc mặt tiền, z âm ra đường; hệ Unity shop đổi dấu x/z).
     float ax=Mathf.Abs(l.x),ay=l.y,az=-l.z;
     if(ax<=3.72f&&ay>=2.35f&&ay<=3.3f&&az>=3.95f&&az<=5.45f){inside++;minY=Mathf.Min(minY,ay);}
    }
   }
   total+=inside;report.Add(tree.name+" z="+tree.localPosition.z.ToString("F1")+" x="+tree.localPosition.x.ToString("F1")+" vertsInAwnings="+inside);
  }
  return new{success=true,total,report=report.ToArray()};
 }
}
