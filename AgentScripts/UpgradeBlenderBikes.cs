public static class UpgradeBlenderBikes
{
    public static object Main()
    {
        PhoViet.Editor.PhoVietBikeUpgrade.UpgradeOpenScene();
        var game=UnityEngine.Object.FindAnyObjectByType<PhoViet.PhoVietGame>();
        return new{success=true,bikes=game.GetComponentsInChildren<PhoViet.BikeVisualAnimator>(true).Length,scene=game.gameObject.scene.path};
    }
}
