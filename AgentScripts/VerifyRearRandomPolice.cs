using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;

public static class VerifyRearRandomPolice
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float scale=Time.timeScale;bool surprises=g.trafficSurprises,danger=g.recklessTrafficEnabled;
        var checks=new List<string>();
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        Action<string,float> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action<string,object[]> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
        Action<float> tick=dt=>call("TickRide",new object[]{dt});
        Action clean=()=>{g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);};
        Action chase=()=>
        {
            clean();tick(.02f);var target=g.SpawnTraffic(3,0,1);g.TryKick();call("RetireVehicle",new object[]{target});
            g.policeBike.localPosition=new Vector3(.45f,0,-10);g.policeBike.localRotation=Quaternion.identity;
            typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,10f);typeof(PhoVietGame).GetProperty("PoliceSpeed").SetValue(g,17f);
            typeof(PhoVietGame).GetField("policeArrived",F).SetValue(g,true);
        };
        Action<float> chaseStep=dt=>
        {
            foreach(var v in g.vehicles)if(v.active)
            {
                v.previousPosition=v.model.localPosition;
                Vector3 velocity=v.wallCrashed?Vector3.zero:v.knocked?v.knockVelocity:new Vector3(0,0,v.speed);
                v.model.localPosition+=(velocity-Vector3.forward*g.Speed)*dt;v.motionVelocity=velocity;
            }
            set("timeRiding",(float)typeof(PhoVietGame).GetField("timeRiding",F).GetValue(g)+dt);call("TickChase",new object[]{dt});
        };
        Func<int,float,bool,PhoVietGame.TrafficVehicle> obstacle=(kind,z,fallen)=>
        {
            int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
            var v=g.SpawnTraffic(2,z,index);v.speed=0;v.baseTrafficSpeed=0;v.motionVelocity=Vector3.zero;
            v.model.localPosition=new Vector3(.45f,0,z);v.previousPosition=v.model.localPosition;
            if(fallen){v.knocked=true;v.physicalCrash=true;v.knockVelocity=Vector3.zero;v.model.localRotation=Quaternion.Euler(0,0,-78);}
            return v;
        };
        try
        {
            Time.timeScale=0;
            var seeds=new HashSet<int>();var openings=new HashSet<string>();var models=new HashSet<int>();var frequencies=new List<float>();
            for(int i=0;i<24;i++)
            {
                g.ResetRide();seeds.Add(g.RunSeed);
                openings.Add(string.Join("|",g.vehicles.Where(v=>v.active).Select(v=>g.vehicles.IndexOf(v)+":"+v.lane+":"+v.model.localPosition.z.ToString("F3")+":"+v.speed)));
                foreach(var v in g.vehicles.Where(v=>v.active))models.Add(g.vehicles.IndexOf(v));
                var d=g.SpawnReckless(false);frequencies.Add(d.swerveFrequency);
            }
            check(seeds.Count==24,"Every restart receives a new non-repeating run seed");
            check(openings.Count==24,"Opening vehicle types, lanes, positions and speeds vary across restarts");
            check(models.Count>10,"Opening traffic draws from many models rather than fixed pool indices");
            check(frequencies.Max()-frequencies.Min()>.15f,"Dangerous drivers receive varied weave frequencies");
            var crossModels=new HashSet<int>();var turnChoices=new HashSet<int>();
            clean();for(int i=0;i<60;i++){var v=g.SpawnCrossTraffic(i%2==0?1:-1,70,true,8);crossModels.Add(g.vehicles.IndexOf(v));turnChoices.Add(v.turnChoice);call("RetireVehicle",new object[]{v});}
            check(crossModels.Count>=4,"Junctions randomly choose among eligible car and truck models");
            check(turnChoices.Count==3,"Junction traffic varies between left, right and straight routes");

            foreach(bool chased in new[]{false,true})foreach(bool truck in new[]{false,true})
            {
                if(chased)chase();else clean();
                if(chased)call("PositionCamera",new object[]{true});
                var v=g.SpawnReckless(truck);check(v!=null,"Rear overtaker can be created: "+chased+","+truck);
                float maxDepth=float.MinValue;
                foreach(var renderer in v.model.GetComponentsInChildren<Renderer>())
                {
                    Bounds bounds=renderer.bounds;
                    for(int ix=0;ix<2;ix++)for(int iy=0;iy<2;iy++)for(int iz=0;iz<2;iz++)
                    {
                        var p=new Vector3(ix==0?bounds.min.x:bounds.max.x,iy==0?bounds.min.y:bounds.max.y,iz==0?bounds.min.z:bounds.max.z);
                        maxDepth=Mathf.Max(maxDepth,g.rideCamera.transform.InverseTransformPoint(p).z);
                    }
                }
                check(maxDepth<0&&v.enteringFromBehind,"Entire model and overhead name start outside the rear camera plane: "+chased+","+truck);
                float startZ=v.model.localPosition.z;tick(.02f);
                check(v.active&&v.model.localPosition.z>startZ,"Rear vehicle moves upward and is not immediately culled behind the player: "+chased+","+truck);
                check(v.recklessAge==0&&Mathf.Abs(v.model.localPosition.x-v.rearStartX)<.01f,"No weaving or house crash begins while entering from offscreen: "+chased+","+truck);
                // Remove police for the rest of this appearance-only check, preserving the chase camera.
                typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,false);g.policeBike.gameObject.SetActive(false);set("collisionImmunity",10000);
                bool monotonic=true;float last=v.model.localPosition.z;int frames=0;
                while(v.enteringFromBehind&&frames++<400)
                {
                    tick(.02f);monotonic&=v.model.localPosition.z>=last-.0001f;last=v.model.localPosition.z;
                }
                check(v.active&&!v.enteringFromBehind&&monotonic&&frames<400,"Overtaker travels continuously from below screen to beside the rider: "+chased+","+truck);
                float x=v.model.localPosition.x;for(int j=0;j<25&&v.active&&!v.knocked;j++)tick(.02f);
                check(Mathf.Abs(v.model.localPosition.x-x)>.15f,"Weaving begins only after the overtaker has approached: "+chased+","+truck);
            }

            foreach(int kind in new[]{0,1,2})foreach(bool fallen in new[]{false,true})
            {
                chase();var v=obstacle(kind,4,fallen);float originalX=g.policeBike.localPosition.x;
                for(int i=0;i<95;i++)chaseStep(.02f);
                check(g.PoliceAvoidances>0&&g.PoliceFalls==0,"Police proactively avoid standing or fallen bike/car/truck: "+kind+","+fallen+" avoid="+g.PoliceAvoidances+" falls="+g.PoliceFalls+" police="+g.policeBike.localPosition+" obstacle="+v.model.localPosition);
                check(Mathf.Abs(g.policeBike.localPosition.x-originalX)>.5f||g.PoliceAvoidances>0,"Police steer around the obstacle rather than passing through it: "+kind+","+fallen);
            }
            foreach(bool truck in new[]{false,true})
            {
                chase();g.MoveLane(1);call("PositionCamera",new object[]{true});g.policeBike.localPosition=new Vector3(1.55f,0,-31.5f);
                var overtaker=g.SpawnReckless(truck);
                check(overtaker!=null&&Mathf.Abs(overtaker.model.localPosition.z-g.policeBike.localPosition.z)>overtaker.halfLength+1.2f,"New rear traffic cannot materialize inside the pursuing police motorcycle: "+truck);
                check(overtaker.swerveDirection>0,"Overtaker retains a valid intended weave direction even during a player lane transition: "+truck);
            }
            foreach(float trafficSpeed in new[]{7f,-10f})
            {
                chase();var moving=obstacle(1,4,false);moving.speed=trafficSpeed;moving.baseTrafficSpeed=trafficSpeed;
                moving.model.localRotation=Quaternion.Euler(0,trafficSpeed<0?180:0,0);
                for(int i=0;i<140;i++)chaseStep(.02f);
                check(g.PoliceAvoidances>0&&g.PoliceFalls==0,"Police predict and avoid moving traffic in either direction: "+trafficSpeed);
            }
            chase();var hit=obstacle(1,-9.9f,false);chaseStep(.02f);
            check(g.PoliceFallen&&g.PoliceFalls==1&&Mathf.Abs(g.PoliceFallRemaining-1.2f)<.0001f,"Unavoidable contact knocks police down for exactly 1.2 seconds");
            check(g.PoliceSpeed==0&&g.State==PhoVietGame.RideState.Riding,"Downed police stop pursuit movement without ending the ride");
            call("RetireVehicle",new object[]{hit});float gap=g.PoliceDistance;
            chaseStep(.6f);check(g.PoliceFallen&&g.PoliceDistance>gap&&Quaternion.Angle(g.policeBike.localRotation,Quaternion.identity)>60,"Player gains distance while the police motorcycle lies on the road");
            float remaining=g.PoliceFallRemaining;g.TogglePause();call("Update",null);
            check(g.PoliceFallRemaining==remaining,"Pause freezes police recovery timer");g.TogglePause();
            chaseStep(.59f);check(g.PoliceFallen&&g.PoliceFallRemaining>.005f,"Police cannot recover early before 1.2 seconds");
            chaseStep(.01f);check(!g.PoliceFallen&&g.PoliceFallRemaining==0&&Quaternion.Angle(g.policeBike.localRotation,Quaternion.identity)<.01f,"Police stand the motorcycle upright at 1.2 seconds");
            float recoveredZ=g.policeBike.localPosition.z;chaseStep(.02f);check(g.PoliceSpeed>0&&g.IsChased&&g.PoliceFalls==1,"Police resume the same pursuit after standing up");
            check(Mathf.Abs(g.policeBike.localPosition.z-recoveredZ)<1,"Recovery does not teleport police back to a fixed pursuit gap");
            chase();hit=obstacle(1,0,false);hit.speed=-100;hit.baseTrafficSpeed=-100;hit.model.localRotation=Quaternion.Euler(0,180,0);chaseStep(.2f);
            check(g.PoliceFallen,"Swept collision catches a fast vehicle crossing through the police path within one step");

            chase();hit=obstacle(2,-9.9f,true);chaseStep(.02f);call("RetireVehicle",new object[]{hit});
            g.policeBike.localPosition=new Vector3(g.rider.localPosition.x,0,-1);typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,1f);
            chaseStep(.02f);check(g.State==PhoVietGame.RideState.Riding&&g.PoliceFallen,"A fallen officer cannot arrest the rider even when very close");
            chase();g.recklessTrafficEnabled=true;g.trafficSurprises=true;set("spawnTimer",0);set("nextIntersectionDistance",70);set("collisionImmunity",10000);
            bool stable=true;
            for(int i=0;i<2000;i++)
            {
                tick(.02f);
                stable&=!float.IsNaN(g.PoliceDistance)&&!float.IsInfinity(g.policeBike.localPosition.sqrMagnitude);
                // Pursuit traffic collisions now immediately lose, even with a damage
                // cooldown injected by this test. Continue in a fresh chase after a loss.
                if(g.State==PhoVietGame.RideState.Arrested||g.State==PhoVietGame.RideState.Falling)
                {chase();g.recklessTrafficEnabled=true;g.trafficSurprises=true;set("spawnTimer",0);set("nextIntersectionDistance",70);set("collisionImmunity",10000);}
            }
            check(stable&&g.State==PhoVietGame.RideState.Riding&&g.IsChased,"Forty-second integrated chase remains stable through immediate collision losses and capture/restart with all hazards enabled");
            g.ResetRide();check(g.PoliceFalls==0&&g.PoliceAvoidances==0&&!g.PoliceFallen&&g.PoliceFallRemaining==0,"Restart clears police avoidance and recovery state");
            return new {success=true,count=checks.Count,checks};
        }
        finally
        {
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);
            g.trafficSurprises=surprises;g.recklessTrafficEnabled=danger;g.ResetRide();Time.timeScale=scale;
        }
    }
}
