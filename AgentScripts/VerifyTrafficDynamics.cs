using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using PhoViet;

public static class VerifyTrafficDynamics
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool originalReckless=g.recklessTrafficEnabled;
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float scale=Time.timeScale;bool surprises=g.trafficSurprises;
        var checks=new List<string>();
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);checks.Add(name);};
        Action<string,float> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action<string,object[]> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
        Action<float> tick=dt=>call("TickRide",new object[]{dt});
        Action clean=()=>{g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);g.recklessTrafficEnabled=false;};
        Func<int,int,float,PhoVietGame.TrafficVehicle> spawn=(kind,lane,z)=>
        {
            int index=g.vehicles.FindIndex(v=>!v.eventOnly&&!v.active&&(kind==2?v.halfLength>3:kind==1?v.halfWidth>.7f&&v.halfLength<3&&!v.model.name.Contains("tải"):v.halfWidth<.7f));
            if(index<0)throw new Exception("No pool entry for kind="+kind);
            var v=g.SpawnTraffic(lane,z,index);v.brakeCooldown=10000;v.laneCooldown=10000;return v;
        };
        Action<PhoVietGame.TrafficVehicle,Vector3,float> place=(v,p,s)=>{v.model.localPosition=p;v.previousPosition=p;v.speed=s;v.baseTrafficSpeed=s;v.motionVelocity=Vector3.zero;v.model.localRotation=Quaternion.Euler(0,s<0?180:0,0);};
        Func<PhoVietGame.TrafficVehicle,Vector3> velocity=v=>v.knocked?v.knockVelocity:new Vector3(0,0,v.speed);
        Func<PhoVietGame.TrafficVehicle,float> energy=v=>.5f*PhoVietGame.VehicleMass(v)*velocity(v).sqrMagnitude;
        try
        {
            Time.timeScale=0;g.trafficSurprises=true;
            check(g.vehicles.Count(v=>!v.eventOnly&&v.halfLength>3)==2,"Saved scene includes two ordinary medium trucks");
            foreach(int kind in new[]{0,1,2})
            {
                clean();var v=spawn(kind,3,25);float old=v.speed;
                check(g.BeginSuddenBrake(v)&&v.brakeLights.gameObject.activeSelf,"Sudden brake lights activate for every vehicle class: "+kind);
                call("TickLongitudinalVehicle",new object[]{v,.2f});
                check(v.speed<old*.5f&&v.speed>=0,"Braking sharply reduces speed without reversing: "+kind);
                for(int i=0;i<120;i++)call("TickLongitudinalVehicle",new object[]{v,.02f});
                check(v.brakeRemaining==0&&!v.brakeLights.gameObject.activeSelf&&v.speed>old*.5f,"Brake release restores speed and clears lamps: "+kind);
                clean();v=spawn(kind,3,0);check(g.BeginSuddenLaneChange(v,2),"Parallel lane change accepts bike, car and truck: "+kind);
                call("TickLongitudinalVehicle",new object[]{v,.12f});
                check(v.model.localPosition.x<4.65f&&v.model.localPosition.x>1.55f,"Lane change travels continuously instead of teleporting: "+kind);
                check(Quaternion.Angle(v.model.localRotation,Quaternion.identity)>2,"Vehicle points into its lane change: "+kind);
                for(int i=0;i<60;i++)call("TickLongitudinalVehicle",new object[]{v,.02f});
                check(!v.changingLane&&Mathf.Abs(v.model.localPosition.x-1.55f)<.001f&&v.lane==2,"Lane change finishes in target lane: "+kind);
                clean();v=spawn(kind,1,20);check(!g.BeginSuddenBrake(v)&&!g.BeginSuddenLaneChange(v,2),"Oncoming ordinary traffic does not use same-direction maneuvers: "+kind);
                clean();v=spawn(kind,3,1);v.brakeCooldown=0;v.laneCooldown=0;
                for(int i=0;i<800&&(g.SuddenBrakes==0||g.SuddenLaneChanges==0);i++)
                {v.model.localPosition=new Vector3(v.model.localPosition.x,0,1);call("TickLongitudinalVehicle",new object[]{v,.02f});}
                check(g.SuddenBrakes>0&&g.SuddenLaneChanges>0,"Natural driver scheduler triggers both hazards near rider: "+kind);
            }
            clean();var paused=spawn(1,3,25);g.BeginSuddenBrake(paused);g.BeginSuddenLaneChange(paused,2);
            float brake=paused.brakeRemaining,progress=paused.changeProgress;g.TogglePause();call("Update",null);
            check(paused.brakeRemaining==brake&&paused.changeProgress==progress,"Pause freezes brakes and lane-change progress");
            clean();paused=spawn(0,3,0);paused.speed=g.Speed-.5f;paused.baseTrafficSpeed=paused.speed;g.BeginSuddenLaneChange(paused,2);for(int i=0;i<40&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
            check(g.State==PhoVietGame.RideState.Falling,"Ignoring a motorcycle cut-in causes a player collision");

            foreach(int direction in new[]{-1,1})foreach(int choice in new[]{-1,1})foreach(bool car in new[]{false,true})
            {
                clean();var v=g.SpawnCrossTraffic(direction,70,car,8);v.turnChoice=choice;float largestStep=0;Vector3 before=v.model.localPosition;
                for(int i=0;i<250&&v.crossing;i++)
                {
                    call("TickIntersectionVehicle",new object[]{v,.02f});largestStep=Mathf.Max(largestStep,(v.model.localPosition-before).magnitude);before=v.model.localPosition;
                }
                int sign=-direction*choice;
                check(v.hasTurned&&!v.crossing&&v.speed*sign>0,"Cross traffic completes each left/right turn from both sides: "+direction+","+choice+","+car);
                check(largestStep<.6f,"Turning motion stays continuous along a road arc: "+direction+","+choice+","+car);
                check(Mathf.Abs(v.model.localPosition.x-PhoVietGame.LaneX[v.lane])<.001f&&(sign>0?v.lane>=2:v.lane<2),"Turn exits into correct directional lane: "+direction+","+choice+","+car);
            }
            clean();var waiting=g.SpawnCrossTraffic(1,70,false,8,1);waiting.turnChoice=1;call("TickIntersectionVehicle",new object[]{waiting,.2f});
            check(!waiting.turning&&waiting.model.localPosition.x==-16,"Waiting crossing vehicle cannot turn before release");
            clean();var straight=g.SpawnCrossTraffic(-1,70,true,8);straight.turnChoice=0;for(int i=0;i<200;i++)call("TickIntersectionVehicle",new object[]{straight,.02f});
            check(!straight.hasTurned&&Mathf.Abs(straight.model.localPosition.x+16)<.01f,"Some junction traffic still continues straight");

            // Frontal impact: same mass, momentum conserved and energy dissipated, no lateral random force.
            clean();var a=spawn(1,2,40);var b=spawn(1,2,44.7f);place(a,new Vector3(1.55f,0,40),16);place(b,new Vector3(1.55f,0,44.7f),-16);
            float ma=PhoVietGame.VehicleMass(a),mb=PhoVietGame.VehicleMass(b),beforeEnergy=energy(a)+energy(b);
            Vector3 momentum=velocity(a)*ma+velocity(b)*mb;call("ResolveVehicleCollisions",null);
            check(a.knocked&&b.knocked&&g.TrafficCollisions==1,"Two untouched normal vehicles collide with each other");
            check((a.knockVelocity*ma+b.knockVelocity*mb-momentum).xzMagnitude()<.1f,"Head-on collision conserves horizontal momentum");
            check(energy(a)+energy(b)<beforeEnergy,"Restitution dissipates collision energy");
            check(Mathf.Abs(a.knockVelocity.x)<.001f&&Mathf.Abs(b.knockVelocity.x)<.001f,"Centred frontal impact does not invent sideways force");
            check(a.knockVelocity.z<0&&b.knockVelocity.z>0,"Equal opposing cars rebound in the correct directions");
            float afterEnergy=energy(a)+energy(b);call("ResolveVehicleCollisions",null);check(g.TrafficCollisions==1&&energy(a)+energy(b)==afterEnergy,"Persistent contact cannot repeatedly inject impulses");
            check(!g.IsChased&&g.KickCount==0&&g.ChainCount==0,"Automatic traffic collisions do not blame player or summon police");

            clean();a=spawn(1,2,40);b=spawn(1,2,44.7f);place(a,new Vector3(1.55f,0,40),20);place(b,new Vector3(1.55f,0,44.7f),2);call("ResolveVehicleCollisions",null);
            check(a.knocked&&b.knocked&&a.knockVelocity.z<20&&b.knockVelocity.z>2,"Rear-end impact slows follower and pushes braked leader forward");
            clean();a=spawn(2,2,40);b=spawn(0,2,44.3f);place(a,new Vector3(1.55f,0,40),16);place(b,new Vector3(1.55f,0,44.3f),-12);call("ResolveVehicleCollisions",null);
            check(a.knocked&&b.knocked&&Mathf.Abs(a.knockVelocity.z-16)<Mathf.Abs(b.knockVelocity.z+12),"Heavy truck changes velocity less than the motorcycle it hits");
            check(a.knockVelocity.y<1&&b.knockVelocity.y<=2.8f,"A motorcycle cannot launch a medium truck into the sky");

            clean();a=spawn(1,2,50);b=spawn(0,3,50);place(a,new Vector3(0,0,50),0);place(b,new Vector3(1,0,50),0);a.motionVelocity=Vector3.right*10;b.motionVelocity=Vector3.zero;call("ResolveVehicleCollisions",null);
            check(b.knocked&&b.knockVelocity.x>0&&a.knockVelocity.x<10,"Side impact pushes target along impact direction");
            clean();a=spawn(1,2,50);b=spawn(1,2,52);place(a,new Vector3(0,0,50),0);place(b,new Vector3(1.8f,0,52),0);a.motionVelocity=Vector3.right*10;
            beforeEnergy=.5f*PhoVietGame.VehicleMass(a)*100;call("ResolveVehicleCollisions",null);
            float ia=PhoVietGame.VehicleMass(a)*(4*a.halfLength*a.halfLength+4*a.halfWidth*a.halfWidth)/12;
            float ib=PhoVietGame.VehicleMass(b)*(4*b.halfLength*b.halfLength+4*b.halfWidth*b.halfWidth)/12;
            float spinEnergy=.5f*ia*Mathf.Pow(a.angularVelocity.y*Mathf.Deg2Rad,2)+.5f*ib*Mathf.Pow(b.angularVelocity.y*Mathf.Deg2Rad,2);
            check(Mathf.Abs(a.angularVelocity.y)>1&&Mathf.Abs(b.angularVelocity.y)>1,"Off-centre collision generates angular motion from the contact lever");
            check(energy(a)+energy(b)+spinEnergy<=beforeEnergy+1,"Glancing collision does not add horizontal and yaw kinetic energy");
            clean();a=spawn(1,2,50);b=spawn(1,2,50);place(a,new Vector3(0,0,60),30);a.previousPosition=new Vector3(0,0,40);place(b,new Vector3(0,0,50),0);call("ResolveVehicleCollisions",null);
            check(a.knocked&&b.knocked&&a.model.localPosition.z<=b.model.localPosition.z,"Swept collision catches high-speed tunnelling and restores contact positions");
            clean();a=spawn(1,2,50);b=spawn(1,2,50);place(a,new Vector3(0,0,50),20);place(b,new Vector3(0,5,50),-20);call("ResolveVehicleCollisions",null);
            check(!a.knocked&&!b.knocked,"Airborne vehicle passing above traffic does not cause phantom contact");
            clean();a=spawn(0,2,50);b=spawn(0,3,50);place(a,new Vector3(-2,0,50),10);place(b,new Vector3(2,0,50),10);call("ResolveVehicleCollisions",null);
            check(!a.knocked&&!b.knocked,"Separated parallel traffic has no false collision");
            clean();a=spawn(1,2,50);b=spawn(1,2,50);place(a,new Vector3(0,0,50),10);place(b,new Vector3(0,0,54.7f),-10);call("ResolveVehicleCollisions",null);
            for(int i=0;i<1000;i++)call("TickCollisionBody",new object[]{a,.02f});
            check(new Vector2(a.knockVelocity.x,a.knockVelocity.z).magnitude<.01f&&a.model.localPosition.y>=0,"Friction brings a wreck to rest without falling below the road");
            check(a.angularVelocity.magnitude<.01f,"Collision spin damps instead of accelerating forever");
            clean();a=spawn(1,3,30);int reusedIndex=g.vehicles.IndexOf(a);a.knockFromPlayer=true;a.model.localPosition=new Vector3(9,0,30);call("CheckBuildingImpact",new object[]{a});call("RetireVehicle",new object[]{a});
            var reused=g.SpawnTraffic(3,60,reusedIndex);
            check(reused==a&&!reused.wallCrashed&&!reused.knocked&&!reused.knockFromPlayer&&!reused.physicalCrash,"Reusing a crashed vehicle clears wall lock and player blame");
            float reusedZ=reused.model.localPosition.z;tick(.02f);check(reused.model.localPosition.z<reusedZ&&reused.speed>0&&!reused.knocked,"A recycled wreck drives normally when it reappears");
            g.RestartRide();g.recklessTrafficEnabled=true;set("collisionImmunity",10000);
            for(int i=0;i<4500;i++)tick(.02f);
            check(g.State==PhoVietGame.RideState.Riding&&g.Distance>2000,"All traffic systems remain stable through a ninety-second integrated run");
            check(g.SuddenBrakes>5&&g.SuddenLaneChanges>5&&g.IntersectionTurns>5&&g.TrafficCollisions>5,"Integrated run naturally produces braking, lane changes, turns and vehicle collisions");
            check(g.RecklessEvents>=4&&g.vehicles.Any(v=>v.active&&!v.knocked),"Reckless schedule and normal pool continue working amid traffic chaos");
            check(g.vehicles.All(v=>!float.IsNaN(v.model.localPosition.x)&&!float.IsNaN(v.model.localPosition.z)&&!float.IsInfinity(v.knockVelocity.sqrMagnitude)),"No non-finite positions or unstable collision velocities");
            g.ResetRide();check(g.SuddenBrakes==0&&g.SuddenLaneChanges==0&&g.IntersectionTurns==0&&g.TrafficCollisions==0,"Restart clears maneuver and collision counters");
            check(g.vehicles.All(v=>!v.changingLane&&!v.turning&&!v.physicalCrash&&v.brakeRemaining==0&&!v.brakeLights.gameObject.activeSelf),"Restart clears driver motion, physics and brake lamps");
            return new {success=true,count=checks.Count,checks};
        }
        finally
        {
            g.recklessTrafficEnabled=originalReckless;g.trafficSurprises=surprises;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);g.ResetRide();Time.timeScale=scale;
        }
    }
    static float xzMagnitude(this Vector3 v)=>new Vector2(v.x,v.z).magnitude;
}

