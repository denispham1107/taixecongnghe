using System.Linq;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.324 — liệt kê cấu trúc xe hơi (renderer, submesh/material, xương, đèn) cho xe người chơi, CSGT, 1 xe mỗi mẫu giao thông.
public static class CarParts21324
{
    public static string Main()
    {
        var sb = new System.Text.StringBuilder();
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); if (p) Dump(sb, p.transform);
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Include)) Dump(sb, pol.transform);
        var seen = new System.Collections.Generic.HashSet<string>();
        var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
        if (tr) foreach (var s in tr.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var root = s.transform; while (root.parent && root.parent != tr.transform && root.parent.parent != tr.transform) root = root.parent;
                var key = s.sharedMesh ? s.sharedMesh.name : "-"; if (!seen.Add(key)) continue;
                sb.AppendLine($"[traffic {root.name}] {s.name} mesh {key} v{(s.sharedMesh ? s.sharedMesh.vertexCount : 0)} sub{(s.sharedMesh ? s.sharedMesh.subMeshCount : 0)} read {(s.sharedMesh && s.sharedMesh.isReadable)} mats [{string.Join(",", s.sharedMaterials.Select(m => m ? m.name + ":" + m.shader.name : "-"))}] bones [{string.Join(",", s.bones.Select(b => b ? b.name : "-").Distinct())}]");
            }
        return sb.ToString();
    }
    static void Dump(System.Text.StringBuilder sb, Transform t)
    {
        sb.AppendLine("== " + t.name);
        foreach (var r in t.GetComponentsInChildren<Renderer>(true))
        {
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) continue;
            Mesh m = null; if (r is SkinnedMeshRenderer s) m = s.sharedMesh; else if (r.TryGetComponent<MeshFilter>(out var mf)) m = mf.sharedMesh;
            var bones = r is SkinnedMeshRenderer s2 ? string.Join(",", s2.bones.Select(b => b ? b.name : "-").Distinct()) : "";
            sb.AppendLine($"  {Path(r.transform, t)} {r.GetType().Name} active {r.gameObject.activeInHierarchy} mesh {(m ? m.name + " v" + m.vertexCount + " sub" + m.subMeshCount + " read " + m.isReadable : "-")} mats [{string.Join(",", r.sharedMaterials.Select(x => x ? x.name + ":" + x.shader.name : "-"))}] {(bones.Length > 0 ? "bones [" + bones + "]" : "")}");
        }
        foreach (var l in t.GetComponentsInChildren<Light>(true)) sb.AppendLine($"  LIGHT {Path(l.transform, t)} {l.type} int {l.intensity} col {l.color} local {l.transform.localPosition}");
        foreach (var lg in t.GetComponentsInChildren<LODGroup>(true)) sb.AppendLine($"  LODGroup {lg.name} lods {lg.lodCount}");
    }
    static string Path(Transform x, Transform root) { var s = x.name; while (x.parent && x != root) { x = x.parent; if (x == root) break; s = x.name + "/" + s; } return s; }
}
