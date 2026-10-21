using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureNightJumpLighting
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame g;static float scale;static bool jobs,surprises,reckless,exits,running,waiting;static int phase;static double next;static string mode;
    static readonly string[] names={"ground","wheelie","rising","apex","landing","roof"};
    static readonly float[] times={0,.18f,.5f,.9f,1.54f,0};
    static object Call(string n,params object[] args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
    static void Set(string n,object value)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,value);
    public static object Main()
    {
        if(running)return new{started=true};if(!Application.isPlaying)throw new Exception("Play Mode required");
        g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();scale=Time.timeScale;jobs=g.jobsEnabled;surprises=g.trafficSurprises;reckless=g.recklessTrafficEnabled;exits=g.turnExitTrafficEnabled;
        Time.timeScale=0;phase=0;running=true;waiting=false;mode=Screen.width<Screen.height?"portrait":"wide";
        Setup();next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;
        return new{started=true,frames=names.Length,mode};
    }
    static void Setup()
    {
        g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();
        foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}Set("spawnTimer",10000f);Set("nextIntersectionDistance",float.PositiveInfinity);
        var animator=g.rider.GetComponent<BikeVisualAnimator>();
        if(phase>0&&phase<5){g.TryJump();for(int i=0;i<Mathf.RoundToInt(times[phase]*100);i++){Call("TickRide",.01f);animator.Advance(.01f);}}
        if(phase==5)
        {
            int index=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfLength>3);var truck=g.SpawnTraffic(2,0,index);truck.brakeCooldown=truck.laneCooldown=1000;
            Set("roofVehicle",truck);Set("jumpHeight",3.2f);Set("jumpPreviousHeight",3.2f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],3.2f,0);
        }
        animator.Advance(.01f);g.daylight.SetCycleTime(30);Call("PositionCamera",true);g.daylight.UpdateLamps();
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            string path="Captures/night-jump-native-"+mode+"-"+names[phase]+".png";
            if(!waiting){ScreenCapture.CaptureScreenshot(path);waiting=true;next=EditorApplication.timeSinceStartup+.8;return;}
            if(!File.Exists(path)||new FileInfo(path).Length==0)throw new Exception("Screenshot missing "+path);
            phase++;waiting=false;
            if(phase==names.Length){Finish();File.WriteAllText("Captures/night-jump-"+mode+"-visual-results.json","{\"success\":true,\"frames\":6}");return;}
            Setup();next=EditorApplication.timeSinceStartup+.8;
        }
        catch(Exception e){File.WriteAllText("Captures/night-jump-"+mode+"-visual-results.json",JsonUtility.ToJson(new Error{error=e.ToString()}));Finish();}
    }
    static void Finish(){running=false;EditorApplication.update-=Update;g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;g.ResetRide();Time.timeScale=scale;}
    [Serializable]class Error{public string error;}
}
