using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Reflection;using UnityEngine;using PhoViet;
public static class VerifyEnergyRoutes2122
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;static PhoVietGame g;static List<string> checks=new List<string>();
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);static void Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);static void Check(bool b,string n){if(!b)throw new Exception(n);checks.Add(n);}
 static void Fresh()
 {g.RestartRide();g.riversEnabled=false;g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.tankBarrageEnabled=false;g.dinosaursEnabled=false;g.thunderstormEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float time=Time.timeScale;int best=g.BestDistance,coin=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);Time.timeScale=0;
  bool[] flags={g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled,g.tankBarrageEnabled,g.dinosaursEnabled,g.thunderstormEnabled};
  try
  {
   Fresh();Set("jumpFlying",true);Set("jumpHeight",2f);Set("jumpPreviousHeight",2f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],2,0);g.TryJump();Call("TickJump",.4f);g.TryJump();g.TryJump();Call("TickJump",.1f);Check(g.Energy==140,"Rapid hover toggles preserve unpaid flight fraction");
   foreach(int dir in new[]{-1,1})foreach(bool boost in new[]{false,true})
   {
    Fresh();int lane=dir<0?1:2;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);if(boost)g.TryBoost();
    Set("jumpFlying",true);Set("jumpHeight",3f);Set("jumpPreviousHeight",3f);g.rider.localPosition+=Vector3.up*3;Check(g.TryJump(),"Enter hover before turn "+dir+"/"+boost);
    bool requested=false;int steps=0;float dt=boost?.02f:1f/30;
    while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&steps++<1000)
    {if(!requested&&g.NextJunctionDistance<=25){g.MoveLane(dir);requested=true;}Call("TickRide",dt);}
    Check(requested&&g.CompletedTurns==1&&g.State==PhoVietGame.RideState.Riding,"Hover finishes real junction at road speed "+dir+"/"+boost);
    Check(g.RiderFlying&&Mathf.Abs(g.RiderJumpHeight-3)<.001f,"Rebase keeps held altitude "+dir+"/"+boost);
    Check(g.EnergyPickupPoolCount==70,"No new pickup allocations after turn "+dir+"/"+boost);
    var blocks=(List<Transform>)Get("blocks");var lookup=(IDictionary)Get("energyByTile");
    foreach(var tile in blocks){var e=lookup[tile];var lanes=(int[])e.GetType().GetField("lanes").GetValue(e);Check(lanes[0]!=lanes[1],"Destination tile still has two distinct pickup lanes");}
   }
   Fresh();var row=(List<Transform>)Get("blocks");var map=(IDictionary)Get("energyByTile");var tile0=row[0];var entry=map[tile0];var collected=(bool[])entry.GetType().GetField("collected").GetValue(entry);collected[0]=collected[1]=true;Call("ScrollRoutes",600f);Check(!collected[0]&&!collected[1]&&g.EnergyPickupPoolCount==70,"Actual scrolling recycles the same tile and renews only its pickup pair");
   var r=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/energy2122-route-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
  }
  finally{g.riversEnabled=flags[0];g.recklessTrafficEnabled=flags[1];g.trafficSurprises=flags[2];g.turnExitTrafficEnabled=flags[3];g.tankBarrageEnabled=flags[4];g.dinosaursEnabled=flags[5];g.thunderstormEnabled=flags[6];PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coin);Time.timeScale=time;g.ResetRide();}
 }
}
