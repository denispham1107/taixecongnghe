using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyWheelStability
{
    const BindingFlags F=BindingFlags.NonPublic|BindingFlags.Instance;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        var checks=new List<string>();var metrics=new List<object>();Action<bool,string> check=(ok,msg)=>{if(!ok)throw new Exception(msg);checks.Add(msg);};
        float time=Time.timeScale;
        try
        {
            Time.timeScale=0;
            var drivers=game.GetComponentsInChildren<BikeVisualAnimator>(true).Where(d=>d.AssetName!="PV_TrafficPolice").GroupBy(d=>d.AssetName).Select(g=>g.First()).ToArray();
            check(drivers.Length==5,"All five non-police motorcycle schemes covered");
            foreach(var d in drivers)
            {
                d.Initialize();typeof(BikeVisualAnimator).GetMethod("Sample",F).Invoke(d,new object[]{"Ride",.3f});
                var front=d.frontWheel.localRotation;var rear=d.rearWheel.localRotation;
                foreach(var skin in d.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    d.frontWheel.localRotation=Quaternion.identity;d.rearWheel.localRotation=Quaternion.identity;
                    // BakeMesh's default omits renderer scale. Include it before
                    // converting to actor-local metres (parked NPCs use scale .86).
                    var mesh=new Mesh();skin.BakeMesh(mesh,true);
                    var basis=mesh.vertices.Select(v=>d.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
                    var ws=skin.sharedMesh.boneWeights;var boneNames=skin.bones.Select(b=>b.name).ToArray();
                    Func<int,bool> bike=i=>{
                        var w=ws[i];float sum=0;int[] ids={w.boneIndex0,w.boneIndex1,w.boneIndex2,w.boneIndex3};float[] weights={w.weight0,w.weight1,w.weight2,w.weight3};
                        for(int a=0;a<4;a++)if(new[]{"Frame","Steering","WheelFront","WheelRear"}.Contains(boneNames[ids[a]]))sum+=weights[a];return sum>.999f;
                    };
                    foreach(var wheel in new[]{d.frontWheel,d.rearWheel})
                    {
                        float z=wheel==d.frontWheel ? .78f : -.73f;
                        var outer=Enumerable.Range(0,basis.Length).Where(i=>bike(i)&&Mathf.Abs(basis[i].x)<.22f&&Mathf.Sqrt(Mathf.Pow(basis[i].y-.34f,2)+Mathf.Pow(basis[i].z-z,2))>.255f&&Mathf.Sqrt(Mathf.Pow(basis[i].y-.34f,2)+Mathf.Pow(basis[i].z-z,2))<.43f).ToArray();
                        check(outer.Length>10,d.AssetName+"/"+skin.name+"/"+wheel.name+": tyre/fender samples present");
                        float maxError=0,maxMoving=0;
                        foreach(float angle in new[]{30f,90f,135f,180f,270f,315f})
                        {
                            wheel.localRotation=Quaternion.AngleAxis(angle,wheel.parent.InverseTransformDirection(d.transform.right));skin.BakeMesh(mesh,true);var posed=mesh.vertices.Select(v=>d.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
                            float drift=outer.Max(i=>Vector3.Distance(posed[i],basis[i]));maxError=Mathf.Max(maxError,drift);maxMoving=Mathf.Max(maxMoving,posed.Select((p,i)=>Vector3.Distance(p,basis[i])).Max());
                            if(drift>=.0001f){int worst=outer.OrderByDescending(i=>Vector3.Distance(posed[i],basis[i])).First();throw new Exception(d.AssetName+"/"+skin.name+"/"+wheel.name+" drift="+drift+" base="+basis[worst].ToString("F6")+" posed="+posed[worst].ToString("F6")+" axle="+d.transform.InverseTransformPoint(wheel.position).ToString("F6")+" bone0="+boneNames[ws[worst].boneIndex0]+" weight="+ws[worst].weight0);}
                            check(drift<.0001f,d.AssetName+"/"+skin.name+"/"+wheel.name+" @"+angle+": outer tyre/fender remains stable within 0.1mm");
                        }
                        check(maxMoving>.02f,d.AssetName+"/"+skin.name+"/"+wheel.name+": rim genuinely rotates");
                        metrics.Add(new{asset=d.AssetName,lod=skin.name,wheel=wheel.name,outerVertices=outer.Length,maxOuterDrift=maxError,maxRimMovement=maxMoving});wheel.localRotation=Quaternion.identity;
                    }
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
                d.frontWheel.localRotation=front;d.rearWheel.localRotation=rear;
            }
            check(!game.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain disabled");
            return new{success=true,count=checks.Count,checks,metrics};
        }
        finally{Time.timeScale=time;}
    }
}
