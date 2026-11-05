using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyPlayerMissiles2128
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);static void Field(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
 static object H(int i)=>((Array)Get("helicopters")).GetValue(i);static object M(int i)=>((Array)Get("playerMissiles")).GetValue(i);
 static Transform MR(int i)=>(Transform)Field(M(i),"root");static Transform HR(int i)=>g.GetHelicopterRoot(i);
 static void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
 static void Fresh(){g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);Set("Speed",20f);}
 static void Hover(){g.TryJump();Call("TickJump",.4f);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;if(!g.TryJump()||!g.RiderFlying)throw new Exception("Hover fixture failed");}
 static void Aircraft(float x,float z,int slot=0)
 {
  if(!g.IsHelicopterActive(slot))Call("SpawnHelicopter");var h=H(slot);HR(slot).localPosition=new Vector3(x,g.rider.localPosition.y+g.PlayerHeight*.5f,z);HR(slot).localRotation=Quaternion.Euler(0,180,0);Field(h,"missilePrevious",HR(slot).localPosition);Field(h,"shotRemaining",100f);Field(h,"shotInterval",1f);
 }
 static void Tick(float dt){Call("ScrollHelicopter",g.Speed*dt);Call("TickHelicopter",dt,g.rider.localPosition,false);Call("RenderThunderstorm");}
 static IEnumerable<object> Tiles()=>((IEnumerable)Get("missilePickupTiles")).Cast<object>();
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float scale=Time.timeScale,ratio=(float)Get("browserPixelRatio");int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool sound=(bool)Get("sound"),insets=(bool)Get("hasBrowserInsets");var oldInsets=(Vector4)Get("browserInsets");
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;Set("sound",false);Fresh();
   Check(g.PlayerMissiles==3&&PhoVietGame.MaximumPlayerMissiles==3,"Fresh run starts at3/3");
   Check(!g.TryFirePlayerMissile()&&g.PlayerMissiles==3,"Ground input cannot fire or spend ammunition");
   g.TryJump();Check(!g.TryFirePlayerMissile(),"Jump alone is not hover flight");Fresh();Hover();
   Check(g.TryFirePlayerMissile()&&g.PlayerMissiles==2&&Field(M(0),"target")==null,"No target still launches one straight missile and spends one");
   Vector3 direction=(Vector3)Field(M(0),"velocity");Check(Vector3.Dot(direction.normalized,Vector3.forward)>.999f,"Unguided missile follows travel heading");
   Check(!g.TryFirePlayerMissile()&&g.PlayerMissiles==2,"Repeated input in same frame cannot double-spend");Call("TickPlayerMissiles",.399f);Check(!g.TryFirePlayerMissile(),"Cooldown does not finish early");Call("TickPlayerMissiles",.001f);Check(g.TryFirePlayerMissile()&&g.PlayerMissiles==1,"Next shot accepted at0.4seconds");Call("TickPlayerMissiles",.4f);Check(g.TryFirePlayerMissile()&&g.PlayerMissiles==0,"Third shot uses final carried missile");Call("TickPlayerMissiles",.4f);Check(!g.TryFirePlayerMissile()&&g.PlayerMissiles==0,"Empty magazine cannot go negative");
   g.TogglePause();var frozen=MR(0).localPosition;float clock=g.PlayerMissileCooldown;Call("TickPlayerMissiles",2f);Check(MR(0).localPosition==frozen&&g.PlayerMissileCooldown==clock&&!g.TryFirePlayerMissile(),"Pause freezes projectiles/cooldown and rejects fire");g.TogglePause();
   Fresh();Hover();Set("Distance",1201f);Aircraft(0,110);Aircraft(3,55,1);Aircraft(0,-20,2);Aircraft(0,220,3);Check(g.TryFirePlayerMissile()&&Field(M(0),"target")==H(1),"Auto-lock selects nearest valid aircraft ahead, not behind or beyond160m");
   var missileVelocity=(Vector3)Field(M(0),"velocity");HR(1).localPosition+=Vector3.right*8;Field(H(1),"missilePrevious",HR(1).localPosition);Call("TickPlayerMissiles",.05f);Check(((Vector3)Field(M(0),"velocity")-missileVelocity).sqrMagnitude>.001f,"Homing turns toward a laterally moving target");
   foreach(float dt in new[]{1f/60,1f/30,.05f})foreach(float speed in new[]{17f,38f,90f})
   {
    Fresh();Hover();Set("Speed",speed);Aircraft(3,85);Check(g.TryFirePlayerMissile(),"Launch at speed"+speed+" dt"+dt);
    for(int i=0;i<300&&g.PlayerHelicoptersDowned==0;i++)Tick(dt);
    Check(g.PlayerHelicoptersDowned==1&&g.BurningHelicopterCount==1&&g.PlayerMissiles==2,"One hit burns/drops aircraft and spends exactly one, speed"+speed+" dt"+dt);
    Check(g.State==PhoVietGame.RideState.Riding&&!g.RiderAirAttackBurning&&!g.HitByHelicopterMissile,"Player missile never damages its owner");
    Call("RenderThunderstorm");var fx=(ThunderstormVisual)Get("stormVisual");Check(fx.BurningCount==1&&g.IsAircraftBurning(HR(0)),"Baked fire/smoke is active on the shot-down helicopter");
    for(int i=0;i<75;i++)Tick(.05f);Check(HR(0).localPosition.y<=.86f,"Shot aircraft falls to the ground instead of hovering");
   }
   Fresh();Hover();Aircraft(0,75);g.TryFirePlayerMissile();Field(H(0),"generation",(int)Field(H(0),"generation")+1);HR(0).localPosition=new Vector3(30,5,150);Field(H(0),"missilePrevious",HR(0).localPosition);Call("TickPlayerMissiles",.01f);Check(Field(M(0),"target")==null,"Reused aircraft generation never inherits an old homing lock");
   Fresh();Hover();Aircraft(0,75);g.TryFirePlayerMissile();Call("KnockHelicopter",H(0),Vector3.zero,Vector3.forward);Call("TickPlayerMissiles",.01f);Check(Field(M(0),"target")==null&&g.PlayerHelicoptersDowned==0,"Missile drops an already-wrecked target without crediting a second kill");
   Fresh();Hover();g.TryFirePlayerMissile();var roots=Enumerable.Range(0,12).Select(MR).ToArray();var before=MR(0).localPosition;var velocity=(Vector3)Field(M(0),"velocity");var inv=Quaternion.Euler(0,-90,0);var origin=new Vector3(2,0,7);Call("ScrollPlayerMissiles",3f);Call("RebaseHelicopter",inv,origin);
   Check(Vector3.Distance(MR(0).localPosition,inv*(before-origin))<.001f&&Vector3.Distance((Vector3)Field(M(0),"velocity"),inv*velocity)<.001f&&Vector3.Distance((Vector3)Field(M(0),"scroll"),inv*Vector3.forward*3)<.001f,"Turn rebase preserves projectile/velocity/queued scroll");
   Call("TickPlayerMissiles",4.1f);Check(g.ActivePlayerMissileCount==0,"Expired or escaped missiles retire");
   Check(g.MissilePickupPoolCount==70,"Two pickup roots per each of35existing road tiles");int bridge=0,street=0;
   var bridgeTile=(Transform)Field(Tiles().First(),"tile");((RiverBlock)Call("RiverOf",bridgeTile)).SetBridge(true);Call("RefreshMissilePickupTile",bridgeTile);
   foreach(var e in Tiles())
   {
    var tile=(Transform)Field(e,"tile");var rb=tile.GetComponent<RiverBlock>();bool onBridge=rb!=null&&rb.IsBridge;if(onBridge)bridge++;else street++;
    var lanes=(int[])Field(e,"lanes");var icons=(Transform[])Field(e,"icons");Check(lanes[0]!=lanes[1]&&lanes.All(x=>x>=0&&x<4),"Pickup lanes distinct and valid "+tile.name);
    if(onBridge)Check(lanes.All(x=>x==1||x==2),"Bridge pickups remain on the two bridge lanes");
    foreach(var icon in icons){Check(onBridge?icon.localPosition.z>RiverBlock.Near&&icon.localPosition.z<RiverBlock.Far:icon.localPosition.z>=27&&icon.localPosition.z<63,"Pickup stays on the street or within the actual bridge span");Check(icon.GetComponent<LODGroup>().GetLODs().Length==3&&icon.GetComponentsInChildren<ParticleSystem>(true).Length==0&&icon.GetComponentsInChildren<Light>(true).Length==0,"Pickup uses3LOD and no particles/lights");}
   }
   Check(bridge>0&&street>0,"Both street and bridge pickups exist");
   foreach(float dt in new[]{.016f,.05f})
   {
    Fresh();var e=Tiles().First();var tile=(Transform)Field(e,"tile");tile.gameObject.SetActive(true);var icons=(Transform[])Field(e,"icons");var done=(bool[])Field(e,"collected");Vector3 pickup=g.transform.InverseTransformPoint(icons[0].position);g.rider.localPosition=new Vector3(pickup.x,0,pickup.z);Set("Speed",0f);Call("TickMissilePickups",dt,g.rider.localPosition);Check(!done[0]&&g.PlayerMissiles==3,"Full magazine leaves pickup available");
    Set("PlayerMissiles",2);Call("TickMissilePickups",dt,g.rider.localPosition);Check(done[0]&&!icons[0].gameObject.activeSelf&&g.PlayerMissiles==3&&g.MissilePickupsCollected==1,"Touching pickup adds exactly1 and clamps3");Call("TickMissilePickups",dt,g.rider.localPosition);Check(g.MissilePickupsCollected==1,"Collected pickup cannot be taken twice");
    Call("RefreshMissilePickupTile",tile);Check(!done[0]&&icons[0].gameObject.activeSelf,"Recycled tile refreshes spent icons");
    pickup=g.transform.InverseTransformPoint(icons[0].position);Set("PlayerMissiles",0);g.rider.localPosition=new Vector3(pickup.x,4.5f,pickup.z);Call("TickMissilePickups",dt,g.rider.localPosition);Check(!done[0]&&g.PlayerMissiles==0,"High flight does not collect ground pickups remotely");
    g.rider.localPosition=new Vector3(pickup.x,0,pickup.z+5);Set("Speed",100f);Call("TickMissilePickups",dt,new Vector3(pickup.x,0,pickup.z-5));Check(done[0]&&g.PlayerMissiles==1,"Swept pickup contact survives high speed and lowFPS");
   }
   Fresh();Hover();g.ApplyBrowserInsets("{\"left\":0.04,\"top\":0.08,\"right\":0.03,\"bottom\":0.04,\"pixelRatio\":3}");Check((float)Get("browserPixelRatio")==3,"Web render ratio reaches touch sizing without changing resolution");g.ApplyBrowserInsets("{\"left\":0.04,\"top\":0.08,\"right\":0.03,\"bottom\":0.04,\"pixelRatio\":100}");Check((float)Get("browserPixelRatio")==3,"Invalid browser ratio cannot enlarge controls");var rect=g.PlayerMissileButtonPixels();var area=(Rect)Call("GameGuiArea");Check(rect.xMax<area.x+area.width*.5f&&rect.x>=area.x,"Missile button sits at the bottom-left of the game area (21.134)");
   Check(area.Contains(rect.min)&&area.Contains(rect.max)&&rect.width==rect.height,"Circular fire button fits within safe area");
   Vector2 pointer=new Vector2(rect.center.x,Screen.height-rect.center.y);Check((int)Call("HudButtonAt",pointer)==2&&!(bool)Call("GameplayPointerAllowed",pointer),"Fire touch wins HUD hit test and is excluded from swipe/doubletap");
   string tap=JsonUtility.ToJson(new Tap{x=pointer.x/Screen.width,y=1-pointer.y/Screen.height});
   g.ApplyBrowserGesture("{\"phase\":0,\"id\":8,\"x\":"+(pointer.x/Screen.width).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"y\":"+(1-pointer.y/Screen.height).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"at\":10}");g.ApplyHudTap(tap);
   g.ApplyBrowserGesture("{\"phase\":2,\"id\":8,\"x\":"+(pointer.x/Screen.width).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"y\":"+(1-pointer.y/Screen.height).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"at\":10.1}");
   Check(g.PlayerMissiles==2&&g.PlayerMissilesFired==1&&!g.RiderRolling,"Browser fire tap shoots once and never rolls");g.ApplyHudTap(tap);Check(g.PlayerMissiles==2,"Duplicate compatibility tap within cooldown cannot double-shoot");
   g.TogglePause();g.ApplyHudTap(tap);Check(g.PlayerMissiles==2&&g.State==PhoVietGame.RideState.Paused,"Paused fire tap does not toggle sound/resume/spend ammo");g.TogglePause();
   g.ResetRide();Check(g.PlayerMissiles==3&&g.PlayerMissilesFired==0&&g.PlayerHelicoptersDowned==0&&g.ActivePlayerMissileCount==0&&g.MissilePickupsCollected==0,"Retry restores all ammo and clears projectiles/counters");Check(Enumerable.Range(0,12).All(i=>MR(i)==roots[i]),"Retry preserves the fixed12-projectile pool");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/player2128-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("hasBrowserInsets",insets);Set("browserInsets",oldInsets);Set("browserPixelRatio",ratio);Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.ResetRide();Time.timeScale=scale;}
 }
 [Serializable]class Tap{public float x,y;}
}
