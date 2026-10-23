using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PhoViet;
public static class VerifyMeshyRelease
{
    public static object Main()
    {
        if(Application.isPlaying)throw new Exception("Exit Play Mode before release");
        var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(game==null||game.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Wrong scene");
        // Keep the user's loose import for editor reference, excluding it from the game build.
        int referenceModels=0;
        foreach(var root in game.gameObject.scene.GetRootGameObjects())
        {
            var source=PrefabUtility.GetCorrespondingObjectFromSource(root);
            if(source==null||!AssetDatabase.GetAssetPath(source).StartsWith("Assets/MeshyImports/",StringComparison.Ordinal))continue;
            if(root.GetComponentInChildren<BikeVisualAnimator>(true)!=null)throw new Exception("Do not exclude an integrated vehicle");
            if(!root.CompareTag("EditorOnly")){root.tag="EditorOnly";EditorSceneManager.MarkSceneDirty(game.gameObject.scene);}
            referenceModels++;
        }
        if(game.jobsEnabled||PhoVietGame.RideJobsReleased)throw new Exception("Jobs must stay locked");
        int missingScripts=game.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        if(missingScripts!=0)throw new Exception("Missing scene scripts");
        var bikes=game.GetComponentsInChildren<BikeVisualAnimator>(true);
        foreach(var bike in bikes)
        {
            bool police=bike.AssetName=="PV_TrafficPolice";
            foreach(var skin in bike.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string path=AssetDatabase.GetAssetPath(skin.sharedMesh);
                if(police?!path.Contains("Bikes/PV_TrafficPolice.fbx"):path!=PhoViet.Editor.PhoVietBikeUpgrade.ModelPath(bike.AssetName))throw new Exception("Wrong bike mesh "+path);
                if(skin.sharedMaterial==null||skin.sharedMaterial.shader==null)throw new Exception("Missing material");
            }
        }
        if(bikes.Length!=33||game.daylight.vehicles.Any(v=>v.head==null||v.halos.Any(h=>h.source==null)))throw new Exception("Incomplete visuals or lamps");
        var cars=game.GetComponentsInChildren<MeshyCarVariants>(true);
        if(cars.Length!=11||game.daylight.vehicles.Length!=30)throw new Exception("Incomplete Meshy cars or lamp bindings");
        foreach(var car in cars)foreach(var variant in car.variants)
        {
            if(variant.skins.Length!=3||variant.clips.Length!=7||variant.clips.Any(c=>c==null||!c.legacy)||variant.roofProfile.Length!=49)throw new Exception("Incomplete car variant "+variant.asset);
            foreach(var skin in variant.skins)
            {
                var mesh=skin as SkinnedMeshRenderer;
                if(mesh==null||!AssetDatabase.GetAssetPath(mesh.sharedMesh).StartsWith("Assets/PhoViet/Art/Cars/Meshy/",StringComparison.Ordinal)||skin.sharedMaterial==null||skin.sharedMaterial.shader.name!="PhoViet/Meshy Car Paint")throw new Exception("Wrong car mesh/material "+variant.asset);
            }
        }
        if(game.gameObject.scene.isDirty)EditorSceneManager.SaveScene(game.gameObject.scene);
        AssetDatabase.SaveAssets();
        return new{success=true,scene=game.gameObject.scene.path,version=PlayerSettings.bundleVersion,bikes=bikes.Length,cars=cars.Length,lamps=game.daylight.vehicles.Length,police=bikes.Count(b=>b.AssetName=="PV_TrafficPolice"),jobs=game.jobsEnabled,missingScripts,referenceModelsEditorOnly=referenceModels};
    }
}
