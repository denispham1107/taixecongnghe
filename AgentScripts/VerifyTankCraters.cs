using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyTankCraters
{
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Set(object g,string key,object value)=>g.GetType().GetField(key,Flags).SetValue(g,value);
 static object Call(object g,string name,params object[] args)=>g.GetType().GetMethod(name,Flags).Invoke(g,args);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
  bool tanks=g.tankBarrageEnabled,dinos=g.dinosaursEnabled,rivers=g.riversEnabled;float scale=Time.timeScale;var checks=new List<string>();var traces=new List<object>();
  void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
  void Fixture()
  {
   g.tankBarrageEnabled=false;g.dinosaursEnabled=false;g.riversEnabled=false;g.ResetRide();g.StartRide();
   foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set(g,"spawnTimer",10000f);Set(g,"nextIntersectionDistance",float.PositiveInfinity);
  }
  void Terrain(float dt)=>Call(g,"UpdateTankCraterTerrain",dt);
  void Step(float dt){Call(g,"TickRide",dt);Terrain(dt);}
  try
  {
   Time.timeScale=0;Fixture();
   Check(g.CreateTankCrater(new Vector3(1.75f,2,8))==null,"Aerial explosion cannot create a floating crater");
   var c=g.CreateTankCrater(new Vector3(1.75f,0,8));Check(c!=null&&g.ActiveCraterCount==1,"Ground crater has road owner");Terrain(.3f);
   Check(Mathf.Abs(c.model.localScale.x-PhoVietGame.TankCraterRadius)<.001f,"Crater opens to full radius");
   var same=g.CreateTankCrater(new Vector3(1.9f,0,8));Check(same==c&&g.ActiveCraterCount==1,"Overlapping repeated hits reuse depression");
   for(int i=0;i<400;i++)Terrain(.05f);Check(g.ActiveCraterCount==1&&c.age>20,"No 15-second expiry on a stationary visible road");
   float age=c.age;g.TogglePause();Terrain(2);Check(c.age==age,"Pause freezes crater reveal/age");g.TogglePause();
   var before=g.transform.InverseTransformPoint(c.model.position);float distance=g.Distance;Step(.05f);var after=g.transform.InverseTransformPoint(c.model.position);
   Check(Mathf.Abs((before.z-after.z)-(g.Distance-distance))<.003f,"Crater follows exactly the road scroll");
   c.model.position=g.transform.TransformPoint(new Vector3(1.75f,0,-95));Terrain(0);Check(g.ActiveCraterCount==0,"Only far-behind terrain is retired");
   Fixture();var initial=g.CreateTankCrater(new Vector3(1.75f,0,8));Terrain(.3f);Call(g,"ClearShells");Check(g.ActiveCraterCount==1,"Clearing shells/turn entry never clears ground damage");g.ResetRide();
   Check(g.ActiveCraterCount==0&&!g.CraterSteeringActive&&Shader.GetGlobalInt("_PVCraterCount")==0,"Retry clears all craters/drift/cutout masks");
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(int side in new[]{-1,1})foreach(bool boost in new[]{false,true})
   {
    Fixture();if(boost)g.TryBoost();float x=g.rider.localPosition.x;
    var crater=g.CreateTankCrater(new Vector3(x+side*1.4f,0,boost?16:10));Terrain(.3f);
    int frames=0;while(g.CraterWheelHits==0&&frames++<180&&g.State==PhoVietGame.RideState.Riding)Step(dt);
    Check(g.CraterWheelHits==1&&g.State==PhoVietGame.RideState.Riding,"Wheels enter once / no automatic loss "+dt+" side"+side+" boost"+boost);
    for(int i=0;i<Mathf.CeilToInt(.35f/dt);i++)Step(dt);
    Check(g.CraterSteeringActive&&Mathf.Sign(g.CraterDriftOffset)==side&&Mathf.Abs(g.CraterDriftOffset)>.05f,"Visible persistent deflection to impacted side "+dt+" / "+side+" / "+boost);
    float offset=g.CraterDriftOffset;for(int i=0;i<Mathf.CeilToInt(.45f/dt);i++)Step(dt);
    Check(g.CraterSteeringActive&&Mathf.Abs(g.CraterDriftOffset)>Mathf.Abs(offset)+.1f,"No automatic heading recovery after leaving hole "+dt+" / "+side+" / "+boost);
    var p=g.rider.localPosition;g.MoveLane(-side);
    Check(!g.CraterSteeringActive&&Vector3.Distance(p,g.rider.localPosition)<.00001f,"Counter input responds immediately without teleport "+dt+" / "+side+" / "+boost);
    for(int i=0;i<Mathf.CeilToInt(1.8f/dt);i++)Step(dt);
    Check(Mathf.Abs(g.CraterDriftOffset)<.012f&&g.State==PhoVietGame.RideState.Riding,"Controlled smooth balance recovery "+dt+" / "+side+" / "+boost);
    traces.Add(new{dt,side,boost,hitFrame=frames,offsetAtCounter=p.x-x,finalOffset=g.CraterDriftOffset});
   }
   foreach(float dt in new[]{1f/60,1f/30,.05f})
   {
    Fixture();g.CreateTankCrater(new Vector3(g.rider.localPosition.x+.5f,0,17));Terrain(.3f);Check(g.TryJump(),"Jump accepted "+dt);
    for(int i=0;i<Mathf.CeilToInt(1.6f/dt);i++)Step(dt);
    Check(g.CraterWheelHits==0&&!g.CraterSteeringActive&&g.State==PhoVietGame.RideState.Riding,"Jumping wheels clear crater at "+dt);
   }
   Fixture();g.riversEnabled=true;var tile=g.blocks[1];var river=tile.GetComponent<RiverBlock>();river.SetBridge(true);
   Vector3 P(float x,float y)=>g.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(x,y,30)));
   Check(g.CreateTankCrater(P(5.25f,0))==null,"Water outside bridge cannot grow an asphalt crater");
   var deck=g.CreateTankCrater(P(1.75f,0));Terrain(.3f);Check(deck!=null&&deck.block.GetVector("_PVCraterClipBand").w==1,"Bridge depression is clipped to deck instead of floating over river");
   int splashes=g.ShellSplashes;Call(g,"Detonate",new PhoVietGame.TankShell{overWater=true},P(5.25f,RiverBlock.WaterY),false);
   Check(g.ShellSplashes==splashes+1&&g.ActiveCraterCount==1,"Water detonation keeps splash behavior / no new crater");
   Fixture();int blasts=g.ShellBlasts;Call(g,"Detonate",new PhoVietGame.TankShell(),new Vector3(-5.25f,0,12),false);
   Check(g.ShellBlasts==blasts+1&&g.ActiveCraterCount==1,"Real ground shell detonation creates terrain damage");
   Fixture();Call(g,"Detonate",new PhoVietGame.TankShell(),g.rider.localPosition+Vector3.up*2,true);
   Check(g.KilledByShell&&g.ActiveCraterCount==0,"Direct shell damage unchanged / no airborne crater");
   var report=new{success=true,count=checks.Count,checks,traces};File.WriteAllText("Captures/crater-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{report.success,report.count};
  }
  finally{g.tankBarrageEnabled=tanks;g.dinosaursEnabled=dinos;g.riversEnabled=rivers;g.ResetRide();Time.timeScale=scale;}
 }
}
