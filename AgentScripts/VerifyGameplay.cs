using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyGameplay
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Run in Play Mode");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();bool originalReckless=g.recklessTrafficEnabled;
        bool originalSurprises=g.trafficSurprises;g.trafficSurprises=false;int originalBest=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
        var passed=new List<string>();
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);passed.Add(name);};
        var tick=typeof(PhoVietGame).GetMethod("TickRide",BindingFlags.Instance|BindingFlags.NonPublic);
        Action<float> step=dt=>tick.Invoke(g,new object[]{dt});
        Action clear=()=>{g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("nextIntersectionDistance",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,float.PositiveInfinity);};
        try
        {
            g.ResetRide();check(g.State==PhoVietGame.RideState.Ready&&g.Distance==0,"Initial ready state and zero distance");
            g.MoveLane(-1);check(g.CurrentLane==2,"Lane input ignored before starting");
            g.StartRide();clear();
            typeof(PhoVietGame).GetField("spawnTimer",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,10000f);
            check(g.State==PhoVietGame.RideState.Riding&&g.Speed>0,"Start begins ride");
            for(int i=0;i<8;i++)g.MoveLane(-1);check(g.CurrentLane==0,"Left lane clamps to road");
            // Đổi làn kiểu đánh lái (24/10/2026) tăng tốc rồi hãm êm: 2 làn ~0,59 s, 3 làn ~0,8 s (kiểu trượt cũ 0,41/0,62 s).
            for(int i=0;i<35;i++)step(.02f);
            check(Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[0])<.01f,"Lane transition reaches left lane smoothly");
            for(int i=0;i<8;i++)g.MoveLane(1);check(g.CurrentLane==3,"Right lane clamps to road");
            for(int i=0;i<50;i++)step(.02f);
            check(Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[3])<.01f,"Lane transition reaches right lane");
            float distance=g.Distance;g.TogglePause();check(g.State==PhoVietGame.RideState.Paused,"Pause state");
            g.MoveLane(-1);check(g.CurrentLane==3&&g.Distance==distance,"Pause blocks lane input");g.TogglePause();
            check(g.State==PhoVietGame.RideState.Riding,"Resume state");
            var same=g.SpawnTraffic(3,35,0);var opposite=g.SpawnTraffic(0,35,1);
            check(same.speed>0&&opposite.speed<0,"Same direction and oncoming traffic velocities");
            clear();g.SpawnTraffic(2,0,1);step(.02f);check(g.State==PhoVietGame.RideState.Riding,"Adjacent lane does not cause false collision");
            clear();g.SpawnTraffic(3,1.2f,0);step(.02f);check(g.State==PhoVietGame.RideState.Falling,"Car collision triggers fall");
            g.RestartRide();clear();g.SpawnTraffic(2,1,1);step(.02f);check(g.State==PhoVietGame.RideState.Falling,"Motorcycle collision triggers fall");
            g.RestartRide();clear();g.MoveLane(-1);g.MoveLane(-1);for(int i=0;i<25;i++)step(.02f);clear();
            g.SpawnTraffic(0,1,2);step(.02f);check(g.State==PhoVietGame.RideState.Falling,"Oncoming collision triggers fall");
            g.RestartRide();clear();
            typeof(PhoVietGame).GetField("spawnTimer",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,10000f);
            for(int i=0;i<6000;i++)step(.02f);
            check(g.Distance>2500&&g.Speed>29&&g.Speed<=30,"Distance progresses and difficulty reaches capped speed");
            var zs=g.blocks.Select(b=>b.localPosition.z).OrderBy(z=>z).ToArray();
            check(zs[0]>=-160.1f&&zs[6]<401,"Recycled city blocks stay inside active range");
            check(zs.Zip(zs.Skip(1),(a,b)=>Mathf.Abs(b-a-80)<.02f).All(x=>x),"No seams or gaps after 120 seconds of road recycling");
            var allText=g.scenery.GetComponentsInChildren<TextMesh>(true).Select(t=>t.text).ToArray();
            var streetCatalog=(string[])typeof(PhoVietGame).GetField("RouteNames",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            check(VietnamStreet.Streets.All(s=>streetCatalog.Contains(s))&&allText.Contains(g.CurrentStreet.ToUpperInvariant())&&allText.Contains(g.NextStreet.ToUpperInvariant()),"Original seven street names remain available; visible route and next-street signs match the connected journey");
            var shopNames=g.scenery.GetComponentsInChildren<Transform>(true).Select(t=>t.name).ToArray();
            check(VietnamStreet.Shops.All(s=>shopNames.Contains("Blender • "+s)),"All Vietnamese food, grocery and repair storefronts use Blender signs");
            check(g.vietnameseFont!=null&&g.trafficLights.Count==84*g.roadRows.Count,"Bundled Vietnamese font and complete signals on every pooled road row");
            var temp=new GameObject("Star mesh verification");StreetArt.Star(temp.transform,Vector3.zero,1);
            var mesh=temp.GetComponentInChildren<MeshFilter>().sharedMesh;
            check(mesh.vertexCount==11&&mesh.triangles.Length==30&&mesh.vertices.Skip(1).Count(v=>v.magnitude>.9f)==5,"Vietnamese star has exactly five outer points");
            UnityEngine.Object.DestroyImmediate(temp);UnityEngine.Object.DestroyImmediate(mesh);
            g.ResetRide();check(g.State==PhoVietGame.RideState.Ready&&g.Distance==0&&g.CurrentLane==2,"Reset restores rider, distance and lanes");
            return new {success=true,count=passed.Count,checks=passed};
        }
        finally {g.recklessTrafficEnabled=originalReckless;g.trafficSurprises=originalSurprises;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",originalBest);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,originalBest);g.ResetRide();}
    }
}



