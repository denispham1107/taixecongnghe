using System.Reflection;
using UnityEngine;
using PhoViet;

public static class PreviewReckless
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame Setup()
    {
        Time.timeScale=0;var g=Object.FindAnyObjectByType<PhoVietGame>();g.RestartRide();g.recklessTrafficEnabled=false;
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        foreach(string n in new[]{"spawnTimer","nextIntersectionDistance"})typeof(PhoVietGame).GetField(n,F).SetValue(g,float.PositiveInfinity);
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return g;
    }
    public static string Racer()
    {
        var g=Setup();var v=g.SpawnReckless(false);v.willHitWall=false;
        v.model.localPosition=new Vector3(1,0,9);v.previousPosition=v.model.localPosition;v.model.localRotation=Quaternion.Euler(0,145,-15);
        g.SpawnTraffic(0,22,0);g.SpawnTraffic(3,30,2);g.SpawnTraffic(2,48,3);
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);
        return "Racer preview ready";
    }
    public static string Truck()
    {
        var g=Setup();var v=g.SpawnReckless(true);v.willHitWall=false;
        v.model.localPosition=new Vector3(-.5f,0,17);v.previousPosition=new Vector3(-2,0,19);v.model.localRotation=Quaternion.Euler(0,150,-3);
        var bike=g.SpawnTraffic(3,17,1);bike.model.localPosition=new Vector3(1.8f,0,17);bike.previousPosition=bike.model.localPosition;
        var car=g.SpawnTraffic(1,17,0);car.model.localPosition=new Vector3(-2,0,17);car.previousPosition=car.model.localPosition;
        typeof(PhoVietGame).GetMethod("ResolveRecklessRams",F).Invoke(g,null);
        foreach(var t in new[]{bike,car})typeof(PhoVietGame).GetMethod("TickKnockedVehicle",F).Invoke(g,new object[]{t,.2f});
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return "Truck ram preview ready";
    }
    public static string Fire()
    {
        var g=Setup();var v=g.SpawnReckless(true);v.model.localPosition=new Vector3(-9,0,17);
        typeof(PhoVietGame).GetMethod("CheckBuildingImpact",F).Invoke(g,new object[]{v});
        v.smoke.Simulate(2,true,true,false);v.fire.Simulate(.8f,true,true,false);v.smoke.Play();v.fire.Play();
        g.SpawnTraffic(3,25,1);g.SpawnTraffic(0,38,2);
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return "Burning truck preview ready";
    }
    public static string Defense()
    {
        var g=Setup();var v=g.SpawnReckless(false);v.model.localPosition=new Vector3(4.65f,0,.8f);
        v.routeDetached=true;v.routeVelocity=Vector3.forward*17;v.speed=v.baseTrafficSpeed=17;v.model.localRotation=Quaternion.identity;g.TryKick();for(int i=0;i<100&&!v.knocked;i++)typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{.01f});
        typeof(PhoVietGame).GetField("kickTime",F).SetValue(g,.19f);typeof(PhoVietGame).GetMethod("AnimateKick",F).Invoke(g,null);
        return "Self-defense preview ready; chased="+g.IsChased;
    }
}

