using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.324 — tìm sẵn đỉnh KÍNH và cụm ĐÈN trên lưới LOD0 của từng mẫu ô tô (chạy trong Play, đọc texture qua RenderTexture):
//  • kính: đỉnh thuộc xương thân, không phải vùng sơn, tối (V < ngưỡng), không phát sáng, nằm cao hơn ~42 % chiều cao xe;
//  • đèn: đỉnh có ảnh phát sáng (_EmissionMap) sáng → gom 4 cụm trước/sau × trái/phải; lưu đỉnh gần tâm cụm + bán kính.
// Ghi Assets/PhoViet/Resources/XeHong/PV_XeHong_Data.json (đọc lúc chạy bởi OpenWorldCarDent).
// Show(): dựng bản sao từng mẫu xe trên vỉa hè cạnh xe người chơi, tô đỏ tam giác kính + cầu vàng ở đèn để kiểm bằng ảnh.
public static class CarGlass21324
{
    public const string OutPath = "Assets/PhoViet/Resources/XeHong/PV_XeHong_Data.json";
    static readonly string[] BodyBones = { "Body", "Frame", "BumperFront", "BumperRear", "Cargo" };

    class Target { public string key; public Renderer r; public Transform root; }

    static List<Target> Targets()
    {
        var list = new List<Target>();
        var p = UnityEngine.Object.FindAnyObjectByType<OpenWorldDriver>();
        foreach (var r in p.GetComponentsInChildren<SkinnedMeshRenderer>(true)) if (r.name.EndsWith("_Body")) list.Add(new Target { key = r.sharedMesh.name, r = r, root = p.transform });
        var pol = UnityEngine.Object.FindAnyObjectByType<OpenWorldPolice>();
        if (pol) foreach (var r in pol.GetComponentsInChildren<MeshRenderer>(true)) if (r.name.StartsWith("Vietnam")) list.Add(new Target { key = r.GetComponent<MeshFilter>().sharedMesh.name + "@CSGT", r = r, root = pol.transform });
        var tr = UnityEngine.Object.FindAnyObjectByType<OpenWorldTraffic>();
        var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
        var seen = new HashSet<string>();
        foreach (var v in pool)
        {
            var t = v.GetType(); if ((bool)t.GetField("bike").GetValue(v)) continue;
            var rends = (Renderer[])t.GetField("renderers").GetValue(v); var root = (Transform)t.GetField("root").GetValue(v);
            foreach (var r in rends) if (r is SkinnedMeshRenderer s && s.name.EndsWith("_Body") && seen.Add(s.sharedMesh.name)) list.Add(new Target { key = s.sharedMesh.name, r = s, root = root });
        }
        return list;
    }

    static Color[] Read(Texture tex, out int w, out int h)
    {
        w = h = 0; if (!tex) return null;
        w = Mathf.Min(tex.width, 1024); h = Mathf.Min(tex.height, 1024);
        var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(tex, rt); var prev = RenderTexture.active; RenderTexture.active = rt;
        var t2 = new Texture2D(w, h, TextureFormat.RGBA32, false); t2.ReadPixels(new Rect(0, 0, w, h), 0, 0); t2.Apply();
        RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
        var px = t2.GetPixels(); UnityEngine.Object.DestroyImmediate(t2); return px;
    }
    static Color Sample(Color[] px, int w, int h, Vector2 uv)
    {
        if (px == null) return Color.black;
        float u = uv.x - Mathf.Floor(uv.x), v = uv.y - Mathf.Floor(uv.y);
        int x = Mathf.Clamp((int)(u * w), 0, w - 1), y = Mathf.Clamp((int)(v * h), 0, h - 1);
        return px[y * w + x];
    }

    // vị trí đỉnh trong toạ độ gốc xe; null nếu đỉnh không thuộc thân (bánh, gương…)
    static Vector3[] lastNormals;
    static Vector3[] CarSpace(Target t, Mesh m, out bool[] body)
    {
        var verts = m.vertices; var res = new Vector3[verts.Length]; body = new bool[verts.Length];
        var nr = m.normals; lastNormals = new Vector3[verts.Length];
        var toRoot = t.root.worldToLocalMatrix;
        if (t.r is SkinnedMeshRenderer s)
        {
            var bw = m.boneWeights; var bp = m.bindposes; var bones = s.bones;
            var mats = new Matrix4x4[bones.Length]; var isBody = new bool[bones.Length];
            for (int i = 0; i < bones.Length; i++) { mats[i] = toRoot * bones[i].localToWorldMatrix * bp[i]; isBody[i] = bones[i] && BodyBones.Contains(bones[i].name); }
            for (int i = 0; i < verts.Length; i++) { int b = bw[i].boneIndex0; res[i] = mats[b].MultiplyPoint3x4(verts[i]); lastNormals[i] = mats[b].MultiplyVector(nr[i]).normalized; body[i] = isBody[b]; }
        }
        else
        {
            var mtx = toRoot * t.r.transform.localToWorldMatrix;
            for (int i = 0; i < verts.Length; i++) { res[i] = mtx.MultiplyPoint3x4(verts[i]); lastNormals[i] = mtx.MultiplyVector(nr[i]).normalized; body[i] = true; }
        }
        return res;
    }

    [Serializable] public class Lamp { public int v; public float r; public bool front; }
    [Serializable] public class Entry { public string mesh; public string glass; public int glassCount; public List<Lamp> lamps = new List<Lamp>(); }
    [Serializable] public class Data { public List<Entry> cars = new List<Entry>(); }

    static Data last;
    public static string Analyze(float darkV, float minHeight, float meshy) { darkMeshy = meshy; return Compute(darkV, minHeight, true); }
    static float darkMeshy = 0.3f; static string debugKey;
    public static string Debug(string key) { debugKey = key; darkMeshy = 0.6f; var r = Compute(0.3f, 0.42f, false); debugKey = null; return r; }
    static string Compute(float darkV, float minHeight, bool write)
    {
        var data = new Data(); var sb = new System.Text.StringBuilder(); last = data;
        foreach (var t in Targets())
        {
            var m = t.r is SkinnedMeshRenderer s ? s.sharedMesh : t.r.GetComponent<MeshFilter>().sharedMesh;
            if (!m.isReadable) { sb.AppendLine(t.key + ": lưới không đọc được"); continue; }
            var mat = t.r.sharedMaterial;
            var alb = Read(mat.GetTexture("_MainTex"), out int aw, out int ah);
            var em = mat.HasProperty("_EmissionMap") ? Read(mat.GetTexture("_EmissionMap"), out int ew, out int eh) : null; ew = eh = 0;
            if (em != null) { var et = mat.GetTexture("_EmissionMap"); ew = Mathf.Min(et.width, 1024); eh = Mathf.Min(et.height, 1024); }
            var pm = mat.HasProperty("_PaintMask") && mat.GetTexture("_PaintMask") ? Read(mat.GetTexture("_PaintMask"), out int pw, out int ph) : null;
            int pw2 = 0, ph2 = 0; if (pm != null) { var pt = mat.GetTexture("_PaintMask"); pw2 = Mathf.Min(pt.width, 1024); ph2 = Mathf.Min(pt.height, 1024); }
            var uv = m.uv; var pos = CarSpace(t, m, out var body); var cn = lastNormals;
            float minY = float.MaxValue, maxY = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            for (int i = 0; i < pos.Length; i++) if (body[i]) { minY = Mathf.Min(minY, pos[i].y); maxY = Mathf.Max(maxY, pos[i].y); minZ = Mathf.Min(minZ, pos[i].z); maxZ = Mathf.Max(maxZ, pos[i].z); }
            float H = maxY - minY;
            var glass = new bool[pos.Length]; int gc = 0; var lampIdx = new List<int>();
            for (int i = 0; i < pos.Length; i++)
            {
                if (!body[i]) continue;
                var a = Sample(alb, aw, ah, uv[i]); float V = Mathf.Max(a.r, Mathf.Max(a.g, a.b));
                float e = em != null ? Sample(em, ew, eh, uv[i]).maxColorComponent : 0f;
                float paint = pm != null ? Sample(pm, pw2, ph2, uv[i]).r : 0f;
                if (e > 0.35f) { lampIdx.Add(i); continue; }
                bool isGlass = pm != null ? V < darkMeshy && cn[i].y < 0.88f && pos[i].y > minY + minHeight * H
                                         : V < darkV && pos[i].y > minY + minHeight * H;
                if (isGlass) { glass[i] = true; gc++; }
            }
            if (debugKey != null && t.key.Contains(debugKey))
            {   // thống kê theo dải cao: V/paint/gloss trung bình của đỉnh trên 70 % chiều cao, |x| < 0.3
                var gl = mat.HasProperty("_MetallicGlossMap") && mat.GetTexture("_MetallicGlossMap") ? Read(mat.GetTexture("_MetallicGlossMap"), out int gw, out int gh) : null;
                int gw2 = 0, gh2 = 0; if (gl != null) { var gt = mat.GetTexture("_MetallicGlossMap"); gw2 = Mathf.Min(gt.width, 1024); gh2 = Mathf.Min(gt.height, 1024); }
                var nrm = m.normals;
                for (float zz = minZ; zz < maxZ; zz += (maxZ - minZ) / 10f)
                {
                    float sv = 0, sp = 0, sg = 0; int n = 0;
                    for (int i = 0; i < pos.Length; i++)
                    {
                        if (!body[i] || pos[i].y < minY + 0.6f * H || Mathf.Abs(pos[i].x) > 0.35f || pos[i].z < zz || pos[i].z >= zz + (maxZ - minZ) / 10f) continue;
                        var a = Sample(alb, aw, ah, uv[i]); sv += Mathf.Max(a.r, Mathf.Max(a.g, a.b)); sp += pm != null ? Sample(pm, pw2, ph2, uv[i]).r : 0; sg += gl != null ? Sample(gl, gw2, gh2, uv[i]).a : 0; n++;
                    }
                    if (n > 0) sb.AppendLine($"   z {zz:F2}: n {n} V {sv / n:F2} paint {sp / n:F2} gloss {sg / n:F2} maxY {maxY:F2}");
                }
            }
            var entry = new Entry { mesh = t.key.Replace("@CSGT", ""), glassCount = gc };
            var bits = new byte[(pos.Length + 7) / 8]; for (int i = 0; i < pos.Length; i++) if (glass[i]) bits[i >> 3] |= (byte)(1 << (i & 7));
            entry.glass = Convert.ToBase64String(bits);
            // 4 cụm đèn
            foreach (bool front in new[] { true, false })
                foreach (float side in new[] { -1f, 1f })
                {
                    var c = lampIdx.Where(i => (pos[i].z > (minZ + maxZ) * 0.5f) == front && Mathf.Sign(pos[i].x + 1e-4f) == side).ToList();
                    if (c.Count < 3) continue;
                    var ctr = Vector3.zero; foreach (var i in c) ctr += pos[i]; ctr /= c.Count;
                    int best = c.OrderBy(i => (pos[i] - ctr).sqrMagnitude).First();
                    float r = Mathf.Clamp(c.Max(i => (pos[i] - ctr).magnitude), 0.08f, 0.35f);
                    entry.lamps.Add(new Lamp { v = best, r = r, front = front });
                }
            data.cars.Add(entry);
            sb.AppendLine($"{t.key}: v{pos.Length} H {H:F2} L {maxZ - minZ:F2} kính {gc} đèn {lampIdx.Count} cụm {entry.lamps.Count} [{string.Join(" ", entry.lamps.Select(l => (l.front ? "T" : "S") + pos[l.v].ToString("F2") + " r" + l.r.ToString("F2")))}] albedo {aw}x{ah} em {(em != null)} paint {(pm != null)}");
        }
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(OutPath));
        if (write) System.IO.File.WriteAllText(OutPath, ToJson(data));
        return sb.ToString();
    }

    // Kiểm bằng ảnh: bản sao từng mẫu đặt thành hàng; tam giác kính tô đỏ, đèn cầu vàng.
    // JsonUtility không đọc được kiểu trong assembly tạm của run_script → ghi tay
    static string ToJson(Data d)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        return "{\"cars\":[" + string.Join(",", d.cars.Select(e => "{\"mesh\":\"" + e.mesh + "\",\"glass\":\"" + e.glass + "\",\"glassCount\":" + e.glassCount +
            ",\"lamps\":[" + string.Join(",", e.lamps.Select(l => "{\"v\":" + l.v + ",\"r\":" + l.r.ToString("0.###", inv) + ",\"front\":" + (l.front ? "true" : "false") + "}")) + "]}")) + "]}";
    }
    public static string Show(float spacing, float darkV, float minHeight, float meshy)
    {
        darkMeshy = meshy;
        Compute(darkV, minHeight, false); var data = last;
        var p = UnityEngine.Object.FindAnyObjectByType<OpenWorldDriver>();
        foreach (var o in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)) if (o && o.name == "KiemKinh21324") UnityEngine.Object.DestroyImmediate(o.gameObject);
        var holder = new GameObject("KiemKinh21324").transform;
        var right = p.transform.right; var fwd = p.transform.forward; var basePos = p.transform.position;
        var red = new Material(Shader.Find("Unlit/Color")) { color = Color.red };
        var yellow = new Material(Shader.Find("Unlit/Color")) { color = Color.yellow };
        int k = 0; var sb = new System.Text.StringBuilder();
        foreach (var t in Targets())
        {
            var e = data.cars.FirstOrDefault(c => c.mesh == t.key.Replace("@CSGT", "")); if (e == null) continue;
            var m = t.r is SkinnedMeshRenderer s ? s.sharedMesh : t.r.GetComponent<MeshFilter>().sharedMesh;
            var pos = CarSpace(t, m, out _);
            var bits = Convert.FromBase64String(e.glass);
            var tris = m.triangles; var gv = new List<Vector3>();
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                bool ga = (bits[a >> 3] & (1 << (a & 7))) != 0, gb = (bits[b >> 3] & (1 << (b & 7))) != 0, gcc = (bits[c >> 3] & (1 << (c & 7))) != 0;
                if (ga && gb && gcc) { gv.Add(pos[a]); gv.Add(pos[b]); gv.Add(pos[c]); }
            }
            // bản sao xe
            var src = t.r is SkinnedMeshRenderer ? t.r.transform.parent.gameObject : t.r.gameObject;
            var clone = UnityEngine.Object.Instantiate(t.root.gameObject == p.gameObject ? src : src, holder);
            var at = basePos + fwd * (8f + k * spacing) + right * 5.5f;
            var anchor = new GameObject(t.key).transform; anchor.SetParent(holder, false); anchor.SetPositionAndRotation(at, Quaternion.LookRotation(-right));
            // đặt bản sao: cùng vị trí tương đối với gốc xe như bản gốc
            clone.transform.SetParent(anchor, false);
            clone.transform.localPosition = t.root.InverseTransformPoint(src.transform.position);
            clone.transform.localRotation = Quaternion.Inverse(t.root.rotation) * src.transform.rotation;
            clone.transform.localScale = src.transform.lossyScale;
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
            clone.SetActive(true);
            var hm = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            var nv = gv.Select(v => v + Vector3.zero).ToList(); hm.SetVertices(nv); hm.SetTriangles(Enumerable.Range(0, nv.Count).ToArray(), 0);
            var hg = new GameObject("kinh"); hg.transform.SetParent(anchor, false); hg.AddComponent<MeshFilter>().sharedMesh = hm; hg.AddComponent<MeshRenderer>().sharedMaterial = red;
            foreach (var l in e.lamps)
            {
                var sp = GameObject.CreatePrimitive(PrimitiveType.Sphere); UnityEngine.Object.Destroy(sp.GetComponent<Collider>());
                sp.transform.SetParent(anchor, false); sp.transform.localPosition = pos[l.v]; sp.transform.localScale = Vector3.one * l.r * 2f; sp.GetComponent<Renderer>().sharedMaterial = yellow;
            }
            sb.Append($"{t.key}@{k} tam giác kính {gv.Count / 3}; ");
            k++;
        }
        var cam = Camera.main;
        cam.transform.position = basePos + fwd * (8f + (k - 1) * spacing * 0.5f) - right * 6f + Vector3.up * 5.5f;
        cam.transform.LookAt(basePos + fwd * (8f + (k - 1) * spacing * 0.5f) + right * 5.5f);
        return sb.ToString();
    }

    public static string Cam(float along, float side, float up, float lookAlong)
    {
        var p = UnityEngine.Object.FindAnyObjectByType<OpenWorldDriver>(); var cam = Camera.main;
        var right = p.transform.right; var fwd = p.transform.forward; var b = p.transform.position;
        cam.transform.position = b + fwd * along + right * side + Vector3.up * up;
        cam.transform.LookAt(b + fwd * lookAlong + right * 5.5f + Vector3.up * 0.8f);
        return "ok";
    }
}
public static class CarGlass21324b
{
    // nhìn bản sao thứ k: từ phía trước-trái (az độ quanh xe), cách dist, cao up
    public static string Look(int k, float az, float dist, float up)
    {
        var anchors = new System.Collections.Generic.List<Transform>();
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            if (t.name == "KiemKinh21324") foreach (Transform c in t) if (c.Find("kinh")) anchors.Add(c);
        if (k >= anchors.Count) return "k quá " + anchors.Count;
        var a = anchors[k]; var cam = Camera.main;
        var dir = Quaternion.Euler(0, az, 0) * a.forward;
        var size = 2.2f; foreach (var r in a.GetComponentsInChildren<Renderer>()) size = Mathf.Max(size, r.bounds.extents.z, r.bounds.extents.x);
        cam.transform.position = a.position + dir * dist * size / 2.2f + Vector3.up * up;
        cam.transform.LookAt(a.position + Vector3.up * 0.8f);
        return a.name;
    }
}
