using UnityEngine;
// 21.282: (Play) args "0"/"1" bật/tắt occlusion trên camera chính; "read" đọc số liệu khung vừa vẽ.
public static class OcclusionStats21282
{
 public static object Main(string on)
 {
  var cam=Camera.main;
  if(on!="read"){cam.useOcclusionCulling=on=="1";return "set "+cam.useOcclusionCulling;}
  return "occlusion="+cam.useOcclusionCulling+" drawCalls="+UnityEditor.UnityStats.drawCalls+" setPass="+UnityEditor.UnityStats.setPassCalls+" tris="+UnityEditor.UnityStats.triangles+" shadowCasters="+UnityEditor.UnityStats.shadowCasters+" instBatches="+UnityEditor.UnityStats.instancedBatches+" std="+UnityEditor.UnityStats.standardDrawCalls+" stdInst="+UnityEditor.UnityStats.standardInstancedDrawCalls;
 }
}
