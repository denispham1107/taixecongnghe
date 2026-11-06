using System;using System.IO;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Actual pickup contacts and browser gesture routing, with physics frozen between inputs.
public static class VerifyMissileInput2142
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static List<string> checks;const int Max=PhoVietGame.MaximumPlayerMissiles;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Check(bool ok,string n){if(!ok)throw new Exception(n);checks.Add(n);}
 static void Fresh(){g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);}
 static void Hover(){Check(g.TryToggleFlightButton(),"Flight button starts ascent");for(int i=0;i<100&&!g.RiderFlying;i++)Call("TickJump",1/120f);Check(g.RiderFlying,"Fixture reaches flight height");var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;}
 static void Swipe(int id)
 {
  // Choose the clear central play area, outside HUD and action buttons.
  g.ApplyBrowserGesture("{\"phase\":0,\"id\":"+id+",\"x\":0.55,\"y\":0.55,\"at\":10}");
  g.ApplyBrowserGesture("{\"phase\":1,\"id\":"+id+",\"x\":0.55,\"y\":0.8,\"action\":3,\"at\":10.1}");
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();
  float scale=Time.timeScale;int best=g.BestDistance;bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   Check(PhoVietGame.PickupEnergy==15,"Each pickup restores15, distinct from jump cost5");
   foreach(bool air in new[]{false,true})foreach(int initial in new[]{50,140,150})
   {
    Fresh();if(air)Hover();
    var tiles=(IList)Get("energyTiles");Transform target=null;object entry=null;
    foreach(var e in tiles)
    {
     var type=e.GetType();foreach(string field in new[]{"icons","airIcons"})foreach(Transform t in (Transform[])type.GetField(field).GetValue(e))t.position=new Vector3(10000,10000,10000);
     if(target==null&&((Transform)type.GetField("tile").GetValue(e)).gameObject.activeInHierarchy){target=((Transform[])type.GetField(air?"airIcons":"icons").GetValue(e))[0];entry=e;}
    }
    Check(target!=null,"Active "+(air?"air":"ground")+" pickup fixture");target.gameObject.SetActive(true);
    var at=g.rider.position;if(air)at.y+=g.PlayerHeight*.5f;target.position=at;
    Set("Energy",initial);int before=g.EnergyPickupsCollected;Call("TickEnergyPickups",0f,g.rider.localPosition);
    Check(g.Energy==Math.Min(150,initial+15),(air?"Air":"Ground")+" pickup "+initial+" -> "+Math.Min(150,initial+15));
    Check(g.EnergyPickupsCollected==before+1&&!target.gameObject.activeSelf,"Pickup collected once and hidden");
    Call("TickEnergyPickups",0f,g.rider.localPosition);Check(g.EnergyPickupsCollected==before+1,"Repeated contact cannot collect twice");
   }
   Fresh();float speed=g.Speed;Check(!PhoVietGame.BoostReleased&&!g.TryBoost()&&!g.BoostActive,"Boost locked on ground");Swipe(1);
   Check(g.PlayerMissiles==Max-1&&g.PlayerMissilesFired==1&&g.Speed==speed&&!g.BoostActive,"Down swipe on ground fires once without boosting");
   Fresh();Hover();speed=g.Speed;Swipe(2);Check(g.PlayerMissiles==Max-1&&g.PlayerMissilesFired==1,"Down swipe in flight fires immediately");
   Check(g.Speed==speed&&!g.BoostActive&&!g.TryBoost(),"Firing does not accelerate or enable boost");
   g.ApplyBrowserGesture("{\"phase\":1,\"id\":2,\"x\":0.55,\"y\":0.9,\"action\":3,\"at\":10.2}");
   g.ApplyBrowserGesture("{\"phase\":2,\"id\":2,\"x\":0.55,\"y\":0.9,\"at\":10.3}");
   Check(g.PlayerMissilesFired==1,"Move/up of same swipe cannot double fire");Swipe(3);Check(g.PlayerMissilesFired==1,"New swipe respects shared0.4s cooldown");
   Call("TickPlayerMissiles",.4f);Swipe(4);Check(g.PlayerMissilesFired==2&&g.PlayerMissiles==Max-2,"Next swipe after cooldown fires");
   Call("TickPlayerMissiles",.4f);Check(g.TryFirePlayerMissile()&&g.PlayerMissiles==Max-3,"Existing F/button API shares the same ammo");while(g.PlayerMissiles>0){Call("TickPlayerMissiles",.4f);Check(g.TryFirePlayerMissile(),"Spend remaining carried missile");}Call("TickPlayerMissiles",.4f);Swipe(5);
   Check(g.PlayerMissiles==0&&g.PlayerMissilesFired==Max&&!g.BoostActive,"Empty down swipe cannot spend negative ammo or boost");
   Fresh();Hover();g.TogglePause();Swipe(6);Check(g.PlayerMissiles==Max&&g.PlayerMissilesFired==0,"Paused swipe cannot fire");g.TogglePause();
   Set("boostRemaining",3f);Set("boostCooldown",2f);Check(!g.BoostActive,"Stale boost cannot affect gameplay while hidden");Call("TickBoost",.1f);
   Check(g.BoostRemaining==0&&g.BoostCooldown==0,"Disabled boost clears stale timers");
   Fresh();Check(g.PlayerMissiles==Max&&g.Energy==150&&!g.BoostActive,"Retry restores ammo/energy with boost locked");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs stay locked");
   string source=File.ReadAllText("Assets/PhoViet/Scripts/PhoVietGame.cs");
   Check(source.Contains("Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.F)))TryFirePlayerMissile()"),"W and F share one launch branch (no duplicate shot)");
   string html=File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/index.html");Check(!html.Contains("Tăng tốc")&&!html.Contains("tăng tốc"),"Published template has no boost instructions");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/input2142-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.thunderstormEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Time.timeScale=scale;}
 }
}
