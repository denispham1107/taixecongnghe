using UnityEditor;
using UnityEditor.Compilation;
// 21.336 — nhập lại mã + palette đã sửa ngoài Unity rồi yêu cầu biên dịch (Test21336 cần mã mới).
public static class Reimport21336
{
    public static string Run()
    {
        foreach (var p in new[] {
            "Assets/PhoViet/Scripts/OpenWorld/OpenWorldHazards.Weapons.cs", "Assets/PhoViet/Scripts/OpenWorld/OpenWorldHazards.Laser.cs",
            "Assets/PhoViet/Scripts/OpenWorld/OpenWorldHazards.cs", "Assets/PhoViet/Scripts/OpenWorld/OpenWorldTraffic.cs",
            "Assets/PhoViet/Scripts/OpenWorld/OpenWorldWeather.cs", "Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar_Palette.png" })
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        CompilationPipeline.RequestScriptCompilation();
        return "reimported, compiling";
    }
}
