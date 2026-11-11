using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyPursuit21182
{
    const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static PhoVietGame game;
    static int passed;

    static object Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,Instance).Invoke(game,args);
    static object Field(string name)=>typeof(PhoVietGame).GetField(name,Instance).GetValue(game);
    static object Field(object owner,string name)=>owner.GetType().GetField(name,Instance).GetValue(owner);
    static void Check(bool condition,string description)
    {
        if(!condition)throw new InvalidOperationException(description);
        passed++;
    }

    static void ClearTraffic()
    {
        foreach(var v in game.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
    }

    static void FreshRun()
    {
        game.RestartRide();
        ClearTraffic();
        Check(game.IsChased&&game.policeBike.gameObject.activeSelf&&game.Speed==0&&game.PoliceDistance==38,
            "Restart activates the officer and preserves a stationary start.");
    }

    static void RearContact(bool bike,float closing)
    {
        var v=game.vehicles.First(candidate=>!candidate.eventOnly&&(candidate.halfWidth<=.7f)==bike);
        var p=game.rider.localPosition;
        v.active=true;v.armored=false;v.knocked=false;v.wallCrashed=false;v.model.gameObject.SetActive(true);
        v.previousYaw=0;v.model.localRotation=Quaternion.identity;
        float clearance=v.halfLength+(float)typeof(PhoVietGame).GetProperty("PlayerHalfLength",Instance).GetValue(game);
        v.previousPosition=p-Vector3.forward*(clearance+.5f);
        v.model.localPosition=p-Vector3.forward*(clearance-.3f);
        v.motionVelocity=Vector3.forward*(18+closing);
        typeof(PhoVietGame).GetProperty("Speed",Instance).SetValue(game,18f);
        bool riding=(bool)Call("ResolveRiderContacts",p.x,.05f);
        if(bike)
            Check(riding&&game.Speed<18&&game.Speed>0&&game.rider.localPosition.y==p.y&&
                  Mathf.Abs(game.rider.localPosition.x-p.x)>.1f&&Quaternion.Angle(game.rider.localRotation,Quaternion.identity)>3,
                  "Bike contact steers and slows the player without throwing the vehicle.");
        else Check(!riding&&game.State==PhoVietGame.RideState.Falling&&game.CrashDuringPursuit,"A car crash awaits arrest.");
        ClearTraffic();
    }

    static void FinishCrash(PhoVietGame.RideState expected)
    {
        for(int i=0;i<250&&game.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",.04f);
        Check(game.State==expected,$"Expected {expected} after crash, got {game.State}, police gap {game.PoliceDistance}.");
    }

    public static object Main()
    {
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(!Application.isPlaying||game==null)throw new InvalidOperationException("Open Phố Việt in Play Mode.");
        float scale=Time.timeScale;
        bool rivers=game.riversEnabled;
        try
        {
            Time.timeScale=0;game.riversEnabled=false;passed=0;
            FreshRun();VerifyCruiser();
            FreshRun();RearContact(true,8);float weak=game.Speed;
            FreshRun();RearContact(true,32);float strong=game.Speed;
            Check(strong<weak&&game.IsChased,"Faster motorcycle impacts cause a larger speed loss during the chase.");
            game.policeBike.localPosition=game.rider.localPosition-Vector3.forward*1.7f;
            Call("TickPoliceDriving",.01f);
            Check(game.State==PhoVietGame.RideState.Arrested,"The officer catches a slow player in reach.");
            Call("TickArrest",1.2f);
            var wide=(Texture2D)Field("jailWide");var tall=(Texture2D)Field("jailTall");
            Check(game.State==PhoVietGame.RideState.Jailed&&wide!=null&&tall!=null&&wide.width>wide.height&&tall.width<tall.height,
                "The jail shows the authored landscape and portrait resources.");

            FreshRun();RearContact(false,12);game.Arrest();
            Check(game.State==PhoVietGame.RideState.Falling,"Arrest waits for the crash animation.");
            game.StrikeLightning(game.policeBike.localPosition);
            Check(game.IsChased&&game.CrashDuringPursuit&&!game.PoliceFallen,"Lightning cannot interrupt an officer approaching a crashed player.");
            FinishCrash(PhoVietGame.RideState.Arrested);

            FreshRun();Call("BlastRider",game.rider.localPosition);
            Check(!game.IsChased&&!game.CrashDuringPursuit,"A tank shell bypasses the jail.");
            FinishCrash(PhoVietGame.RideState.GameOver);

            var source=typeof(PhoVietGame).GetNestedType("EnemyMissileSource",BindingFlags.NonPublic);
            foreach(var name in new[]{"Tank","Helicopter","Skywing","None"})
            {
                FreshRun();Call("BurnAirAttackRider",game.rider.localPosition+Vector3.forward,Enum.Parse(source,name),false);
                bool body=name=="None";
                Check(game.CrashDuringPursuit==body,"Aircraft body crashes and enemy missiles follow their separate rules.");
                FinishCrash(body?PhoVietGame.RideState.Arrested:PhoVietGame.RideState.GameOver);
            }

            FreshRun();var dinosaur=game.SpawnDinosaur();
            Check(dinosaur!=null,"Dinosaur spawns.");
            var center=game.rider.localPosition;
            for(int z=-6;z<=6&&!dinosaur.hitRider;z++)
            {
                dinosaur.root.localPosition=center+Vector3.forward*z;
                Call("ResolveDinosaurRiderHit",dinosaur,1f);
            }
            Check(dinosaur.hitRider&&!game.CrashDuringPursuit,"A dinosaur hit bypasses jail.");
            FinishCrash(PhoVietGame.RideState.GameOver);
            return new {passed,weakSpeed=weak,strongSpeed=strong,wide=$"{wide.width}x{wide.height}",tall=$"{tall.width}x{tall.height}"};
        }
        finally{game.riversEnabled=rivers;game.ResetRide();Time.timeScale=scale;}
    }

    static void VerifyCruiser()
    {
        var police=game.policeBike;
        var cruiser=police.Find("Vietnam Traffic Police Cruiser");
        Check(cruiser!=null&&police.GetComponent<BikeVisualAnimator>()==null,"The new Meshy sedan occupies the chase slot.");
        var body=cruiser.GetComponentInChildren<MeshRenderer>();
        var size=police.InverseTransformVector(body.bounds.size);
        var playerSize=game.rider.GetComponent<PlayerCarVisual>().size;
        float scale=PhoVietGame.PoliceCruiserScale;
        Check(Mathf.Abs(size.x-1.83f*scale)<.03f&&Mathf.Abs(size.y-1.60f*scale)<.03f&&Mathf.Abs(size.z-playerSize.z)<.03f,
              $"The new cruiser stands on its wheels and faces along the road. Actual bounds: {size:F3}.");
        var livery=body.sharedMaterial.mainTexture;
        Check(livery.name=="PV_PoliceLivery"&&livery.width==2048,"The cruiser uses the corrected 2K side lettering atlas.");
        var lamps=game.daylight.vehicles.Single(v=>v.root==police);
        Check(lamps.halos.Count(h=>!h.rear)==2&&lamps.halos.Count(h=>h.rear)==2,"The sedan has paired lamps.");
        Check(Mathf.Abs(lamps.head.localPosition.z-2f*scale)<.01f&&Mathf.Abs(game.redBeacon.transform.localPosition.y-1.595f*scale)<.01f,
              "The front lamps and roof beacon match the scaled model.");

        Vector3 before=police.localPosition;
        int strikes=game.LightningStrikes;
        game.StrikeLightning(before);
        Check(game.LightningStrikes==strikes+1&&game.IsChased&&!game.PoliceFallen&&!game.PoliceLightningWreck&&police.localPosition==before,
              "A direct lightning strike leaves the patrol car unaffected.");

        var tank=game.SpawnScheduledTank();
        Check(tank!=null&&Call("LaunchTankShell",tank,2,before,false)!=null,"A tank shell is fired at the cruiser.");
        Call("TickShells",3f);
        Check(game.ShellBlasts==1&&game.IsChased&&!game.PoliceFallen&&police.localPosition==before,"Tank shell detonates without affecting the cruiser.");
        ClearTraffic();

        var source=typeof(PhoVietGame).GetNestedType("EnemyMissileSource",BindingFlags.NonPublic);
        foreach(var name in new[]{"Tank","Helicopter","Skywing"})
        {
            Call("ResetHelicopterMissiles");
            Vector3 origin=before+Vector3.back*4+Vector3.up*.7f;
            Check((bool)Call("FireEnemyMissile",origin,Enum.Parse(source,name)),"Enemy missile is fired along the cruiser route.");
            var shot=((IList)Field("helicopterMissiles")).Cast<object>().Single(m=>(bool)Field(m,"active"));
            Call("TickHelicopterMissiles",.22f,game.rider.localPosition,true);
            Check((bool)Field(shot,"active")&&((Transform)Field(shot,"root")).localPosition.z>before.z+2&&game.IsChased&&!game.PoliceFallen,
                  "A "+name+" missile sweeps through the cruiser without blocking it.");
        }
        Call("ResetHelicopterMissiles");

        police.localPosition=new Vector3(PhoVietGame.LaneX[2],0,-24);
        var obstacle=game.vehicles.First(v=>!v.eventOnly&&v.halfWidth>.7f);
        float originalWidth=obstacle.halfWidth,originalLength=obstacle.halfLength;
        obstacle.halfWidth=.94f;obstacle.halfLength=2.15f;
        obstacle.active=true;obstacle.model.gameObject.SetActive(true);
        obstacle.model.localPosition=obstacle.previousPosition=police.localPosition+Vector3.forward*14.4f;
        obstacle.model.localRotation=Quaternion.identity;obstacle.previousYaw=0;obstacle.speed=0;obstacle.motionVelocity=Vector3.zero;
        obstacle.knocked=false;obstacle.wallCrashed=false;obstacle.crossing=false;obstacle.routeDetached=false;
        Check(!(bool)Call("PolicePathClear",police.localPosition,police.localPosition.x,15f,.75f),"A normal vehicle blocks the direct chase route.");
        object[] choice={police.localPosition,15f,0f};
        Check((bool)Call("ChoosePolicePath",choice)&&Mathf.Abs((float)choice[2]-police.localPosition.x)>2,
              $"Police chooses an adjacent detour around traffic. Target: {choice[2]}.");
        Check(!(bool)Call("RoutePolicePathClear",police.localPosition,obstacle.model.localPosition,0f,.1f),"Junction and crash routing also reject driving through traffic.");
        typeof(PhoVietGame).GetField("timeRiding",Instance).SetValue(game,4f);
        typeof(PhoVietGame).GetField("cruiseSpeed",Instance).SetValue(game,17f);
        float initialX=police.localPosition.x;
        for(int i=0;i<10;i++)Call("TickPoliceDriving",.02f);
        Check(Mathf.Abs(police.localPosition.x-initialX)>.6f&&game.PoliceAvoidances>0&&!game.PoliceFallen,
              "The cruiser starts steering around the actual obstruction instead of colliding.");
        obstacle.halfWidth=originalWidth;obstacle.halfLength=originalLength;
    }
}
