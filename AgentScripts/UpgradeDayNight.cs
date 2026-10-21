using UnityEngine;
using PhoViet;
public static class UpgradeDayNight
{
    public static object Main()
    {
        PhoViet.Editor.PhoVietLightingUpgrade.Main();
        var g=Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;
        return new{success=true,streets=l.streets.Length,vehicles=l.vehicles.Length,realtimePool=l.lightPool.Length,night=l.NightLevel,
            fixture=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.fbx").GetComponentInChildren<Renderer>().bounds.size};
    }
}
