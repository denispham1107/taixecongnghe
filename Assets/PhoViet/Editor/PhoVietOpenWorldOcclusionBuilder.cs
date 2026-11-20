using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.OpenWorldEditor
{
    // 21.282 — Occlusion culling cho bản đồ mở (menu Phố Việt/Bản đồ mở/12).
    // Trước đây 15.255 renderer đã gắn cờ Occluder/Occludee và camera bật occlusion nhưng CHƯA TỪNG nướng dữ liệu
    // → mỗi khung vẽ mọi thứ trong tầm nhìn 1.500 m (~2.900 lệnh vẽ, ~700 bộ đệm instancing/khung) → Safari iPhone sập
    // vài giây sau khi vào game. Nướng dữ liệu che khuất chỉ bỏ những gì bị che hoàn toàn: hình ảnh không đổi.
    // Quy tắc cờ:
    //  • Nội thất (LODGroup «NoiThat», nhìn qua kính): không cờ + allowOcclusionWhenDynamic = false → không bao giờ bị loại.
    //  • Renderer chỉ có vật liệu trong suốt (kính, hạt, nước): chỉ Occludee.
    //  • Renderer ở LOD > 0 của LODGroup: chỉ Occludee (vật che lấy LOD0).
    //  • Còn lại (đặc, kể cả nhà mặt tiền kính vì tường hông/sau vẫn chắn thật): Occluder + Occludee.
    public static class PhoVietOpenWorldOcclusionBuilder
    {
        const string Scene = "Assets/PhoViet/Scenes/PhoVietOpenWorld.unity";
        public const float SmallestOccluder = 5f, SmallestHole = 0.25f, BackfaceThreshold = 100f;

        [MenuItem("Phố Việt/Bản đồ mở/12. Occlusion culling (cờ + nướng)")]
        public static void Menu()
        {
            string r = ApplyFlags();
            Debug.Log("[Phố Việt] " + r);
            Bake(false);
        }

        public static string ApplyFlags()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != Scene) throw new System.Exception("Mở scene bản đồ mở trước: " + Scene);
            var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "BanDoMo");
            if (!root) throw new System.Exception("Không thấy BanDoMo");
            var lodLevel = new Dictionary<Renderer, int>(); var interior = new HashSet<Renderer>();
            foreach (var lg in root.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = lg.GetLODs();
                for (int i = 0; i < lods.Length; i++) foreach (var r in lods[i].renderers) if (r) { lodLevel[r] = i; if (lg.name == "NoiThat") interior.Add(r); }
                if (lg.name == "NoiThat") foreach (var r in lg.GetComponentsInChildren<Renderer>(true)) interior.Add(r);
            }
            int nInt = 0, nDee = 0, nBoth = 0, changed = 0;
            const StaticEditorFlags Occ = StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var f = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                var keep = f & ~Occ;
                StaticEditorFlags want; bool dyn = true;
                bool allTransparent = r.sharedMaterials.Length > 0 && r.sharedMaterials.All(m => m && m.renderQueue >= 2450);
                // 21.286: cây dừa (tán lá thưa, bệ thấp) không che khuất vật khác
                if (r.sharedMaterials.Any(m => m && m.shader && m.shader.name == "PhoViet/OpenWorld/Palm")) allTransparent = true;
                if (interior.Contains(r)) { want = 0; dyn = false; nInt++; }
                else if (allTransparent || (lodLevel.TryGetValue(r, out int lv) && lv > 0)) { want = StaticEditorFlags.OccludeeStatic; nDee++; }
                else { want = Occ; nBoth++; }
                if ((f & Occ) != want) { GameObjectUtility.SetStaticEditorFlags(r.gameObject, keep | want); changed++; }
                if (r.allowOcclusionWhenDynamic != dyn) { Undo.RecordObject(r, "occlusion"); r.allowOcclusionWhenDynamic = dyn; changed++; }
            }
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)) cam.useOcclusionCulling = true;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return $"cờ che khuất: nội thất {nInt} (không loại), chỉ occludee {nDee}, occluder+occludee {nBoth}, đổi {changed}";
        }

        public static void Bake(bool blocking)
        {
            StaticOcclusionCulling.smallestOccluder = SmallestOccluder;
            StaticOcclusionCulling.smallestHole = SmallestHole;
            StaticOcclusionCulling.backfaceThreshold = BackfaceThreshold;
            if (blocking) StaticOcclusionCulling.Compute(); else StaticOcclusionCulling.GenerateInBackground();
        }

        public static string Finish()
        {
            if (StaticOcclusionCulling.isRunning) return "đang nướng";
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "dữ liệu che khuất " + (StaticOcclusionCulling.umbraDataSize / 1024) + " KB";
        }
    }
}
