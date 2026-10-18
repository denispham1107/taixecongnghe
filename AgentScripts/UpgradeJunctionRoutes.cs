public static class UpgradeJunctionRoutes
{
    public static object Main()
    {
        PhoViet.Editor.PhoVietRouteUpgrade.UpgradeOpenScene();var g=UnityEngine.Object.FindAnyObjectByType<PhoViet.PhoVietGame>();
        return new{success=true,rows=g.roadRows.Count,blocks=g.scenery.GetComponentsInChildren<UnityEngine.MeshFilter>(true).Length,lights=g.trafficLights.Count};
    }
}
