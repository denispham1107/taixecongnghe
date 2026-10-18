using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;

public static class VerifyBikeArt
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);float scale=Time.timeScale;
        bool surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled;
        var checks=new List<string>();Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        Action<string,float> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action clean=()=>
        {
            g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}
            set("spawnTimer",10000);set("nextIntersectionDistance",float.PositiveInfinity);
        };
        Func<BikeVisualAnimator,string,Transform> bone=(d,n)=>d.visual.GetComponentsInChildren<Transform>(true).Single(t=>t.name==n);
        try
        {
            Time.timeScale=0;
            var drivers=g.GetComponentsInChildren<BikeVisualAnimator>(true);
            check(drivers.Count(d=>d.role!=BikeVisualAnimator.ActorRole.Parked)==19,"All nineteen player, civilian, racer and police actors use Blender models");
            check(drivers.Count(d=>d.role==BikeVisualAnimator.ActorRole.Parked)==14,"All fourteen parked sidewalk motorcycles also use Blender models");
            check(drivers.Select(d=>d.AssetName).Distinct().Count()==6,"Six authored motorcycle and rider designs are used in the scene");
            foreach(var d in drivers)
            {
                d.Initialize();var lod=d.visual.GetComponent<LODGroup>();var renderers=d.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                check(d.clips.Length==7&&d.clips.All(c=>c.legacy)&&d.clips.Select(c=>c.name).Distinct().Count()==7,d.name+": seven imported animation clips");
                check(lod!=null&&lod.GetLODs().Length==3&&renderers.Length==3,d.name+": three usable mobile LODs");
                check(renderers.All(r=>r.sharedMaterials.Length==1&&r.quality==SkinQuality.Bone1&&r.bones.Length==17&&r.bones.All(b=>b!=null)),d.name+": one atlas material and complete seventeen-bone rig");
                var counts=lod.GetLODs().Select(l=>(long)((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.GetIndexCount(0)/3).ToArray();
                check(counts[0]>counts[1]&&counts[1]>counts[2]&&counts[2]<6000,d.name+": triangle count decreases to fewer than six thousand at distance");
            }
            check(g.vehicles.Where(v=>v.halfWidth>.7f).All(v=>v.model.GetComponent<BikeVisualAnimator>()==null),"Four-wheel actors have separate visuals and retain their physics roots");
            var parked=drivers.First(d=>d.role==BikeVisualAnimator.ActorRole.Parked);parked.Advance(.2f);
            check(parked.CurrentClip=="Idle"&&parked.WheelAngle==0,"Parked riders idle naturally while their wheels remain still");
            clean();var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(.1f);
            Quaternion wheel=hero.frontWheel.localRotation;hero.Advance(.15f);
            check(hero.CurrentClip=="Ride"&&Quaternion.Angle(wheel,hero.frontWheel.localRotation)>1,"Player wheels turn with actual road speed during riding");
            var torso=bone(hero,"Torso");Quaternion ridingTorso=torso.localRotation;float motion=0;
            for(int i=0;i<8;i++){hero.Advance(.1f);motion=Mathf.Max(motion,Quaternion.Angle(ridingTorso,torso.localRotation));}
            check(motion>.1f,"Ride clip animates torso rather than moving only the outer vehicle");
            g.TogglePause();float sample=hero.SampleTime,angle=hero.WheelAngle;Quaternion pausedWheel=hero.frontWheel.localRotation;
            hero.animationPlayer.Sample();hero.Advance(.4f);
            check(hero.SampleTime==sample&&hero.WheelAngle==angle&&Quaternion.Angle(pausedWheel,hero.frontWheel.localRotation)<.01f,"Pause freezes animation and wheels even after Unity samples the bound clip");g.TogglePause();
            var target=g.SpawnTraffic(3,0,1);check(g.TryKick(),"Gameplay still selects and kicks a nearby motorcycle");hero.Advance(.27f);
            check(hero.CurrentClip=="KickRight"&&Quaternion.Angle(bone(hero,"RThigh").localRotation,Quaternion.identity)>25,"Real right kick deforms the correct skeletal leg");
            check(bone(hero,"RFoot").position.x-g.rider.position.x>.6f,"Right kick extends the foot toward the actual right-side target in Unity");
            check(Quaternion.Angle(bone(hero,"LThigh").localRotation,Quaternion.identity)<5,"Right kick leaves the supporting leg in its riding pose");
            var npc=target.model.GetComponent<BikeVisualAnimator>();target.knockAge=.4f;target.model.localRotation=Quaternion.Euler(0,0,-50);Quaternion outer=target.model.localRotation;npc.Advance(.02f);
            check(npc.CurrentClip=="FallRight"&&Quaternion.Angle(bone(npc,"RUpperArm").localRotation,Quaternion.identity)>10,"Kicked rider releases the handlebar and reacts to falling");
            check(target.model.localRotation==outer&&npc.visual.position.y>target.model.position.y,"Fall animation lifts only the visual skin and leaves physics rotation intact");
            typeof(PhoVietGame).GetMethod("RetireVehicle",F).Invoke(g,new object[]{target});
            g.SpawnTraffic(3,20,1);npc.Advance(.02f);
            check(npc.CurrentClip=="Ride"&&npc.visual.localPosition.sqrMagnitude<.0001f,"Recycled motorcycle clears its old fall pose and visual ground offset");
            target.knocked=true;target.knockAge=.3f;target.model.localRotation=Quaternion.Euler(0,0,55);npc.Advance(.02f);
            check(npc.CurrentClip=="FallLeft","Recycled motorcycle reacts to a new fall using the current pool record");
            clean();g.MoveLane(1);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[3],0,0);g.SpawnTraffic(2,0,1);
            check(g.TryKick(),"Left-side kick still selects the correct target");hero.Advance(.27f);
            check(hero.CurrentClip=="KickLeft"&&Quaternion.Angle(bone(hero,"LThigh").localRotation,Quaternion.identity)>25,"Left kick uses a separate mirrored skeletal animation");
            check(bone(hero,"LFoot").position.x-g.rider.position.x<-.6f,"Left kick extends the foot toward the actual left-side target in Unity");
            var police=g.policeBike.GetComponent<BikeVisualAnimator>();
            typeof(PhoVietGame).GetProperty("PoliceFallRemaining").SetValue(g,1f);g.policeBike.localRotation=Quaternion.Euler(0,0,-50);police.Advance(.02f);
            check(police.CurrentClip=="FallRight","Police use a rider fall animation during the existing fall timer");
            typeof(PhoVietGame).GetProperty("PoliceFallRemaining").SetValue(g,.15f);police.Advance(.02f);
            check(police.CurrentClip=="Recover"&&Mathf.Abs(g.PoliceFallRemaining-.15f)<.001f,"Police recovery animation does not change the 1.2 second gameplay timer");
            check(Mathf.Abs(Mathf.DeltaAngle(0,police.visual.eulerAngles.z))<Mathf.Abs(Mathf.DeltaAngle(0,g.policeBike.eulerAngles.z)),"Police motorcycle visually rises smoothly during recovery");
            g.Crash();hero.Advance(.3f);
            check(g.State==PhoVietGame.RideState.Falling&&hero.CurrentClip.StartsWith("Fall")&&g.Speed==0,"Player crash immediately loses while playing a skeletal fall reaction");
            g.ResetRide();hero.Advance(.02f);
            check(hero.CurrentClip=="Idle"&&hero.visual.localPosition.sqrMagnitude<.0001f,"Restart restores idle posture and removes ground lift");
            return new{success=true,count=checks.Count,checks};
        }
        finally
        {
            g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;
            PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();typeof(PhoVietGame).GetProperty("BestDistance").SetValue(g,best);
            g.ResetRide();Time.timeScale=scale;
        }
    }
}
