using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class VerifyGestures
{
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required");
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool jobs=g.jobsEnabled;float scale=Time.timeScale;
  var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  Action reset=()=>{g.jobsEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}};
  Action<int,int,int,float,float,float> packet=(phase,id,action,x,y,at)=>g.ApplyBrowserGesture(JsonUtility.ToJson(new Packet{phase=phase,id=id,action=action,x=x,y=y,at=at}));
  Action begin=()=>packet(0,7,0,.5f,.5f,10);
  try
  {
   Time.timeScale=0;g.ApplyBrowserInsets("{\"left\":0,\"right\":0,\"top\":0,\"bottom\":0}");
   foreach(var dimensions in new[]{new Vector2(412,839),new Vector2(839,412),new Vector2(768,1024),new Vector2(1024,768)})
   {
    float edge=Mathf.Min(dimensions.x,dimensions.y);
    check(PhoVietGame.SwipeAction(Vector2.left*edge*.08f,edge)==-1,"Left at 8 percent short edge: "+dimensions);
    check(PhoVietGame.SwipeAction(Vector2.right*edge*.08f,edge)==1,"Right at 8 percent short edge: "+dimensions);
    check(PhoVietGame.SwipeAction(Vector2.up*edge*.13f,edge)==2,"Jump at 13 percent short edge: "+dimensions);
    check(PhoVietGame.SwipeAction(Vector2.down*edge*.13f,edge)==3,"Boost at 13 percent short edge: "+dimensions);
    check(PhoVietGame.SwipeAction(new Vector2(4,5),edge)==0,"Jitter is not swipe: "+dimensions);
   }
   reset();begin();packet(1,7,-1,.4f,.5f,10.01f);check(g.CurrentLane==1,"Left applies before pointer release");
   packet(1,7,-1,.3f,.5f,10.02f);check(g.CurrentLane==1,"Holding/moving same pointer cannot repeat lane change");
   packet(2,7,0,.3f,.5f,10.03f);check(g.CurrentLane==1,"Release cannot duplicate a swipe");
   packet(0,8,0,.5f,.5f,11);packet(1,8,1,.6f,.5f,11.01f);check(g.CurrentLane==2,"Next distinct pointer accepts right");
   reset();begin();packet(1,9,-1,.4f,.5f,10.01f);check(g.CurrentLane==2,"Other pointer cannot control active gesture");
   packet(3,7,0,.5f,.5f,10.02f);packet(1,7,-1,.4f,.5f,10.03f);check(g.CurrentLane==2,"Canceled gesture cannot later trigger");
   reset();begin();packet(1,7,2,.5f,.3f,10.01f);check(g.RiderJumpActive,"Up begins actual jump without release");
   reset();begin();packet(1,7,3,.5f,.7f,10.01f);check(g.BoostActive,"Down begins actual boost without release");
   float remaining=g.BoostRemaining;packet(1,7,3,.5f,.8f,10.02f);check(g.BoostRemaining==remaining,"Repeated event cannot stack boost");
   reset();packet(0,7,0,.5f,.02f,10);packet(1,7,2,.5f,.3f,10.1f);check(!g.RiderJumpActive,"Gesture starting on HUD is excluded");
   reset();g.TogglePause();begin();g.TogglePause();packet(1,7,2,.5f,.3f,10.1f);check(!g.RiderJumpActive,"Paused-start gesture cannot activate after resume");
   reset();begin();g.TogglePause();g.TogglePause();packet(1,7,-1,.4f,.5f,10.1f);check(g.CurrentLane==2,"Pause/resume cancels old held pointer");
   reset();begin();g.RestartRide();packet(1,7,-1,.4f,.5f,10.1f);check(g.CurrentLane==2,"Restart cancels previous run pointer");
   reset();g.jobsEnabled=true;g.ShowJobOffers();begin();packet(1,7,3,.5f,.7f,10.1f);check(!g.BoostActive&&g.JobOffersOpen,"Job selection never triggers gameplay gesture");
   reset();begin();packet(1,7,99,.5f,.5f,10.1f);check(g.CurrentLane==2&&!g.RiderJumpActive&&!g.BoostActive,"Unknown action ignored");
   reset();int bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);var target=g.SpawnTraffic(3,0,bike);
   packet(0,7,0,.5f,.5f,20);packet(2,7,0,.5f,.5f,20.04f);check(!target.knocked,"Single browser tap does not kick");
   packet(0,8,0,.5f,.5f,20.15f);packet(2,8,0,.5f,.5f,20.19f);check(target.knocked,"Two quick taps in one Unity frame still kick using browser timestamps");
   reset();var last=typeof(PhoVietGame).GetField("lastTapTime",BindingFlags.Instance|BindingFlags.NonPublic);
   begin();packet(1,7,-1,.4f,.5f,10.05f);packet(2,7,0,.5f,.5f,10.1f);check((float)last.GetValue(g)==-100,"Swipe returning to start cannot count as tap");
   reset();begin();packet(2,7,0,.5f,.5f,11);check((float)last.GetValue(g)==-100,"Long hold is not double-tap component");
   return new{success=true,count=checks.Count,checks,width=Screen.width,height=Screen.height};
  }
  finally{g.jobsEnabled=jobs;g.ResetRide();Time.timeScale=scale;}
 }
 [Serializable]class Packet{public int phase,id,action;public float x,y,at;}
}
