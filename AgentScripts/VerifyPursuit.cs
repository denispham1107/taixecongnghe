using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using PhoViet;

public static class VerifyPursuit
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool originalReckless=g.recklessTrafficEnabled;
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        bool originalSurprises=g.trafficSurprises;g.trafficSurprises=false;int originalBest=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float originalScale=Time.timeScale;
        var checks=new List<string>();
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);checks.Add(name);};
        Action<string,float> set=(name,v)=>typeof(PhoVietGame).GetField(name,flags).SetValue(g,v);
        Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",flags).Invoke(g,new object[]{dt});
        Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}g.recklessTrafficEnabled=false;set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);};
        Action clean=()=>{g.RestartRide();clear();};
        try
        {
            Time.timeScale=1;
            check(PhoVietGame.DifficultyMultiplier==3,"Difficulty clock is 50 percent faster than v1.1 (2 -> 3)");
            check(g.leftKickLeg!=null&&g.rightKickLeg!=null&&g.policeBike!=null&&g.jailRoom!=null,"Saved scene includes animated legs, police motorcycle and jail");
            g.ResetRide();check(!g.TryKick()&&!g.IsChased,"Cannot kick before the ride begins");
            clean();check(!g.TryKick()&&!g.IsChased,"Empty kick does not summon police");
            g.SpawnTraffic(3,0,0);check(!g.TryKick(),"Cars are not valid kick targets");
            clean();var bike=g.SpawnTraffic(3,5,1);check(!g.TryKick(),"Distant motorcycle is not parallel");
            bike.model.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);bike.speed=-10;check(g.TryKick()&&bike.knocked&&g.IsChased,"Oncoming ordinary motorcycles can be kicked and summon police");
            clean();bike=g.SpawnTraffic(3,0,1);
            bike.speed=10;check(g.TryKick()&&bike.knocked,"Q action knocks a valid parallel motorcycle");
            check(bike.knockVelocity.x>0&&bike.knockDirection==1,"Right kick sends the bike away to the right");
            check(g.KickCount==1&&g.IsChased&&g.policeBike.gameObject.activeSelf,"First successful kick starts police pursuit");
            check(g.PoliceSpeed>g.Speed&&g.PoliceDistance>=37,"Police initially approach rapidly from behind");
            var next=g.SpawnTraffic(3,.2f,2);check(!g.TryKick()&&!next.knocked,"Kick cooldown prevents repeated impacts in the same instant");
            clear();for(int i=0;i<125;i++)tick(.02f);
            check(g.State==PhoVietGame.RideState.Riding&&g.PoliceDistance>2.2f,"Unobstructed rider avoids capture");
            check(Mathf.Abs((g.Speed-g.PoliceSpeed)-.65f)<.002f,"After arrival the player is slightly faster than police");
            float speed=g.Speed;for(int i=0;i<50;i++)tick(.02f);check(g.Speed>speed,"Player speed keeps increasing during pursuit");
            float gap=g.PoliceDistance;g.SpawnTraffic(3,.1f,1);check(g.TryKick()&&g.PoliceDistance==gap,"Further kicks do not reset the police gap");

            clean();g.MoveLane(1);for(int i=0;i<15;i++)tick(.02f);bike=g.SpawnTraffic(2,0,1);
            check(g.TryKick()&&bike.knockVelocity.x<0,"Left-side target produces a leftward kick");
            set("kickTime",.19f);typeof(PhoVietGame).GetMethod("AnimateKick",flags).Invoke(g,null);
            check(Quaternion.Angle(g.leftKickLeg.localRotation,Quaternion.identity)>70&&Quaternion.Angle(g.rightKickLeg.localRotation,Quaternion.identity)<.1f,"Only the kicking-side leg extends");
            g.TogglePause();check(!g.TryKick(),"No kick while paused");

            clean();g.MoveLane(-1);for(int i=0;i<15;i++)tick(.02f);
            bike=g.SpawnTraffic(2,0,1);var second=g.SpawnTraffic(3,0,2);var car=g.SpawnTraffic(3,-7,0);car.speed=22;car.baseTrafficSpeed=22;
            check(g.TryKick(),"Domino setup starts with a valid rightward kick");
            for(int i=0;i<20;i++)tick(.02f);
            check(second.knocked,"Falling motorcycle knocks down another motorcycle");
            check(car.knocked&&car.physicalCrash,"Car running into the domino wreck receives a mass-based collision response");
            check(g.ChainCount>=2,"Chain counter records secondary motorcycle and car impacts");
            check(second.knockFromPlayer&&car.knockFromPlayer,"Player credit propagates through the chain without arbitrary launch directions");
            int count=g.ChainCount;typeof(PhoVietGame).GetMethod("ResolveChainReactions",flags).Invoke(g,null);
            check(g.ChainCount==count,"Already-fallen vehicles are not counted repeatedly");

            clean();bike=g.SpawnTraffic(3,0,1);g.TryKick();bike.knockVelocity=new Vector3(0,0,7);bike.knockAge=.5f;
            car=g.SpawnTraffic(3,-6,0);car.speed=30;car.baseTrafficSpeed=30;
            for(int i=0;i<15;i++)tick(.02f);
            check(car.knocked&&car.knockVelocity.z<30,"A following car that runs over the downed motorcycle slows from the physical impact");

            clean();bike=g.SpawnTraffic(3,0,1);Vector2 centre=new Vector2(Screen.width*.5f,Screen.height*.5f);
            check(!g.RegisterTap(centre,10),"A single mobile tap does not kick");
            check(g.RegisterTap(centre,10.2f)&&bike.knocked,"Two close taps trigger the same kick action");
            clean();g.SpawnTraffic(3,0,1);g.RegisterTap(centre,10);check(!g.RegisterTap(centre,10.6f),"Separated taps are not a double tap");
            clean();g.SpawnTraffic(3,0,1);g.RegisterTap(new Vector2(30,10),10);check(!g.RegisterTap(new Vector2(30,10),10.2f)&&!g.IsChased,"Tapping steering controls cannot accidentally kick");
            clean();g.SpawnTraffic(3,0,1);g.RegisterTap(centre,10);check(!g.RegisterTap(centre+Vector2.right*Screen.width*.25f,10.2f),"Spatially separated taps are not a double tap");

            clean();g.SpawnTraffic(3,0,1);g.TryKick();clear();for(int i=0;i<120;i++)tick(.02f);
            var obstacle=g.SpawnTraffic(g.CurrentLane,1,0);tick(.02f);
            check(g.State==PhoVietGame.RideState.Falling&&g.Speed==0,"Traffic impact during pursuit starts a physical crash and begins a fall");
            clean();typeof(PhoVietGame).GetMethod("StartChase",flags).Invoke(g,null);
            g.policeBike.localPosition=new Vector3(g.rider.localPosition.x,0,-1);
            typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,1f);
            tick(.02f);
            check(g.State==PhoVietGame.RideState.Arrested,"Police catching the rider still triggers capture independently of traffic crashes");
            typeof(PhoVietGame).GetMethod("TickArrest",flags).Invoke(g,new object[]{1.2f});
            check(g.State==PhoVietGame.RideState.Jailed&&g.jailRoom.gameObject.activeSelf,"Capture transitions to the jail scene");
            check(!g.scenery.gameObject.activeSelf&&!g.rider.gameObject.activeSelf&&!g.policeBike.gameObject.activeSelf,"Street actors are hidden in jail");
            check(Vector3.Distance(g.rideCamera.transform.position,g.jailRoom.position)<12,"Camera switches to the prison set");
            check(!g.TryKick(),"Cannot attack from the jail screen");
            g.RestartRide();check(g.State==PhoVietGame.RideState.Riding&&!g.IsChased&&!g.jailRoom.gameObject.activeSelf&&g.scenery.gameObject.activeSelf,"Restart exits prison and restores the city");
            check(g.KickCount==0&&g.ChainCount==0&&g.vehicles.All(v=>!v.knocked),"Restart clears combat and pursuit state");
            return new {success=true,count=checks.Count,checks=checks};
        }
        finally
        {
            g.recklessTrafficEnabled=originalReckless;g.trafficSurprises=originalSurprises;
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",originalBest);PlayerPrefs.Save();
            typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,originalBest);g.ResetRide();Time.timeScale=originalScale;
        }
    }
}



