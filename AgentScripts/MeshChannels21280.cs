using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
// 21.280: bộ nhớ đỉnh/chỉ số của mesh trong scene bản đồ mở theo thuộc tính (tìm phần thừa: tangent, uv phụ, màu...).
public static class MeshChannels21280
{
 public static object Main()
 {
  var deps=AssetDatabase.GetDependencies("Assets/PhoViet/Scenes/PhoVietOpenWorld.unity",true);
  var meshes=deps.SelectMany(p=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>().Select(m=>(p,m))).ToList();
  var attr=new Dictionary<string,long>();long vtx=0,idx=0;var byDir=new Dictionary<string,long>();
  foreach(var (p,m) in meshes){
   long vb=0;
   foreach(var a in m.GetVertexAttributes()){
    int sz=a.format switch{VertexAttributeFormat.Float32=>4,VertexAttributeFormat.Float16=>2,VertexAttributeFormat.UNorm8=>1,VertexAttributeFormat.SNorm8=>1,VertexAttributeFormat.UInt8=>1,VertexAttributeFormat.SInt8=>1,VertexAttributeFormat.UNorm16=>2,VertexAttributeFormat.SNorm16=>2,_=>4}*a.dimension;
    long b=(long)sz*m.vertexCount; string k=a.attribute+":"+a.format+"x"+a.dimension; attr[k]=(attr.TryGetValue(k,out var o)?o:0)+b; vb+=b;
   }
   long ib=0;for(int s=0;s<m.subMeshCount;s++)ib+=(long)m.GetIndexCount(s)*(m.indexFormat==IndexFormat.UInt32?4:2);
   vtx+=vb;idx+=ib; var d=string.Join("/",p.Split('/').Take(5)); byDir[d]=(byDir.TryGetValue(d,out var q)?q:0)+vb+ib;
  }
  Func<long,string> M=b=>Math.Round(b/1048576.0,1)+"MB";
  return new{meshes=meshes.Count,vertex=M(vtx),index=M(idx),attrs=attr.OrderByDescending(x=>x.Value).Select(x=>x.Key+" "+M(x.Value)).ToArray(),dirs=byDir.OrderByDescending(x=>x.Value).Take(12).Select(x=>x.Key+" "+M(x.Value)).ToArray(),
   strip=PlayerSettings.stripUnusedMeshComponents};
 }
}
