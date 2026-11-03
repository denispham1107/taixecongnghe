using System;using System.IO;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyEnergy2122
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks=new List<string>();
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
 static void Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 static void Energy(int n)=>Set("<Energy>k__BackingField",n);
 static void Reset()
 {
  g.RestartRide();g.recklessTrafficEnabled=false;g.trafficSurprises=false;g.turnExitTrafficEnabled=false;g.tankBarrageEnabled=false;g.dinosaursEnabled=false;g.thunderstormEnabled=false;
  foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);
 }
 static void Air(float height=2)
 {Set("jumpFlying",true);Set("jumpHeight",height);Set("jumpPreviousHeight",height);Set("jumpVelocity",0f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],height,0);}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);Time.timeScale=0;
  bool reckless=g.recklessTrafficEnabled,surprise=g.trafficSurprises,exit=g.turnExitTrafficEnabled,tank=g.tankBarrageEnabled,dino=g.dinosaursEnabled,storm=g.thunderstormEnabled;
  try
  {
   Reset();Check(g.Energy==150&&g.EnergyPickupsCollected==0,"New run starts at150/150");Check(g.EnergyPickupPoolCount==70,"Exactly two pooled icons for each of35 road tiles");
   int street=0,bridge=0;
   foreach(object entry in (IList)Get("energyTiles"))
   {
    var t=entry.GetType();var tile=(Transform)t.GetField("tile").GetValue(entry);var lanes=(int[])t.GetField("lanes").GetValue(entry);var icons=(Transform[])t.GetField("icons").GetValue(entry);
    Check(lanes[0]!=lanes[1],"Two distinct lanes: "+tile.name);
    bool water=tile.GetComponent<RiverBlock>().IsBridge;if(water){bridge++;Check(lanes[0]>=1&&lanes[0]<=2&&lanes[1]>=1&&lanes[1]<=2,"Bridge pickups stay on actual two-lane deck");}else street++;
    foreach(var icon in icons){Check(icon.localPosition.z>=13&&icon.localPosition.z<49&&icon.localPosition.y>.6f,"Pickup is inside block away from junctions");Check(icon.GetComponent<LODGroup>().GetLODs().Length==3,"Pickup C has three valid LODs");}
   }
   Check(street>0&&bridge>0,"Both streets and bridges covered");
   Check(g.TryJump()&&g.Energy==145,"Successful jump spends5");Check(!g.TryJump()&&g.Energy==145,"Repeated input in windup spends no more");
   Call("TickJump",.45f);Check(g.RiderAirborne&&g.RiderJumpHeight>0,"Energy jump still physically takes off");
   float held=g.RiderJumpHeight;Check(g.TryJump()&&g.RiderFlying&&g.Energy==145,"Second jump enters hover without extra jump charge");
   Call("TickJump",.49f);Check(g.Energy==145&&Mathf.Abs(g.RiderJumpHeight-held)<.00001f,"No early hover cost; altitude unchanged");Call("TickJump",.01f);Check(g.Energy==135,"Half-second hover costs10");
   Check(g.TryJump()&&!g.RiderFlying,"Third jump exits hover");Call("TickJump",.1f);Check(g.RiderJumpHeight<held,"Manual exit descends using gravity");
   foreach(int fps in new[]{20,30,60,120})
   {
    Reset();Energy(145);Air();Check(g.TryJump()&&g.RiderFlying,"Hover at"+fps+"FPS");for(int i=0;i<fps*2;i++)Call("TickJump",1f/fps);
    Check(g.Energy==105&&Mathf.Abs(g.RiderJumpHeight-2)<.00001f,"Exactly40 energy/2s and fixed altitude at"+fps+"FPS");
   }
   Reset();Energy(3);Air();g.TryJump();Call("TickJump",.51f);Check(g.Energy==0&&!g.RiderFlying&&g.RiderJumpHeight<2&&g.State==PhoVietGame.RideState.Riding,"Partial last charge empties energy and descends without automatic loss");
   Reset();Energy(4);Check(!g.TryJump()&&g.Energy==4&&!g.RiderJumpActive,"Insufficient energy blocks jump without spending");Energy(5);Check(g.TryJump()&&g.Energy==0,"Exactly5 can fund one jump");Call("TickJump",.5f);Check(!g.TryJump()&&!g.RiderFlying,"Empty energy cannot enable hover");
   Reset();Air();g.TryJump();Check(g.TryKick()&&g.RiderRolling&&g.KickCount==0&&!g.IsChased,"Q in hover starts roll instead of ram or pursuit");Check(!g.TryKick(),"Roll cannot stack");Call("TickJump",.45f);Check(Mathf.Abs(g.RiderBarrelRoll-180)<.001f&&g.RiderJumpHeight==2,"Half roll reaches180 with same altitude");
   var car=g.rider.GetComponent<PlayerCarVisual>();car.Advance(.02f);var p=car.visual.TransformPoint(Vector3.up*(car.size.y*.5f/car.ModelScale));Check(Mathf.Abs(p.y-(g.rider.position.y+car.size.y*.5f))<.05f,"Roll rotates around body centre without height jump");
   Call("TickJump",.45f);car.Advance(.02f);Check(!g.RiderRolling&&g.RiderFlying&&g.Energy==140,"Full360 completes level; hover billing continues");
   Reset();Air();g.TryJump();var point=new Vector2(Screen.width*.5f,Screen.height*.6f);Check(!g.RegisterTap(point,10)&&g.RegisterTap(point,10.15f)&&g.RiderRolling,"Mobile double tap in hover uses same roll action");
   int before=g.Energy;float angle=g.RiderBarrelRoll;g.TogglePause();Check(!g.TryJump()&&!g.TryKick()&&g.Energy==before&&g.RiderBarrelRoll==angle,"Paused input cannot spend or flip");g.TogglePause();
   Call("ResetJump");Check(!g.RiderFlying&&!g.RiderRolling,"Crash/river reset cancels hover and roll");
   Reset();Energy(100);
   var list=(IList)Get("energyTiles");object chosen=null;foreach(object e in list){var tile=(Transform)e.GetType().GetField("tile").GetValue(e);if(tile.gameObject.activeInHierarchy&&Mathf.Abs(tile.eulerAngles.y)<1){chosen=e;break;}}
   Check(chosen!=null,"Main-road pickup fixture exists");
   foreach(object e in list){var got=(bool[])e.GetType().GetField("collected").GetValue(e);got[0]=got[1]=true;}
   var ct=chosen.GetType();var ci=(Transform[])ct.GetField("icons").GetValue(chosen);var collected=(bool[])ct.GetField("collected").GetValue(chosen);collected[0]=false;ci[0].gameObject.SetActive(true);
   Vector3 at=g.transform.InverseTransformPoint(ci[0].position);g.rider.localPosition=new Vector3(at.x,0,at.z+4);Call("TickEnergyPickups",.05f,new Vector3(at.x,0,at.z-4));
   Check(g.Energy==115&&g.EnergyPickupsCollected==1&&collected[0]&&!ci[0].gameObject.activeSelf,"Swept pickup at fast speed restores15 and disappears once");Call("TickEnergyPickups",.05f,new Vector3(at.x,0,at.z-4));Check(g.Energy==115&&g.EnergyPickupsCollected==1,"Collected icon cannot award twice");
   Energy(145);collected[0]=false;ci[0].gameObject.SetActive(true);Call("TickEnergyPickups",.05f,new Vector3(at.x,0,at.z-4));Check(g.Energy==150,"Pickup caps at150");
   Energy(100);collected[0]=false;ci[0].gameObject.SetActive(true);g.rider.localPosition=new Vector3(at.x,5,at.z+4);Call("TickEnergyPickups",.05f,new Vector3(at.x,5,at.z-4));Check(g.Energy==100&&!collected[0],"Flying above the icon does not collect remotely");
   var tileChosen=(Transform)ct.GetField("tile").GetValue(chosen);Call("RefreshEnergyTile",tileChosen);Check(!collected[0]&&!collected[1],"Recycled tile resets exactly its two pickups");
   g.RestartRide();Check(g.Energy==150&&g.EnergyPickupsCollected==0&&g.EnergyPickupPoolCount==70&&!g.RiderFlying,"Retry resets energy and reuses70 pooled objects");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/energy2122-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }
  finally{Time.timeScale=scale;Set("<BestDistance>k__BackingField",best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprise;g.turnExitTrafficEnabled=exit;g.tankBarrageEnabled=tank;g.dinosaursEnabled=dino;g.thunderstormEnabled=storm;g.ResetRide();}
 }
}
