using System.Collections;
using System.Reflection;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.284: kiểm thử «xe kẹt lơ lửng sau khi nhảy va xe khác» (Play mode, CSGT nên tắt trước).
// "drop <heavy>": thả xe người chơi cách nóc một xe NPC đang đứng yên 2 m (heavy=1: chọn xe tải ≥ 2 t), giữ ga.
// "state": in độ cao xe, vận tốc, số lần gỡ kẹt, xe NPC bên dưới.
public static class StuckOnCar21284
{
 static int lastIdx = -1;
 public static object Main(string mode, int heavy)
 {
  var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
  var drv = Object.FindAnyObjectByType<OpenWorldDriver>();
  var rb = drv.GetComponent<Rigidbody>();
  var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
  if (mode == "place")
  {   // đặt một xe (heavy=0 ô tô, 1 xe máy) đứng yên 14 m trước mặt người chơi
   var f = drv.transform.forward; f.y = 0; f.Normalize();
   int i = tr.TestPlace(drv.transform.position + f * 14f, f, heavy == 1, 25f);
   return "placed " + i;
  }
  if (mode == "drop")
  {
   object best = null; float bd = 1e9f;
   foreach (var v in pool)
   {
    var t = v.GetType();
    if (!(bool)t.GetField("active").GetValue(v) || (bool)t.GetField("bike").GetValue(v) || (bool)t.GetField("wreck").GetValue(v)) continue;
    if ((float)t.GetField("speed").GetValue(v) > 0.1f) continue;
    float m = (float)t.GetField("mass").GetValue(v); if (heavy == 1 ? m < 2f : m > 2f) continue;
    var r = (Transform)t.GetField("root").GetValue(v); float d = Vector3.Distance(r.position, drv.transform.position);
    if (d < bd) { bd = d; best = v; }
   }
   if (best == null) return "no npc";
   lastIdx = (int)best.GetType().GetField("index").GetValue(best);
   var root = (Transform)best.GetType().GetField("root").GetValue(best);
   if (heavy == 2)
   {   // "drop 2": lao tới xe đứng yên 9 m phía trước ở 9 m/s rồi nhảy → rơi trúng nóc/đuôi xe kia
    var f = root.forward; f.y = 0; f.Normalize();
    var s = root.position - f * 9f; s.y = 0.02f; var q = Quaternion.LookRotation(f);
    rb.position = s; rb.rotation = q; drv.transform.SetPositionAndRotation(s, q); rb.linearVelocity = f * 9f;
    typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drv, 9f);
    typeof(OpenWorldDriver).GetField("jumpAt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drv, -10f);
    typeof(OpenWorldDriver).GetField("supportAt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drv, Time.time);
    drv.scriptedInput = new Vector2(0, 0.35f); drv.RequestJump();
    return "jump at npc " + lastIdx + " t=" + Time.time.ToString("F1");
   }
   var top = root.GetComponent<BoxCollider>().bounds.max.y;
   var p = root.position; p.y = top + 2f; var rot = root.rotation;
   rb.position = p; rb.rotation = rot; drv.transform.SetPositionAndRotation(p, rot); rb.linearVelocity = Vector3.zero;
   drv.scriptedInput = new Vector2(0, 1f);
   return "drop on npc " + lastIdx + " top " + top.ToString("F2") + " t=" + Time.time.ToString("F1");
  }
  string npc = "";
  if (lastIdx >= 0) { var v = pool[lastIdx]; var t = v.GetType(); npc = " npc=" + ((Transform)t.GetField("root").GetValue(v)).position.ToString("F2") + " wreck=" + t.GetField("wreck").GetValue(v); }
  return "t=" + Time.time.ToString("F1") + " car=" + drv.transform.position.ToString("F2") + " vel=" + rb.linearVelocity.ToString("F2") + " speed=" + drv.Speed.ToString("F1") + " unsticks=" + drv.Unsticks + " frozen=" + drv.frozen + npc;
 }
}
