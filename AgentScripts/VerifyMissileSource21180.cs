using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyMissileSource21180
{
    const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static PhoVietGame game;
    static int passed;

    static object Call(string name,params object[] args)=>typeof(PhoVietGame).GetMethod(name,Instance).Invoke(game,args);
    static object Field(object target,string name)=>target.GetType().GetField(name,Instance).GetValue(target);
    static string HitLabel=>(string)typeof(PhoVietGame).GetProperty("MissileHitSourceLabel",Instance).GetValue(game);
    static void Check(bool condition,string description)
    {
        if(!condition)throw new InvalidOperationException(description);
        passed++;
    }
    static void ClearTraffic()
    {
        foreach(var v in game.vehicles)
        {
            v.active=false;
            v.model.gameObject.SetActive(false);
        }
    }

    public static object Main()
    {
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(!Application.isPlaying||game==null)throw new InvalidOperationException("Open Phố Việt in Play Mode.");
        float scale=Time.timeScale;
        bool tanks=game.tankBarrageEnabled;
        try
        {
            Time.timeScale=0;
            passed=0;
            game.tankBarrageEnabled=true;
            game.RestartRide();
            ClearTraffic();
            Check((bool)Call("SpawnHelicopter"),"Initial helicopter launches.");
            var helicopter=((IList)Field(game,"helicopters"))[0];
            var root=(Transform)Field(helicopter,"root");
            var start=root.localPosition;
            Call("TickHelicopter",.1f,game.rider.localPosition,false);
            Check(Mathf.Abs(start.z-root.localPosition.z-1.84f)<.001f,"The first helicopter travels at 18.4 m/s.");

            object reused=null;
            foreach(var label in new[]{"xe tăng","trực thăng","máy bay hai tầng cánh"})
            {
                game.RestartRide();
                ClearTraffic();
                Check(HitLabel==null&&!game.HitByTankMissile&&!game.HitByHelicopterMissile,"Restart clears the last shooter.");
                Check(game.TryToggleFlightButton(),"Player starts flight.");
                for(int i=0;i<42;i++)Call("TickJump",1/60f);
                Check(game.RiderFlying,"Player has reached a stable flight height.");
                var pose=game.rider.localPosition;
                pose.y=game.RiderJumpHeight;
                game.rider.localPosition=pose;
                if(label=="xe tăng")
                {
                    var tank=game.SpawnScheduledTank();
                    Check(tank!=null,"Tank spawns.");
                    tank.model.localPosition=new Vector3(pose.x,0,34);
                    Call("TickTankFire",.5f);
                    Check(game.TankMissilesFired==1&&game.ShellsFired==0,"Tank fires a missile while the player is flying.");
                    ClearTraffic();
                }
                else
                {
                    bool plane=label=="máy bay hai tầng cánh";
                    object actor=helicopter;
                    if(plane)
                    {
                        Check((bool)Call("LaunchSkywing",0,0),"Skywing spawns.");
                        actor=((IList)Field(game,"skywings"))[0];
                    }
                    Call("LaunchHelicopter",actor,pose+Vector3.forward*27+Vector3.up*.6f,false);
                    if(plane)Check(Mathf.Abs(((Vector3)Field(actor,"velocity")).magnitude-20)<.001f,"Skywing retains 20 m/s.");
                    Check((bool)Call("FireHelicopterMissile",actor),"Aircraft fires its missile.");
                    Call("ResetSkywings"); // Source is stored on the missile, independently of the aircraft.
                    root.gameObject.SetActive(false);
                    actor.GetType().GetField("active",Instance).SetValue(actor,false);
                }
                var active=((IList)Field(game,"helicopterMissiles")).Cast<object>().Single(m=>(bool)Field(m,"active"));
                Check(reused==null||ReferenceEquals(reused,active),"The projectile slot is reused across shooters.");
                reused=active;
                Check(Mathf.Abs(((Vector3)Field(active,"velocity")).magnitude-36)<.001f,"Enemy missile travels at 36 m/s.");
                for(int i=0;i<150&&game.State==PhoVietGame.RideState.Riding;i++)
                    Call("TickHelicopterMissiles",.02f,pose,true);
                Check(game.State==PhoVietGame.RideState.Falling&&game.RiderAirAttackBurning&&HitLabel==label,
                    "A swept hit identifies the actual shooter and starts the burning crash.");
                for(int i=0;i<190&&game.State==PhoVietGame.RideState.Falling;i++)Call("TickRiderCrash",.02f);
                Check(game.State==PhoVietGame.RideState.GameOver&&HitLabel==label,"The end card still has the correct shooter.");
            }

            game.RestartRide();
            ClearTraffic();
            Check(game.TryFirePlayerMissile(),"Player launches normally.");
            var playerMissile=((IList)Field(game,"playerMissiles")).Cast<object>().Single(m=>(bool)Field(m,"active"));
            Check(Mathf.Abs(((Vector3)Field(playerMissile,"velocity")).magnitude-70)<.001f,"Player missile retains its launch speed.");
            return new {passed,helicopterSpeed=18.4f,enemyMissileSpeed=36f,playerMissileSpeed=70f};
        }
        finally
        {
            game.tankBarrageEnabled=tanks;
            game.ResetRide();
            Time.timeScale=scale;
        }
    }
}
