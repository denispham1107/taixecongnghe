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
    static void SetField(string name,object value)=>typeof(PhoVietGame).GetField(name,Instance).SetValue(game,value);
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
        if(bike)Check(riding&&game.State==PhoVietGame.RideState.Riding&&game.IsChased&&game.Speed>0&&game.Speed<18&&v.knocked,
            "A motorcycle is launched while the player car slows and keeps driving.");
        else Check(!riding&&game.State==PhoVietGame.RideState.Falling&&game.CrashDuringPursuit&&
              (bool)Field("exitAfterCollision"),"A traffic impact starts the forced exit.");
        ClearTraffic();
    }

    static void FinishCrash(PhoVietGame.RideState expected)
    {
        for(int i=0;i<250&&game.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",.04f);
        Check(game.State==expected,$"Expected {expected} after crash, got {game.State}, police gap {game.PoliceDistance}.");
    }

    static void CheckGrounded()
    {
        var police=game.policeBike;
        Check(Mathf.Abs(police.localPosition.y)<.001f&&Vector3.Dot(police.localRotation*Vector3.up,Vector3.up)>.9999f,
              $"The cruiser stays level on the road, got {police.localPosition} / {police.localEulerAngles}.");
    }

    static void VerifyGroundedPursuit()
    {
        FreshRun();
        // Record an airborne path using the real route tracker, then follow it through a turn.
        typeof(PhoVietGame).GetProperty("QueuedTurn",Instance).SetValue(game,1);
        Call("StartJunctionTurnAt",false,10f);
        game.rider.localPosition=new Vector3(4,6,0);
        typeof(PhoVietGame).GetProperty("Distance",Instance).SetValue(game,4f);
        Call("EndRouteFrame",.02f);
        Check(((Vector3)Field(Call("SampleRoute",2f),"point")).y==0,"The junction trail interpolates at road height.");
        Check(((Vector3)Field(Call("SampleRoute",6f),"point")).y==0,"The junction trail extrapolates at road height.");
        SetField("policeRouteAt",1f);
        game.policeBike.localPosition=new Vector3(1.55f,3,-7);
        for(int i=0;i<20;i++){Call("TickRoutePolice",.02f);CheckGrounded();}
        Check(game.State==PhoVietGame.RideState.Riding,"A flying player cannot be arrested from the road.");

        foreach(bool route in new[]{false,true})
        {
            FreshRun();
            SetField("routePolice",route);
            var police=game.policeBike;
            var dinosaur=game.SpawnDinosaur();
            Check(dinosaur!=null,"Dinosaur spawns for patrol impact.");
            var before=police.localPosition;
            for(int z=-6;z<=6&&!dinosaur.hitPolice;z++)
            {
                dinosaur.root.localPosition=before+Vector3.forward*z;
                Call("ResolveDinosaurHits",dinosaur,1f);
            }
            Check(dinosaur.hitPolice&&game.PoliceFallen&&game.IsChased,"Dinosaur contact starts patrol recovery.");
            string tick=route?"TickRoutePolice":"TickPoliceDriving";
            for(int i=0;i<60;i++){Call(tick,.02f);CheckGrounded();}
            Check(!game.PoliceFallen&&Vector3.Distance(before,police.localPosition)>.5f,
                  "The cruiser slides horizontally and recovers in 1.2 seconds.");
        }
    }

    static void VerifyJailViewport()
    {
        var savedInsets=Field("browserInsets");
        var hadInsets=Field("hasBrowserInsets");
        var savedScale=Field("uiScale");
        var savedPixelRatio=Field("browserPixelRatio");
        try
        {
            game.ApplyBrowserInsets("{\"left\":0.05,\"right\":0.04,\"top\":0.02,\"bottom\":0.05,\"pixelRatio\":2}");
            var safe=(Rect)Call("GameGuiArea");
            Check(safe.width<Screen.width&&safe.height<Screen.height,"The controls retain a smaller safe area.");
            const float scale=.72f;
            SetField("uiScale",scale);
            var image=(Rect)Call("FullscreenGuiRect");
            var minimum=safe.min+image.min*scale;
            var maximum=safe.min+image.max*scale;
            Check(minimum.sqrMagnitude<.00001f&&(maximum-new Vector2(Screen.width,Screen.height)).sqrMagnitude<.00001f,
                  "The jail image reaches every canvas edge even with asymmetric safe insets.");
        }
        finally
        {
            SetField("browserInsets",savedInsets);
            SetField("hasBrowserInsets",hadInsets);
            SetField("uiScale",savedScale);
            SetField("browserPixelRatio",savedPixelRatio);
        }
    }

    static void VerifyPatrolRiverFall()
    {
        game.riversEnabled=true;
        try
        {
            FreshRun();
            var tile=game.blocks.First(b=>b.parent.gameObject.activeInHierarchy);
            var bridge=tile.GetComponent<RiverBlock>();
            bool wasBridge=bridge.IsBridge;
            try
            {
                bridge.SetBridge(true);
                float mid=(RiverBlock.Near+RiverBlock.Far)*.5f;
                Vector3 water=game.transform.InverseTransformPoint(tile.TransformPoint(new Vector3(RiverBlock.DeckHalf+1,0,mid)));
                Check(game.OverRiver(water),"The river fixture is outside a real bridge deck.");
                game.policeBike.localPosition=water;
                Call("TickPoliceDriving",.02f);
                Check(game.PoliceInRiver&&!game.IsChased,"The patrol car enters sinking physics when it leaves the deck.");
                for(int i=0;i<150;i++)Call("TickPoliceRiver",.02f,false);
                Check(game.policeBike.localPosition.y<RiverBlock.WaterY,"Sinking is not snapped back to road height.");
            }
            finally{bridge.SetBridge(wasBridge);}
        }
        finally{game.riversEnabled=false;}
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
            VerifyGroundedPursuit();
            FreshRun();RearContact(true,8);float weak=game.LastRiderImpactSpeed;
            FreshRun();RearContact(true,32);float strong=game.LastRiderImpactSpeed;
            Check(strong>weak&&game.IsChased,"Faster motorcycle impacts transfer more force during the chase.");
            FreshRun();
            game.policeBike.localPosition=game.rider.localPosition-Vector3.forward*1.7f;
            Call("TickPoliceDriving",.01f);
            Check(game.State==PhoVietGame.RideState.Arrested,"The officer catches a slow player in reach.");
            Call("TickArrest",1.2f);
            var wide=(Texture2D)Field("jailWide");var tall=(Texture2D)Field("jailTall");
            Check(game.State==PhoVietGame.RideState.Jailed&&wide!=null&&tall!=null&&wide.width>wide.height&&tall.width<tall.height,
                "The jail shows the authored landscape and portrait resources.");
            VerifyJailViewport();

            FreshRun();RearContact(false,12);game.Arrest();
            Check(game.State==PhoVietGame.RideState.Falling,"Arrest waits for the crash animation.");
            game.StrikeLightning(game.policeBike.localPosition);
            Check(game.IsChased&&game.CrashDuringPursuit&&!game.PoliceFallen,"Lightning cannot interrupt an officer approaching a crashed player.");
            FinishCrash(PhoVietGame.RideState.OnFoot);

            FreshRun();Call("BlastRider",game.rider.localPosition);
            Check(!game.IsChased&&!game.CrashDuringPursuit,"A tank shell bypasses the jail.");
            FinishCrash(PhoVietGame.RideState.GameOver);

            var source=typeof(PhoVietGame).GetNestedType("EnemyMissileSource",BindingFlags.NonPublic);
            foreach(var name in new[]{"Tank","Helicopter","Skywing","None"})
            {
                FreshRun();Call("BurnAirAttackRider",game.rider.localPosition+Vector3.forward,Enum.Parse(source,name),false);
                bool body=name=="None";
                Check(game.CrashDuringPursuit==body,"Aircraft body crashes and enemy missiles follow their separate rules.");
                FinishCrash(body?PhoVietGame.RideState.OnFoot:PhoVietGame.RideState.GameOver);
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
            VerifyPatrolRiverFall();
            return new {passed,weakImpact=weak,strongImpact=strong,wide=$"{wide.width}x{wide.height}",tall=$"{tall.width}x{tall.height}"};
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
        Check(livery.name=="PV_PoliceLivery"&&(livery.width==1024||livery.width==2048),
              "The cruiser uses the corrected side lettering atlas at the active platform's texture size.");
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
        obstacle.model.localPosition=obstacle.previousPosition=police.localPosition+Vector3.forward*(obstacle.halfLength+1f);
        Call("TickPoliceDriving",.02f);
        Check(game.PoliceFallen,"An unavoidable vehicle contact starts patrol recovery.");
        CheckGrounded();
        ClearTraffic();
        for(int i=0;i<60;i++){Call("TickPoliceDriving",.02f);CheckGrounded();}
        Check(!game.PoliceFallen,"The cruiser recovers from a traffic collision at road height.");
        obstacle.halfWidth=originalWidth;obstacle.halfLength=originalLength;
    }
}
