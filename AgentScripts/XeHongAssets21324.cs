using UnityEditor;
using UnityEngine;
// 21.324 — nhập ảnh vết hỏng + JSON dữ liệu kính/đèn, tạo vật liệu Resources/XeHong/PV_XeHong.mat (shader CarDamage).
public static class XeHongAssets21324
{
    const string Dir = "Assets/PhoViet/Resources/XeHong/";
    public static string Main()
    {
        AssetDatabase.ImportAsset(Dir + "PV_XeHong_Atlas.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(Dir + "PV_XeHong_Data.json", ImportAssetOptions.ForceUpdate);
        var ti = (TextureImporter)AssetImporter.GetAtPath(Dir + "PV_XeHong_Atlas.png");
        ti.textureType = TextureImporterType.Default; ti.sRGBTexture = true; ti.alphaSource = TextureImporterAlphaSource.FromInput;
        ti.alphaIsTransparency = true; ti.mipmapEnabled = true; ti.wrapMode = TextureWrapMode.Clamp; ti.maxTextureSize = 1024;
        ti.textureCompression = TextureImporterCompression.Compressed; ti.anisoLevel = 2;
        ti.SaveAndReimport();
        var sh = Shader.Find("PhoViet/OpenWorld/CarDamage"); if (!sh) return "chưa có shader CarDamage (biên dịch lại?)";
        var path = Dir + "PV_XeHong.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(sh) { name = "PV_XeHong" }; AssetDatabase.CreateAsset(mat, path); }
        mat.shader = sh;
        mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_XeHong_Atlas.png"));
        mat.SetFloat("_Glossiness", 0.55f); mat.SetFloat("_Metallic", 0.45f);
        mat.enableInstancing = false;
        EditorUtility.SetDirty(mat); AssetDatabase.SaveAssets();
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + "PV_XeHong_Data.json");
        return $"mat {path} shader {mat.shader.name} tex {mat.mainTexture?.name} json {(ta ? ta.text.Length : -1)} ký tự";
    }
}
