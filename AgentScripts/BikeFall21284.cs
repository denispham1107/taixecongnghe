using System.Collections;
using System.Reflection;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.284: kiểm thử xe máy nghiêng ngã khi bị húc (Play mode, CSGT nên tắt trước).
// "ram <kmh>": đặt xe máy đứng yên 14 m trước xe người chơi, lao vào lệch phải 0,6 m ở <kmh>.
// "look": đặt camera TestCamBike nhìn xe máy vừa đặt, in trạng thái (nghiêng, nâng gốc, đã chạm đất…).
public static class BikeFall21284
{
 public static object Main(string mode, int kmh)
 {
  var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
  var drv = Object.FindAnyObjectByType<OpenWorldDriver>();
  var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
  if (mode == "ram")
  {
   var f = drv.transform.forward; f.y = 0; f.Normalize();
   int i = tr.TestPlace(drv.transform.position + f * 14f, f, true, 25f);
   if (i < 0) return "no place";
   PlayerPrefs.SetInt("BikeFall21284", i);
   var root = (Transform)pool[i].GetType().GetField("root").GetValue(pool[i]);
   var rb = drv.GetComponent<Rigidbody>(); float sp = kmh / 3.6f;
   var s = root.position - f * 8f + drv.transform.right * 0.6f; s.y = 0.02f;
   rb.position = s; drv.transform.position = s; rb.linearVelocity = f * sp;
   typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drv, sp);
   drv.scriptedInput = new Vector2(0, 0.5f);
   return "bike " + i + " t=" + Time.time.ToString("F1");
  }
  int k = PlayerPrefs.GetInt("BikeFall21284", -1); if (k < 0) return "none";
  var v = pool[k]; var t = v.GetType();
  var r = (Transform)t.GetField("root").GetValue(v); var vis = (Transform)t.GetField("visual").GetValue(v);
  var go = GameObject.Find("TestCamBike"); Camera c;
  if (!go) { c = new GameObject("TestCamBike").AddComponent<Camera>(); c.CopyFrom(Camera.main); c.enabled = false; } else c = go.GetComponent<Camera>();
  c.transform.position = r.position + new Vector3(4.5f, 2.2f, -2.5f); c.transform.LookAt(r.position + Vector3.up * 0.4f);
  drv.scriptedInput = Vector2.zero;
  return "t=" + Time.time.ToString("F1") + " wreck=" + t.GetField("wreck").GetValue(v) + " landed=" + t.GetField("landed").GetValue(v) + " rec=" + t.GetField("recovering").GetValue(v)
   + " pos=" + r.position.ToString("F2") + " roll=" + ((float)t.GetField("fallRoll").GetValue(v)).ToString("F0") + " pitch=" + ((float)t.GetField("fallPitch").GetValue(v)).ToString("F0")
   + " lift=" + ((float)t.GetField("fallLift").GetValue(v)).ToString("F2") + " vis=" + vis.localEulerAngles.ToString("F0") + " hit=" + drv.LastHit;
 }
}
