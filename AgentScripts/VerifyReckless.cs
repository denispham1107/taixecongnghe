using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using PhoViet;

public static class VerifyReckless
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool originalReckless=g.recklessTrafficEnabled;
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        bool originalSurprises=g.trafficSurprises;g.trafficSurprises=false;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float scale=Time.timeScale;
        var checks=new List<string>();
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);checks.Add(name);};
        Action<string,float> set=(name,value)=>typeof(PhoVietGame).GetField(name,F).SetValue(g,value);
        Action<string,object[]> call=(name,args)=>typeof(PhoVietGame).GetMethod(name,F).Invoke(g,args);
        Action<float> tick=dt=>call("TickRide",new object[]{dt});
        Action clean=()=>{g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);g.recklessTrafficEnabled=false;};
        try
        {
            Time.timeScale=0;
            check(g.recklessBikeModel!=null&&g.recklessTruckModel!=null,"Saved scene includes sport motorcycle and medium truck");
            check(g.vehicles.Count(v=>v.eventOnly)==2&&g.vehicles.Count(v=>!v.eventOnly)==26,"Dedicated event pool preserves all 26 ordinary vehicles");
            check(g.vehicles.Where(v=>v.eventOnly).All(v=>v.crashEffect!=null&&v.smoke!=null&&v.fire!=null),"Both event vehicles have smoke and flames");
            check(g.recklessTruckModel.name=="Xe tải ngủ gật"&&g.recklessTruckModel.GetComponentsInChildren<TextMesh>(true).Any(t=>t.text=="XE TẢI NGỦ GẬT"),"Drowsy truck has its exact name and visible Vietnamese sign");
            check(typeof(PhoVietGame).GetField("nextRecklessAt",F)==null,"The old twenty-second event timer is removed");
            clean();g.recklessTrafficEnabled=true;set("collisionImmunity",10000);
            for(int i=0;i<12;i++){set("spawnTimer",0);tick(.02f);}
            check(g.RecklessEvents>0&&g.Distance<10,"Dangerous traffic can appear in ordinary waves immediately, without waiting twenty seconds");
            int events=g.RecklessEvents;g.TogglePause();call("Update",null);
            check(g.RecklessEvents==events,"Pause cannot spawn dangerous traffic");
            clean();var busy=g.SpawnReckless(true);Vector3 busyPosition=busy.model.localPosition;
            check(g.SpawnReckless(true)==null&&busy.active&&busy.model.localPosition==busyPosition,"A busy dangerous model is never teleported away to make another spawn");
            clean();var first=g.SpawnReckless(true);var secondDanger=g.SpawnReckless(false);
            check(secondDanger!=null&&(Mathf.Abs(first.model.localPosition.x-secondDanger.model.localPosition.x)>=first.halfWidth+secondDanger.halfWidth||Mathf.Abs(first.model.localPosition.z-secondDanger.model.localPosition.z)>=first.halfLength+secondDanger.halfLength),"New dangerous traffic never spawns overlapping the other dangerous vehicle");
            clean();g.recklessTrafficEnabled=true;
            for(int i=0;i<40;i++)
            {
                call("MaybeSpawnReckless",null);
                foreach(var v in g.vehicles.Where(t=>t.eventOnly&&t.active))call("RetireVehicle",new object[]{v});
            }
            check(g.RecklessEvents>20,"Dangerous drivers appear frequently across ordinary traffic waves");
            check(g.OncomingRecklessEvents>0&&g.OncomingRecklessEvents<g.RecklessEvents,"Natural traffic waves include both oncoming and same-direction dangerous drivers");
            check(g.recklessTruckModel.Find("Biển xe nguy hiểm/Tên xe mặt trước")!=null&&g.recklessBikeModel.Find("Biển xe nguy hiểm/Tên xe mặt trước")!=null,"Both dangerous models have readable front-facing names for oncoming travel");

            foreach(bool truck in new[]{false,true})
            {
                clean();var incoming=g.SpawnReckless(truck,true);incoming.willHitWall=false;
                check(incoming.speed<0&&incoming.lane<2&&incoming.model.localPosition.x<0,"Oncoming dangerous driver starts in a real opposing lane: "+truck);
                check(Vector3.Dot(incoming.model.forward,Vector3.back)>.99f,"Oncoming model faces the rider instead of driving backward: "+truck);
                float beforeZ=incoming.model.localPosition.z,maxX=-10;
                for(int i=0;i<75;i++){call("TickRecklessVehicle",new object[]{incoming,.02f});maxX=Mathf.Max(maxX,incoming.model.localPosition.x);}
                check(maxX>3.1f&&incoming.model.localPosition.z<beforeZ,"Oncoming driver invades player lanes while approaching: "+truck);
                clean();incoming=g.SpawnReckless(truck,true);incoming.model.localPosition=new Vector3(g.rider.localPosition.x,0,.5f);tick(.02f);
                check(g.State==PhoVietGame.RideState.Falling,"An oncoming dangerous driver can hit the player: "+truck);
                foreach(bool carTarget in new[]{false,true})
                {
                    clean();incoming=g.SpawnReckless(truck,true);incoming.model.localPosition=new Vector3(0,0,40);incoming.previousPosition=new Vector3(-2,0,42);incoming.motionVelocity=new Vector3(10,0,incoming.speed);
                    var target=g.SpawnTraffic(1,39,carTarget?0:1);target.model.localPosition=new Vector3(.8f,0,39);target.previousPosition=target.model.localPosition;target.speed=0;
                    call("ResolveRecklessRams",null);
                    check(target.knocked&&g.RecklessRamCount==1&&!g.IsChased,"Oncoming threat physically rams ordinary bike/car without blaming rider: "+truck+","+carTarget);
                }
                foreach(float wallSide in new[]{-1f,1f})
                {
                    clean();incoming=g.SpawnReckless(truck,true);incoming.model.localPosition=new Vector3(incoming.model.localPosition.x,0,150);incoming.startPosition=incoming.model.localPosition;
                    incoming.willHitWall=true;incoming.wallSide=wallSide;incoming.wallCrashAt=.7f;set("collisionImmunity",10000);
                    for(int i=0;i<240&&incoming.active&&!incoming.wallCrashed;i++)tick(.02f);
                    check(incoming.wallCrashed&&Mathf.Sign(incoming.model.localPosition.x)==wallSide,"Natural oncoming trajectory crashes into either house row: "+truck+","+wallSide);
                    check(incoming.smoke.isPlaying&&incoming.fire.isPlaying&&incoming.crashEffect.gameObject.activeSelf,"Oncoming building impact visibly starts fire and smoke: "+truck+","+wallSide);
                }
            }
            foreach(float side in new[]{-1f,1f})
            {
                clean();var incoming=g.SpawnReckless(false,true);incoming.model.localPosition=new Vector3(g.rider.localPosition.x+side*3.1f,0,3.3f);
                check(g.TryKick()&&incoming.knocked&&incoming.knockDirection==side&&!g.IsChased,"Self-defense against an actual oncoming racer works on either side without police: "+side);
            }

            foreach(bool truck in new[]{false,true})
            {
                clean();var v=g.SpawnReckless(truck);v.willHitWall=false;
                check(v.isReckless&&v.eventOnly&&v.speed>0&&Mathf.Abs(v.lane-g.CurrentLane)==1,"Dangerous driver begins moving forward in an adjacent lane: "+truck);
                check(v.enteringFromBehind&&v.model.localPosition.z<g.rideCamera.transform.localPosition.z-v.halfLength&&v.speed>g.Speed,"Same-direction dangerous driver starts behind the camera and overtakes: "+truck);
                for(int i=0;i<400&&v.enteringFromBehind;i++)call("TickRecklessVehicle",new object[]{v,.02f});
                check(!v.enteringFromBehind&&v.model.localPosition.z>=1.5f,"Dangerous driver approaches continuously before starting its weave: "+truck);
                float initialYaw=v.model.localEulerAngles.y,maxX=-10,minX=10,maxTurn=0;
                for(int i=0;i<75;i++){call("TickRecklessVehicle",new object[]{v,.02f});maxX=Mathf.Max(maxX,v.model.localPosition.x);minX=Mathf.Min(minX,v.model.localPosition.x);maxTurn=Mathf.Max(maxTurn,Quaternion.Angle(Quaternion.Euler(0,initialYaw,0),v.model.localRotation));}
                check(minX<=g.rider.localPosition.x&&maxX>=g.rider.localPosition.x,"Swerving crosses into the player's lane: "+truck);
                check(maxTurn>2,"Vehicle heading follows its swerve: "+truck);
                check(truck?v.halfLength>3&&v.halfWidth>1:v.halfWidth<.7f,"Event collision bounds match its model: "+truck);
            }
            foreach(int playerLane in new[]{0,1,2,3})
            {
                clean();while(g.CurrentLane<playerLane)g.MoveLane(1);while(g.CurrentLane>playerLane)g.MoveLane(-1);
                for(int i=0;i<30;i++)tick(.02f);
                var nearby=g.SpawnReckless(true);
                check(Mathf.Abs(nearby.lane-playerLane)==1&&nearby.swerveDirection*(g.rider.localPosition.x-nearby.startPosition.x)>0,"Drowsy truck overtakes in an adjacent lane and then steers toward each player lane: "+playerLane);
            }
            clean();var racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(4.65f,0,1.8f);racer.speed=-10;
            check(g.TryKick()&&racer.knocked&&racer.knockDirection==1,"Oncoming reckless motorcycle can be kicked rightward");
            check(!g.IsChased&&!g.policeBike.gameObject.activeSelf,"Self-defense does not summon police");
            clean();racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(-1.55f,0,0);
            check(g.TryKick()&&racer.knockVelocity.x<0&&!g.IsChased,"Self-defense works on the left too");
            clean();var innocent=g.SpawnTraffic(3,0,1);racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(-1.55f,0,1);
            check(g.TryKick()&&racer.knocked&&!innocent.knocked&&!g.IsChased,"Self-defense prioritizes the dangerous rider over innocent adjacent traffic");
            clean();g.SpawnTraffic(3,0,1);g.TryKick();float gap=g.PoliceDistance;set("kickCooldown",0);
            racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(-1.55f,0,0);g.TryKick();
            check(g.IsChased&&g.PoliceDistance==gap,"Self-defense preserves an existing chase without resetting or adding police");
            clean();var truckTarget=g.SpawnReckless(true);truckTarget.model.localPosition=new Vector3(4.65f,0,0);
            check(!g.TryKick()&&!truckTarget.knocked&&!g.IsChased,"A medium truck cannot be kicked");
            clean();var oncoming=g.SpawnTraffic(1,3.3f,1);check(g.TryKick()&&oncoming.knocked&&g.IsChased,"Ordinary oncoming bikes are kickable within the extended reach window");
            clean();oncoming=g.SpawnTraffic(1,3.6f,1);check(!g.TryKick(),"Oncoming bike beyond reach cannot be kicked");
            foreach(bool truck in new[]{false,true})
            {
                clean();var threat=g.SpawnReckless(truck);threat.willHitWall=false;threat.model.localPosition=new Vector3(1.55f,0,.5f);tick(.02f);
                check(g.State==PhoVietGame.RideState.Falling&&g.LivesRemaining==2,"Being hit by reckless traffic spends one life: "+truck);
                foreach(float side in new[]{-1f,1f})
                {
                    clean();threat=g.SpawnReckless(truck);threat.enteringFromBehind=false;threat.model.localPosition=new Vector3(threat.model.localPosition.x,0,30);threat.startPosition=threat.model.localPosition;threat.willHitWall=true;threat.wallSide=side;threat.wallCrashAt=.9f;
                    set("collisionImmunity",10000);bool crossed=false;
                    for(int i=0;i<300&&threat.active&&!threat.wallCrashed;i++){tick(.02f);crossed|=threat.model.localPosition.x>1;}
                    check(threat.wallCrashed,"Natural driving trajectory reaches a house: truck="+truck+", side="+side);
                    check(side<0||crossed,"Right-side building crash traverses player lanes: truck="+truck);
                }
            }

            foreach(bool truck in new[]{false,true})
            foreach(bool targetCar in new[]{false,true})
            {
                clean();var threat=g.SpawnReckless(truck);threat.model.localPosition=new Vector3(0,0,40);threat.previousPosition=new Vector3(-2,0,42);threat.motionVelocity=new Vector3(10,0,threat.speed);
                var target=g.SpawnTraffic(1,39,targetCar?0:1);target.model.localPosition=new Vector3(.8f,0,39);target.previousPosition=target.model.localPosition;target.speed=0;
                call("ResolveRecklessRams",null);
                check(target.knocked&&g.RecklessRamCount==1,"Reckless vehicle physically hits bike/car: "+truck+","+targetCar);
                check(Vector3.Dot(target.knockVelocity,new Vector3(10,0,threat.speed))>0,"Ram transfers momentum in the impact direction: "+truck+","+targetCar);
                check(!target.knockFromPlayer&&g.ChainCount==0&&!g.IsChased,"Unprovoked ram does not credit player or call police: "+truck+","+targetCar);
            }
            foreach(float side in new[]{-1f,1f})
            {
                clean();racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(side*9,0,30);
                call("CheckBuildingImpact",new object[]{racer});
                check(racer.wallCrashed&&racer.knocked&&racer.crashEffect.gameObject.activeSelf,"Building collision starts a wreck on either side: "+side);
                check(racer.smoke.isPlaying&&racer.fire.isPlaying,"Building impact emits smoke and flame: "+side);
                check(Vector3.Dot(racer.crashEffect.up,Vector3.up)>.999f,"Smoke rises upright regardless of fallen motorcycle rotation: "+side);
                g.TogglePause();check(racer.smoke.isPaused&&racer.fire.isPaused,"Pause freezes active fire effects: "+side);g.TogglePause();
                float worldZ=racer.model.localPosition.z+g.Distance;tick(.02f);
                check(Mathf.Abs(racer.model.localPosition.z+g.Distance-worldZ)<.001f,"Crashed vehicle remains fixed beside the scrolling house: "+side);
                call("RetireVehicle",new object[]{racer});
                check(!racer.active&&!racer.crashEffect.gameObject.activeSelf&&racer.smoke.particleCount==0,"Retiring a wreck clears particles and returns its model: "+side);
            }
            clean();racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(10,0,70);call("CheckBuildingImpact",new object[]{racer});
            check(!racer.wallCrashed,"Empty junctions are not treated as house walls");
            clean();racer=g.SpawnReckless(false);racer.model.localPosition=new Vector3(9,0,30);call("CheckBuildingImpact",new object[]{racer});g.ResetRide();
            check(g.RecklessEvents==0&&g.RecklessRamCount==0&&g.OncomingRecklessEvents==0,"Restart clears dangerous-driver counters without reinstating the old timer");
            check(g.vehicles.Where(v=>v.eventOnly).All(v=>!v.active&&!v.isReckless&&!v.wallCrashed&&!v.crashEffect.gameObject.activeSelf),"Restart clears every event and effect state");
            clean();for(int i=0;i<24;i++)check(g.SpawnTraffic(i%4,100+i*8)!=null,"Ordinary traffic pool remains available: "+i);
            check(g.vehicles.Where(v=>v.eventOnly).All(v=>!v.active),"Normal traffic never consumes special event models");
            return new {success=true,count=checks.Count,checks=checks};
        }
        finally
        {
            g.recklessTrafficEnabled=originalReckless;g.trafficSurprises=originalSurprises;
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);
            g.ResetRide();Time.timeScale=scale;
        }
    }
}



