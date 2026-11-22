using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet.OpenWorld
{
    // Hư hỏng thân ô tô theo điểm va chạm (bản đồ mở). Dùng cho xe người chơi (Supercar), 4 mẫu ô tô Meshy giao thông
    // và xe CSGT. Xe giữ nguyên vết hỏng khi hết choáng và chạy tiếp; chỉ trả về nguyên vẹn khi chơi lại / xe sinh lại.
    //
    // 21.259: lõm hình elip theo điểm chạm, sâu 0,025–0,31 m (lõi từ PlayerCarDamage 21.192).
    // 21.324 (người dùng: «biến dạng, bể, móp méo ở chỗ va chạm, tốc độ càng nhanh càng nhiều, giữ nguyên khi chạy tiếp»):
    // • Mức hỏng s = (vận tốc khép 2,5 → 25 m/s, tức 9 → 90 km/h)^0,8. Lõm sâu 0,05 → 0,5 m, rộng 0,45 → 1,15 m, cao
    //   0,32 → 0,7 m, ăn sâu vào thân 0,45 → 0,9 m; méo lởm chởm (nhiễu 3D theo vị trí đỉnh) + nếp nhăn ngang/dọc; cộng dồn
    //   qua nhiều cú, mỗi đỉnh lệch tối đa 0,75 m. Pháp tuyến tính lại ở vùng móp (giữ nguyên chỗ không móp).
    // • Vết dán (shader PhoViet/OpenWorld/CarDamage, atlas Resources/XeHong/PV_XeHong_Atlas): xước tróc sơn (lớp lót +
    //   kim loại trần + vết cào) trên đúng các tam giác thân quanh chỗ va; kính nứt mạng nhện từ ~25 km/h trên tấm kính gần
    //   nhất (đỉnh kính tìm sẵn theo ảnh texture, Resources/XeHong/PV_XeHong_Data.json — AgentScripts/CarGlass21324.cs);
    //   đèn pha/hậu vỡ (lấy cụm đèn phát sáng trong JSON + tấm đèn phát sáng của xe giao thông/CSGT — tấm đó tắt).
    // • Sự kiện Broke → OpenWorldHazards bắn mảnh vỡ cùng màu sơn + mảnh kính + mảnh đèn.
    // • Tải: mỗi xe một bản sao lưới động tạo lần đầu bị móp, các cú sau cập nhật tại chỗ; cú liên tiếp < 0,3 s bị bỏ trừ khi
    //   mạnh hơn hẳn (xe trượt cọ tường không móp lại mỗi khung). Dữ liệu gốc (đỉnh, pháp tuyến, xương, tam giác) dùng chung
    //   theo từng lưới gốc; LOD2 không móp (xa, rất nhỏ trên màn hình).
    public sealed class OpenWorldCarDent
    {
        // ------------------------------------------------------------ dữ liệu chung
        [Serializable] class LampJ { public int v; public float r; public bool front; }
        [Serializable] class CarJ { public string mesh; public string glass; public int glassCount; public List<LampJ> lamps; }
        [Serializable] class DataJ { public List<CarJ> cars; }
        static Dictionary<string, CarJ> data; static Material decalMat; static bool loaded;
        sealed class Shared { public Vector3[] rest, normals; public int[] bone; public int[] tris; public bool[] glass; public bool glassFiltered; public List<LampJ> lamps; }
        static readonly Dictionary<Mesh, Shared> shared = new Dictionary<Mesh, Shared>();

        // điểm (thế giới), hướng ra ngoài chỗ va, mức hỏng 0..1, màu sơn, vừa nứt kính, vừa vỡ đèn
        public static event Action<Vector3, Vector3, float, Color, bool, bool> Broke;
        public static int TotalDents { get; private set; }

        static void Load()
        {
            if (loaded) return; loaded = true;
            data = new Dictionary<string, CarJ>();
            var ta = Resources.Load<TextAsset>("XeHong/PV_XeHong_Data");
            if (ta) { var d = JsonUtility.FromJson<DataJ>(ta.text); if (d?.cars != null) foreach (var c in d.cars) data[c.mesh] = c; }
            decalMat = Resources.Load<Material>("XeHong/PV_XeHong");
        }

        static Shared Share(Mesh m, bool withTris)
        {
            if (!shared.TryGetValue(m, out var s))
            {
                s = new Shared { rest = m.vertices, normals = m.normals };
                var bw = m.boneWeights; s.bone = new int[s.rest.Length];
                if (bw.Length == s.rest.Length) for (int i = 0; i < bw.Length; i++) s.bone[i] = bw[i].boneIndex0;
                if (data.TryGetValue(m.name, out var cj) && !string.IsNullOrEmpty(cj.glass))
                {
                    var bits = Convert.FromBase64String(cj.glass);
                    if (bits.Length * 8 >= s.rest.Length && bits.Length * 8 < s.rest.Length + 8)
                    {
                        s.glass = new bool[s.rest.Length];
                        for (int i = 0; i < s.rest.Length; i++) s.glass[i] = (bits[i >> 3] & (1 << (i & 7))) != 0;
                        s.lamps = cj.lamps;
                    }
                }
                shared[m] = s;
            }
            if (withTris && s.tris == null) s.tris = m.triangles;
            return s;
        }

        // ------------------------------------------------------------ một xe
        public Color paint = new Color(.62f, .63f, .66f);
        public bool Damaged { get; private set; }
        public int GlassCracks => cracks.Count;
        public int LampsBroken { get; private set; }
        readonly Transform car;
        readonly List<Patch> patches = new List<Patch>();
        Patch main;
        struct Impact { public Vector3 p, o; public float s; public int seed; }
        struct Crack { public Vector3 c, n; public float r; }
        sealed class Lamp { public Vector3 p; public float r; public bool front, broken; public Renderer quad; public int v = -1; }
        readonly List<Impact> impacts = new List<Impact>();
        readonly List<Crack> cracks = new List<Crack>();
        readonly List<Lamp> lamps = new List<Lamp>();
        GameObject decalGo; Mesh decalMesh; float lastDentAt = -10f, lastS;

        public OpenWorldCarDent(Transform carRoot, IEnumerable<Renderer> renderers, IEnumerable<Renderer> lampQuads = null)
        {
            Load();
            car = carRoot;
            foreach (var r in renderers)
            {
                if (!r || r.name.EndsWith("_LOD2", StringComparison.Ordinal)) continue;
                Mesh m = null;
                if (r is SkinnedMeshRenderer s) m = s.sharedMesh;
                else if (r is MeshRenderer && r.TryGetComponent<MeshFilter>(out var mf)) m = mf.sharedMesh;
                if (!m || !m.isReadable || m.vertexCount < 2000) continue;
                var p = new Patch(r, m); patches.Add(p);
                if (main == null || (!p.lod1 && (main.lod1 || m.vertexCount > main.source.vertexCount))) main = p;
            }
            if (main != null) main.sh = Share(main.source, true);
            foreach (var p in patches) if (p.sh == null) p.sh = Share(p.source, false);
            // Đèn: xe có tấm đèn phát sáng riêng (giao thông, CSGT) → dùng đúng vị trí tấm (tấm tắt khi vỡ + vết đèn vỡ ở đó);
            // xe người chơi (đèn nằm trong ảnh phát sáng) → cụm đèn dò sẵn trong JSON. Cụm JSON của xe tải không chuẩn (đèn
            // phụ trên thùng) nên không dùng cho xe có tấm đèn.
            bool quads = false;
            if (lampQuads != null) foreach (var q in lampQuads) if (q) { quads = true; lamps.Add(new Lamp { quad = q, r = Mathf.Max(q.transform.lossyScale.x, q.transform.lossyScale.y) * 0.6f + 0.06f }); }
            if (!quads && main?.sh.lamps != null) foreach (var l in main.sh.lamps) if (l.v >= 0 && l.v < main.sh.rest.Length) lamps.Add(new Lamp { v = l.v, r = Mathf.Max(0.1f, l.r), front = l.front });
            main?.FilterGlass();
        }

        // point/outward theo toạ độ thế giới; outward = hướng từ trong thân xe ra phía bị va.
        public void Dent(Vector3 worldPoint, Vector3 worldOutward, float closing)
        {
            if (closing < .35f || !car || main == null) return;
            var m = car.worldToLocalMatrix;
            var p = m.MultiplyPoint3x4(worldPoint);
            var o = m.MultiplyVector(worldOutward); o.y = 0; if (o.sqrMagnitude < 1e-4f) return; o.Normalize();
            float s = Mathf.Pow(Mathf.InverseLerp(OpenWorldTraffic.MinClosing, OpenWorldTraffic.FatalClosing, closing), 0.8f);
            s = Mathf.Max(s, 0.04f);
            // chống dồn: xe trượt cọ tường/xe khác gọi liên tục mỗi khung → chỉ nhận cú mới sau 0,3 s, trừ khi mạnh hơn hẳn
            float now = Time.time;
            if (now - lastDentAt < 0.3f && s < lastS * 1.25f + 0.05f) return;
            lastDentAt = now; lastS = s;
            // điểm chạm có thể nằm ngoài thân (vd. xe con thấp tông đuôi xe tải cao: điểm ở ngang gầm) → dời tới đỉnh thân gần
            // nhất để vết móp nằm đúng chỗ hai xe chạm nhau
            var before = main.CarPositions(car); int nearest = -1; float nd = float.MaxValue;
            for (int i = 0; i < before.Length; i++) if (main.IsBody(i)) { float d = (before[i] - p).sqrMagnitude; if (d < nd) { nd = d; nearest = i; } }
            if (nearest >= 0 && nd > 0.25f * 0.25f) p = before[nearest];
            int seed = impacts.Count * 7 + TotalDents * 3 + 1;
            bool changed = false;
            foreach (var patch in patches) if (patch.Dent(car, p, o, s, seed)) changed = true;
            if (!changed) return;
            Damaged = true; TotalDents++;
            // gộp cú gần nhau (cùng chỗ) để vết dán không chồng quá dày
            int near = impacts.FindIndex(x => (x.p - p).sqrMagnitude < 0.35f * 0.35f);
            if (near >= 0) { var x = impacts[near]; x.s = Mathf.Min(1f, Mathf.Max(x.s, s) + s * 0.25f); impacts[near] = x; }
            else { impacts.Add(new Impact { p = p, o = o, s = s, seed = seed }); if (impacts.Count > 10) impacts.RemoveAt(0); }
            bool glassNew = false, lampNew = false;
            var cpos = main.CarPositions(car);
            if (s >= 0.22f && main.sh.glass != null) glassNew = AddCrack(cpos, p, o, s);
            float reachW = Mathf.Lerp(.45f, 1.15f, s) * 0.9f;
            foreach (var l in lamps)
            {
                if (l.broken || s < 0.1f) continue;
                var lp = LampPos(l, cpos);
                if ((lp - p).magnitude > reachW + l.r) continue;
                l.broken = true; LampsBroken++; lampNew = true;
                if (l.quad) l.quad.enabled = false;
            }
            RebuildDecals(cpos);
            Broke?.Invoke(worldPoint, car.TransformDirection(o), s, paint, glassNew, lampNew);
        }

        Vector3 LampPos(Lamp l, Vector3[] cpos)
        {
            if (l.v >= 0) return cpos[l.v];
            if (!l.quad) return new Vector3(0, -99, 0);
            var lp = car.InverseTransformPoint(l.quad.transform.position);
            l.front = lp.z > 0; return lp;
        }

        bool AddCrack(Vector3[] cpos, Vector3 p, Vector3 o, float s)
        {
            var g = main.sh.glass; int best = -1; float bd = 1.7f * 1.7f;
            for (int i = 0; i < cpos.Length; i++) if (g[i]) { float d = (cpos[i] - p).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            if (best < 0) return false;
            var c = cpos[best]; float r = Mathf.Lerp(.3f, .8f, s);
            for (int k = 0; k < cracks.Count; k++)
                if ((cracks[k].c - c).sqrMagnitude < 0.3f * 0.3f) { var x = cracks[k]; x.r = Mathf.Max(x.r, r); cracks[k] = x; return false; }
            // pháp tuyến tấm kính quanh tâm
            var n = Vector3.zero; var t = main.sh.tris;
            for (int i = 0; i < t.Length; i += 3)
            {
                int a = t[i], b = t[i + 1], cc = t[i + 2]; if (!g[a] || !g[b] || !g[cc]) continue;
                var ctr = (cpos[a] + cpos[b] + cpos[cc]) / 3f; if ((ctr - c).sqrMagnitude > 0.35f * 0.35f) continue;
                var cr = Vector3.Cross(cpos[b] - cpos[a], cpos[cc] - cpos[a]);
                n += Vector3.Dot(cr, main.CarNormal(a)) < 0f ? -cr : cr;
            }
            n = n.sqrMagnitude > 1e-8f ? n.normalized : o;
            cracks.Add(new Crack { c = c, n = n, r = r });
            return true;
        }

        // ------------------------------------------------------------ lưới dán vết hỏng
        readonly List<Vector3> dv = new List<Vector3>(); readonly List<Vector3> dn = new List<Vector3>();
        readonly List<Vector2> duv = new List<Vector2>(); readonly List<Color32> dc = new List<Color32>();

        void RebuildDecals(Vector3[] cpos)
        {
            if (!decalMat) return;
            var t = main.sh.tris; if (t == null) return;
            int tc = t.Length / 3;
            var cen = new Vector3[tc]; var nor = new Vector3[tc]; var ok = new bool[tc];
            for (int k = 0; k < tc; k++)
            {
                int a = t[k * 3], b = t[k * 3 + 1], c = t[k * 3 + 2];
                if (!main.IsBody(a) || !main.IsBody(b) || !main.IsBody(c)) continue;
                var cr = Vector3.Cross(cpos[b] - cpos[a], cpos[c] - cpos[a]); if (cr.sqrMagnitude < 1e-12f) continue;
                if (Vector3.Dot(cr, main.CarNormal(a) + main.CarNormal(b) + main.CarNormal(c)) < 0f) cr = -cr;   // lưới FBX có thể lật chiều
                cen[k] = (cpos[a] + cpos[b] + cpos[c]) / 3f; nor[k] = cr.normalized; ok[k] = true;
            }
            dv.Clear(); dn.Clear(); duv.Clear(); dc.Clear();
            var parent = main.DecalParent; var toParent = parent.worldToLocalMatrix * car.localToWorldMatrix;
            var glass = main.sh.glass;
            // 1) xước tróc sơn
            foreach (var im in impacts)
            {
                float W = Mathf.Lerp(.45f, 1.15f, im.s) * 1.15f, Hh = Mathf.Lerp(.32f, .7f, im.s) * 1.2f, R = Mathf.Lerp(.45f, .9f, im.s) * 1.1f;
                var across = Vector3.Cross(Vector3.up, im.o).normalized;
                float ang = (Hash(im.seed, 3, 7) - .5f) * 0.45f, ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
                var q = (im.seed & 1) == 0 ? new Vector2(0f, 0f) : new Vector2(.5f, 0f);
                byte alpha = (byte)(Mathf.Lerp(.7f, 1f, im.s) * 255);
                for (int k = 0; k < tc; k++)
                {
                    if (!ok[k] || Vector3.Dot(nor[k], im.o) < 0.12f) continue;
                    if (glass != null && glass[t[k * 3]] && glass[t[k * 3 + 1]] && glass[t[k * 3 + 2]]) continue;
                    var d = cen[k] - im.p;
                    float x = Vector3.Dot(d, across) / W, y = d.y / Hh, z = Vector3.Dot(d, im.o) / R;
                    if (x * x + y * y + z * z >= 1f) continue;
                    for (int j = 0; j < 3; j++)
                    {
                        var vp = cpos[t[k * 3 + j]]; var vd = vp - im.p;
                        float lx = Vector3.Dot(vd, across), ly = vd.y;
                        float rx = lx * ca - ly * sa, ry = lx * sa + ly * ca;
                        var uv = new Vector2(Mathf.Clamp01(.5f + rx / (2f * W)), Mathf.Clamp01(.5f + ry / (2f * Hh)));
                        Add(toParent, vp, nor[k], q + uv * .5f, new Color32(255, 255, 255, alpha));
                    }
                }
            }
            // 2) kính nứt
            if (glass != null)
                foreach (var cr in cracks)
                {
                    var t1 = Vector3.Cross(Vector3.up, cr.n); if (t1.sqrMagnitude < 1e-3f) t1 = Vector3.Cross(Vector3.forward, cr.n); t1.Normalize();
                    var t2 = Vector3.Cross(cr.n, t1);
                    for (int k = 0; k < tc; k++)
                    {
                        if (!ok[k] || !glass[t[k * 3]] || !glass[t[k * 3 + 1]] || !glass[t[k * 3 + 2]]) continue;
                        if ((cen[k] - cr.c).sqrMagnitude > cr.r * cr.r * 1.35f || Vector3.Dot(nor[k], cr.n) < 0.3f) continue;
                        for (int j = 0; j < 3; j++)
                        {
                            var vp = cpos[t[k * 3 + j]]; var vd = vp - cr.c;
                            var uv = new Vector2(Mathf.Clamp01(.5f + Vector3.Dot(vd, t1) / (2f * cr.r)), Mathf.Clamp01(.5f + Vector3.Dot(vd, t2) / (2f * cr.r)));
                            Add(toParent, vp, nor[k], new Vector2(0f, .5f) + uv * .5f, new Color32(255, 255, 255, 235));
                        }
                    }
                }
            // 3) đèn vỡ
            foreach (var l in lamps)
            {
                if (!l.broken) continue;
                var lp = LampPos(l, cpos); var dir = l.front ? Vector3.forward : Vector3.back;
                var t1 = Vector3.Cross(Vector3.up, dir).normalized; float R = l.r * 1.3f;
                for (int k = 0; k < tc; k++)
                {
                    if (!ok[k] || Vector3.Dot(nor[k], dir) < 0.2f || (cen[k] - lp).sqrMagnitude > R * R) continue;
                    for (int j = 0; j < 3; j++)
                    {
                        var vp = cpos[t[k * 3 + j]]; var vd = vp - lp;
                        var uv = new Vector2(Mathf.Clamp01(.5f + Vector3.Dot(vd, t1) / (2f * R)), Mathf.Clamp01(.5f + vd.y / (1.4f * R)));
                        Add(toParent, vp, nor[k], new Vector2(.5f, .5f) + uv * .5f, new Color32(255, 255, 255, 250));
                    }
                }
            }
            if (!decalGo)
            {
                decalGo = new GameObject("VetHong"); decalGo.layer = main.r.gameObject.layer;
                decalGo.transform.SetParent(parent, false);
                decalMesh = new Mesh { name = "VetHong", indexFormat = IndexFormat.UInt32 }; decalMesh.MarkDynamic();
                decalGo.AddComponent<MeshFilter>().sharedMesh = decalMesh;
                var mr = decalGo.AddComponent<MeshRenderer>(); mr.sharedMaterial = decalMat;
                mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                mr.lightProbeUsage = main.r.lightProbeUsage; mr.reflectionProbeUsage = main.r.reflectionProbeUsage; mr.probeAnchor = main.r.probeAnchor;
            }
            decalMesh.Clear();
            decalMesh.SetVertices(dv); decalMesh.SetNormals(dn); decalMesh.SetUVs(0, duv); decalMesh.SetColors(dc);
            var idx = new int[dv.Count]; for (int i = 0; i < idx.Length; i++) idx[i] = i;
            decalMesh.SetTriangles(idx, 0, true);
        }

        void Add(Matrix4x4 toParent, Vector3 carPos, Vector3 carNormal, Vector2 uv, Color32 c)
        {
            dv.Add(toParent.MultiplyPoint3x4(carPos + carNormal * 0.004f)); dn.Add(toParent.MultiplyVector(carNormal).normalized); duv.Add(uv); dc.Add(c);
        }

        public void Restore()
        {
            foreach (var patch in patches) patch.Restore();
            foreach (var l in lamps) { if (l.broken && l.quad) l.quad.enabled = true; l.broken = false; }
            impacts.Clear(); cracks.Clear(); LampsBroken = 0;
            if (decalGo) UnityEngine.Object.Destroy(decalGo);
            if (decalMesh) UnityEngine.Object.Destroy(decalMesh);
            decalGo = null; decalMesh = null;
            Damaged = false;
        }

        // ------------------------------------------------------------ nhiễu (méo lởm chởm)
        static float Hash(int x, int y, int z)
        {
            unchecked { int h = x * 374761393 + y * 668265263 + z * 1274126177; h = (h ^ (h >> 13)) * 1274126177; return (h & 0xffff) / 65535f; }
        }
        static float Noise(Vector3 p)
        {
            int x = Mathf.FloorToInt(p.x), y = Mathf.FloorToInt(p.y), z = Mathf.FloorToInt(p.z);
            float fx = p.x - x, fy = p.y - y, fz = p.z - z;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy); fz = fz * fz * (3 - 2 * fz);
            float a = Mathf.Lerp(Hash(x, y, z), Hash(x + 1, y, z), fx), b = Mathf.Lerp(Hash(x, y + 1, z), Hash(x + 1, y + 1, z), fx);
            float c = Mathf.Lerp(Hash(x, y, z + 1), Hash(x + 1, y, z + 1), fx), d = Mathf.Lerp(Hash(x, y + 1, z + 1), Hash(x + 1, y + 1, z + 1), fx);
            return Mathf.Lerp(Mathf.Lerp(a, b, fy), Mathf.Lerp(c, d, fy), fz);
        }

        // ------------------------------------------------------------ một lưới (LOD0/LOD1 hoặc lưới thường của CSGT)
        sealed class Patch
        {
            public readonly Renderer r; public readonly Mesh source; public readonly bool lod1; public Shared sh;
            readonly SkinnedMeshRenderer skin; readonly MeshFilter mf;
            Mesh instance; Vector3[] vertices; bool[] moved;
            Transform[] bones; bool[] bodyBone; Matrix4x4[] toCar, toMesh; int bodyIdx = -1;
            static readonly string[] BodyNames = { "Body", "Frame", "BumperFront", "BumperRear", "Cargo" };

            public Patch(Renderer rr, Mesh m)
            {
                r = rr; source = m; lod1 = rr.name.EndsWith("_LOD1", StringComparison.Ordinal);
                skin = rr as SkinnedMeshRenderer; if (!skin) mf = rr.GetComponent<MeshFilter>();
                if (skin)
                {
                    bones = skin.bones; bodyBone = new bool[bones.Length];
                    for (int i = 0; i < bones.Length; i++) { bodyBone[i] = bones[i] && Array.IndexOf(BodyNames, bones[i].name) >= 0; if (bones[i] && bones[i].name == "Body") bodyIdx = i; }
                }
                else bodyBone = new[] { true };
                toCar = new Matrix4x4[bodyBone.Length]; toMesh = new Matrix4x4[bodyBone.Length];
            }

            public Transform DecalParent => skin ? (bodyIdx >= 0 && bones[bodyIdx] ? bones[bodyIdx] : r.transform) : r.transform;
            public bool IsBody(int i) => bodyBone[sh.bone[i]];
            // kính chỉ nằm trên xương Body/Frame (bỏ thùng hàng/bạt tối màu gắn xương Cargo, cản trước/sau)
            public void FilterGlass()
            {
                if (sh.glass == null || sh.glassFiltered || !skin) { if (sh != null) sh.glassFiltered = true; return; }
                for (int i = 0; i < sh.glass.Length; i++)
                    if (sh.glass[i]) { var b = bones[sh.bone[i]]; if (!b || (b.name != "Body" && b.name != "Frame")) sh.glass[i] = false; }
                sh.glassFiltered = true;
            }
            // pháp tuyến gốc của đỉnh trong toạ độ xe (chỉ để định chiều mặt; dùng ma trận của lần CarPositions gần nhất)
            public Vector3 CarNormal(int i) => sh.normals != null && i < sh.normals.Length ? toCar[sh.bone[i]].MultiplyVector(sh.normals[i]) : Vector3.up;

            void Matrices(Transform car)
            {
                var carFromWorld = car.worldToLocalMatrix;
                if (skin)
                {
                    var bp = source.bindposes;
                    for (int i = 0; i < bones.Length && i < bp.Length; i++)
                    {
                        if (!bodyBone[i] || !bones[i]) continue;
                        toCar[i] = carFromWorld * bones[i].localToWorldMatrix * bp[i]; toMesh[i] = toCar[i].inverse;
                    }
                }
                else { toCar[0] = carFromWorld * r.transform.localToWorldMatrix; toMesh[0] = toCar[0].inverse; }
            }

            // vị trí hiện tại (đã móp) của mọi đỉnh, toạ độ gốc xe
            public Vector3[] CarPositions(Transform car)
            {
                Matrices(car);
                var v = vertices ?? sh.rest; var res = new Vector3[v.Length];
                for (int i = 0; i < v.Length; i++) { int b = sh.bone[i]; res[i] = bodyBone[b] ? toCar[b].MultiplyPoint3x4(v[i]) : Vector3.zero; }
                return res;
            }

            public bool Dent(Transform car, Vector3 p, Vector3 o, float s, int seed)
            {
                if (!source || !r) return false;
                if (skin && (source.bindposes.Length != bones.Length)) return false;
                Matrices(car);
                if (vertices == null) { vertices = (Vector3[])sh.rest.Clone(); moved = new bool[vertices.Length]; }
                float depth = Mathf.Lerp(.05f, .5f, s), width = Mathf.Lerp(.45f, 1.15f, s), height = Mathf.Lerp(.32f, .7f, s), reach = Mathf.Lerp(.45f, .9f, s);
                var across = Vector3.Cross(Vector3.up, o).normalized;
                var off1 = new Vector3(seed * 1.7f, seed * 0.3f, seed * 2.9f); var off2 = new Vector3(seed * 0.9f, seed * 4.1f, seed * 1.3f);
                bool changed = false;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int b = sh.bone[i]; if (!bodyBone[b]) continue;
                    var c = toCar[b].MultiplyPoint3x4(sh.rest[i]); var d = c - p;
                    float x = Vector3.Dot(d, across) / width, y = d.y / height, z = Vector3.Dot(d, o) / reach;
                    float f = 1f - x * x - y * y - z * z; if (f <= 0f) continue;
                    float n1 = Noise(c * 5.1f + off1), n2 = Noise(c * 12.7f + off2);
                    float push = depth * f * f * (0.6f + 0.8f * n1);
                    float wr = depth * 0.25f * f * (n2 * 2f - 1f);
                    var disp = -o * push + across * (wr * 0.5f) + Vector3.up * (wr * 0.7f);
                    var prev = toCar[b].MultiplyVector(vertices[i] - sh.rest[i]);
                    var next = Vector3.ClampMagnitude(prev + disp, .75f);
                    if ((next - prev).sqrMagnitude < 1e-10f) continue;
                    vertices[i] = sh.rest[i] + toMesh[b].MultiplyVector(next); moved[i] = true; changed = true;
                }
                if (changed) Apply();
                return changed;
            }

            // Lưới móp tạo MỘT lần mỗi xe (bản sao động) rồi cập nhật tại chỗ — không tạo/xoá lưới mỗi cú (đỡ tải GPU/bộ nhớ).
            void Apply()
            {
                if (!instance)
                {
                    instance = UnityEngine.Object.Instantiate(source); instance.name = source.name + " (móp)"; instance.MarkDynamic();
                    if (skin) skin.sharedMesh = instance; else if (mf) mf.sharedMesh = instance;
                }
                instance.SetVertices(vertices);
                instance.RecalculateNormals();
                var nn = instance.normals; var on = sh.normals;
                if (on != null && on.Length == nn.Length) for (int i = 0; i < nn.Length; i++) if (!moved[i]) nn[i] = on[i];
                instance.normals = nn;
                instance.bounds = source.bounds;
            }

            public void Restore()
            {
                if (instance)
                {
                    if (skin) skin.sharedMesh = source; else if (mf) mf.sharedMesh = source;
                    UnityEngine.Object.Destroy(instance); instance = null;
                }
                vertices = null; moved = null;
            }
        }
    }
}
