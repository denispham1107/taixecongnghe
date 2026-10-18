using System.Linq;
using UnityEngine;
using PhoViet;
public static class UpgradeBlenderCars
{
    public static object Main()
    {
        PhoViet.Editor.PhoVietCarUpgrade.UpgradeOpenScene();
        var g=Object.FindAnyObjectByType<PhoVietGame>();
        return new {count=g.GetComponentsInChildren<CarVisualAnimator>(true).Length,assets=g.GetComponentsInChildren<CarVisualAnimator>(true).Select(v=>v.AssetName).Distinct().ToArray()};
    }
}
