using UnityEditor;
using UnityEngine;
// 21.304 — chạy lại menu 20 (bán đảo) sau khi cắt cạnh nam/tây vào 32,5 m + bỏ tàu du lịch trên sông.
public static class Peninsula21304
{
    public static string Main()
    {
        AssetDatabase.ImportAsset("Assets/PhoViet/Scripts/TankWarning.shader", ImportAssetOptions.ForceUpdate);
        var r = PhoViet.Editor.PhoVietOpenWorldPeninsulaBuilder.Run();
        return r + " | shaderErr=" + ShaderUtil.ShaderHasError(Shader.Find("PhoViet/Tank Warning")) + " tau=" + (GameObject.Find("OW_TauDuLich_The_Goc") != null);
    }
}
