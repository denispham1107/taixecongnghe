using System.Reflection;
using UnityEngine;
using PhoViet;

public static class PreviewOncoming
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame Setup()
    {
        Time.timeScale=0;var g=Object.FindAnyObjectByType<PhoVietGame>();g.RestartRide();g.recklessTrafficEnabled=false;
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        foreach(string n in new[]{"spawnTimer","nextIntersectionDistance"})typeof(PhoVietGame).GetField(n,F).SetValue(g,float.PositiveInfinity);
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return g;
    }
    public static string Invade()
    {
        var g=Setup();var truck=g.SpawnReckless(true,true);truck.willHitWall=false;
        truck.startPosition=new Vector3(-4.65f,0,55);truck.model.localPosition=truck.startPosition;
        typeof(PhoVietGame).GetMethod("TickRecklessVehicle",F).Invoke(g,new object[]{truck,.95f});
        var bike=g.SpawnReckless(false,true);bike.willHitWall=false;bike.model.localPosition=new Vector3(-1.55f,0,18);bike.startPosition=bike.model.localPosition;
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return "Oncoming truck invading right-hand lanes";
    }
    public static string Fire()
    {
        var g=Setup();var truck=g.SpawnReckless(true,true);truck.model.localPosition=new Vector3(9,0,18);
        typeof(PhoVietGame).GetMethod("CheckBuildingImpact",F).Invoke(g,new object[]{truck});
        truck.smoke.Simulate(2,true,true,false);truck.fire.Simulate(.8f,true,true,false);truck.smoke.Play();truck.fire.Play();
        typeof(PhoVietGame).GetField("noticeTimer",F).SetValue(g,0f);return "Oncoming truck crashed into right-side house, smoke and fire";
    }
}
