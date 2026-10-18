using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureJunctionFixEvidence
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame g;static bool running,exit,reckless,surprises;static float scale,after;static int side,phase,frames;static double next;
    public static object Main()
    {
        if(running)return new{started=true};g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
        scale=Time.timeScale;exit=g.turnExitTrafficEnabled;reckless=g.recklessTrafficEnabled;surprises=g.trafficSurprises;Time.timeScale=0;frames=0;side=1;Setup();
        running=true;next=EditorApplication.timeSinceStartup+.7;EditorApplication.update+=Update;return new{started=true};
    }
    static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
    static void Step(float dt)=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
    static void Setup()
    {
        g.turnExitTrafficEnabled=true;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        Set("spawnTimer",100000f);Set("nextIntersectionDistance",float.PositiveInfinity);
        while(g.NextJunctionDistance>(side>0?0:-6))Step(.02f);
        int lane=side<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
        typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});phase=0;after=0;
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            if(phase==0)
            {
                if(side>0)
                {
                    var c=g.rideCamera;Vector3 pos=c.transform.position;Quaternion rot=c.transform.rotation;float fov=c.fieldOfView;
                    c.transform.position=new Vector3(0,34,-21);c.transform.LookAt(new Vector3(0,0,g.NextJunctionDistance));c.fieldOfView=64;Capture("arrows");c.transform.position=pos;c.transform.rotation=rot;c.fieldOfView=fov;
                }
                g.MoveLane(side);phase=1;
            }
            Step(.04f);frames++;Set("spawnTimer",100000f);Set("nextIntersectionDistance",float.PositiveInfinity);
            if(phase==1&&Mathf.Abs(g.TurnYaw)>75){Capture(side<0?"left-mid":"right-mid");phase=2;}
            if(phase==2&&!g.IsTurning&&g.CompletedTurns==1)
            {
                phase=3;after=0;
                // Demonstrate ordinary post-exit steering to avoid a fast rear entrant.
                if(side>0){g.MoveLane(-1);g.MoveLane(-1);}
            }
            if(phase==3)
            {
                after+=.04f;
                if(after>=2.65f)
                {
                    Capture(side<0?"left-traffic":"right-traffic");
                    if(side<0){File.WriteAllText("Captures/junction-fix-frame-evidence.json",JsonUtility.ToJson(new Evidence{success=true,frames=frames,rear=g.ExitRearSpawns,oncoming=g.ExitOncomingSpawns}));Finish();return;}
                    side=-1;Setup();next=EditorApplication.timeSinceStartup+.7;return;
                }
            }
            next=EditorApplication.timeSinceStartup+.055;
        }
        catch(Exception e){File.WriteAllText("Captures/junction-fix-frame-evidence.json",JsonUtility.ToJson(new Evidence{error=e.ToString()}));Finish();}
    }
    static void Capture(string name)
    {
        var c=g.rideCamera;var rt=new RenderTexture(1440,1000,24);var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes("Captures/junction-fix-"+name+".png",image.EncodeToPNG());}
        finally{c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
    static void Finish(){running=false;EditorApplication.update-=Update;g.turnExitTrafficEnabled=exit;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.ResetRide();Time.timeScale=scale;}
    [Serializable]public sealed class Evidence{public bool success;public int frames,rear,oncoming;public string error;}
}
