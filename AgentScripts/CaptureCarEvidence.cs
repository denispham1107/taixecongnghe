using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
// Leave real editor frames between skeletal poses and GPU screenshots.
public static class CaptureCarEvidence
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static PhoVietGame game;static Camera camera;static PhoVietGame.TrafficVehicle[] cars;
    static bool running,surprises,reckless;static float scale,fov;static Vector3 cameraPosition;static Quaternion cameraRotation;
    static Vector3 shotPosition,shotTarget;static int phase;static double next;
    static readonly string[] names={"traffic","collision","truck-fire"};
    public static object Main()
    {
        if(running)return new{started=true,phase};if(!Application.isPlaying)throw new Exception("Play Mode required");
        game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();camera=game.rideCamera;
        scale=Time.timeScale;fov=camera.fieldOfView;cameraPosition=camera.transform.position;cameraRotation=camera.transform.rotation;
        surprises=game.trafficSurprises;reckless=game.recklessTrafficEnabled;Time.timeScale=0;game.RestartRide();game.trafficSurprises=false;game.recklessTrafficEnabled=false;
        foreach(var v in game.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
        typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(game,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(game,float.PositiveInfinity);
        cars=game.vehicles.Where(v=>v.halfWidth>.7f).GroupBy(v=>v.model.GetComponent<CarVisualAnimator>().AssetName).Select(g=>g.First()).ToArray();
        for(int i=0;i<cars.Length;i++)
        {
            var v=cars[i];v.active=true;v.model.gameObject.SetActive(true);v.speed=v.baseTrafficSpeed=12;
            v.model.localPosition=new Vector3(PhoVietGame.LaneX[i%4],0,9+i/4*13+(i%2)*7);v.previousPosition=v.model.localPosition;v.model.localRotation=Quaternion.Euler(0,i<2?180:0,0);
            v.model.GetComponent<CarVisualAnimator>().Advance(.17f);
            foreach(var skin in v.model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
        }
        shotPosition=new Vector3(0,7.0f,-10);shotTarget=new Vector3(0,1,17);
        phase=0;running=true;next=EditorApplication.timeSinceStartup+.9;EditorApplication.update+=Update;return new{started=true,frames=3};
    }
    static void Update()
    {
        if(!running||EditorApplication.timeSinceStartup<next)return;
        try
        {
            Capture(names[phase]);phase++;
            if(phase==3){Finish();File.WriteAllText("Captures/car-art-frame-evidence.json","{\"success\":true,\"frames\":3}");return;}
            foreach(var v in cars){v.active=false;v.model.gameObject.SetActive(false);}
            if(phase==1)
            {
                var a=cars.First(v=>v.model.GetComponent<CarVisualAnimator>().AssetName=="PV_Sedan");var b=cars.First(v=>v.model.GetComponent<CarVisualAnimator>().AssetName=="PV_Hatchback");
                foreach(var v in new[]{a,b}){v.active=true;v.model.gameObject.SetActive(true);v.knocked=false;v.model.localRotation=Quaternion.identity;}
                a.model.localPosition=new Vector3(-1.55f,0,15);b.model.localPosition=new Vector3(-1.15f,0,19);a.previousPosition=a.model.localPosition;b.previousPosition=b.model.localPosition;
                a.motionVelocity=new Vector3(0,0,27);b.motionVelocity=new Vector3(0,0,5);
                typeof(PhoVietGame).GetMethod("ResolveVehicleCollisions",F).Invoke(game,null);
                foreach(var v in new[]{a,b}){typeof(PhoVietGame).GetMethod("TickCollisionBody",F).Invoke(game,new object[]{v,.16f});v.model.GetComponent<CarVisualAnimator>().Advance(.16f);}
                shotPosition=new Vector3(-6.8f,3.7f,9);shotTarget=new Vector3(-1.3f,.85f,16);
            }
            else
            {
                var v=cars.First(v=>v.eventOnly);v=game.SpawnReckless(true);v.model.localPosition=new Vector3(9.5f,0,20);
                typeof(PhoVietGame).GetMethod("CheckBuildingImpact",F).Invoke(game,new object[]{v});v.model.GetComponent<CarVisualAnimator>().Advance(.18f);
                v.smoke.Simulate(1.1f,true,false);v.fire.Simulate(.55f,true,false);
                shotPosition=new Vector3(3,5.2f,11.0f);shotTarget=new Vector3(7.7f,1.6f,20);
            }
            next=EditorApplication.timeSinceStartup+.9;
        }
        catch(Exception e){Finish();File.WriteAllText("Captures/car-art-frame-evidence.json",JsonUtility.ToJson(new Failure{error=e.ToString()}));}
    }
    static void Capture(string name)
    {
        var rt=new RenderTexture(1440,1000,24);var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);var old=camera.targetTexture;var active=RenderTexture.active;
        try{camera.transform.position=shotPosition;camera.transform.LookAt(shotTarget);camera.fieldOfView=50;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes("Captures/blender-cars-"+name+".png",image.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
    static void Finish()
    {
        running=false;EditorApplication.update-=Update;
        foreach(var v in cars)foreach(var skin in v.model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=false;
        game.trafficSurprises=surprises;game.recklessTrafficEnabled=reckless;game.ResetRide();Time.timeScale=scale;camera.transform.position=cameraPosition;camera.transform.rotation=cameraRotation;camera.fieldOfView=fov;
    }
    [Serializable]sealed class Failure{public bool success=false;public string error;}
}
