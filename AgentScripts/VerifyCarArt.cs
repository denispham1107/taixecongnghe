using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyCarArt
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        float scale=Time.timeScale;bool surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
        Action clean=()=>{g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}typeof(PhoVietGame).GetField("spawnTimer",F).SetValue(g,10000f);typeof(PhoVietGame).GetField("nextIntersectionDistance",F).SetValue(g,float.PositiveInfinity);};
        try
        {
            Time.timeScale=0;clean();var cars=g.vehicles.Where(v=>v.halfWidth>.7f).ToArray();
            check(cars.Length==11&&cars.All(v=>v.model.GetComponent<CarVisualAnimator>()!=null),"All eleven four-wheel traffic actors replaced");
            check(cars.Select(v=>v.model.GetComponent<CarVisualAnimator>().AssetName).Distinct().Count()==6,"Six distinct car/taxi/cargo/truck designs installed");
            check(g.GetComponentsInChildren<BikeVisualAnimator>(true).Length==33,"All thirty-three existing motorcycle/rider upgrades preserved");
            foreach(var v in cars)
            {
                v.active=true;v.model.gameObject.SetActive(true);v.model.localRotation=Quaternion.identity;v.speed=v.baseTrafficSpeed=14;
                var d=v.model.GetComponent<CarVisualAnimator>();d.Initialize();
                var skins=d.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);var lod=d.visual.GetComponent<LODGroup>();
                check(skins.Length==3&&lod!=null&&lod.GetLODs().Length==3,v.model.name+": three working mobile LODs");
                check(skins.All(s=>s.bones.Length==15&&s.bones.All(b=>b!=null)&&s.sharedMaterials.Length==1&&s.quality==SkinQuality.Bone1),v.model.name+": fifteen bones and one atlas draw per LOD");
                check(d.clips.Length==7&&d.clips.All(c=>c.legacy)&&d.clips.Select(c=>c.name).Distinct().Count()==7,v.model.name+": seven editable Blender animations imported");
                var counts=lod.GetLODs().Select(l=>(long)((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.GetIndexCount(0)/3).ToArray();
                check(counts[0]>counts[1]&&counts[1]>counts[2]&&counts[2]<4000,v.model.name+": distant LOD below four thousand triangles");
                check(d.wheels.Length==4&&d.wheels[0].position.x<v.model.position.x&&d.wheels[1].position.x>v.model.position.x&&d.wheels[0].position.z>v.model.position.z,v.model.name+": correct wheel pivots and left/right/forward axes");
                d.Advance(.05f);Quaternion wheel=d.wheels[0].localRotation;Vector3 body=d.body.localPosition;float bounce=0;
                for(int i=0;i<10;i++){d.Advance(.05f);bounce=Mathf.Max(bounce,Vector3.Distance(body,d.body.localPosition));}
                check(d.CurrentClip=="Drive"&&Quaternion.Angle(wheel,d.wheels[0].localRotation)>1&&bounce>.005f,v.model.name+": wheels rotate and suspension actually moves");
                float angle=d.WheelAngle;v.speed=0;d.Advance(.2f);check(d.CurrentClip=="Idle"&&Mathf.Abs(d.WheelAngle-angle)<.001f,v.model.name+": stopped vehicle has stationary wheels");
                v.speed=-14;v.model.localRotation=Quaternion.Euler(0,180,0);d.Advance(.05f);check(d.CurrentClip=="Drive"&&d.WheelAngle!=angle,v.model.name+": oncoming vehicles animate too");
                v.model.localRotation=Quaternion.identity;v.speed=14;d.Advance(.05f);v.model.localRotation=Quaternion.Euler(0,8,0);d.Advance(.05f);
                check(d.SteeringAngle>0&&Quaternion.Angle(d.steering[0].localRotation,Quaternion.identity)>0,v.model.name+": front wheels steer with actual turns");
                v.model.localRotation=Quaternion.identity;v.brakeRemaining=1;d.Advance(.18f);check(d.CurrentClip=="Brake"&&Quaternion.Angle(d.body.localRotation,Quaternion.identity)>.1f,v.model.name+": braking visibly pitches the body");v.brakeRemaining=0;
                g.TogglePause();angle=d.WheelAngle;wheel=d.wheels[0].localRotation;float sample=d.SampleTime;d.animationPlayer.Sample();d.Advance(.4f);
                check(d.WheelAngle==angle&&d.SampleTime==sample&&Quaternion.Angle(wheel,d.wheels[0].localRotation)<.01f,v.model.name+": pause holds suspension and wheels");g.TogglePause();
                v.knocked=true;var originalPosition=v.model.position;var originalRotation=v.model.rotation;
                foreach(var pair in new[]{(Vector3.back,"ImpactFront"),(Vector3.forward,"ImpactRear"),(Vector3.right,"ImpactLeft"),(Vector3.left,"ImpactRight")})
                {
                    d.ReceiveImpact(pair.Item1,14);d.Advance(.12f);
                    check(d.CurrentClip==pair.Item2,v.model.name+": directional "+pair.Item2+" selected from physical impulse");
                }
                check(v.model.position==originalPosition&&v.model.rotation==originalRotation,v.model.name+": animation leaves collision root untouched");
                d.ReceiveImpact(Vector3.back,12);d.Advance(.7f);check(d.bumperFront.localPosition.z<2.02f-.05f||d.wheelRadius>.45f&&d.bumperFront.localPosition.z<3.12f-.05f,v.model.name+": impact deforms actual bumper bone");
                v.knocked=false;d.Advance(.01f);check(d.CurrentClip=="Drive"&&d.visual.localPosition.sqrMagnitude<.0001f,v.model.name+": recycling clears wreck pose and lift");
                v.active=false;v.model.gameObject.SetActive(false);
            }
            clean();var a=cars[0];var b=cars[3];
            foreach(var v in new[]{a,b}){v.active=true;v.model.gameObject.SetActive(true);v.speed=v.baseTrafficSpeed=15;v.model.localRotation=Quaternion.identity;v.model.GetComponent<CarVisualAnimator>().Advance(.01f);}
            a.model.localPosition=new Vector3(-1.55f,0,20);b.model.localPosition=new Vector3(-1.55f,0,24);a.previousPosition=a.model.localPosition;b.previousPosition=b.model.localPosition;a.motionVelocity=new Vector3(0,0,25);b.motionVelocity=new Vector3(0,0,5);
            typeof(PhoVietGame).GetMethod("ResolveVehicleCollisions",F).Invoke(g,null);
            a.model.GetComponent<CarVisualAnimator>().Advance(.12f);b.model.GetComponent<CarVisualAnimator>().Advance(.12f);
            check(a.physicalCrash&&b.physicalCrash&&g.TrafficCollisions>0,"Real vehicle collision still uses mass and impulse physics");
            check(a.model.GetComponent<CarVisualAnimator>().CurrentClip=="ImpactFront"&&b.model.GetComponent<CarVisualAnimator>().CurrentClip=="ImpactRear","Real collision hook triggers opposite front/rear animations");
            clean();var truck=g.SpawnReckless(true);truck.model.localPosition=new Vector3(9.5f,0,20);typeof(PhoVietGame).GetMethod("CheckBuildingImpact",F).Invoke(g,new object[]{truck});truck.model.GetComponent<CarVisualAnimator>().Advance(.12f);
            check(truck.wallCrashed&&truck.smoke.isPlaying&&truck.fire.isPlaying&&truck.model.GetComponent<CarVisualAnimator>().CurrentClip.StartsWith("Impact"),"Drowsy truck building impact retains flames/smoke and animates its skin");
            return new{success=true,count=checks.Count,checks};
        }
        finally{g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=scale;}
    }
}
