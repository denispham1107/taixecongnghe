using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Installs the version-2 Blender motorcycles (pho_viet_motorcycles.py) into the open scene and
// re-bakes the static parked bikes that the four spare road rows share, keeping their asset GUIDs.
public static class UpgradeBikesV2
{
    public static object Main()
    {
        if(Application.isPlaying)throw new Exception("Exit Play Mode first");
        PhoViet.Editor.PhoVietBikeUpgrade.UpgradeOpenScene();
        var game=UnityEngine.Object.FindAnyObjectByType<PhoViet.PhoVietGame>();
        // The stock upgrader skips already-converted sidewalk bikes; re-instance them with the new FBX settings.
        var replace=typeof(PhoViet.Editor.PhoVietBikeUpgrade).GetMethod("Replace",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
        int parkedReplaced=0;
        foreach(var bike in game.roadRows[0].root.GetComponentsInChildren<PhoViet.BikeVisualAnimator>(true).Where(b=>b.role==PhoViet.BikeVisualAnimator.ActorRole.Parked).ToArray())
        {replace.Invoke(null,new object[]{bike.transform,bike.AssetName,PhoViet.BikeVisualAnimator.ActorRole.Parked});parkedReplaced++;}
        var rebaked=new List<string>();
        var parked=game.roadRows[0].root.GetComponentsInChildren<PhoViet.BikeVisualAnimator>(true).Where(b=>b.role==PhoViet.BikeVisualAnimator.ActorRole.Parked);
        foreach(var bike in parked.GroupBy(b=>b.AssetName).Select(g=>g.First()))
        {
            string path="Assets/PhoViet/Generated/RouteParked_"+bike.AssetName+".asset";
            var target=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(target==null)continue;
            var skin=bike.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(s=>s.name.EndsWith("_LOD1",StringComparison.Ordinal)); // side-street parked bikes are never close to the camera
            var baked=new Mesh{name=target.name};skin.BakeMesh(baked);
            EditorUtility.CopySerialized(baked,target);target.name="PV_RouteParked_"+bike.AssetName;EditorUtility.SetDirty(target);
            UnityEngine.Object.DestroyImmediate(baked);rebaked.Add(bike.AssetName+":"+target.vertexCount);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        var drivers=game.GetComponentsInChildren<PhoViet.BikeVisualAnimator>(true);
        return new{success=true,parkedReplaced,bikes=drivers.Length,assets=drivers.Select(d=>d.AssetName).Distinct().ToArray(),rebaked,scene=game.gameObject.scene.path};
    }
}
