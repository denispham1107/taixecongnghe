using System.Reflection;
using UnityEngine;
using PhoViet;

public static class PreviewRearPolice
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Set(PhoVietGame g,string n,float v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
    static void Call(PhoVietGame g,string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
    static PhoVietGame Setup()
    {
        Time.timeScale=0;var g=Object.FindAnyObjectByType<PhoVietGame>();g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        Set(g,"spawnTimer",10000);Set(g,"nextIntersectionDistance",float.PositiveInfinity);Set(g,"collisionImmunity",10000);Set(g,"noticeTimer",0);return g;
    }
    public static string Rear()
    {
        var g=Setup();var truck=g.SpawnReckless(true);truck.lane=3;truck.startPosition=new Vector3(4.65f,0,truck.startPosition.z);truck.model.localPosition=new Vector3(truck.rearStartX,0,truck.startPosition.z);truck.swerveDirection=-1;truck.willHitWall=false;
        int guard=0;while(truck.model.localPosition.z<-4.5f&&guard++<500)Call(g,"TickRide",.02f);
        Set(g,"noticeTimer",0);return "Truck enters through lower rear edge; z="+truck.model.localPosition.z;
    }
    public static string Alongside()
    {
        var g=Object.FindAnyObjectByType<PhoVietGame>();var truck=g.vehicles.Find(v=>v.active&&v.isReckless&&v.halfLength>3);
        int guard=0;while(truck.model.localPosition.z<2&&guard++<500)Call(g,"TickRide",.02f);Set(g,"noticeTimer",0);return "Overtaking truck is now alongside, following its continuous trajectory";
    }
    static PhoVietGame Chase()
    {
        var g=Setup();Call(g,"TickRide",.02f);Call(g,"StartChase");
        g.policeBike.localPosition=new Vector3(.45f,0,-5.5f);typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,5.5f);typeof(PhoVietGame).GetProperty("PoliceSpeed").SetValue(g,17f);
        typeof(PhoVietGame).GetField("policeArrived",F).SetValue(g,true);Call(g,"PositionCamera",true);Set(g,"noticeTimer",0);return g;
    }
    public static string Avoid()
    {
        var g=Chase();var car=g.SpawnTraffic(2,8,0);car.speed=0;car.baseTrafficSpeed=0;car.model.localPosition=new Vector3(.45f,0,8);car.previousPosition=car.model.localPosition;
        for(int i=0;i<25;i++)
        {car.previousPosition=car.model.localPosition;car.model.localPosition-=Vector3.forward*g.Speed*.02f;Call(g,"TickChase",.02f);}
        Set(g,"noticeTimer",0);return "Police steers around stationary taxi; avoidances="+g.PoliceAvoidances;
    }
    public static string Fallen()
    {
        var g=Chase();g.policeBike.localPosition=new Vector3(.45f,0,-1.5f);typeof(PhoVietGame).GetProperty("PoliceDistance").SetValue(g,1.5f);
        var car=g.SpawnTraffic(2,-1.4f,0);car.speed=0;car.baseTrafficSpeed=0;car.model.localPosition=new Vector3(.45f,0,-1.4f);car.previousPosition=car.model.localPosition;
        Call(g,"TickChase",.02f);Call(g,"RetireVehicle",car);Call(g,"TickChase",.2f);Set(g,"noticeTimer",0);
        return "Police motorcycle down; recovery remaining="+g.PoliceFallRemaining;
    }
}
