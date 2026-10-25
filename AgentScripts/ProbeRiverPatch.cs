using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Gỡ lỗi 21.47: đếm tam giác của từng mesh gộp đang hiển thị trong khối cầu nằm ở vùng làn ngoài trên mặt sông.
public static class ProbeRiverPatch
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  g.riversEnabled=true;g.RestartRide();
  foreach(var b in g.blocks)b.GetComponent<RiverBlock>().SetBridge(Mathf.Abs(b.localPosition.z-80)<1);
  var tile=g.blocks.First(b=>Mathf.Abs(b.localPosition.z-80)<1);var rb=tile.GetComponent<RiverBlock>();
  var res=new List<string>();
  foreach(var mf in tile.GetComponentsInChildren<MeshFilter>(false))
  {
   var r=mf.GetComponent<Renderer>();if(r==null||!r.enabled||mf.sharedMesh==null)continue;if(mf.transform.IsChildOf(rb.bridge.transform))continue;
   var m=mf.sharedMesh;if(!m.isReadable){res.Add(mf.name+" mesh="+m.name+" (not readable)");continue;}
   var v=m.vertices;var tris=m.triangles;int n=0;float zmin=999,zmax=-999;
   for(int i=0;i<tris.Length;i+=3)
   {
    Vector3 c=(v[tris[i]]+v[tris[i+1]]+v[tris[i+2]])/3;c=tile.InverseTransformPoint(mf.transform.TransformPoint(c));
    if(c.z>RiverBlock.Near+.05f&&c.z<RiverBlock.Far-.05f&&Mathf.Abs(c.x)>3.4f&&Mathf.Abs(c.x)<30f&&c.y>-1&&c.y<1){n++;zmin=Mathf.Min(zmin,c.z);zmax=Mathf.Max(zmax,c.z);}
   }
   if(n>0)res.Add(mf.name+" mesh="+m.name+" inClip="+Array.IndexOf(rb.clipFilters,mf)+" tris="+n+" z["+zmin.ToString("F1")+","+zmax.ToString("F1")+"] local="+mf.transform.localPosition+" parent="+mf.transform.parent.name);
  }
  g.ResetRide();
  return new{success=true,tile=tile.name,res=res.ToArray()};
 }
}
