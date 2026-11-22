using UnityEngine;
using UnityEditor;
using System.Text;

// 21.317: thiết lập nhập ảnh/FBX của Resources/ThoiTiet và tạo vật liệu mưa/sét.
public static class WeatherAssets21317
{
    const string Dir = "Assets/PhoViet/Resources/ThoiTiet/";

    static void Tex(string name, TextureWrapMode wrap, int max)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(Dir + name + ".png");
        ti.textureType = TextureImporterType.Default; ti.alphaSource = TextureImporterAlphaSource.FromInput; ti.alphaIsTransparency = true;
        ti.wrapMode = wrap; ti.mipmapEnabled = true; ti.maxTextureSize = max; ti.sRGBTexture = true;
        ti.SaveAndReimport();
    }
    static Material Mat(string name, string shader)
    {
        var path = Dir + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        var sh = Shader.Find(shader);
        if (!m) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
        else m.shader = sh;
        return m;
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        Tex("PV_Mua_Hat", TextureWrapMode.Clamp, 512);
        Tex("PV_Mua_Man", TextureWrapMode.Repeat, 1024);
        Tex("PV_Mua_Toe", TextureWrapMode.Clamp, 512);
        Tex("PV_Set_VetChay", TextureWrapMode.Clamp, 256);
        foreach (var f in new[] { "PV_Mua", "PV_Set" })
        {
            var mi = (ModelImporter)AssetImporter.GetAtPath(Dir + f + ".fbx");
            mi.materialImportMode = ModelImporterMaterialImportMode.None; mi.importAnimation = false; mi.importCameras = false; mi.importLights = false;
            mi.meshCompression = ModelImporterMeshCompression.Off; mi.isReadable = false; mi.importBlendShapes = false;
            mi.optimizeMeshVertices = false; mi.weldVertices = false;
            mi.SaveAndReimport();
        }
        var hat = Mat("PV_Mua_Hat", "PhoViet/OpenWorld/Rain"); hat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_Mua_Hat.png"); hat.SetFloat("_Mode", 0); hat.SetColor("_Color", new Color(.75f, .8f, .88f, .5f));
        var toe = Mat("PV_Mua_Toe", "PhoViet/OpenWorld/Rain"); toe.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_Mua_Toe.png"); toe.SetFloat("_Mode", 0); toe.SetColor("_Color", new Color(.8f, .84f, .9f, .7f));
        var man = Mat("PV_Mua_Man", "PhoViet/OpenWorld/Rain"); man.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_Mua_Man.png"); man.SetFloat("_Mode", 1); man.SetColor("_Color", new Color(.75f, .8f, .88f, .3f));
        var vet = Mat("PV_Set_VetChay", "PhoViet/OpenWorld/Rain"); vet.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "PV_Set_VetChay.png"); vet.SetFloat("_Mode", 2); vet.SetColor("_Color", new Color(1, 1, 1, .92f));
        vet.renderQueue = 2460;   // vết trên mặt đường: trước mưa/hạt trong suốt
        var tia = Mat("PV_Set_Tia", "PhoViet/OpenWorld/Lightning"); tia.SetFloat("_CoreWidth", 0.95f); tia.SetFloat("_GlowWidth", 6.5f); tia.SetFloat("_MinPix", 0.006f); tia.SetColor("_GlowColor", new Color(.42f, .5f, 1.25f, 1f));
        var hatSet = Mat("PV_Set_Hat", "PhoViet/OpenWorld/Electric"); hatSet.SetFloat("_Mode", 1); hatSet.SetColor("_Color", new Color(1.3f, 1.55f, 2.4f, 1f));
        foreach (var m in new[] { hat, toe, man, vet, tia, hatSet }) EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        // kiểm tra kênh UV của tia sét
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(Dir + "PV_Set.fbx");
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = mf.sharedMesh;
            sb.AppendLine($"{mf.name} v={mesh.vertexCount} uvCh={(mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord3) ? 4 : mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord2) ? 3 : 2)} bounds={mesh.bounds} rot={mf.transform.localRotation.eulerAngles} scale={mf.transform.localScale}");
        }
        var tube = AssetDatabase.LoadAssetAtPath<GameObject>(Dir + "PV_Mua.fbx");
        foreach (var mf in tube.GetComponentsInChildren<MeshFilter>(true)) sb.AppendLine($"{mf.name} bounds={mf.sharedMesh.bounds} rot={mf.transform.localRotation.eulerAngles}");
        return sb.ToString();
    }
}
