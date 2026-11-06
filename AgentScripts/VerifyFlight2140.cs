using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
// 21.140: swipes in every orientation, the flight button and the flight-height energy icons.
public static class VerifyFlight2140
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static PhoVietGame g;static List<string> checks=new List<string>();
 static void Check(bool ok,string n){if(!ok)throw new Exception(n);checks.Add(n);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null&&p.CanWrite)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static void Tick(float dt,int n=1){var tick=(Action<float>)Delegate.CreateDelegate(typeof(Action<float>),g,typeof(PhoVietGame).GetMethod("TickRide",F));for(int i=0;i<n;i++){tick(dt);foreach(var v in g.vehicles)if(v.active){v.active=false;v.model.gameObject.SetActive(false);}}}
 static bool isolatePickups;
 static void Fresh(){g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);
  // Unrelated random pickups can refill energy during the jump/drain assertions.
  // Restore normal positions through RestartRide before the pickup-specific tests below.
  if(isolatePickups)foreach(var e in (System.Collections.IList)Get("energyTiles"))foreach(string field in new[]{"icons","airIcons"})foreach(Transform t in (Transform[])e.GetType().GetField(field).GetValue(e))t.position=new Vector3(10000,10000,10000);
 }
 static string Gesture(int phase,int id,int action,float x,float y)=>JsonUtility.ToJson(new G{phase=phase,id=id,action=action,x=x,y=y,at=Time.unscaledTime});
 [Serializable] class G{public int phase,id,action;public float x,y,at;}
 [Serializable] class Tap{public float x,y;}
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks.Clear();isolatePickups=true;
  float time=Time.timeScale;bool[] flags={g.dinosaursEnabled,g.tankBarrageEnabled,g.thunderstormEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};int best=g.BestDistance;
  try
  {
   Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.thunderstormEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   // ---- every swipe direction does something
   foreach(float edge in new[]{390f,1170f,720f})
   {
    int dead=0,ups=0;
    for(int angle=0;angle<360;angle+=3)
    {
     var d=new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad))*edge*.2f;int a=PhoVietGame.SwipeAction(d,edge);
     if(a==0)dead++;if(a==2)ups++;
    }
    Check(dead==0,"No swipe angle is dead, short edge "+edge);
    Check(ups>=30,"A wide fan of upward angles counts as jump, short edge "+edge+" ("+ups*3+" degrees)");
    foreach(float deg in new[]{-40f,-30f,-15f,0f,15f,30f,40f})
    {
     var d=new Vector2(Mathf.Sin(deg*Mathf.Deg2Rad),Mathf.Cos(deg*Mathf.Deg2Rad))*edge*.2f;
     Check(PhoVietGame.SwipeAction(d,edge)==2,"Thumb arc "+deg+" degrees off vertical jumps, short edge "+edge);
    }
    Check(PhoVietGame.SwipeAction(Vector2.up*edge*.09f,edge)==2,"Short 9 percent up-swipe jumps, short edge "+edge);
    Check(PhoVietGame.SwipeAction(Vector2.right*edge*.2f+Vector2.up*edge*.05f,edge)==1&&PhoVietGame.SwipeAction(Vector2.left*edge*.2f+Vector2.down*edge*.05f,edge)==-1,"Nearly level swipes still change lane, short edge "+edge);
    Check(PhoVietGame.SwipeAction(Vector2.down*edge*.1f,edge)==0&&PhoVietGame.SwipeAction(Vector2.down*edge*.13f,edge)==3,"Down-swipe distance unchanged, short edge "+edge);
   }
   // ---- where a swipe may start, on both orientations. Insets stand in for each viewport.
   var allowed=typeof(PhoVietGame).GetMethod("GameplayPointerAllowed",F);
   g.RestartRide();
   float w=Screen.width,h=Screen.height;bool wide=w>=h;
   Vector2 Px(float x,float y)=>new Vector2(x*w,y*h);
   Check((bool)allowed.Invoke(g,new object[]{Px(.6f,.015f)}),"A swipe may start at the very bottom of the screen");
   Check((bool)allowed.Invoke(g,new object[]{Px(.9f,.08f)}),"A swipe may start in the bottom right corner");
   Check((bool)allowed.Invoke(g,new object[]{Px(.5f,.5f)}),"A swipe may start mid screen");
   Check(!(bool)allowed.Invoke(g,new object[]{Px(.5f,.97f)}),"A swipe cannot start on the HUD bar");
   if(wide)Check((bool)allowed.Invoke(g,new object[]{Px(.04f,.97f)}),"Beside the landscape HUD bar a swipe may start at the top");
   var flight=g.FlightButtonPixels();var missile=g.PlayerMissileButtonPixels();
   Vector2 Bottom(Rect r)=>new Vector2(r.center.x,Screen.height-r.center.y);
   Check(!(bool)allowed.Invoke(g,new object[]{Bottom(flight)})&&!(bool)allowed.Invoke(g,new object[]{Bottom(missile)}),"Swipes do not start on the two buttons");
   // ---- a resting finger cannot block the swiping one
   Fresh();int energy=g.Energy;
   g.ApplyBrowserGesture(Gesture(0,1,0,.1f,.5f));g.ApplyBrowserGesture(Gesture(3,1,0,.1f,.5f));g.ApplyBrowserGesture(Gesture(0,2,0,.7f,.3f));g.ApplyBrowserGesture(Gesture(1,2,2,.7f,.2f));
   Check(g.RiderAirborne&&g.Energy==energy-PhoVietGame.JumpEnergyCost,"Second finger jumps after the shell hands over");
   Fresh();energy=g.Energy;
   g.ApplyBrowserGesture(Gesture(0,1,0,.1f,.5f));g.ApplyBrowserGesture(Gesture(0,2,0,.7f,.3f));g.ApplyBrowserGesture(Gesture(1,2,2,.7f,.2f));
   Check(g.RiderAirborne,"Newest finger wins even without a cancel for the first");
   Fresh();g.ApplyBrowserGesture(Gesture(0,1,0,.5f,.6f));g.ApplyBrowserGesture(Gesture(1,1,2,.5f,.5f));float e2=g.Energy;g.ApplyBrowserGesture(Gesture(1,1,2,.5f,.4f));Check(g.Energy==e2,"One finger triggers one action only");
   // ---- jump input is kept across a landing
   Fresh();g.TryJump();for(int i=0;i<300&&g.RiderAirborne;i++)Tick(1/60f);
   Check(!g.RiderAirborne,"Landed");Check(!g.TryJump()&&!g.RiderAirborne,"Immediately after landing the jump waits");
   bool fired=false;for(int i=0;i<40&&!fired;i++){Tick(1/60f);fired=g.RiderAirborne;}
   Check(fired,"A swipe just after landing fires on its own within the buffer");
   Fresh();energy=g.Energy;g.TryJump();for(int i=0;i<300&&g.RiderAirborne;i++)Tick(1/60f);for(int i=0;i<60;i++)Tick(1/60f);
   Check(!g.RiderAirborne&&g.Energy==energy-PhoVietGame.JumpEnergyCost,"No jump fires without a new input");
   // ---- flight button
   foreach(var size in new[]{new Vector2(844,390),new Vector2(932,430),new Vector2(667,375),new Vector2(1440,900),new Vector2(390,844),new Vector2(430,932),new Vector2(375,667),new Vector2(1024,768)})
   {
    var area=new Rect(0,0,size.x,size.y);bool narrow=size.x<size.y;var a=PhoVietGame.PlayerMissileButtonLayout(area,narrow,1);var b=PhoVietGame.FlightButtonLayout(area,narrow,1);
    Check(b.yMax<=a.y&&Mathf.Abs(b.x-a.x)<.01f&&b.width==a.width,"Flight button sits straight above the missile button "+size);
    Check(b.width>=56&&b.y>=size.y*(narrow?.2f:.16f)&&b.x<size.x*.25f,"Flight button is a full touch target clear of the top HUD, on the left "+size);
   }
   Fresh();energy=g.Energy;float min=PhoVietGame.FlightHeightAboveSupport;
   Check((int)Call("HudButtonAt",Bottom(flight))==3,"Button hit-test on its centre");
   Check((int)Call("HudButtonAt",new Vector2(flight.xMin+1,Screen.height-flight.yMin-1))!=3,"Button hit-test is round");
   var hudTap=JsonUtility.ToJson(new Tap{x=flight.center.x/Screen.width,y=flight.center.y/Screen.height});
   g.ApplyHudTap(hudTap);
   Check(g.RiderFlightPending&&g.RiderAirborne&&g.Energy==energy-PhoVietGame.JumpEnergyCost,"One browser tap lifts off, charging the jump cost");
   for(int i=0;i<60&&!g.RiderFlying;i++)Tick(1/60f);
   Check(g.RiderFlying,"Without any swipe the car reaches flight");
   Check(Mathf.Abs(g.RiderJumpHeight-min)<.25f,"It holds the flight height of the swipe route ("+g.RiderJumpHeight.ToString("F2")+")");
   float held=g.RiderJumpHeight;Tick(1/60f,30);Check(Mathf.Abs(g.RiderJumpHeight-held)<.001f&&g.RiderFlying,"Height is held");
   g.ApplyHudTap(hudTap);Check(!g.RiderFlying&&g.RiderAirborne,"Second tap starts the descent");
   for(int i=0;i<200&&g.RiderAirborne;i++)Tick(1/60f);Check(!g.RiderAirborne&&g.RiderJumpHeight==0,"The car lands on its own");
   Check(g.TryToggleFlightButton()&&g.RiderFlightPending,"Works immediately after landing, without the jump cooldown");
   g.TryToggleFlightButton();Check(!g.RiderFlightPending&&g.RiderAirborne,"A second press during the climb cancels the flight");
   Fresh();g.TryJump();for(int i=0;i<300&&!(g.RiderJumpHeight<min*.5f&&g.RiderJumpHeight>0&&Time.frameCount>=0&&(float)Get("jumpVelocity")<0);i++)Tick(1/60f);
   Check(g.TryToggleFlightButton(),"Press while falling from a plain jump");for(int i=0;i<60&&!g.RiderFlying;i++)Tick(1/60f);Check(g.RiderFlying&&Mathf.Abs(g.RiderJumpHeight-min)<.3f,"Falling car climbs back to flight height");
   Fresh();Set("Energy",PhoVietGame.JumpEnergyCost-1);Check(!g.TryToggleFlightButton()&&!g.RiderAirborne&&!g.FlightButtonReady,"Too little energy: refused and shown as unavailable");
   Fresh();Set("Energy",PhoVietGame.JumpEnergyCost+10);g.TryToggleFlightButton();for(int i=0;i<400&&g.RiderAirborne;i++)Tick(1/60f);Check(!g.RiderFlying&&g.Energy==0&&!g.RiderAirborne,"Empty energy lands the car as before");
   Fresh();g.TogglePause();Check(!g.TryToggleFlightButton(),"Pause blocks the button");g.TogglePause();
   // Swipes keep working as before
   Fresh();g.TryJump();Check(g.RiderAirborne&&!g.RiderFlightPending,"First swipe is a plain jump");g.TryJump();Check(g.RiderFlightPending,"Second swipe still requests flight");
   // ---- air energy icons
   isolatePickups=false;
   var tiles=(System.Collections.IList)Get("energyTiles");Check(tiles.Count==35,"Every road tile has flight icons");
   var type=tiles[0].GetType();var airIcons=type.GetField("airIcons");var airLanes=type.GetField("airLanes");var airCollected=type.GetField("airCollected");var tileField=type.GetField("tile");
   Func<int> shown=()=>{int n=0;foreach(var e in tiles)foreach(Transform t in (Transform[])airIcons.GetValue(e))if(t.gameObject.activeSelf)n++;return n;};
   Fresh();Tick(1/60f,10);Check(shown()==0,"No flight icons on the ground");
   var lanes=new HashSet<int>();int bridges=0;foreach(var e in tiles){foreach(int l in (int[])airLanes.GetValue(e))lanes.Add(l);var block=((Transform)tileField.GetValue(e)).GetComponent<RiverBlock>();if(block!=null&&block.IsBridge){bridges++;foreach(int l in (int[])airLanes.GetValue(e))lanes.Add(l);}}
   Check(lanes.SetEquals(new[]{0,1,2,3}),"Flight icons use all four lanes");
   Check(Mathf.Abs(PhoVietGame.AirPickupHeight-(min+.65f))<.001f,"Icons float level with the flying car's body");
   Fresh();g.TryToggleFlightButton();for(int i=0;i<40&&!g.RiderFlying;i++)Tick(1/60f);Tick(1/60f,2);
   int visible=shown();Check(visible>0,"Flying shows icons ("+visible+")");
   bool tooClose=false;foreach(var e in tiles)foreach(Transform t in (Transform[])airIcons.GetValue(e))if(t.gameObject.activeInHierarchy){var at=g.transform.InverseTransformPoint(t.position)-g.rider.localPosition;if(at.magnitude<40)tooClose=true;}
   Check(!tooClose,"Icons appear in the distance, not in front of the car");
   // Fly into one: put an unclaimed icon directly ahead in the car's lane.
   object target=null;Transform icon=null;int index=0;foreach(var e in tiles){var ts=(Transform[])airIcons.GetValue(e);for(int i=0;i<ts.Length;i++)if(ts[i].gameObject.activeInHierarchy&&target==null){target=e;icon=ts[i];index=i;}}
   Check(icon!=null,"An icon is available to collect");
   var world=g.rider.TransformPoint(Vector3.zero);var p=icon.parent.InverseTransformPoint(new Vector3(g.rider.position.x,0,g.rider.position.z+8));icon.localPosition=new Vector3(p.x,PhoVietGame.AirPickupHeight,p.z);
   Set("Energy",50);int before=g.Energy,got=g.EnergyPickupsCollected;for(int i=0;i<90&&g.EnergyPickupsCollected==got;i++)Tick(1/60f);
   Check(g.EnergyPickupsCollected==got+1&&((bool[])airCollected.GetValue(target))[index]&&!icon.gameObject.activeSelf,"Flying through an icon collects it");
   Check(g.Energy>before-40&&g.Energy<=PhoVietGame.MaximumEnergy,"Collecting adds energy");
   g.TryToggleFlightButton();for(int i=0;i<200&&g.RiderAirborne;i++)Tick(1/60f);Tick(1/60f,2);Check(shown()==0,"Landing hides every flight icon");
   // A jump on the ground cannot reach them, and ground icons stay as they were.
   Check(g.EnergyPickupPoolCount==70,"Ground icon pool unchanged");
   Check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Ride jobs stay locked");
   var report=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/flight2140-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return new{success=true,count=checks.Count,visible,bridges};
  }
  finally{g.dinosaursEnabled=flags[0];g.tankBarrageEnabled=flags[1];g.thunderstormEnabled=flags[2];g.recklessTrafficEnabled=flags[3];g.trafficSurprises=flags[4];g.turnExitTrafficEnabled=flags[5];g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);Time.timeScale=time;}
 }
}
