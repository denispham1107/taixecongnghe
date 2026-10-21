using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureStreetColors
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame g;static float scale;static bool jobs,surprises,reckless,exits,running,waiting;static int phase;static double next;static string mode;
    static readonly string[] names={"welcome","morning","night"};
    static readonly float[] seconds={0,0,30};
    static object Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
    static void Set(string n,object value)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,value);
    public static object Main()
    {
        if(running)return new{started=true};if(!Application.isPlaying)throw new Exception("Play Mode required");
        g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;surprises=g.trafficSurprises;reckless=g.recklessTrafficEnabled;exits=g.turnExitTrafficEnabled;
        Time.timeScale=0;phase=0;running=true;waiting=false;mode=Screen.width<Screen.height?"portrait":"wide";
        Setup();next=EditorApplication.timeSinceStartup+.9;EditorApplication.update+=Update;
        return new{started=true,frames=names.Length,mode};
    }
    static void Setup()
    {
        g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;
        if(phase==0)g.ResetRide();else g.RestartRide();
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);
        int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3),bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);
        g.SpawnTraffic(1,18,car);g.SpawnTraffic(0,30,bike);
        int other=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3);g.SpawnTraffic(3,29,other);
        int truck=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfLength>3);g.SpawnTraffic(2,59,truck);
        g.daylight.SetCycleTime(seconds[phase]);Call("PositionCamera",true);g.daylight.UpdateLamps();
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            string path="Captures/street-colors-native-"+mode+"-"+names[phase]+".png";
            if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.9;return;}
            if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Screenshot missing "+path);
            phase++;waiting=false;
            if(phase==names.Length){Finish();File.WriteAllText("Captures/street-colors-"+mode+"-visual-results.json","{\"success\":true,\"frames\":3}");return;}
            Setup();next=EditorApplication.timeSinceStartup+.9;
        }
        catch(Exception e){File.WriteAllText("Captures/street-colors-"+mode+"-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
    }
    static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
    [Serializable]class Error{public string error;}
}
