using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureRouteEvidence
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame g;static bool running,reckless,surprises;static float scale;static int side=1,phase,frames,count;
    static double next;static Quaternion globalHeading,previousCamera;static float maxCameraStep;static int completed;
    public static object Main()
    {
        if(running)return new{started=true};g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play mode required");
        scale=Time.timeScale;Time.timeScale=0;reckless=g.recklessTrafficEnabled;surprises=g.trafficSurprises;
        side=1;count=0;maxCameraStep=0;frames=0;Setup();running=true;next=EditorApplication.timeSinceStartup+.7;EditorApplication.update+=Update;
        return new{started=true};
    }
    static void Set(string n,object v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
    static void Step(float dt)=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
    static void Setup()
    {
        g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        Set("spawnTimer",100000f);Set("nextIntersectionDistance",float.PositiveInfinity);
        int lane=side<0?0:3;typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);
        while(g.NextJunctionDistance>23)Step(.02f);
        typeof(PhoVietGame).GetMethod("PositionCamera",F).Invoke(g,new object[]{true});
        g.MoveLane(side);phase=0;globalHeading=Quaternion.identity;previousCamera=g.rideCamera.transform.localRotation;completed=0;
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            if(phase==0){Capture(side<0?"left-approach":"right-approach");phase=1;}
            Step(.04f);frames++;
            if(g.CompletedTurns>completed){globalHeading*=Quaternion.Euler(0,side*90,0);completed=g.CompletedTurns;}
            Quaternion camera=globalHeading*g.rideCamera.transform.localRotation;
            maxCameraStep=Mathf.Max(maxCameraStep,Quaternion.Angle(previousCamera,camera));previousCamera=camera;
            if(phase==1&&Mathf.Abs(g.TurnYaw)>38){Capture(side<0?"left-mid":"right-mid");phase=2;}
            if(phase==2&&!g.IsTurning&&g.CompletedTurns==1){phase=3;count=12;}
            if(phase==3&&--count<=0)
            {
                Capture(side<0?"left-exit":"right-exit");
                if(side<0)
                {
                    File.WriteAllText("Captures/routes-frame-evidence.json",JsonUtility.ToJson(new Evidence{success=true,frames=frames,maxCameraDegreesPerStep=maxCameraStep}));Finish();return;
                }
                side=-1;Setup();next=EditorApplication.timeSinceStartup+.7;return;
            }
            next=EditorApplication.timeSinceStartup+.04;
        }
        catch(Exception e){File.WriteAllText("Captures/routes-frame-evidence.json",JsonUtility.ToJson(new Evidence{success=false,error=e.ToString()}));Finish();}
    }
    static void Capture(string name)
    {
        var c=g.rideCamera;var rt=new RenderTexture(1440,1000,24);var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes("Captures/routes-"+name+".png",image.EncodeToPNG());}
        finally{c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
    }
    static void Finish(){running=false;EditorApplication.update-=Update;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.ResetRide();Time.timeScale=scale;}
    [Serializable]public sealed class Evidence{public bool success;public int frames;public float maxCameraDegreesPerStep;public string error;}
}
