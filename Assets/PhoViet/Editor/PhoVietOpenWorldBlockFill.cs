using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.306 — người dùng (ảnh kẻ 2 đường xanh lá): khu nhà cao tầng phía đông sông (x 430–675, z 229–416, sát khúc cua sông)
    // thiếu hàng nhà phố ở mặt BẮC (cả hàng) và mặt NAM (đoạn từ góc tây tới tháp đầu tiên + các khe giữa tháp) → xe chạy vào sân
    // trong được. Menu 21 dựng hàng nhà phố còn thiếu:
    //   • Sao chép lần lượt các căn của hàng mặt tây cùng khu (OW_Mau_NhaPho_* x = 432,5, quay 0°) — giữ mặt tiền kính, nội thất,
    //     hộp LOD xa, biển hiệu đi kèm và hộp va chạm (VaCham_NhaPho) theo đúng vị trí tương đối với căn gốc.
    //   • Mặt tiền mẫu nhà = trục −X cục bộ; tâm căn đặt ĐÚNG mép trong vỉa hè (như hàng gốc). Mặt bắc quay 90° (mặt tiền ra
    //     đường phía bắc, z+), mặt nam quay 270° (ra đường phía nam, z−) — không quay vào sân trong.
    //   • Bỏ chỗ trùng nhà/tháp có sẵn (kiểm tra hộp va chạm lùi 0,3 m). Chạy lại được: xóa các căn «_Lap21306» cũ trước.
    public static class PhoVietOpenWorldBlockFill
    {
        const string Tag = "_Lap21306";
        // (mép trong vỉa hè, góc quay, x bắt đầu, x kết thúc)
        static readonly (float facadeZ, float rotY, float x0, float x1)[] Rows =
        {
            (417.6f, 90f, 448.5f, 656.5f),    // mặt bắc
            (227.9f, 270f, 448.5f, 656.5f),   // mặt nam
        };

        [MenuItem("Phố Việt/Bản đồ mở/21. Lấp hàng nhà phố thiếu ở khu phía đông khúc cua sông (21.306)")]
        public static void Menu() => Debug.Log(Run());

        public static string Run()
        {
            var map = GameObject.Find("BanDoMo"); if (!map) return "mở scene PhoVietOpenWorld trước";
            var nhaPho = map.transform.Find("NhaPho"); var bangHieu = map.transform.Find("BangHieu"); var vaCham = map.transform.Find("VaCham_NhaPho");
            // dọn lần chạy trước
            int removed = 0;
            foreach (var root in new[] { nhaPho, bangHieu, vaCham })
                for (int i = root.childCount - 1; i >= 0; i--) if (root.GetChild(i).name.EndsWith(Tag)) { Object.DestroyImmediate(root.GetChild(i).gameObject); removed++; }

            // căn gốc: hàng mặt tây của khu (x = 432,5, quay 0°)
            var src = new List<Transform>();
            foreach (Transform t in nhaPho)
                if (Mathf.Abs(t.position.x - 432.5f) < .2f && t.position.z > 229f && t.position.z < 416f && Mathf.Abs(Mathf.DeltaAngle(t.eulerAngles.y, 0f)) < 1f) src.Add(t);
            src.Sort((a, b) => a.position.z.CompareTo(b.position.z));
            if (src.Count == 0) return "không tìm thấy hàng nhà gốc";

            // bề rộng mỗi căn gốc (theo hộp va chạm) + biển hiệu + hộp va chạm tương đối
            // khung tham chiếu = vị trí căn gốc + chỉ quay quanh Y (gốc FBX còn xoay X −90° nên không dùng trục cục bộ của nó)
            var info = new List<(Transform t, float w, Matrix4x4 houseRel, List<(GameObject go, Matrix4x4 rel)> signs, List<(Vector3 c, Vector3 s)> cols)>();
            foreach (var t in src)
            {
                var inv = Matrix4x4.Translate(t.position).inverse; var signs = new List<(GameObject, Matrix4x4)>(); var cols = new List<(Vector3, Vector3)>();
                float zMin = float.MaxValue, zMax = float.MinValue;
                foreach (Transform c in vaCham)
                {
                    var bc = c.GetComponent<BoxCollider>(); if (!bc) continue; var b = bc.bounds;
                    if (b.min.x > 431f && b.max.x < 450f && b.center.z > t.position.z - 4.5f && b.center.z < t.position.z + 4.5f && b.min.z < t.position.z && b.max.z > t.position.z)
                    { cols.Add((inv.MultiplyPoint3x4(b.center), b.size)); zMin = Mathf.Min(zMin, b.min.z); zMax = Mathf.Max(zMax, b.max.z); }
                }
                float w = zMax > zMin ? zMax - zMin : 5f;
                foreach (Transform s in bangHieu)
                {
                    var p = s.position; var rs = s.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
                    var bb = rs[0].bounds; foreach (var r in rs) bb.Encapsulate(r.bounds);
                    if (bb.center.x > 426f && bb.center.x < 434f && bb.center.z > t.position.z - w * .5f && bb.center.z < t.position.z + w * .5f)
                        signs.Add((s.gameObject, inv * s.localToWorldMatrix));
                }
                info.Add((t, w, inv * t.localToWorldMatrix, signs, cols));
            }

            int houses = 0, skipped = 0, signsMade = 0, k = 0; var log = new System.Text.StringBuilder();
            foreach (var row in Rows)
            {
                var rot = Quaternion.Euler(0, row.rotY, 0);
                float x = row.x0;
                while (x < row.x1 - 3f)
                {
                    var s = info[k % info.Count]; k++;
                    float w = s.w; if (x + w > row.x1 + .5f) { // căn cuối: chọn căn vừa chỗ còn lại
                        int best = -1; float left = row.x1 - x;
                        for (int i = 0; i < info.Count; i++) if (info[i].w <= left + .3f && (best < 0 || info[i].w > info[best].w)) best = i;
                        if (best < 0) break; s = info[best]; w = s.w; }
                    var pos = new Vector3(x + w * .5f, 0f, row.facadeZ);
                    var m = Matrix4x4.TRS(pos, rot, Vector3.one);
                    // trùng nhà/tháp có sẵn?
                    bool clash = false;
                    foreach (var (cc, cs) in s.cols)
                    {
                        var wc = m.MultiplyPoint3x4(cc); var half = Abs(rot * cs) * .5f - new Vector3(.3f, .3f, .3f); half.y = Mathf.Max(half.y, .5f);
                        foreach (var hit in Physics.OverlapBox(wc + Vector3.up * .5f, half, Quaternion.identity, ~((1 << 2) | (1 << 28)), QueryTriggerInteraction.Ignore))
                            if (hit.name != "MatDat" && !(hit.transform.parent && hit.transform.parent.name == "VaCham_Cay")) { clash = true; break; }
                        if (clash) break;
                    }
                    if (clash) { skipped++; x += 1f; continue; }
                    var go = Object.Instantiate(s.t.gameObject, nhaPho); go.name = s.t.name + Tag;
                    var hm = m * s.houseRel; go.transform.SetPositionAndRotation(hm.GetColumn(3), hm.rotation);
                    foreach (var (sg, rel) in s.signs)
                    {
                        var g = Object.Instantiate(sg, bangHieu); g.name = sg.name + Tag; var mm = m * rel;
                        g.transform.SetPositionAndRotation(mm.GetColumn(3), mm.rotation); signsMade++;
                    }
                    foreach (var (cc, cs) in s.cols)
                    {
                        var cgo = new GameObject("VaCham" + Tag); cgo.transform.SetParent(vaCham, false);
                        cgo.transform.SetPositionAndRotation(m.MultiplyPoint3x4(cc), rot);
                        var bc = cgo.AddComponent<BoxCollider>(); bc.center = Vector3.zero; bc.size = cs;
                        GameObjectUtility.SetStaticEditorFlags(cgo, GameObjectUtility.GetStaticEditorFlags(s.t.gameObject));
                    }
                    houses++; x += w;
                }
                log.Append($"hàng z={row.facadeZ}: tới x={x:F1}; ");
            }
            // khe hẹp còn lại (cạnh tháp có sẵn, < bề rộng căn nhỏ nhất): lấy căn gần vừa nhất rồi co/giãn bề ngang cho khớp khe
            int fillers = 0;
            foreach (var row in Rows)
            {
                var rot = Quaternion.Euler(0, row.rotY, 0);
                float probeZ = row.facadeZ + (row.rotY == 90f ? -2.5f : 2.5f);
                bool Solid(float px)
                {
                    foreach (var h in Physics.OverlapBox(new Vector3(px, 1f, probeZ), new Vector3(.1f, .8f, .1f), Quaternion.identity, ~((1 << 2) | (1 << 28)), QueryTriggerInteraction.Ignore))
                        if (h.name != "MatDat") return true;
                    return false;
                }
                float gx = -1f;
                for (float px = row.x0 + .3f; px <= row.x1 - .3f + .001f; px += .25f)
                {
                    bool solid = Solid(px);
                    if (!solid && gx < 0f) gx = px;
                    bool end = solid || px + .25f > row.x1 - .3f;
                    if (gx >= 0f && end)
                    {
                        float a0 = gx - .25f, a1 = solid ? px : row.x1; float gw = a1 - a0;
                        if (gw >= .8f && gw <= 9f)
                        {
                            int best = 0; for (int i = 1; i < info.Count; i++) if (Mathf.Abs(info[i].w - gw) < Mathf.Abs(info[best].w - gw)) best = i;
                            var sI = info[best]; float k2 = gw / sI.w;
                            var pos = new Vector3((a0 + a1) * .5f, 0f, row.facadeZ);
                            var m = Matrix4x4.TRS(pos, rot, Vector3.one) * Matrix4x4.Scale(new Vector3(1f, 1f, k2));   // khung gốc: bề ngang = trục z
                            var go = Object.Instantiate(sI.t.gameObject, nhaPho); go.name = sI.t.name + "_Hep" + Tag;
                            var hm = m * sI.houseRel; go.transform.position = hm.GetColumn(3); go.transform.rotation = hm.rotation;
                            var ls = go.transform.localScale; var wAxis = go.transform.InverseTransformDirection(rot * Vector3.forward);
                            go.transform.localScale = new Vector3(ls.x * (Mathf.Abs(wAxis.x) > .5f ? k2 : 1f), ls.y * (Mathf.Abs(wAxis.y) > .5f ? k2 : 1f), ls.z * (Mathf.Abs(wAxis.z) > .5f ? k2 : 1f));
                            foreach (var (cc, cs) in sI.cols)
                            {
                                var cgo = new GameObject("VaCham" + Tag); cgo.transform.SetParent(vaCham, false);
                                cgo.transform.SetPositionAndRotation(m.MultiplyPoint3x4(cc), rot);
                                var bc = cgo.AddComponent<BoxCollider>(); bc.size = new Vector3(cs.x, cs.y, cs.z * k2);
                            }
                            fillers++;
                        }
                        gx = -1f;
                    }
                }
            }
            log.Append($"lấp {fillers} khe hẹp; ");
            var scene = EditorSceneManager.GetActiveScene(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return $"Dọn {removed} vật cũ; dựng {houses} căn nhà phố + {signsMade} biển hiệu, bỏ {skipped} chỗ trùng nhà/tháp; mẫu {info.Count} căn gốc. {log}";
        }
        static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}
