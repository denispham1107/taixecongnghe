using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet;
using PhoViet.Editor;

public static class UpgradeMobileClarity
{
    public static object Main()
    {
        if(Application.isPlaying)throw new Exception("Exit Play Mode first");
        AssetDatabase.Refresh();
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if(g==null||g.gameObject.scene.path!="Assets/PhoViet/Scenes/PhoViet.unity")throw new Exception("Wrong scene");
        // Update existing material/texture GUIDs in place, without replacing actors or police.
        var configure=typeof(PhoVietBikeUpgrade).GetMethod("AtlasMaterial",BindingFlags.NonPublic|BindingFlags.Static);
        foreach(var name in PhoVietBikeUpgrade.Assets.Where(n=>n!="PV_TrafficPolice"))configure.Invoke(null,new object[]{name});
        AssetDatabase.SaveAssets();
        return new{success=true,atlasWidth=2048,atlasHeight=4096,sourceTile=2048,bikes=g.GetComponentsInChildren<BikeVisualAnimator>(true).Length,jobs=g.jobsEnabled};
    }
}
