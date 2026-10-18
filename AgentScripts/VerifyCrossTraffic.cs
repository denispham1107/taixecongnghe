using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyCrossTraffic
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool originalReckless=g.recklessTrafficEnabled;
        bool originalSurprises=g.trafficSurprises;g.trafficSurprises=false;int originalBest=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var tick=typeof(PhoVietGame).GetMethod("TickRide",flags);
        var passed=new List<string>();
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);passed.Add(name);};
        Action<string,float> set=(field,value)=>typeof(PhoVietGame).GetField(field,flags).SetValue(g,value);
        Action<float> step=dt=>tick.Invoke(g,new object[]{dt});
        Action clean=()=>{g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}g.recklessTrafficEnabled=false;set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);};
        try
        {
            clean();for(int i=0;i<1500;i++)step(.02f);
            check(Mathf.Abs(g.Speed-27.8f)<.02f,"30 seconds reaches 27.8 m/s: acceleration is 50 percent faster than v1.1");
            for(int i=0;i<310;i++)step(.02f);
            check(g.Speed==30,"Maximum speed reached in about 36.2 seconds");
            clean();set("timeRiding",3.99f);set("spawnTimer",0);step(.001f);
            check(g.vehicles.Count(v=>v.active)==1,"Before 4 seconds, one longitudinal vehicle per wave");
            clean();set("timeRiding",4.01f);set("spawnTimer",0);step(.001f);
            check(g.vehicles.Count(v=>v.active)==2,"Double-vehicle waves begin after 4 seconds");
            clean();set("timeRiding",110f/6);set("spawnTimer",0);step(.001f);
            float jittered=(float)typeof(PhoVietGame).GetField("spawnTimer",flags).GetValue(g);
            check(jittered>=1.925f*.8f-.01f&&jittered<=1.925f*1.2f+.01f,"Traffic density keeps its difficulty ramp with randomized spawn intervals");

            clean();var left=g.SpawnCrossTraffic(1,50,false,8);var right=g.SpawnCrossTraffic(-1,50,true,8);
            check(left!=null&&left.halfWidth<.7f&&right!=null&&right.halfWidth>.7f,"Cross traffic selects real motorcycle and car models");
            check(Mathf.Abs(left.model.localEulerAngles.y-90)<.01f&&Mathf.Abs(right.model.localEulerAngles.y-270)<.01f,"Models face left-to-right and right-to-left");
            step(.05f);check(left.model.localPosition.x>-16&&right.model.localPosition.x<16,"Both crossing directions move horizontally");
            check(Mathf.Abs(left.model.localPosition.z+g.Distance-50)<.001f,"Cross traffic stays attached to the scrolling intersection");
            clean();left=g.SpawnCrossTraffic(1,50,false,8,.1f);step(.05f);
            check(left.model.localPosition.x==-16,"Scheduled vehicle waits off the main road");
            step(.075f);check(Mathf.Abs(left.model.localPosition.x+15.8f)<.001f,"Release delay preserves only the elapsed moving fraction");

            foreach(int direction in new[]{1,-1})foreach(bool car in new[]{false,true})
            {
                clean();var v=g.SpawnCrossTraffic(direction,.5f,car,400);v.model.localPosition=new Vector3(g.rider.localPosition.x-direction*8,0,.5f);
                step(.05f);check(g.State==PhoVietGame.RideState.Falling,(car?"Car":"Motorcycle")+" side impact from "+(direction==1?"left":"right")+" triggers immediate loss even across an entire frame");
            }
            clean();left=g.SpawnCrossTraffic(1,10,true,400);left.model.localPosition=new Vector3(-6,0,10);step(.05f);
            check(g.State==PhoVietGame.RideState.Riding,"Crossing ahead without Z overlap is a safe near miss");
            check(!PhoVietGame.SweptOverlap(new Vector2(-6,4),new Vector2(6,4),new Vector2(2.78f,1.85f)),"Swept collision rejects a parallel pass at a safe distance");
            check(!PhoVietGame.SweptOverlap(new Vector2(-5,0),new Vector2(0,5),Vector2.one),"Collision requires simultaneous overlap, not independent X and Z crossings");

            clean();set("nextIntersectionDistance",70);step(.02f);
            check(g.ScheduledIntersections==1&&g.vehicles.Count(v=>v.active&&v.crossing)==2,"First actual intersection schedules two low-difficulty crossing vehicles");
            var cross=g.vehicles.First(v=>v.active&&v.crossing);
            check(Mathf.Abs(cross.model.localPosition.z+g.Distance-(70+(cross.crossSpeed>0?-2.8f:2.8f)))<.01f,"Crossing is inside the real cross street, with either randomized entry direction");
            step(.02f);check(g.ScheduledIntersections==1,"Intersection does not spawn duplicates on subsequent frames");
            clean();set("timeRiding",10);set("nextIntersectionDistance",70);step(.02f);
            check(g.vehicles.Count(v=>v.active&&v.crossing)==3,"Medium difficulty schedules three crossing vehicles");
            check(g.vehicles.Any(v=>v.active&&v.crossSpeed>0)&&g.vehicles.Any(v=>v.active&&v.crossSpeed<0),"Busy intersection includes vehicles from both sides");
            clean();set("timeRiding",40);set("nextIntersectionDistance",70);step(.02f);
            check(g.vehicles.Count(v=>v.active&&v.crossing)==5,"High difficulty schedules five crossing vehicles");
            check(g.vehicles.Where(v=>v.active&&v.crossing).All(v=>Mathf.Abs(v.crossSpeed)>9),"Crossing traffic also speeds up with difficulty");
            clean();left=g.SpawnCrossTraffic(1,50,false,8);left.model.localPosition=new Vector3(20,0,50);step(.02f);
            check(!left.active&&!left.model.gameObject.activeSelf,"Vehicles return to pool after crossing the far side");
            int reused=g.vehicles.IndexOf(left);g.SpawnTraffic(2,90,reused);
            check(!left.crossing&&left.crossSpeed==0&&left.crossDelay==0,"Reused crossing model resets to normal longitudinal traffic");
            g.ResetRide();check(g.ScheduledIntersections==0&&g.vehicles.All(v=>!v.crossing),"Restart clears crossing schedules and vehicle motion state");
            return new {success=true,count=passed.Count,checks=passed};
        }
        finally
        {
            g.recklessTrafficEnabled=originalReckless;g.trafficSurprises=originalSurprises;
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",originalBest);PlayerPrefs.Save();
            typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,originalBest);g.ResetRide();
        }
    }
}



