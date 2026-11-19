using System.Linq;
using UnityEngine;
using UnityEditor;
// 21.280: vật liệu gán lúc chạy (tàu, xe NPC, CSGT...) có cần tangent (normal map) không — để bật lọc kênh mesh an toàn.
public static class RuntimeMaterials21280
{
 static string D(Material m)=>m?m.name+"("+m.shader.name+(m.IsKeywordEnabled("_NORMALMAP")||m.GetTexturePropertyNames().Any(p=>p.Contains("Bump")&&m.GetTexture(p))?" NM":"")+")":"null";
 public static object Main()
 {
  var sb=new System.Text.StringBuilder();
  foreach(var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)){
   if(mb==null||mb.GetType().Namespace!="PhoViet.OpenWorld")continue;
   var so=new SerializedObject(mb);var it=so.GetIterator();
   while(it.NextVisible(true)){
    if(it.propertyType!=SerializedPropertyType.ObjectReference||!it.objectReferenceValue)continue;
    var o=it.objectReferenceValue;
    if(o is Material m)sb.Append(mb.GetType().Name+"."+it.propertyPath+" = "+D(m)+"\n");
    else if(o is GameObject g&&!g.scene.IsValid())sb.Append(mb.GetType().Name+"."+it.propertyPath+" prefab "+g.name+" mats: "+string.Join(",",g.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Distinct().Select(D))+"\n");
   }
  }
  return sb.ToString();
 }
}
