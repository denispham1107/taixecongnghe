using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PhoViet;

// Play Mode evidence for the Meshy motorcycles (pho_viet_wave.py): ride camera, close views, chase and night.
public static class CaptureBikesMeshy
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        bool jobs=g.jobsEnabled,reckless=g.recklessTrafficEnabled;float scale=Time.timeScale;
        var tick=typeof(PhoVietGame).GetMethod("TickRide",F);var camera=typeof(PhoVietGame).GetMethod("PositionCamera",F);
        var drivers=g.GetComponentsInChildren<BikeVisualAnimator>(true);
        Action<int> run=steps=>{for(int i=0;i<steps;i++){tick.Invoke(g,new object[]{1/60f});camera.Invoke(g,new object[]{true});foreach(var d in drivers)if(d.isActiveAndEnabled)d.Advance(1/60f);}};
        try
        {
            Time.timeScale=0;g.jobsEnabled=false;g.recklessTrafficEnabled=false;g.RestartRide();
            typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            int bikes=0;
            foreach(var (lane,z) in new[]{(3,7f),(1,22f),(2,15f),(0,34f),(3,28f)})
            {
                int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f&&v.model.GetComponent<BikeVisualAnimator>().AssetName==new[]{"PV_CityScooter","PV_Underbone","PV_SportBike"}[bikes%3]);
                if(index>=0){g.SpawnTraffic(lane,z,index);bikes++;}
            }
            g.trafficSurprises=false;run(30);
            var files=new System.Collections.Generic.List<string>();
            files.Add(Render(g.rideCamera,"Captures/meshy-bikes-game.png",1440,900));
            var probe=new GameObject("Capture probe").AddComponent<Camera>();probe.fieldOfView=32;probe.nearClipPlane=.1f;probe.farClipPlane=200;
            probe.clearFlags=CameraClearFlags.Skybox;probe.backgroundColor=g.rideCamera.backgroundColor;probe.clearFlags=g.rideCamera.clearFlags;
            var hero=g.rider;
            probe.transform.position=hero.TransformPoint(new Vector3(2.6f,2.0f,-3.4f));probe.transform.LookAt(hero.TransformPoint(new Vector3(0,1.05f,.1f)));
            files.Add(Render(probe,"Captures/meshy-bikes-hero-rear.png",1200,900));
            probe.transform.position=hero.TransformPoint(new Vector3(2.9f,1.5f,2.6f));probe.transform.LookAt(hero.TransformPoint(new Vector3(0,1.0f,0)));
            files.Add(Render(probe,"Captures/meshy-bikes-hero-front.png",1200,900));
            // Pursuit + reckless rider evidence with the real chase camera, then a close side view.
            g.RestartRide();typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            typeof(PhoVietGame).GetMethod("StartChase",F).Invoke(g,null);run(70);
            var racer=g.SpawnReckless(false,true);run(25);
            files.Add(Render(g.rideCamera,"Captures/meshy-bikes-chase.png",1440,900));
            var police=g.policeBike;
            probe.transform.position=police.TransformPoint(new Vector3(-2.9f,1.7f,2.4f));probe.transform.LookAt(police.TransformPoint(new Vector3(0,1.0f,0)));
            files.Add(Render(probe,"Captures/meshy-bikes-police.png",1200,900));
            if(racer!=null){probe.transform.position=racer.model.TransformPoint(new Vector3(2.8f,1.6f,2.6f));probe.transform.LookAt(racer.model.TransformPoint(new Vector3(0,1.0f,0)));files.Add(Render(probe,"Captures/meshy-bikes-racer.png",1200,900));}
            // Night: automatic headlights/tail lamps on the Wave bodies.
            g.RestartRide();typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            foreach(var (lane,z) in new[]{(3,9f),(1,24f),(2,17f)}){int i=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly&&v.halfWidth<.7f);if(i>=0)g.SpawnTraffic(lane,z,i);}
            run(20);if(g.daylight!=null){g.daylight.SetCycleTime(30);g.daylight.UpdateLamps();}
            files.Add(Render(g.rideCamera,"Captures/meshy-bikes-night.png",1440,900));
            probe.transform.position=hero.TransformPoint(new Vector3(2.7f,1.6f,3.2f));probe.transform.LookAt(hero.TransformPoint(new Vector3(0,.9f,.3f)));
            files.Add(Render(probe,"Captures/meshy-bikes-night-hero.png",1200,900));
            if(g.daylight!=null)g.daylight.SetCycleTime(0);
            UnityEngine.Object.Destroy(probe.gameObject);
            return new{success=true,files,bikes};
        }
        finally{g.jobsEnabled=jobs;g.recklessTrafficEnabled=reckless;g.trafficSurprises=true;Time.timeScale=scale;}
    }
    static string Render(Camera camera,string path,int w,int h)
    {
        var old=camera.targetTexture;var rt=new RenderTexture(w,h,24){antiAliasing=4};var image=new Texture2D(w,h,TextureFormat.RGB24,false);var active=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());return path;}
        finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
