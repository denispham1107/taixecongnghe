using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifySkywing21177
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static PhoVietGame game;
    static readonly List<string> checks=new List<string>();

    static void Check(bool condition,string description)
    {
        if(!condition)throw new InvalidOperationException(description);
        checks.Add(description);
    }

    static object Call(string name,params object[] args) => typeof(PhoVietGame).GetMethod(name,Fields).Invoke(game,args);
    static object Get(string name) => typeof(PhoVietGame).GetField(name,Fields).GetValue(game);
    static void Set(string name,object value) => typeof(PhoVietGame).GetField(name,Fields).SetValue(game,value);
    static object ReadActor(object actor,string name) => actor.GetType().GetField(name,Fields).GetValue(actor);
    static List<object> ActivePlanes() => ((IList)Get("skywings")).Cast<object>().Where(p=>(bool)ReadActor(p,"active")).ToList();
    static Transform Root(object plane) => (Transform)ReadActor(plane,"root");
    static void JumpTicks(float seconds)
    {
        int steps=Mathf.RoundToInt(seconds*60);
        for(int i=0;i<steps;i++)Call("TickJump",1/60f);
    }

    static void Pointer(int phase,int action=0,int id=21)
    {
        var position=game.FlightButtonPixels().center;
        game.ApplyBrowserGesture(JsonUtility.ToJson(new Gesture
            {phase=phase,action=action,id=id,x=position.x/Screen.width,y=position.y/Screen.height,at=Time.unscaledTime}));
    }

    [Serializable] sealed class Gesture
    {
        public int phase,action,id;
        public float x,y,at;
    }

    static void FlightFromGround()
    {
        game.RestartRide();
        Pointer(0);
        Pointer(4,1);
        Check(game.RiderFlightPending,"A normal Bay click arms flight once.");
        JumpTicks(.7f);
        Check(game.RiderFlying&&Mathf.Abs(game.RiderJumpHeight-PhoVietGame.FlightHeightAboveSupport)<.002f,"Flight starts at the existing apex.");
    }

    public static object Main()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Play Mode required.");
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        checks.Clear();
        float previousTimeScale=Time.timeScale;
        try
        {
            Time.timeScale=0;
            float baseHeight=PhoVietGame.FlightHeightAboveSupport;
            float maximum=PhoVietGame.MaximumFlightHeightAboveSupport;
            Check(Mathf.Abs(maximum/baseHeight-2.2f)<.0001f,"Maximum altitude is 2.2 times base height.");
            Check(PhoVietGame.FlightDrainMultiplier(baseHeight)==1&&PhoVietGame.FlightDrainMultiplier(maximum)==4,"Drain spans exactly 1×–4×.");
            Check(Mathf.Abs(PhoVietGame.FlightDrainMultiplier((baseHeight+maximum)/2)-2.5f)<.001f,"Energy drain grows continuously with altitude.");
            FlightFromGround();
            Call("SpawnSkywings");
            Check(ActivePlanes().Count==0,"Base flight spawns no biplanes.");
            Pointer(0);
            JumpTicks(.2f);
            Check(Mathf.Abs(game.RiderJumpHeight-baseHeight)<.001f,"A press below the long-press threshold does not start climbing.");
            JumpTicks(.55f);
            Check(game.RiderJumpHeight>baseHeight*1.3f&&game.RiderJumpHeight<baseHeight*1.7f,"The first tier is reached by sustained input.");
            Call("SpawnSkywings");
            Check(ActivePlanes().Count==2,"First tier spawns two planes.");
            JumpTicks(.6f);
            Call("SpawnSkywings");
            Check(ActivePlanes().Count==4,"Second tier adds two planes.");
            JumpTicks(.7f);
            Call("SpawnSkywings");
            Check(ActivePlanes().Count==6,"Third tier adds two planes.");
            JumpTicks(.2f);
            Check(Mathf.Abs(game.RiderJumpHeight-maximum)<.002f&&game.RiderFlying,"Climb stops at the cap.");
            Pointer(4,1);
            float heldHeight=game.RiderJumpHeight;
            JumpTicks(.1f);
            Check(game.RiderFlying&&Mathf.Abs(game.RiderJumpHeight-heldHeight)<.001f,"Release holds the attained height.");
            foreach(var group in ActivePlanes().GroupBy(p=>(int)ReadActor(p,"tier")))
            {
                Check(group.Count()==2&&group.Select(p=>(int)ReadActor(p,"spawnLane")).Distinct().Count()==2,"Each tier uses two distinct lanes.");
                foreach(var plane in group)
                {
                    Check(((Vector3)ReadActor(plane,"velocity")).z<0&&Vector3.Dot(Root(plane).forward,Vector3.back)>.99f,"Plane approaches facing the player.");
                    Check(Root(plane).GetComponentInChildren<LODGroup>().lodCount==3&&
                          Root(plane).Find("Airframe/Front propeller").childCount==3,"The moving propeller has a separate pivot and LODs.");
                }
            }
            var lead=ActivePlanes()[0];
            Root(lead).localPosition=new Vector3(0,maximum,38);
            int fired=game.HelicopterMissilesFired;
            Call("TickHelicopterWeapon",lead,1.01f);
            Check(game.HelicopterMissilesFired==fired+1&&game.ActiveHelicopterMissileCount==1,"A biplane fires a shared hostile missile.");
            Check(game.TryFirePlayerMissile(),"The player can fire while at maximum altitude.");
            for(int i=0;i<50&&!(bool)ReadActor(lead,"wrecked");i++)Call("TickHelicopter",1/60f,Vector3.zero,false);
            Check((bool)ReadActor(lead,"wrecked")&&game.IsAircraftBurning(Root(lead)),"Player missile hits the biplane and sets it burning.");

            // Isolate one full charge interval at the cap.
            Set("flightEnergyClock",0f);Set("flightEnergyUsed",0f);
            int energy=game.Energy;
            JumpTicks(.5f);
            Check(energy-game.Energy==40&&game.RiderFlying,"At the cap one half-second costs 40 energy.");
            Pointer(0);
            Pointer(2);
            Check(!game.RiderFlying&&game.RiderAirborne,"A quick second tap lands even after a high hold.");

            FlightFromGround();
            Pointer(0);
            JumpTicks(.45f);
            Pointer(3);
            heldHeight=game.RiderJumpHeight;
            JumpTicks(.15f);
            Check(game.RiderFlying&&Mathf.Abs(game.RiderJumpHeight-heldHeight)<.001f,"Browser pointer cancellation stops climbing without landing.");
            Pointer(0);
            JumpTicks(.45f);
            game.TogglePause();
            game.TogglePause();
            heldHeight=game.RiderJumpHeight;
            JumpTicks(.15f);
            Check(Mathf.Abs(game.RiderJumpHeight-heldHeight)<.001f,"Pause clears the held input.");
            for(int i=0;i<600&&game.RiderAirborne;i++)
            {
                Call("TickJump",1/60f);
                Call("ResolveJumpSurface",1/60f);
            }
            Check(game.Energy==0&&!game.RiderFlying&&!game.RiderAirborne,"An empty battery releases hover and allows normal landing.");
            FlightFromGround();
            Set("jumpHeight",baseHeight*1.4f);
            game.rider.localPosition=new Vector3(0,game.RiderJumpHeight,0);
            Call("SpawnSkywings");
            var incoming=ActivePlanes()[0];
            Root(incoming).localPosition=new Vector3(0,game.RiderJumpHeight+game.PlayerHeight*.5f,.7f);
            Call("TickHelicopter",1/60f,game.rider.localPosition,true);
            Check(game.State==PhoVietGame.RideState.Falling&&game.RiderAirAttackBurning&&game.IsAircraftBurning(Root(incoming)),
                "Direct biplane contact burns both aircraft and player.");
            game.RestartRide();
            Check(ActivePlanes().Count==0&&game.Energy==PhoVietGame.MaximumEnergy,"Restart resets planes and flight energy.");
            return new {checks=checks.Count,details=checks,baseHeight,maximum};
        }
        finally
        {
            Time.timeScale=previousTimeScale;
        }
    }
}
