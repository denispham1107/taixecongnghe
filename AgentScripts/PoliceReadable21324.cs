using UnityEditor;
// 21.324 — bật Read/Write cho lưới xe CSGT Meshy để làm móp thân xe khi va chạm (OpenWorldCarDent).
public static class PoliceReadable21324
{
    const string Path = "Assets/MeshyImports/Vietnam_Traffic_Police_Cruiser_20261110_230018/Meshy_AI_Vietnam_Traffic_Police_Cruiser_20261110_texture.fbx";
    public static string Main()
    {
        var mi = (ModelImporter)AssetImporter.GetAtPath(Path);
        if (mi == null) return "không thấy " + Path;
        if (mi.isReadable) return "đã bật sẵn";
        mi.isReadable = true; mi.SaveAndReimport();
        return "bật Read/Write, nén lưới " + mi.meshCompression;
    }
}
