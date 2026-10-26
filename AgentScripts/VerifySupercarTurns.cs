using System;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifySupercarTurns
{
 public static object Main()
 {
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;int best=g.BestDistance;
  Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);};
  var checks=new List<string>();var failures=new List<string>();Action<bool,string> check=(b,s)=>{if(b)checks.Add(s);else failures.Add(s);};
  try{Time.timeScale=0;g.trafficSurprises=g.recklessTrafficEnabled=g.turnExitTrafficEnabled=false;
   foreach(int lane in new[]{0,1,2,3})foreach(bool boost in new[]{false,true})foreach(float dt in new[]{1f/60,.05f})
   {g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();clear();while(g.NextJunctionDistance>24){tick(.02f);clear();}typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);if(boost)g.TryBoost();g.MoveLane(lane<2?-1:1);
    int guard=0;bool speeds=true;while(g.CompletedTurns==0&&g.State==PhoVietGame.RideState.Riding&&guard++<500){tick(dt);clear();float expected=Mathf.Min(17+g.DifficultyTime*.12f,30)*(g.BoostActive?2.25f:1);speeds&=Mathf.Abs(expected-g.Speed)<.001f;}
    string label=$"lane={lane},boost={boost},dt={dt},state={g.State},scenery={g.LastSceneryImpact}";
    check(speeds,"Turn preserves current cruise/boost speed: "+label);check(g.State==PhoVietGame.RideState.Riding&&g.CompletedTurns==1&&g.CurrentLane==lane,"Car clears planters and completes into same lane: "+label);
   }
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks};File.WriteAllText("Captures/supercar-turn-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
