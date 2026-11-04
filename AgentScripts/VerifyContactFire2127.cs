using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using PhoViet;
public static class VerifyContactFire2127
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static PhoVietGame g;static List<string> checks;
 static object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
 static void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
 static object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
 static void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
 static ContactFireVisual Fire=>(ContactFireVisual)Get("contactFire");
 static void Fresh(){g.RestartRide();g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",99999f);Set("nextIntersectionDistance",999999f);}
 static PhoVietGame.TrafficVehicle Spawn(int index,int variant=-1)
 {var v=g.SpawnTraffic(2,25,index);var m=v.model.GetComponent<MeshyCarVariants>();if(m!=null&&variant>=0)m.Select(variant,0,v);v.hasTurned=true;return v;}
 static void Place(PhoVietGame.TrafficVehicle v,Vector3 p,Vector3 velocity)
 {v.model.localPosition=v.previousPosition=p;v.model.localRotation=Quaternion.identity;v.motionVelocity=velocity;v.speed=velocity.z;v.baseTrafficSpeed=Mathf.Max(5,velocity.magnitude);}
 static IEnumerable<object> Spots()=>((IEnumerable)typeof(ContactFireVisual).GetField("spots",F).GetValue(Fire)).Cast<object>().Where(s=>(bool)s.GetType().GetField("active").GetValue(s));
 static T Field<T>(object s,string n)=>(T)s.GetType().GetField(n).GetValue(s);
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();checks=new List<string>();float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);bool sound=(bool)Get("sound");
  bool[] flags={g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};
  try
  {
   Time.timeScale=0;Set("sound",false);
   var slots=Enumerable.Range(0,g.vehicles.Count).Where(i=>!g.vehicles[i].eventOnly).ToArray();
   foreach(int index in slots)
   {
    int count=g.vehicles[index].model.GetComponent<MeshyCarVariants>()?.variants.Length??1;
    for(int variant=0;variant<count;variant++)
    {
     Fresh();var v=Spawn(index,variant);var p=g.rider.localPosition;Place(v,p-Vector3.forward*(v.halfLength+g.PlayerHalfLength+8),Vector3.forward*50);v.model.localPosition=p;
     Set("Speed",0f);Set("oldRiderZ",p.z);Call("ResolveRiderContacts",p.x,.05f);Call("RenderThunderstorm");
     Check(g.State==PhoVietGame.RideState.Falling,"Swept rear collision still starts crash "+index+"/"+variant);
     Check(Fire.CountFor(g.rider)==1&&Fire.CountFor(v.model)==(v.armored?0:1),"Both contact surfaces burn; armored tank remains immune "+index+"/"+variant);
     foreach(var s in Spots())
     {
      var b=Field<PhoVietGame.TrafficVehicle>(s,"body");var anchor=Field<Vector3>(s,"anchor");
      Check(Mathf.Abs(anchor.x)<=b.halfWidth+.01f&&Mathf.Abs(anchor.z)<=b.halfLength+.01f,"Contact lies on its own footprint, including long trucks "+index+"/"+variant);
     }
    }
   }
   int bike=Array.Find(slots,i=>g.vehicles[i].halfWidth<.7f),car=Array.Find(slots,i=>g.vehicles[i].model.GetComponent<MeshyCarVariants>()!=null);
   foreach(bool swap in new[]{false,true})foreach(float speed in new[]{8f,35f,80f})
   {
    Fresh();var a=Spawn(swap?bike:car,0);var b=Spawn(swap?car:bike,0);Place(a,new Vector3(0,0,40),Vector3.zero);Place(b,new Vector3(0,0,40-a.halfLength-b.halfLength-8),Vector3.forward*speed);b.model.localPosition=new Vector3(0,0,40);
    Call("ResolveVehicleCollisions");Fire.Render();Check(a.knocked&&b.knocked&&Fire.CountFor(a.model)==1&&Fire.CountFor(b.model)==1,"NPC pair burns in both pool orders at speed "+speed+"/"+swap);
   }
   Fresh();var target=Spawn(car,0);Place(target,new Vector3(0,0,50),Vector3.zero);
   // Each fresh striker really crosses a different side face; a fallen vehicle
   // receives the second impulse and a second independent local attachment.
   var striker=Spawn(bike);Place(striker,new Vector3(-target.halfWidth-striker.halfWidth-5,0,50),Vector3.right*30);striker.model.localPosition=new Vector3(0,0,50);Call("ResolveVehicleCollisions");Fire.Render();
   Check(Fire.CountFor(target.model)==1,"First side collision ignites struck panel");
   striker.active=false;striker.model.gameObject.SetActive(false);Set("timeRiding",1f);target.previousPosition=target.model.localPosition;
   int bike2=Array.Find(slots,i=>i!=bike&&g.vehicles[i].halfWidth<.7f);var second=Spawn(bike2);var centre=target.model.localPosition;Place(second,centre+Vector3.right*(target.halfWidth+second.halfWidth+5),Vector3.left*30);second.model.localPosition=centre;Call("ResolveVehicleCollisions");Fire.Render();
   Check(Fire.CountFor(target.model)>=2,"Second collision ignites the other panel on an already fallen car");
   var attached=Spots().Where(s=>Field<PhoVietGame.TrafficVehicle>(s,"body")==target).ToArray();int n=Fire.CountFor(target.model);
   foreach(var s in attached)Fire.Ignite(target,Field<Vector3>(s,"anchor")+Vector3.one*.01f);
   Check(Fire.CountFor(target.model)==n,"Repeated frames at one contact do not stack flames");
   target.model.localPosition+=new Vector3(3,1,-12);target.model.localRotation=Quaternion.Euler(18,90,65);Fire.Render();
   foreach(var s in attached)Check(Vector3.Distance(Field<Transform>(s,"root").position,target.model.TransformPoint(Field<Vector3>(s,"anchor")))<.001f,"Surface flame follows throw, scroll, yaw, pitch and roll");
   Check(Spots().SelectMany(s=>Field<Transform>(s,"root").GetComponentsInChildren<Light>(true)).All(l=>!l.enabled),"Contact fire adds no active lights or shadow casters");
   g.TogglePause();Fire.Render();Check(Spots().SelectMany(s=>Field<ParticleSystem[]>(s,"particles")).All(ps=>ps.isPaused),"Pause freezes every contact emitter");g.TogglePause();Fire.Render();
   Check(Spots().SelectMany(s=>Field<ParticleSystem[]>(s,"particles")).All(ps=>ps.isPlaying),"Resume restarts the existing emitters");
   target.riverFalling=true;Fire.Render();Check(Fire.CountFor(target.model)==0,"River removes all fire attachments for that vehicle");
   Fresh();target=Spawn(car,0);Place(target,new Vector3(PhoVietGame.LaneX[2],0,50),Vector3.forward*15);Call("ActivateCollisionBody",target,Vector3.forward*15,Vector3.zero,Vector2.right*100,new Vector2(.5f,0),false,true,15f);
   Check(Fire.ActiveCount==0,"Generic impulse alone creates no fictitious contact fire");
   Fire.Ignite(target,new Vector3(target.halfWidth,.5f,0));for(int i=0;i<100&&target.knocked;i++)Call("TickTrafficRecovery",target,.05f);Fire.Render();
   Check(!target.knocked&&target.speed>0&&Fire.CountFor(target.model)==1,"Three-second recovery resumes driving WITH fire and smoke");
   target.model.localPosition+=Vector3.forward*2;Fire.Render();Check(Spots().Any(s=>Field<PhoVietGame.TrafficVehicle>(s,"body")==target&&Vector3.Distance(Field<Transform>(s,"root").position,target.model.TransformPoint(Field<Vector3>(s,"anchor")))<.001f),"Recovered moving vehicle carries its existing flames");
   target.spawnGeneration++;Fire.Render();Check(Fire.CountFor(target.model)==0,"Reused pool generation never inherits an old fire");
   for(int i=0;i<20;i++){Fire.Ignite(target,new Vector3(target.halfWidth,.5f,0));Fire.Clear();}int pool=Fire.PoolCount;
   for(int i=0;i<20;i++){Fire.Ignite(target,new Vector3(target.halfWidth,.5f,0));Fire.Clear();}Check(Fire.PoolCount==pool,"Retry/repeated impacts reuse particle roots without pool growth");
   Fresh();var npc=Spawn(bike);Place(npc,new Vector3(0,0,40),Vector3.zero);var police=new PhoVietGame.TrafficVehicle{model=g.policeBike,halfWidth=.5f,halfLength=1.2f};Set("policeBody",police);g.policeBike.gameObject.SetActive(true);Place(police,new Vector3(0,0,30),Vector3.forward*30);g.policeBike.localPosition=new Vector3(0,0,40);Call("ResolvePoliceContact",police.previousPosition,Vector3.forward*30,true);Fire.Render();
   Check(g.PoliceFallen&&Fire.CountFor(g.policeBike)==1&&Fire.CountFor(npc.model)==1,"Actual police/traffic contact burns both and keeps 1.2-second police fall");
   foreach(bool swap in new[]{false,true})
   {
    Fresh();var tank=Spawn(car);var variants=tank.model.GetComponent<MeshyCarVariants>();variants.Select(variants.ArmoredIndex,0,tank);var other=Spawn(bike);
    Place(tank,new Vector3(0,0,50),Vector3.zero);Place(other,new Vector3(0,0,50-tank.halfLength-other.halfLength-8),Vector3.forward*30);other.model.localPosition=new Vector3(0,0,50);
    if(swap){tank.previousPosition=tank.model.localPosition+Vector3.forward*8;tank.motionVelocity=Vector3.back*30;other.previousPosition=other.model.localPosition;other.motionVelocity=Vector3.zero;}
    Call("ResolveVehicleCollisions");Fire.Render();Check(Fire.CountFor(tank.model)==0&&Fire.CountFor(other.model)==1&&!tank.knocked,"Tank immunity applies on NPC contact in both approach directions "+swap);
   }
   Fresh();var fixedTank=Spawn(car);var fixedVariants=fixedTank.model.GetComponent<MeshyCarVariants>();fixedVariants.Select(fixedVariants.ArmoredIndex,0,fixedTank);var fixedWreck=Spawn(bike);fixedWreck.wallCrashed=true;
   Place(fixedTank,new Vector3(0,0,40),Vector3.forward*20);fixedTank.previousPosition-=Vector3.forward*10;Place(fixedWreck,new Vector3(0,0,40),Vector3.zero);Call("ResolveVehicleCollisions");Fire.Render();
   Check(Fire.CountFor(fixedTank.model)==0&&Fire.CountFor(fixedWreck.model)==1,"Tank hitting a wall-locked wreck ignites only the wreck despite both fixed masses");
   Fresh();var firstHit=Spawn(bike);var player=g.rider.localPosition;Place(firstHit,player-Vector3.forward*12,Vector3.forward*40);firstHit.model.localPosition=player;Set("Speed",0f);Set("oldRiderZ",player.z);Call("ResolveRiderContacts",player.x,.05f);
   firstHit.active=false;firstHit.model.gameObject.SetActive(false);var next=Spawn(bike2);player=g.rider.localPosition;Place(next,player-Vector3.right*8,Vector3.right*30);next.model.localPosition=player;Set("riderFallVelocity",Vector3.zero);Set("fallTime",1f);Call("ResolveFallenRiderContacts",player);Fire.Render();
   Check(Fire.CountFor(g.rider)>=2&&Fire.CountFor(next.model)==1,"Fallen player hit on another side gains another fire; second striker burns too");
   var shapes=g.vehicles[car].model.GetComponent<MeshyCarVariants>();
   for(int shape=0;shape<shapes.variants.Length;shape++)
   {
    Fresh();var roof=Spawn(car,shape);player=g.rider.localPosition;Place(roof,new Vector3(player.x,0,player.z),Vector3.zero);roof.previousYaw=0;
    float top=roof.model.GetComponent<MeshyCarVariants>().SupportHeight(-g.PlayerHalfLength,g.PlayerHalfLength)+.04f;
    Set("jumpPreviousHeight",top+.25f);Set("jumpHeight",top);Set("jumpFlying",true);Set("jumpVelocity",-3f);Set("oldRiderZ",player.z);g.rider.localPosition=player+Vector3.up*top;
    Call("ResolveRiderContacts",player.x,.05f);Fire.Render();Check(g.State==PhoVietGame.RideState.Riding&&Fire.ActiveCount==0,"Safe roof approach does not create a crash or flames "+shape);
   }
   g.ResetRide();Check(Fire.ActiveCount==0,"New run clears all contact fires");
   var result=new{success=true,count=checks.Count,pool=Fire.PoolCount,checks};File.WriteAllText("Captures/contact2127-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count,result.pool};
  }
  finally{g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];Set("sound",sound);Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);g.ResetRide();Time.timeScale=scale;}
 }
}
