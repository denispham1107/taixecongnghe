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
        Action clear=()=>{foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);};
        Action clean=()=>{g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;clear();tick(.02f);};
        Action chase=()=>{clean();typeof(PhoVietGame).GetMethod("StartChase",F).Invoke(g,null);};
        Action<float> resolve=oldX=>typeof(PhoVietGame).GetMethod("ResolveRiderContacts",F).Invoke(g,new object[]{oldX,.05f});
        Func<int,float,PhoVietGame.TrafficVehicle> spawn=(kind,z)=>
        {
            int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&(kind==0?v.halfWidth<.7f:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfLength>3));
            if(index<0)throw new Exception("Vehicle kind unavailable: "+kind);
            return g.SpawnTraffic(2,z,index);
        };
        Func<PhoVietGame.TrafficVehicle,bool> solid=v=>
        {
            var half=(Vector2)typeof(PhoVietGame).GetMethod("VehicleFootprint",F).Invoke(g,new object[]{v})+new Vector2(.33f,.91f);
            return Mathf.Abs(v.model.localPosition.x-g.rider.localPosition.x)>=half.x-.001f||Mathf.Abs(v.model.localPosition.z)>=half.y-.001f;
        };
        try
        {
            Time.timeScale=0;
            for(int kind=0;kind<3;kind++)foreach(int direction in new[]{1,-1})
            {
                chase();var v=spawn(kind,12);v.speed=direction*12;v.baseTrafficSpeed=v.speed;
                v.model.localRotation=Quaternion.Euler(0,direction<0?180:0,0);
                v.previousPosition=new Vector3(g.rider.localPosition.x,0,12);v.model.localPosition=new Vector3(g.rider.localPosition.x,0,-12);
                float policeBefore=g.policeBike.localPosition.z,blockBefore=g.blocks[0].localPosition.z;
                resolve(g.rider.localPosition.x);
                check(solid(v)&&v.model.localPosition.z>0,"Swept pursuit contact blocks kind "+kind+" direction "+direction);
                check(Mathf.Abs((g.policeBike.localPosition.z-policeBefore)-(g.blocks[0].localPosition.z-blockBefore))<.001f,"Police and scenery use same corrected road offset "+kind+"/"+direction);
                // Put the same obstacle through the rider again during the existing recovery cooldown.
                v.previousPosition=v.model.localPosition;v.model.localPosition-=Vector3.forward*15;
                resolve(g.rider.localPosition.x);
                check(solid(v)&&v.model.localPosition.z>0,"Recovery cooldown never grants ghosting "+kind+"/"+direction);
                check(g.State==PhoVietGame.RideState.Riding&&g.IsChased,"Collision preserves pursuit slowdown mechanism "+kind+"/"+direction);
            }
            chase();var cross=spawn(1,0);cross.model.localRotation=Quaternion.Euler(0,90,0);
            float start=g.rider.localPosition.x;cross.previousPosition=new Vector3(start-12,0,0);cross.model.localPosition=new Vector3(start+12,0,0);
            resolve(start);check(solid(cross)&&g.rider.localPosition.x>cross.model.localPosition.x,"Crossing car pushes rider on original side instead of tunnelling");

            chase();var side=spawn(0,0);side.previousPosition=new Vector3(4.65f,0,0);side.model.localPosition=side.previousPosition;
            start=g.rider.localPosition.x;g.rider.localPosition=new Vector3(6,0,0);resolve(start);
            check(solid(side)&&g.rider.localPosition.x<side.model.localPosition.x,"Lane change is stopped at side contact");
            side.previousPosition=side.model.localPosition;set("collisionImmunity",.8f);resolve(g.rider.localPosition.x);
            check(solid(side),"Standing side contact remains solid while damage is on cooldown");

            chase();var wreck=spawn(0,8);wreck.knocked=true;wreck.physicalCrash=true;wreck.playerGrace=0;
            wreck.model.localRotation=Quaternion.Euler(0,0,-78);wreck.previousPosition=new Vector3(start,0,8);wreck.model.localPosition=new Vector3(start,0,-8);
            resolve(start);check(solid(wreck)&&wreck.model.localPosition.z>0,"Downed motorcycles remain solid during chase");
            chase();wreck=spawn(0,8);wreck.knocked=true;wreck.playerGrace=.3f;
            wreck.previousPosition=new Vector3(start,0,8);wreck.model.localPosition=new Vector3(start,0,-8);
            resolve(start);check(solid(wreck),"Kicked bike damage grace cannot disable physical chase contact");

            chase();var airborne=spawn(1,-8);airborne.previousPosition=new Vector3(start,3,8);airborne.model.localPosition=new Vector3(start,3,-8);
            resolve(start);check(airborne.model.localPosition.z<0,"Airborne vehicles above the rider remain clear");
            chase();var near=spawn(1,3);near.model.localPosition=new Vector3(start+4,0,-3);near.previousPosition=new Vector3(start+4,0,3);
            resolve(start);check(near.model.localPosition.z<0,"Adjacent lane near miss is not blocked");

            chase();var stopped=spawn(1,5);stopped.speed=0;stopped.baseTrafficSpeed=0;
            bool held=true;for(int i=0;i<30&&g.State==PhoVietGame.RideState.Riding;i++){tick(.02f);held&=solid(stopped)&&stopped.model.localPosition.z>0;}
            check(held,"Repeated real ride ticks cannot drive through a stopped car");
            check(g.Speed<g.PoliceSpeed,"Blocked rider loses speed so police can close");
            g.MoveLane(-1);for(int i=0;i<30&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
            check(Mathf.Abs(g.rider.localPosition.x-stopped.model.localPosition.x)>1.3f,"Rider can steer away from contact");

            clean();var ordinary=spawn(1,1);tick(.02f);
            check(g.State==PhoVietGame.RideState.Falling,"Ordinary ride collision still ends in a fall");
            chase();for(int i=0;i<120;i++)tick(.02f);stopped=spawn(1,1);tick(.02f);clear();
            for(int i=0;i<180&&g.State==PhoVietGame.RideState.Riding;i++)tick(.02f);
            check(g.State==PhoVietGame.RideState.Arrested,"Pursuit collision still permits capture");
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
