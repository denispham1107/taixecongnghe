using System;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using PhoViet;
public static class CaptureBikeActions
{
    public static object Main()
    {
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var camera=g.rideCamera;
        float scale=Time.timeScale;var oldTarget=camera.targetTexture;Vector3 oldPos=camera.transform.localPosition;Quaternion oldRot=camera.transform.localRotation;float fov=camera.fieldOfView;
        var rt=new RenderTexture(1200,900,24);var image=new Texture2D(1200,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        bool surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled;
        Action<string> capture=name=>
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes("Captures/blender-"+name+".png",image.EncodeToPNG());
        };
        try
        {
            Time.timeScale=0;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            var hero=g.rider.GetComponent<BikeVisualAnimator>();float x=g.rider.position.x;
            camera.transform.position=new Vector3(x+2.8f,2.7f,-4.6f);camera.transform.LookAt(new Vector3(x,.98f,0));camera.fieldOfView=44;
            hero.Advance(.15f);capture("ride");
            var target=g.SpawnTraffic(3,0,1);g.TryKick();for(int i=0;i<100&&(!g.KickSwingActive||g.KickSwingPose<.48f);i++)typeof(PhoVietGame).GetMethod("TickRide",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,new object[]{.01f});hero.Advance(0);capture("kick-right");
            target.knockAge=.6f;target.model.localRotation=Quaternion.Euler(0,0,-78);target.model.GetComponent<BikeVisualAnimator>().Advance(.02f);
            camera.transform.position=new Vector3(x+1.5f,3.6f,-5.3f);camera.transform.LookAt(new Vector3(x+1.3f,.7f,0));camera.fieldOfView=50;capture("victim-fall");
            target.active=false;target.model.gameObject.SetActive(false);g.Crash();g.rider.localRotation=Quaternion.Euler(-8,12,-78);g.rider.localPosition=new Vector3(x,-.25f,0);hero.Advance(.7f);
            camera.transform.position=new Vector3(x+2.8f,2.1f,-4.0f);camera.transform.LookAt(new Vector3(x,.55f,0));camera.fieldOfView=40;capture("player-fall");
            var renderer=hero.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r=>r.name.EndsWith("_Body"));var baked=new Mesh();renderer.BakeMesh(baked);
            float minY=baked.vertices.Min(v=>renderer.transform.TransformPoint(v).y);UnityEngine.Object.DestroyImmediate(baked);
            return new{success=true,groundMinimum=minY,playerClip=hero.CurrentClip,files=new[]{"blender-ride.png","blender-kick-right.png","blender-victim-fall.png","blender-player-fall.png"}};
        }
        finally
        {
            g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.ResetRide();Time.timeScale=scale;
            camera.targetTexture=oldTarget;camera.transform.localPosition=oldPos;camera.transform.localRotation=oldRot;camera.fieldOfView=fov;RenderTexture.active=previous;
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
