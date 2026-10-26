using System;using System.Linq;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyUrbanRider
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  bool rivers=g.riversEnabled,dinos=g.dinosaursEnabled,tanks=g.tankBarrageEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
  var checks=new List<string>();var failures=new List<string>();var cases=new List<object>();Action<bool,string> check=(ok,s)=>{if(ok)checks.Add(s);else failures.Add(s);};
  Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  Action reset=()=>{g.riversEnabled=g.dinosaursEnabled=g.tankBarrageEnabled=false;g.RestartRide();g.trafficSurprises=g.recklessTrafficEnabled=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}};
  Func<int,float,PhoVietGame.TrafficVehicle> spawn=(lane,speed)=>{var v=g.SpawnTraffic(lane,40,g.vehicles.FindIndex(t=>t.model.GetComponent<UrbanRiderMotion>()!=null&&!t.eventOnly));v.speed=v.baseTrafficSpeed=speed;v.model.localRotation=Quaternion.Euler(0,speed<0?180:0,0);v.model.GetComponent<BikeVisualAnimator>().Initialize();return v;};
  Action<PhoVietGame.TrafficVehicle,string,float> step=(v,method,dt)=>{var p=v.model.localPosition;call(method,new object[]{v,dt});v.motionVelocity=method=="TickLongitudinalVehicle"?(v.model.localPosition-p)/dt+Vector3.forward*g.Speed:v.model.forward*(method=="TickRoadTurn"?v.roadTurnSpeed:Mathf.Abs(v.crossSpeed));v.model.GetComponent<BikeVisualAnimator>().Advance(dt);};
  try
  {
   Time.timeScale=0;reset();var bikes=g.GetComponentsInChildren<BikeVisualAnimator>(true);var urban=bikes.Where(b=>b.role!=BikeVisualAnimator.ActorRole.Police).ToArray();
   check(urban.Length==31&&urban.Count(b=>b.role==BikeVisualAnimator.ActorRole.Parked)==14,"31 civilian ridden motorcycles including 14 parked actors replaced");
   foreach(var b in urban)
   {
    check(b.urbanMotion!=null&&b.visual.name.StartsWith("Urban Rider"),b.name+": new combined model and steering rig");
    check(b.clips.Length==7&&b.clips.All(c=>c.legacy),b.name+": seven legacy clips");
    var skins=b.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);check(skins.Length==3&&b.visual.GetComponent<LODGroup>().GetLODs().Length==3,b.name+": three LODs");
    check(skins.All(s=>s.sharedMaterial.shader.name=="PhoViet/Urban Rider"&&s.bones.Length==19),b.name+": mapped 19-bone textured skin");
   }
   check(g.GetComponentsInChildren<MeshFilter>(true).Count(f=>f.sharedMesh!=null&&f.sharedMesh.name.StartsWith("PV_UrbanParked"))==56,"56 side road parked meshes replaced");
   check(bikes.Count(b=>b.role==BikeVisualAnimator.ActorRole.Police)==1&&g.policeBike.GetComponent<UrbanRiderMotion>()==null&&g.policeBike.GetComponent<BikeVisualAnimator>().AssetName=="PV_TrafficPolice","Original police person and motorcycle kept");
   check(g.IsPlayerCar&&g.rider.GetComponent<PlayerCarVisual>()!=null&&g.rider.GetComponent<BikeVisualAnimator>()==null,"Player remains blue supercar");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");
   var mats=urban.Select(b=>b.visual.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial).Distinct().ToArray();
   check(mats.Length>=4&&mats.Select(m=>m.GetColor("_ShirtColor")).Distinct().Count()>=4,"Civilian shirts/bike colours retain variety");
   check(!ShaderUtil.GetShaderMessages(mats[0].shader).Any(m=>m.severity.ToString()=="Error"),"No shader compilation errors");
   foreach(float dt in new[]{.016f,.033f,.05f})foreach(float speed in new[]{8f,20f,40f})foreach(bool incoming in new[]{false,true})foreach(int side in new[]{-1,1})
   {
    reset();int lane=incoming?(side<0?1:0):(side<0?3:2);var v=spawn(lane,speed*(incoming?-1:1));var b=v.model.GetComponent<BikeVisualAnimator>();var m=b.urbanMotion;float maxSteer=0,maxBank=0,maxError=0,maxYawStep=0;float yaw=v.model.localEulerAngles.y;
    bool begin=g.BeginSuddenLaneChange(v,lane+side);float duration=v.changeDuration;int n=(int)Math.Ceiling(duration/dt)+100;
    for(int i=0;i<n;i++)
    {
     step(v,"TickLongitudinalVehicle",dt);maxSteer=Mathf.Max(maxSteer,Mathf.Abs(m.SteeringAngle));maxBank=Mathf.Max(maxBank,Mathf.Abs(m.LeanAngle));maxError=Mathf.Max(maxError,Mathf.Max(m.LeftArmError,m.RightArmError));maxYawStep=Mathf.Max(maxYawStep,Mathf.Abs(Mathf.DeltaAngle(yaw,v.model.localEulerAngles.y)));yaw=v.model.localEulerAngles.y;
    }
    string label=$"lane speed={speed},dt={dt},incoming={incoming},side={side}";
    check(begin&&!v.changingLane&&Mathf.Abs(v.model.localPosition.x-PhoVietGame.LaneX[lane+side])<.001f,"Lane centre reached: "+label);
    check(maxError<.015f&&maxSteer>.05f&&maxSteer<=25.01f&&maxBank<=23.01f,"Hands stay on steering/controlled bank: "+label);
    check(Mathf.Abs(m.LeanAngle)<.15f&&Mathf.Abs(m.SteeringAngle)<.15f&&maxYawStep<12,"Smooth turn and damped return: "+label);
    check(m.LowestWheelSupport()>=g.transform.position.y-.001f,"Tyres stay above ground while leaning: "+label);
    cases.Add(new{label,duration,maxSteer,maxBank,maxError,maxYawStep});
   }
   foreach(int direction in new[]{-1,1})foreach(int side in new[]{-1,1})foreach(bool crossing in new[]{false,true})
   {
    reset();var v=spawn(direction>0?2:1,direction*9);var b=v.model.GetComponent<BikeVisualAnimator>();var m=b.urbanMotion;
    if(crossing)
    {
     v.crossing=true;v.crossSpeed=direction*9;v.crossDelay=-1;v.turnChoice=side;v.turning=false;
     float zSign=-direction*side,endX=zSign>0?PhoVietGame.LaneX[direction>0?3:2]:PhoVietGame.LaneX[direction>0?1:0];v.model.localPosition=new Vector3(endX-direction*3,0,40);v.model.localRotation=Quaternion.Euler(0,direction*90,0);g.trafficSurprises=true;
    }
    else {v.roadTurning=true;v.roadTurnAngle=0;v.roadTurnRadius=4.2f;v.roadTurnSpeed=9;v.turnChoice=side;v.roadTurnStart=v.model.localPosition;v.roadTurnForward=Vector3.forward*direction;v.roadTurnSide=Vector3.right*direction*side;}
    m.Rebase(Mathf.DeltaAngle(direction<0?180:0,v.model.localEulerAngles.y));float maxError=0,maxSteer=0;
    for(int i=0;i<220;i++){step(v,crossing?"TickIntersectionVehicle":"TickRoadTurn",.02f);maxError=Mathf.Max(maxError,Mathf.Max(m.LeftArmError,m.RightArmError));maxSteer=Mathf.Max(maxSteer,Mathf.Abs(m.SteeringAngle));if(crossing?!v.turning&&v.hasTurned:!v.roadTurning)break;}
    cases.Add(new{junction=true,direction,side,crossing,v.hasTurned,maxSteer,maxError});check(v.hasTurned&&maxSteer>1&&maxError<.015f,$"Junction {(crossing?"crossing":"longitudinal")} direction={direction},side={side} steers with attached hands");
   }
   reset();var re=spawn(2,12);var bm=re.model.GetComponent<BikeVisualAnimator>();bm.Advance(0);bm.Advance(.02f);check(!float.IsNaN(bm.urbanMotion.SteeringAngle)&&!float.IsNaN(bm.urbanMotion.LeanAngle),"Zero-time pose update cannot poison steering springs");float before=bm.urbanMotion.SteeringAngle;re.model.localRotation=Quaternion.Euler(0,-90,0);bm.urbanMotion.Rebase(-90);re.motionVelocity=Vector3.left*12;bm.Advance(.02f);check(Mathf.Abs(bm.urbanMotion.SteeringAngle-before)<.01f,"World rebase does not produce fake steering");
   for(int i=0;i<80;i++){re.model.localRotation=Quaternion.Euler(0,-90+i*.5f,0);re.motionVelocity=Vector3.left*12;bm.Advance(.02f);}var front=bm.frontWheel.localRotation;var hand=bm.urbanMotion.leftHand.position;
   typeof(PhoVietGame).GetProperty("State").SetValue(g,PhoVietGame.RideState.Paused);bm.Advance(.2f);check(Quaternion.Angle(front,bm.frontWheel.localRotation)<.01f&&Vector3.Distance(hand,bm.urbanMotion.leftHand.position)<.0001f,"Pause freezes steering, wheels and hands");
   var result=new{success=failures.Count==0,count=checks.Count,failures,checks,cases};File.WriteAllText("Captures/urban-rider-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{g.riversEnabled=rivers;g.dinosaursEnabled=dinos;g.tankBarrageEnabled=tanks;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;}
 }
}
