using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 21.283: thuộc tính đỉnh của tấm ảnh xa trong 4 FBX nội thất (có UV hay bị nén mất UV).
public static class CardAttrs21283
{
 public static object Main()
 {
  var res=new List<string>();
  foreach(var f in new[]{"5_0","6_0","8_0","NoiBat"}){
   var path="Assets/PhoViet/OpenWorld/Library/NoiThat/OW_NoiThat_"+f+".fbx";
   int ok=0,bad=0; var badN=new List<string>();
   foreach(var m in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>()){
    if(!m.name.Contains("_Xa")) continue;
    bool uv=m.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0);
    if(uv) ok++; else {bad++; if(badN.Count<3) badN.Add(m.name);}
   }
   res.Add(f+": cards with UV "+ok+", without UV "+bad+" "+string.Join(",",badN));
  }
  return res.ToArray();
 }
}
