using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyVehicleImpact21179
{
    const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static PhoVietGame game;
    static ParticleSystem shards;
    static int passed;

    static void Check(bool condition,string description)
    {
        if(!condition)throw new InvalidOperationException(description);
        passed++;
    }

    static object Call(string method,params object[] args)=>typeof(PhoVietGame).GetMethod(method,Instance).Invoke(game,args);
    static object Field(object target,string name)=>target.GetType().GetField(name,Instance).GetValue(target);
    static void Set(object target,string name,object value)=>target.GetType().GetField(name,Instance).SetValue(target,value);
    static Transform Root(object aircraft)=>(Transform)Field(aircraft,"root");

    static void Fresh()
    {
        game.RestartRide();
        foreach(var v in game.vehicles)
        {
            v.active=false;
            v.model.gameObject.SetActive(false);
        }
    }

    static void RoadBody(PhoVietGame.TrafficVehicle body,Vector3 from,Vector3 to,Vector3 velocity)
    {
        body.model.gameObject.SetActive(true);
        body.active=true;
        body.armored=body.knocked=body.wallCrashed=body.riverFalling=body.physicalCrash=false;
        body.model.localRotation=Quaternion.identity;
        body.previousPosition=from;
        body.model.localPosition=to;
        body.motionVelocity=velocity;
    }

    static object Helicopter(int slot,Vector3 position,Vector3 velocity)
    {
        var actor=((IList)Field(game,"helicopters"))[slot];
        Call("LaunchHelicopter",actor,position,false);
        Set(actor,"velocity",velocity);
        Set(actor,"shotRemaining",99f);
        return actor;
    }

    public static object Main()
    {
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(!Application.isPlaying||game==null)throw new InvalidOperationException("Play Mode with Phố Việt required.");
        float scale=Time.timeScale;
        bool originalSound=game.sound;
        passed=0;
        int slow=0,fast=0;
        try
        {
            Time.timeScale=0;
            game.sound=true;
            Call("EnsureVehicleImpactFx");
            shards=(ParticleSystem)Field(game,"vehicleShards");
            Check(shards!=null&&shards.main.maxParticles==320,"Shards use a bounded pool.");
            Check(shards.GetComponent<ParticleSystemRenderer>().mesh!=null,"Pool renders solid fragments.");
            foreach(float speed in new[]{2f,22f})
            {
                Fresh();
                var pair=game.vehicles.Where(v=>!v.eventOnly).Take(2).ToArray();
                float z=24,span=pair[0].halfLength+pair[1].halfLength;
                RoadBody(pair[0],new Vector3(0,0,z-span-.3f),new Vector3(0,0,z-span+.2f),Vector3.forward*speed);
                RoadBody(pair[1],new Vector3(0,0,z),new Vector3(0,0,z),Vector3.back*speed);
                Call("ResolveVehicleCollisions");
                int count=shards.particleCount;
                Check(count>=4&&pair.All(v=>v.knocked),"Confirmed road contact emits shards during the collision step.");
                Call("RenderContactFire");
                var fire=(ContactFireVisual)Field(game,"contactFire");
                Check(pair.All(v=>fire.CountFor(v.model)>0),
                    "Both contact fire attachments survive the burst.");
                Call("ResolveVehicleCollisions");
                Check(shards.particleCount==count,"Overlap on the next pass does not duplicate debris.");
                if(speed<3)slow=count;else fast=count;
            }
            Check(fast>slow,"Faster road impacts throw more fragments.");
            game.TogglePause();
            Check(shards.isPaused,"Debris simulation pauses.");
            game.TogglePause();
            Check(!shards.isPaused,"Debris resumes.");

            Fresh();
            var traffic=game.vehicles.First(v=>!v.eventOnly);
            var riderAt=game.rider.localPosition;
            float gap=traffic.halfLength+game.PlayerHalfLength+2;
            RoadBody(traffic,riderAt-Vector3.forward*gap,riderAt,Vector3.forward*24);
            Call("ResolveRiderContacts",riderAt.x,.05f);
            Check(game.State==PhoVietGame.RideState.Falling&&shards.particleCount>0,
                "A swept rear impact into the player immediately emits shards and starts the crash.");
            var contactFire=(ContactFireVisual)Field(game,"contactFire");
            Check(contactFire.CountFor(game.rider)>0&&contactFire.CountFor(traffic.model)>0,
                "Player and traffic both keep their contact fires.");

            Fresh();
            Check(shards.particleCount==0,"Restart clears existing shards.");
            var front=Helicopter(0,new Vector3(0,6,28),Vector3.forward*15);
            var back=Helicopter(1,new Vector3(0,6,36),Vector3.back*15);
            Call("TickHelicopter",.20f,game.rider.localPosition,false);
            Check(game.IsAircraftBurning(Root(front))&&game.IsAircraftBurning(Root(back))&&shards.particleCount>0,
                "Two helicopters meet, both catch fire, and debris appears.");
            int airCount=shards.particleCount;
            Call("TickHelicopter",.01f,game.rider.localPosition,false);
            Check(shards.particleCount==airCount,"Two wrecks do not duplicate the air collision.");

            Fresh();
            front=Helicopter(0,new Vector3(0,4,28),Vector3.forward*15);
            back=Helicopter(1,new Vector3(0,8,36),Vector3.back*15);
            Call("TickHelicopter",.2f,game.rider.localPosition,false);
            Check(!game.IsAircraftBurning(Root(front))&&!game.IsAircraftBurning(Root(back))&&shards.particleCount==0,
                "Aircraft crossing at separate altitudes do not collide.");

            Fresh();
            Check((bool)Call("LaunchSkywing",0,0),"Crimson Skywing asset is available.");
            var plane=((IList)Field(game,"skywings"))[0];
            Call("LaunchHelicopter",plane,new Vector3(0,7,30),false);
            front=Helicopter(0,new Vector3(0,7,22),Vector3.forward*20);
            Call("TickHelicopter",.2f,game.rider.localPosition,false);
            Check(game.IsAircraftBurning(Root(front))&&game.IsAircraftBurning(Root(plane))&&shards.particleCount>0,
                "A Crimson Skywing and helicopter can collide.");

            Fresh();
            var road=game.vehicles.First(v=>!v.eventOnly);
            RoadBody(road,new Vector3(0,0,30),new Vector3(0,0,30),Vector3.forward*.1f);
            front=Helicopter(0,new Vector3(0,5,30),Vector3.down*18);
            Call("KnockHelicopter",front,Vector3.zero,Vector3.forward);
            Set(front,"velocity",Vector3.down*18);
            Call("TickHelicopter",.23f,game.rider.localPosition,false);
            Check(road.knocked&&shards.particleCount>0&&((ContactFireVisual)Field(game,"contactFire")).CountFor(road.model)>0,
                "Falling aircraft hits, ignites and knocks road traffic.");

            Fresh();
            game.rider.localPosition=new Vector3(0,5,0);
            front=Helicopter(0,new Vector3(0,5.5f,6),Vector3.back*24);
            Call("TickHelicopter",.25f,game.rider.localPosition,true);
            Check(game.State==PhoVietGame.RideState.Falling&&game.RiderAirAttackBurning&&game.IsAircraftBurning(Root(front)),
                "Player–aircraft collision keeps the falling and burning outcomes.");
            Check(shards.particleCount>0,"Player–aircraft collision emits fragments immediately.");

            Fresh();
            int ammo=game.PlayerMissiles;
            Check(game.TryFirePlayerMissile()&&game.PlayerMissiles==ammo-1,"First missile consumes one round.");
            Call("TickPlayerMissiles",.249f);
            Check(!game.TryFirePlayerMissile()&&game.PlayerMissiles==ammo-1,"Next shot is rejected at 249 ms.");
            Call("TickPlayerMissiles",.001f);
            Check(game.TryFirePlayerMissile()&&game.PlayerMissiles==ammo-2,"Next shot succeeds at 250 ms.");
            Check(!game.TryFirePlayerMissile(),"Two inputs in the same step cannot double fire.");

            Fresh();
            var storm=(ThunderstormVisual)Field(game,"stormVisual");
            var voices=(AudioSource[])Field(storm,"thunderVoices");
            var point=new Vector3(-3,0,22);
            game.WarnLightning(point);
            Check(voices.All(v=>!v.isPlaying),"Warning is silent.");
            game.StrikeLightning(point);
            Check(voices[0].isPlaying&&voices[0].clip.length>2&&voices[0].transform.localPosition==point,
                "Thunder starts at the confirmed strike position and has a rumble tail.");
            Call("StrikeLightningAt",point+Vector3.right*4,1);
            Check(voices[1].isPlaying&&voices[1].volume<voices[0].volume,"Additional bolt has its own quieter spatial voice.");
            game.TogglePause();
            Call("RenderThunderstorm");
            Check(!voices[0].isPlaying&&!voices[1].isPlaying,"Pause freezes thunder.");
            game.TogglePause();
            Call("RenderThunderstorm");
            Check(voices[0].isPlaying&&voices[1].isPlaying,"Thunder resumes after pause.");
            game.sound=false;
            Call("RenderThunderstorm");
            Call("StrikeLightningAt",point,2);
            Check(voices.All(v=>v.mute)&&!voices[2].isPlaying,"Mute applies to active and new strikes.");
            game.ResetRide();
            Check(voices.All(v=>!v.isPlaying),"Reset stops all thunder voices.");
            return new {passed,slow,fast,thunderLength=voices[0].clip.length};
        }
        finally
        {
            game.sound=originalSound;
            game.ResetRide();
            Time.timeScale=scale;
        }
    }
}
