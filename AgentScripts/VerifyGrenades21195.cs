using System;
using System.Collections;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyGrenades21195
{
    const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    const float Step=.02f;
    static PhoVietGame game;
    static int passed;
    static void Check(bool ok,string message)
    {
        if(!ok)throw new InvalidOperationException(message);
        passed++;
    }
    static object Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,Instance).Invoke(game,args);
    static object Read(object owner,string name)=>owner.GetType().GetField(name,Instance).GetValue(owner);
    static void Set(string name,object value)=>typeof(PhoVietGame).GetField(name,Instance).SetValue(game,value);
    static void Speed(float value)=>typeof(PhoVietGame).GetProperty("Speed",Instance).SetValue(game,value);
    static IList Grenades=>(IList)Read(game,"grenades");
    static float GroundSpeed(Vector3 velocity)=>new Vector2(velocity.x,velocity.z).magnitude;

    static void Restart()
    {
        game.RestartRide();
        foreach(var v in game.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        Set("spawnTimer",999f);
        Set("nextIntersectionDistance",float.PositiveInfinity);
    }

    static PhoVietGame.TrafficVehicle Vehicle(int lane,float z,bool car)
    {
        int index=game.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(v.halfWidth>.7f)==car);
        var vehicle=game.SpawnTraffic(lane,z,index);
        if(vehicle==null)throw new InvalidOperationException("Could not spawn a traffic fixture.");
        return vehicle;
    }

    static void CheckControlsAndCraters()
    {
        Restart();
        var tap=new Vector2(Screen.width*.5f,Screen.height*.5f);
        Check(!game.RegisterTap(tap,2)&&game.RegisterTap(tap,2.18f),"Double tap dispatches the new action.");
        Check(Grenades.Count==2&&game.KickCount==0,"Two grenades are thrown without a motorcycle target.");
        Vector3 left=(Vector3)Read(Grenades[0],"target"),right=(Vector3)Read(Grenades[1],"target");
        Check(left.x<game.rider.localPosition.x&&right.x>game.rider.localPosition.x&&left.z<0&&right.z<0,"Both sides land behind the car.");
        game.TogglePause();Call("TickGrenades",.7f);
        Check(!game.TryKick()&&Grenades.Count==2&&game.ActiveCraterCount==0,"Pause freezes the action.");
        game.TogglePause();Call("TickGrenades",.49f);Call("UpdateTankCraterTerrain",.2f);
        Check(Grenades.Count==0&&game.ActiveCraterCount==2,"The pair detonates into two holes on the road.");
        foreach(var c in game.tankCraters)
            Check(c.grenade&&Mathf.Approximately(c.radius,1.15f)&&Mathf.Approximately(c.model.localScale.x,1.15f),"The grenade hole uses its smaller visual and physical radius.");
        var tank=(PhoVietGame.TankCrater)game.CreateTankCrater(left);
        Check(tank!=null&&!tank.grenade&&tank.radius>PhoVietGame.GrenadeCraterRadius,"A tank shell can make its original larger hole at the same spot.");
        Check(!game.TryKick(),"A second throw is blocked during cooldown.");
        Call("TickActionTimers",1.99f);Check(!game.TryKick(),"Cooldown does not expire early.");
        Call("TickActionTimers",.01f);Check(game.TryKick(),"Cooldown expires at two seconds.");
        Restart();Check(Grenades.Count==0&&game.ActiveCraterCount==0&&game.TryKick(),"Retry clears projectiles, holes and cooldown.");
        Restart();Set("energyHover",true);Set("jumpFlying",true);Set("jumpHeight",4.5f);
        Check(game.TryKick()&&game.RiderRolling&&Grenades.Count==0,"Q still rolls in hover mode.");
    }

    static void CheckTrafficAndPolice()
    {
        foreach(bool police in new[]{false,true})
        {
            Restart();Set("timeRiding",10f);Set("cruiseSpeed",20f);
            float x=police?game.rider.localPosition.x-1.1f:PhoVietGame.LaneX[2];
            float z=police?-20:20;
            var c=(PhoVietGame.TankCrater)Call("CreateCrater",new Vector3(x,0,z),PhoVietGame.GrenadeCraterRadius,true);
            Check(c!=null,"Road fixture supports a grenade crater.");
            var v=police?(PhoVietGame.TrafficVehicle)Read(game,"policeBody"):Vehicle(2,z-2,true);
            v.model.localPosition=new Vector3(x,0,z-1.8f);v.speed=v.baseTrafficSpeed=20;
            string tick=police?"TickPoliceDriving":"TickTrafficMotion";
            for(int i=0;i<4&&v.lastGrenadeCrater==0;i++)Call(tick,Step);
            Check(v.lastGrenadeCrater==c.id&&v.grenadeSlowTime>1.1f,"Actual wheel-path sweep detects the hole.");
            Vector3 before=v.model.localPosition;
            Call(tick,.1f);
            float slowed=GroundSpeed((v.model.localPosition-before)/.1f);
            Check(slowed<(police?PhoVietGame.VehicleSpeedLimit:20)*.8f&&slowed>0,"The vehicle loses speed for a short time.");
            Check(Mathf.Abs(v.model.localPosition.x-before.x)>.015f&&!v.knocked&&v.model.localPosition.y==0,"The vehicle veers slightly and stays on the ground.");
            for(int i=0;i<15;i++)Call(tick,.1f);
            Check(v.grenadeSlowTime==0&&v.grenadeYaw==0,"Direction and drive speed recover without repeated slowdown.");
        }
        Restart();
        Vector3 target=new Vector3(1.55f,0,20);
        var car=Vehicle(2,20,true);
        var cruiser=(PhoVietGame.TrafficVehicle)Read(game,"policeBody");cruiser.model.localPosition=target+Vector3.right*.6f;
        game.rider.localPosition=target;
        Call("EnsureTankFx");Call("DetonateGrenade",target);
        Check(car.grenadeSlowTime>0&&cruiser.grenadeSlowTime>0&&!car.knocked&&!game.PoliceFallen,"The blast hinders nearby traffic and the patrol car without launching them.");
        Check(game.State==PhoVietGame.RideState.Riding&&game.IsChased,"The player's own blast does not end the run or cancel pursuit.");
    }

    static void CheckSpeedLimit()
    {
        float max=PhoVietGame.VehicleSpeedLimit;
        Check(Mathf.Abs(max*3.6f-110)<.001f,"The shared limit converts to 110 km/h.");
        Restart();Speed(80);Check(Mathf.Approximately(game.Speed,max),"Even direct player speed writes respect the cap.");
        Set("timeRiding",90f);Set("chaseBoost",12f);
        for(int i=0;i<20;i++)Call("TickRide",Step);
        Check(game.State==PhoVietGame.RideState.Riding&&Mathf.Approximately(game.Speed,max),"A long chase reaches but does not exceed the cap.");
        game.MoveLane(1);
        for(int i=0;i<45;i++)
        {
            float x=game.rider.localPosition.x,metres=game.Distance;
            Call("TickRide",Step);
            Check(new Vector2(game.rider.localPosition.x-x,game.Distance-metres).magnitude<=max*Step+.0002f,"Player lane-change ground velocity respects the cap.");
        }
        Check(game.rider.localPosition.x>PhoVietGame.LaneX[2]+2.8f,"Changing lanes still completes at the cap.");
        Check(game.TryKick(),"The car can throw from the outside lane at full speed.");
        for(int i=0;i<25;i++)Call("TickRide",Step);
        Check(Grenades.Count==0&&game.ActiveCraterCount==2,"Both grenades land while the road scrolls at full speed.");
        foreach(var crater in game.tankCraters)
        {
            Vector3 position=game.transform.InverseTransformPoint(crater.model.position);
            Check(position.z<game.rider.localPosition.z-4&&position.z>game.rider.localPosition.z-9,"Full-speed detonation remains close behind the player.");
        }

        Restart();Set("timeRiding",90f);Set("cruiseSpeed",60f);
        foreach(int lane in new[]{0,2})
        {
            var car=Vehicle(lane,30,true);
            car.baseTrafficSpeed=car.speed=lane==0?-60:60;
            Call("TickTrafficMotion",Step);
            Check(GroundSpeed(car.motionVelocity)<=max+.001f&&GroundSpeed(car.motionVelocity)>max-.01f,"Oncoming and overtaking cars are capped in world motion.");
            car.active=false;
        }
        var detached=Vehicle(1,30,false);
        detached.routeDetached=true;detached.routeVelocity=new Vector3(70,0,60);
        Call("TickTrafficMotion",Step);
        Check(GroundSpeed(detached.motionVelocity)<=max+.001f&&GroundSpeed(detached.motionVelocity)>max-.01f,"Detached route motion uses a planar cap.");
        detached.active=false;
        var police=game.policeBike.localPosition;
        Call("TickPoliceDriving",Step);
        Check(game.PoliceSpeed<=max&&GroundSpeed((game.policeBike.localPosition-police)/Step)<=max+.001f,"Police speed and displacement respect the cap.");
    }

    public static object Main()
    {
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(!Application.isPlaying||game==null)throw new InvalidOperationException("Open Phố Việt in Play Mode.");
        float oldScale=Time.timeScale;
        bool rivers=game.riversEnabled,tanks=game.tankBarrageEnabled,dinos=game.dinosaursEnabled,surprises=game.trafficSurprises;
        bool reckless=game.recklessTrafficEnabled,exits=game.turnExitTrafficEnabled;
        try
        {
            Time.timeScale=0;passed=0;
            game.riversEnabled=game.tankBarrageEnabled=game.dinosaursEnabled=game.trafficSurprises=game.recklessTrafficEnabled=game.turnExitTrafficEnabled=false;
            CheckControlsAndCraters();CheckTrafficAndPolice();CheckSpeedLimit();
            return new {passed,limitKmh=110,grenadeRadius=1.15,tankRadius=PhoVietGame.TankCraterRadius};
        }
        finally
        {
            game.riversEnabled=rivers;game.tankBarrageEnabled=tanks;game.dinosaursEnabled=dinos;game.trafficSurprises=surprises;
            game.recklessTrafficEnabled=reckless;game.turnExitTrafficEnabled=exits;Time.timeScale=oldScale;game.ResetRide();
        }
    }
}
