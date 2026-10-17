using System.Reflection;
using UnityEngine;
using PhoViet;

public static class PreviewTraffic
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Set(PhoVietGame g,string n,float v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
    static void Call(PhoVietGame g,string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
    static PhoVietGame Setup()
    {
        Time.timeScale=0;var g=Object.FindAnyObjectByType<PhoVietGame>();g.trafficSurprises=true;g.RestartRide();g.recklessTrafficEnabled=false;
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        foreach(string n in new[]{"spawnTimer","nextIntersectionDistance"})Set(g,n,float.PositiveInfinity);
        Set(g,"noticeTimer",0);return g;
    }
    static PhoVietGame.TrafficVehicle Spawn(PhoVietGame g,int kind,int lane,float z)
    {
        int i=g.vehicles.FindIndex(v=>!v.eventOnly&&!v.active&&(kind==2?v.halfLength>3:kind==1?v.halfWidth>.7f&&v.halfLength<3:v.halfWidth<.7f));
        var t=g.SpawnTraffic(lane,z,i);t.laneCooldown=10000;t.brakeCooldown=10000;return t;
    }
    public static string Brake()
    {
        var g=Setup();var truck=Spawn(g,2,2,15);var car=Spawn(g,1,3,9);
        g.BeginSuddenBrake(truck);g.BeginSuddenBrake(car);Call(g,"TickLongitudinalVehicle",truck,.2f);Call(g,"TickLongitudinalVehicle",car,.2f);
        Spawn(g,0,0,25);return "Braking truck and car; dynamic brake lamps";
    }
    public static string CutIn()
    {
        var g=Setup();var truck=Spawn(g,2,3,9);var car=Spawn(g,1,2,20);
        g.BeginSuddenLaneChange(truck,2);g.BeginSuddenLaneChange(car,3);
        Call(g,"TickLongitudinalVehicle",truck,.4f);Call(g,"TickLongitudinalVehicle",car,.3f);Spawn(g,0,0,25);return "Truck and car cut-ins";
    }
    public static string Turns()
    {
        var g=Setup();while(g.Distance<45)Call(g,"TickRide",.02f);
        var car=g.SpawnCrossTraffic(1,67.2f-g.Distance,true,8);car.turnChoice=-1;car.model.localPosition=new Vector3(1.55f,0,67.2f-g.Distance);Call(g,"TickIntersectionVehicle",car,.3f);
        var bike=g.SpawnCrossTraffic(-1,72.8f-g.Distance,false,8);bike.turnChoice=-1;bike.model.localPosition=new Vector3(-2.05f,0,72.8f-g.Distance);Call(g,"TickIntersectionVehicle",bike,.28f);
        return "Intersection turns, both approaches";
    }
    public static string Collision()
    {
        var g=Setup();var truck=Spawn(g,2,2,12);truck.model.localPosition=new Vector3(-1,0,12);truck.previousPosition=truck.model.localPosition;truck.motionVelocity=new Vector3(8,0,15);
        var bike=Spawn(g,0,3,11);bike.model.localPosition=new Vector3(.4f,0,11);bike.previousPosition=bike.model.localPosition;bike.motionVelocity=new Vector3(0,0,6);
        Call(g,"ResolveVehicleCollisions");Call(g,"TickCollisionBody",truck,.18f);Call(g,"TickCollisionBody",bike,.18f);
        Spawn(g,1,3,25);Set(g,"noticeTimer",0);return "Truck versus motorcycle mass-based response";
    }
}

