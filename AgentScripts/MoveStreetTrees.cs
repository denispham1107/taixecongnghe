using System.Linq;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Dời cây đường vào khe giữa hai mái hiên (ranh giới hai tiệm, z = 8,2 / 33,4 / 50,2 m) để thân cây không xuyên
// qua mái che (25/10/2026, huongdan.md 21.43); báo các đồ vật vỉa hè bị cây đè để dời theo.
public static class MoveStreetTrees
{
 public static readonly float[] From={10,30,50},To={8.2f,33.4f,50.2f};
 public static object Main()
 {
  if(Application.isPlaying)throw new System.Exception("Exit Play Mode first");
  var g=Object.FindAnyObjectByType<PhoVietGame>();int moved=0;var conflicts=new List<string>();
  foreach(var row in g.roadRows)foreach(var tile in row.tiles)
  {
   foreach(Transform c in tile)
   {
    if(!c.name.StartsWith("PV_StreetTree"))continue;
    var p=c.localPosition;
    for(int i=0;i<From.Length;i++)if(Mathf.Abs(p.z-From[i])<.05f||Mathf.Abs(p.z-To[i])<.05f){if(Mathf.Abs(p.z-To[i])>.001f){p.z=To[i];c.localPosition=p;moved++;}break;}
   }
   // Kiểm tra va chạm thân cây (bán kính 0,35 m, cao tới 2 m) với vật cản mặt đất của tiệm.
   foreach(Transform c in tile)
   {
    if(!c.name.StartsWith("PV_StreetTree"))continue;
    Vector3 trunk=c.localPosition;
    foreach(Transform s in tile)
    {
     var set=s.GetComponent<StreetObstacleSet>();if(set==null||s==c||s.name.StartsWith("PV_StreetTree"))continue;
     foreach(var o in set.obstacles)
     {
      if(o.bounds.min.y>2f)continue;
      var corners=new[]{new Vector3(o.bounds.min.x,0,o.bounds.min.z),new Vector3(o.bounds.max.x,0,o.bounds.max.z)}.Select(v=>tile.InverseTransformPoint(s.TransformPoint(v))).ToArray();
      float x0=Mathf.Min(corners[0].x,corners[1].x),x1=Mathf.Max(corners[0].x,corners[1].x),z0=Mathf.Min(corners[0].z,corners[1].z),z1=Mathf.Max(corners[0].z,corners[1].z);
      float dx=Mathf.Max(x0-trunk.x,0,trunk.x-x1),dz=Mathf.Max(z0-trunk.z,0,trunk.z-z1);
      if(Mathf.Sqrt(dx*dx+dz*dz)<.35f)conflicts.Add(tile.name+" | "+s.name+" | "+o.kind+" box x["+x0.ToString("F2")+","+x1.ToString("F2")+"] z["+z0.ToString("F2")+","+z1.ToString("F2")+"] tree z="+trunk.z.ToString("F1")+" x="+trunk.x.ToString("F1"));
     }
    }
   }
  }
  UnityEditor.EditorUtility.SetDirty(g);
  return new{success=true,moved,conflicts=conflicts.Distinct().ToArray(),conflictKinds=conflicts.Select(c=>c.Split('|')[2].Trim()).Distinct().ToArray()};
 }
}
