// Capture on separate editor frames so GPU skin matrices reflect the sampled bone pose.
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
public static class CaptureBikeEvidence
{
    static PhoVietGame game;static Camera camera;static BikeVisualAnimator hero;static PhoVietGame.TrafficVehicle target;
    static float scale,fov,x;static bool surprises,reckless;static Vector3 cameraPosition;static Quaternion cameraRotation;
    static int phase;static double next;static bool running;
    static Vector3 shotPosition,shotTarget;static float shotFov;
    static readonly string[] names={"ride","kick-right","victim-fall","player-fall"};
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(running)return new{started=true,phase};
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();camera=game.rideCamera;
        scale=Time.timeScale;fov=camera.fieldOfView;cameraPosition=camera.transform.localPosition;cameraRotation=camera.transform.localRotation;
        surprises=game.trafficSurprises;reckless=game.recklessTrafficEnabled;
        Time.timeScale=0;game.RestartRide();game.trafficSurprises=false;game.recklessTrafficEnabled=false;
        foreach(var v in game.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(game,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(game,float.PositiveInfinity);
        hero=game.rider.GetComponent<BikeVisualAnimator>();x=game.rider.position.x;
        SetView(new Vector3(x+2.8f,2.7f,-4.6f),new Vector3(x,.98f,0),44);hero.Advance(.15f);
        foreach(var skin in hero.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
        phase=0;running=true;next=EditorApplication.timeSinceStartup+.8;EditorApplication.update+=Update;
        return new{started=true,frames=4};
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            Capture(names[phase]);phase++;
            if(phase==4){Finish();File.WriteAllText("Captures/blender-art-frame-evidence.json","{\"success\":true,\"frames\":4}");return;}
            if(phase==1)
            {
                target=game.SpawnTraffic(3,0,1);game.TryKick();hero.Advance(.27f);
                SetView(new Vector3(x,2.5f,-4.5f),new Vector3(x+.25f,1,0),45);
                foreach(var skin in target.model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
            }
            else if(phase==2)
            {
                target.knockAge=.6f;target.model.GetComponent<BikeVisualAnimator>().Advance(.02f);
                SetView(new Vector3(x+1.4f,3.4f,-5.5f),new Vector3(x+1.3f,.8f,0),48);
            }
            else
            {
                target.active=false;target.model.gameObject.SetActive(false);game.Crash();typeof(PhoVietGame).GetField("fallTime",F).SetValue(game,.7f);hero.Advance(.7f);
                SetView(new Vector3(x+2.6f,2.1f,-4.0f),new Vector3(x-.2f,.55f,0),40);
            }
            next=EditorApplication.timeSinceStartup+.8;
        }
        catch(Exception error){Finish();File.WriteAllText("Captures/blender-art-frame-evidence.json",JsonUtility.ToJson(new Failure{error=error.ToString()}));}
    }
    static void SetView(Vector3 position,Vector3 targetPoint,float field){shotPosition=position;shotTarget=targetPoint;shotFov=field;}
    static void Capture(string name)
    {
        var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);
        var old=camera.targetTexture;var active=RenderTexture.active;
        try{camera.transform.position=shotPosition;camera.transform.LookAt(shotTarget);camera.fieldOfView=shotFov;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/blender-"+name+".png",image.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
    static void Finish()
    {
        running=false;EditorApplication.update-=Update;game.trafficSurprises=surprises;game.recklessTrafficEnabled=reckless;
        foreach(var skin in hero.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=false;
        if(target!=null)foreach(var skin in target.model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=false;
        game.ResetRide();Time.timeScale=scale;camera.transform.localPosition=cameraPosition;camera.transform.localRotation=cameraRotation;camera.fieldOfView=fov;
    }
    [Serializable]sealed class Failure{public bool success=false;public string error;}
}
