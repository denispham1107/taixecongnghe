using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PhoViet;

public static class VerifyRiderContacts
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float scale=Time.timeScale;
        bool surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled;
        var checks=new List<string>();
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        Action<string,float> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
        Action clean=()=>
        {
            g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);
        };
        Action chase=()=>{clean();typeof(PhoVietGame).GetMethod("StartChase",F).Invoke(g,null);};
        Action resolve=()=>typeof(PhoVietGame).GetMethod("ResolveRiderContacts",F).Invoke(g,new object[]{g.rider.localPosition.x,.05f});
        Func<int,float,PhoVietGame.TrafficVehicle> spawn=(kind,z)=>
        {
            int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
            if(index<0)throw new Exception("Vehicle kind unavailable: "+kind);
            return g.SpawnTraffic(2,z,index);
        };
        Action<string> lost=name=>
        {
            check(g.State==PhoVietGame.RideState.Falling&&g.Speed==0,name+": immediately loses and stops");
            int lane=g.CurrentLane;g.MoveLane(1);check(g.CurrentLane==lane&&!g.TryKick(),name+": input cannot resume riding");
        };
        try
        {
            Time.timeScale=0;
            for(int kind=0;kind<3;kind++)foreach(int direction in new[]{1,-1})foreach(bool immune in new[]{false,true})
            {
                chase();var v=spawn(kind,12);v.speed=direction*12;v.baseTrafficSpeed=v.speed;
                v.model.localRotation=Quaternion.Euler(0,direction<0?180:0,0);
                v.previousPosition=new Vector3(g.rider.localPosition.x,0,12);v.model.localPosition=new Vector3(g.rider.localPosition.x,0,-12);
                if(immune){set("collisionImmunity",10);v.playerGrace=.4f;}
                resolve();lost("Swept kind "+kind+" direction "+direction+" grace "+immune);
            }
            foreach(int direction in new[]{-1,1})
            {
                chase();var cross=spawn(1,0);cross.model.localRotation=Quaternion.Euler(0,direction*90,0);
                float x=g.rider.localPosition.x;cross.previousPosition=new Vector3(x-direction*12,0,0);cross.model.localPosition=new Vector3(x+direction*12,0,0);
                resolve();lost("Crossing car direction "+direction);
            }
            chase();var side=spawn(0,0);float start=g.rider.localPosition.x;
            side.previousPosition=new Vector3(start+3,0,0);side.model.localPosition=new Vector3(start,0,0);
            resolve();lost("Sudden side impact");
            chase();var wreck=spawn(0,1);wreck.knocked=true;wreck.physicalCrash=true;wreck.playerGrace=.4f;
            wreck.model.localRotation=Quaternion.Euler(0,0,-78);set("collisionImmunity",10);resolve();lost("Downed bike despite kick grace");
            foreach(bool truck in new[]{false,true})
            {
                chase();var danger=g.SpawnReckless(truck);danger.model.localPosition=new Vector3(g.rider.localPosition.x,0,-8);
                danger.previousPosition=new Vector3(g.rider.localPosition.x,0,8);danger.model.localRotation=Quaternion.Euler(0,180,0);
                resolve();lost(truck?"Drowsy truck":"Reckless rider");
            }
            chase();var airborne=spawn(1,-8);airborne.previousPosition=new Vector3(start,3,8);airborne.model.localPosition=new Vector3(start,3,-8);
            resolve();check(g.State==PhoVietGame.RideState.Riding,"Vehicle above rider height does not falsely collide");
            chase();var near=spawn(1,3);near.model.localPosition=new Vector3(start+4,0,-3);near.previousPosition=new Vector3(start+4,0,3);
            resolve();check(g.State==PhoVietGame.RideState.Riding,"Adjacent lane near miss does not falsely collide");
            chase();var stopped=spawn(1,5);stopped.speed=0;stopped.baseTrafficSpeed=0;
            for(int i=0;i<100&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
            lost("Real ride ticks approaching stopped car");
            set("fallTime",1);typeof(PhoVietGame).GetMethod("Update",F).Invoke(g,null);
            check(g.State==PhoVietGame.RideState.GameOver,"Fall animation ends at game over, never resumes pursuit");
            g.RestartRide();check(g.State==PhoVietGame.RideState.Riding&&!g.IsChased,"Restart clears crash and pursuit state");
            clean();spawn(1,1);tick(.02f);lost("Ordinary ride collision");
            chase();g.policeBike.localPosition=new Vector3(start,0,-1);typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,1f);tick(.02f);
            check(g.State==PhoVietGame.RideState.Arrested,"Police capture without a traffic collision still works");
            return new{success=true,count=checks.Count,checks};
        }
        finally
        {
            g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);
            g.ResetRide();Time.timeScale=scale;
        }
    }
}