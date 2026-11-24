using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// 21.384 — dán 2 banner «Cù Lao Petshop» (Blender: ArtSource/Blender/OpenWorld/build_petshop_banner.py
// → Library/OW_BangRon_Petshop.fbx) lên 2 mảng tường trống ở ngã 3 nơi xe xuất phát (đường ven sông × đường vào phố z ≈ −430).
//   Ngang 13 × 9,5 m: mặt hông bắc nhà góc nam OW_NhaNoiBat_R2_4005 (mặt phẳng z = −438, x 186…202), quay +z.
//   Dọc   7 × 14 m:   mặt hông đông nhà góc bắc OW_Mau_NhaPho_7_0_0 (mặt phẳng x = 200,2, z −422,5…−413,9), quay +x.
// Ngã 3 phía trước (hướng về cầu, đường vào phố z ≈ −300):
//   Dọc 7 × 14 m:       mặt hông bắc nhà ven sông OW_Mau_NhaPho_5_0_3_B (mặt phẳng z = −309,3, x 193,4…202, cao 21,65), quay +z.
//   Dọc thu 75 %:       mặt hông đông nhà góc OW_Mau_NhaPho_5_5_1 (mặt phẳng x = 199,7, z −292,5…−283,9, cao 15,05), quay +x.
// 21.386 — ngã 3 thứ ba về phía cầu (đường vào phố z ≈ −150):
//   Dọc 7 × 14 m:       mặt hông đông nhà góc OW_Mau_NhaPho_6_0_3_B (mặt phẳng x = 199,7, z −171,6…−163, cao 23,06), quay +x.
//   Dọc thu 75 %:       mặt hông nam nhà ven sông OW_Mau_NhaPho_7_0_3_PetShop (mặt phẳng z = −135,3, x 193,4…202, cao 15,05), quay −z.
// Gốc scene riêng «BangRonPetshop» (ngoài BanDoMo → không đổi dữ liệu occlusion), renderer không static, không đổ bóng,
// LODGroup ẩn khi < 1 % màn hình. Chạy lại được (xoá gốc cũ rồi dựng lại).
public static class InstallPetshopBanner21384
{
    const string Fbx = "Assets/PhoViet/OpenWorld/Library/OW_BangRon_Petshop.fbx";
    const string MatDir = "Assets/PhoViet/OpenWorld/Materials/";
    // 21.389: mặt banner là ảnh mẫu 2 (OW_BangRonPetshop_Doc/Ngang, Standard + texture) — build_petshop_banner_anh.py.
    static readonly string[] Mats = { "OW_SignBG_16808A", "OW_FrameWhite", "OW_SignTextWhite", "OW_PetshopPaw", "OW_AluCharcoal", "OW_LampBulb",
                                      "OW_BangRonPetshop_Doc", "OW_BangRonPetshop_Ngang" };

    public static string Run()
    {
        var log = new System.Text.StringBuilder();
        AssetDatabase.ImportAsset(Fbx, ImportAssetOptions.ForceUpdate);
        var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
        bool changed = false;
        if (imp.materialImportMode != ModelImporterMaterialImportMode.ImportStandard) { imp.materialImportMode = ModelImporterMaterialImportMode.ImportStandard; changed = true; }
        if (imp.importAnimation) { imp.importAnimation = false; changed = true; }
        if (imp.importCameras || imp.importLights) { imp.importCameras = false; imp.importLights = false; changed = true; }
        foreach (var n in Mats)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + n + ".mat");
            if (!m) { log.AppendLine("THIẾU vật liệu " + n); continue; }
            imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), n), m);
            changed = true;
        }
        if (changed) imp.SaveAndReimport();

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var old in scene.GetRootGameObjects().Where(g => g.name == "BangRonPetshop").ToArray()) Object.DestroyImmediate(old);
        var root = new GameObject("BangRonPetshop");
        SceneManager_Move(root, scene);

        Place(root, model, "OW_BangRon_Petshop_Ngang", new Vector3(194f, 8.5f, -438f), Vector3.forward, 1f, log);
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(200.2f, 6.5f, -418.2f), Vector3.right, 1f, log);
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(197.7f, 6.5f, -309.3f), Vector3.forward, 1f, log, "_Nga3Truoc_Nam");
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(199.7f, 3.6f, -288.2f), Vector3.right, 0.75f, log, "_Nga3Truoc_Bac");
        // 21.386 — ngã 3 thứ ba (đường vào phố z ≈ −150).
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(199.7f, 6.5f, -167.3f), Vector3.right, 1f, log, "_Nga3Thu3_Nam");
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(197.7f, 3.6f, -135.3f), Vector3.back, 0.75f, log, "_Nga3Thu3_Bac");
        // 21.390 — ngã 3 thứ tư (đường vào phố z ≈ 0): tường bắc nhà góc nam thấp (z = −9,3, x 193,4…202, cao 11,8)
        // → banner ngang thu 60 %; tường nam nhà góc bắc (z = 9,2, cao 16,3) → banner dọc 75 %.
        Place(root, model, "OW_BangRon_Petshop_Ngang", new Vector3(197.7f, 3.4f, -9.3f), Vector3.forward, 0.6f, log, "_Nga3Thu4_Nam");
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(197.7f, 3.6f, 9.2f), Vector3.back, 0.75f, log, "_Nga3Thu4_Bac");
        // 21.390 — ngã 3 thứ năm (đường vào phố z ≈ 100): mặt đông 2 nhà góc lùi vào, quay ra đường ven sông (+x).
        //   Nam: x = 198,2, z 84,2…92,5 (cao 18,4) → dọc 90 %;  Bắc: x = 200,2, z 107,5…116,1 (cao 16,3) → dọc 75 %.
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(198.2f, 3.2f, 88.4f), Vector3.right, 0.9f, log, "_Nga3Thu5_Nam");
        Place(root, model, "OW_BangRon_Petshop_Doc", new Vector3(200.2f, 3.4f, 111.8f), Vector3.right, 0.75f, log, "_Nga3Thu5_Bac");

        EditorSceneManager.MarkSceneDirty(scene);
        return log.ToString();
    }

    static void SceneManager_Move(GameObject g, UnityEngine.SceneManagement.Scene s)
    {
        if (g.scene != s) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g, s);
    }

    // Đặt mesh tên `name` từ FBX: gốc lưới = giữa mép dưới trên mặt tường; quay sao cho mặt đọc nhìn theo `normal`.
    static void Place(GameObject root, GameObject model, string name, Vector3 wallBottomCentre, Vector3 normal, float scale, System.Text.StringBuilder log, string suffix = "")
    {
        var src = model.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == name);
        var mesh = src.sharedMesh;
        var mats = src.GetComponent<MeshRenderer>().sharedMaterials;

        // Rotation của node trong FBX (đổi trục Blender → Unity) + tìm hướng mặt đọc: phía mà khối chìa ra xa tường nhất.
        var srcRot = src.transform.localRotation;
        var b = mesh.bounds;
        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        Vector3 front = Vector3.zero; Vector3 up = Vector3.up; float best = 0f;
        foreach (var a in axes)
        {
            var w = srcRot * a;                                     // trục cục bộ của mesh sau rotation node
            float lo = Vector3.Dot(b.min, a), hi = Vector3.Dot(b.max, a), size = hi - lo;
            if (Mathf.Abs(w.y) > 0.9f) { up = w.y > 0 ? w : -w; continue; }
            if (best == 0f || size < best) { best = size; front = Mathf.Abs(hi) > Mathf.Abs(lo) ? w : -w; }
        }
        var go = new GameObject(name + suffix);
        go.transform.SetParent(root.transform, false);
        var rot = Quaternion.FromToRotation(front, normal);
        // Gốc lưới nằm trên mặt tường; đẩy ra 2 cm cho khỏi z-fight.
        go.transform.SetPositionAndRotation(wallBottomCentre + normal * 0.02f, rot * srcRot);
        go.transform.localScale = Vector3.one * scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = mats;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = true;
        var lod = go.AddComponent<LODGroup>();
        lod.SetLODs(new[] { new LOD(0.01f, new Renderer[] { mr }) });
        lod.RecalculateBounds();
        var wb = mr.bounds;
        log.AppendLine(go.name + " bounds " + wb.min.ToString("F2") + ".." + wb.max.ToString("F2") + " mats=" + string.Join(",", mats.Select(m => m ? m.name : "null")));
    }
}
