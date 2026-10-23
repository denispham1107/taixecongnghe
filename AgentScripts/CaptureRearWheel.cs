using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class CaptureRearWheel
{
    const BindingFlags F=BindingFlags.NonPublic|BindingFlags.Instance;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var driver=g.rider.GetComponent<BikeVisualAnimator>();
        float time=Time.timeScale;var layers=driver.GetComponentsInChildren<Transform>(true).ToDictionary(t=>t,t=>t.gameObject.layer);
        var probe=new GameObject("Rear wheel probe").AddComponent<Camera>();
        try
        {
            Time.timeScale=0;driver.Initialize();typeof(BikeVisualAnimator).GetMethod("Sample",F).Invoke(driver,new object[]{"Ride",.3f});
            foreach(var t in layers.Keys)t.gameObject.layer=30;
            probe.cullingMask=1<<30;probe.clearFlags=CameraClearFlags.SolidColor;probe.backgroundColor=new Color(.32f,.40f,.44f);probe.orthographic=true;probe.orthographicSize=.66f;probe.nearClipPlane=.05f;probe.farClipPlane=20;
            probe.transform.position=g.rider.TransformPoint(new Vector3(1.5f,.90f,-3.2f));probe.transform.LookAt(g.rider.TransformPoint(new Vector3(0,.48f,-.68f)));
            var skins=driver.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);var skin=skins.OrderByDescending(s=>s.sharedMesh.vertexCount).First();driver.visual.GetComponent<LODGroup>().ForceLOD(0);
            var proxy=new GameObject("Baked wheel probe");proxy.layer=30;proxy.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);proxy.transform.localScale=skin.transform.lossyScale;
            var filter=proxy.AddComponent<MeshFilter>();proxy.AddComponent<MeshRenderer>().sharedMaterial=skin.sharedMaterial;
            var visible=skins.Select(s=>s.enabled).ToArray();foreach(var s in skins)s.enabled=false;
            var vertices=new List<object>();var frames=new List<object>();
            foreach(int angle in new[]{0,45,90,135,180,225,270,315})
            {
                driver.frontWheel.localRotation=Quaternion.AngleAxis(angle,driver.frontWheel.parent.InverseTransformDirection(driver.transform.right));
                driver.rearWheel.localRotation=Quaternion.AngleAxis(angle,driver.rearWheel.parent.InverseTransformDirection(driver.transform.right));
                var mesh=new Mesh();skin.BakeMesh(mesh);var points=mesh.vertices.Select(v=>driver.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
                filter.sharedMesh=mesh;
                if(angle==0){var weights=skin.sharedMesh.boneWeights;int rear=Array.FindIndex(skin.bones,b=>b.name=="WheelRear");for(int i=0;i<points.Length;i++){var w=weights[i];if((w.boneIndex0==rear&&w.weight0>.9f)||(w.boneIndex1==rear&&w.weight1>.9f)){var p=points[i];vertices.Add(new{index=i,x=p.x,y=p.y,z=p.z});}}}
                string label=File.Exists(".tools/rear-wheel-after")?"after":"before";
                string file=$"Captures/rear-wheel-{label}-{angle:D3}.png";Render(probe,file);
                frames.Add(new{angle,file,minY=points.Min(p=>p.y),maxY=points.Max(p=>p.y),minZ=points.Min(p=>p.z),maxZ=points.Max(p=>p.z)});UnityEngine.Object.DestroyImmediate(mesh);
            }
            for(int i=0;i<skins.Length;i++)skins[i].enabled=visible[i];UnityEngine.Object.DestroyImmediate(proxy);
            return new{success=true,vertices,frames};
        }
        finally{foreach(var p in layers)p.Key.gameObject.layer=p.Value;UnityEngine.Object.DestroyImmediate(probe.gameObject);Time.timeScale=time;}
    }
    static void Render(Camera camera,string path)
    {
        var rt=new RenderTexture(512,512,24){antiAliasing=4};var tex=new Texture2D(512,512,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,512,512),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
        finally{camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
}
