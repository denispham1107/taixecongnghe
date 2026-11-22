using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.324 — thử móp/bể khi va chạm (Play, Editor tạm dừng, bước từng khung). CSGT/hiểm họa/thời tiết tắt.
// Hit(kmh, model): đặt 1 ô tô đứng yên 30 m trước mặt, cho xe người chơi lao tới ở kmh (giữ ga), bước tới khi chạm + 2 s.
public static class Dent21324
{
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();
    static readonly BindingFlags NP = BindingFlags.NonPublic | BindingFlags.Instance;

    public static string Setup()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); OpenWorldWeather.ForceNext = false; g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        var h = Object.FindAnyObjectByType<OpenWorldHazards>(); if (h) h.enabled = false;
        EditorApplication.isPaused = true;
        for (int i = 0; i < 5; i++) EditorApplication.Step();
        Application.targetFrameRate = 30;
        return "ok " + P.transform.position;
    }

    public static string Hit(float kmh, string model, float after)
    {
        var p = P; var rb = p.GetComponent<Rigidbody>(); var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
        var f = p.transform.forward; f.y = 0; f.Normalize();
        int idx = tr.TestPlace(p.transform.position + f * 30f, f, false, 25f, string.IsNullOrEmpty(model) ? null : model);
        var sp = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", NP);
        sp.SetValue(p, kmh / 3.6f); rb.linearVelocity = f * (kmh / 3.6f);
        p.testTouchUI = true; p.testPedal = 1;
        int b0 = p.Bumps, s0 = p.Stuns; float t0 = Time.time, hitAt = -1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, cm => cm.cullingMask); foreach (var cm in cams) cm.cullingMask = 0;   // đỡ tải GPU khi bước khung liên tục
        while (Time.time - t0 < 6f)
        {
            if (hitAt < 0) sp.SetValue(p, kmh / 3.6f);
            EditorApplication.Step();
            if (hitAt < 0 && (p.Bumps != b0 || p.Stuns != s0 || p.Crashed)) { hitAt = Time.time; p.testPedal = 0; }
            if (hitAt > 0 && Time.time - hitAt > after) break;
        }
        p.testPedal = 0; for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", NP).GetValue(tr); var vv = pool[idx];
        var dd = (OpenWorldCarDent)vv.GetType().GetField("dent").GetValue(vv);
        return $"npc {idx} hit {(hitAt > 0 ? (hitAt - t0).ToString("F2") : "none")} last={p.LastHit} crashed={p.Crashed} trafficHits={tr.Hits} npcDamaged={dd?.Damaged} wreck={vv.GetType().GetField("wreck").GetValue(vv)} " + DentInfo();
    }

    // độ lún lớn nhất của đỉnh so với lưới gốc (xe người chơi + mọi xe NPC đã móp)
    public static string DentInfo()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var s in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (!s.sharedMesh || !s.sharedMesh.name.Contains("(móp)")) continue;
            sb.Append($" [{s.transform.root.name}/{s.name}]");
        }
        return "dented:" + sb;
    }

    public static string CamBehind(float back, float up, float side)
    {
        var cam = Camera.main; var p = P.transform;
        cam.transform.position = p.position - p.forward * back + Vector3.up * up + p.right * side;
        cam.transform.LookAt(p.position + Vector3.up * 0.6f);
        return "cam " + cam.transform.position;
    }
}
public static class Dent21324b
{
    public static string Decals()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include))
        {
            if (mf.name != "VetHong") continue;
            var mr = mf.GetComponent<MeshRenderer>(); var m = mf.sharedMesh;
            sb.AppendLine($"{mf.transform.root.name}/{mf.transform.parent.name} active {mf.gameObject.activeInHierarchy} en {mr.enabled} v {(m ? m.vertexCount : -1)} bounds {mr.bounds.center} {mr.bounds.size} mat {(mr.sharedMaterial ? mr.sharedMaterial.name + "/" + mr.sharedMaterial.shader.name + " supported " + mr.sharedMaterial.shader.isSupported : "null")}");
        }
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); var d = p.Damage;
        sb.AppendLine(d == null ? "player no dent" : $"player damaged {d.Damaged} cracks {d.GlassCracks} lamps {d.LampsBroken} total {OpenWorldCarDent.TotalDents}");
        return sb.ToString();
    }
}
public static class Dent21324c
{
    public static string Body(bool on)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>();
        foreach (var r in p.GetComponentsInChildren<SkinnedMeshRenderer>()) r.enabled = on;
        var v = p.transform.Find("PV_BlueSupercar");
        var sb = new System.Text.StringBuilder("player " + p.transform.position + " ");
        foreach (var mf in p.GetComponentsInChildren<MeshFilter>(true)) if (mf.name == "VetHong") { var t = mf.transform; sb.Append($"decal parent {t.parent.name} lossy {t.parent.lossyScale} pos {t.parent.position} rot {t.parent.rotation.eulerAngles}"); }
        return sb.ToString();
    }
}
public static class Dent21324d
{
    public static string CamNpc(int idx, float az, float dist, float up)
    {
        var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
        var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
        var v = pool[idx]; var root = (Transform)v.GetType().GetField("root").GetValue(v);
        var cam = Camera.main; var dir = Quaternion.Euler(0, az, 0) * root.forward;
        cam.transform.position = root.position + dir * dist + Vector3.up * up; cam.transform.LookAt(root.position + Vector3.up * 0.7f);
        var dent = (OpenWorldCarDent)v.GetType().GetField("dent").GetValue(v);
        return root.name + " " + (dent == null ? "no dent" : $"damaged {dent.Damaged} cracks {dent.GlassCracks} lamps {dent.LampsBroken}");
    }
}
public static class Dent21324e
{
    // bước thêm `sec` giây rồi báo xe NPC idx: còn hỏng không, đang chạy không (tốc độ), xe người chơi còn hỏng/choáng
    public static string After(int idx, float sec)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); p.testTouchUI = true; p.testPedal = 1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, cm => cm.cullingMask); foreach (var cm in cams) cm.cullingMask = 0;
        float t0 = Time.time; while (Time.time - t0 < sec) EditorApplication.Step();
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
        var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
        var v = pool[idx]; var t = v.GetType();
        var dent = (OpenWorldCarDent)t.GetField("dent").GetValue(v);
        string npc = $"npc active {t.GetField("active").GetValue(v)} wreck {t.GetField("wreck").GetValue(v)} speed {(float)t.GetField("speed").GetValue(v):F1} damaged {dent?.Damaged}";
        p.testPedal = 0;
        return npc + $" | player stunned {p.Stunned} speed {p.Speed * 3.6f:F0} km/h damaged {p.Damage?.Damaged} cracks {p.Damage?.GlassCracks}";
    }
    // đo thời gian một cú móp trên xe người chơi (ms) ở vận tốc khép kmh, điểm chạm bên trái thân
    public static string Time1(float kmh)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>();
        var m = typeof(OpenWorldDriver).GetMethod("Dent", BindingFlags.NonPublic | BindingFlags.Instance);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        m.Invoke(p, new object[] { p.transform.position - p.transform.right * 1.0f + Vector3.up * 0.5f, kmh / 3.6f });
        sw.Stop();
        return $"dent {sw.Elapsed.TotalMilliseconds:F1} ms, cracks {p.Damage.GlassCracks} lamps {p.Damage.LampsBroken}";
    }
}
public static class Dent21324f
{
    public static string Direct(int idx, float kmh)
    {
        var tr = Object.FindAnyObjectByType<OpenWorldTraffic>();
        var pool = (IList)typeof(OpenWorldTraffic).GetField("pool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tr);
        var v = pool[idx]; var t = v.GetType(); var root = (Transform)t.GetField("root").GetValue(v);
        var m = typeof(OpenWorldTraffic).GetMethod("DentAt", BindingFlags.NonPublic | BindingFlags.Instance);
        var pt = root.position - root.forward * 2.0f + Vector3.up * 0.7f;
        m.Invoke(tr, new object[] { v, pt, kmh / 3.6f });
        var dent = (OpenWorldCarDent)t.GetField("dent").GetValue(v);
        var rends = (Renderer[])t.GetField("renderers").GetValue(v);
        return $"{((Transform)t.GetField("visual").GetValue(v)).name} damaged {dent?.Damaged} rends [{string.Join(",", System.Array.ConvertAll(rends, r => r.name + ":" + (r is SkinnedMeshRenderer s ? s.sharedMesh.name + " read " + s.sharedMesh.isReadable : "-")))}]";
    }
}
public static class Dent21324g
{
    // đặt xe CSGT đứng yên 25 m trước mặt, lao tới ở kmh; báo hỏng của CSGT
    public static string Police(float kmh, float after)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); var rb = p.GetComponent<Rigidbody>();
        var pol = Object.FindAnyObjectByType<OpenWorldPolice>(); var prb = pol.GetComponent<Rigidbody>();
        var f = p.transform.forward; f.y = 0; f.Normalize();
        var at = p.transform.position + f * 25f; at.y = p.transform.position.y;
        prb.position = at; prb.rotation = Quaternion.LookRotation(f); pol.transform.SetPositionAndRotation(at, Quaternion.LookRotation(f)); prb.linearVelocity = Vector3.zero;
        var sp = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        sp.SetValue(p, kmh / 3.6f); rb.linearVelocity = f * (kmh / 3.6f); p.testTouchUI = true; p.testPedal = 1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, cm => cm.cullingMask); foreach (var cm in cams) cm.cullingMask = 0;
        float t0 = Time.time, hitAt = -1;
        while (Time.time - t0 < 5f)
        {
            if (hitAt < 0) sp.SetValue(p, kmh / 3.6f);
            EditorApplication.Step();
            if (hitAt < 0 && pol.Damage != null && pol.Damage.Damaged) { hitAt = Time.time; p.testPedal = 0; }
            if (hitAt > 0 && Time.time - hitAt > after) break;
        }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        p.testPedal = 0;
        var cam = Camera.main; cam.transform.position = pol.transform.position - pol.transform.forward * 5f + pol.transform.right * 2.5f + Vector3.up * 2.2f; cam.transform.LookAt(pol.transform.position + Vector3.up * 0.6f);
        return $"police hit {(hitAt > 0 ? (hitAt - t0).ToString("F2") : "none")} damaged {pol.Damage?.Damaged} cracks {pol.Damage?.GlassCracks} lamps {pol.Damage?.LampsBroken} player wallDents {p.WallDents}";
    }
}
public static class Dent21324h
{
    public static string AfterReset()
    {
        var pol = Object.FindAnyObjectByType<OpenWorldPolice>(); var p = Object.FindAnyObjectByType<OpenWorldDriver>();
        int v = 0; foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include)) if (mf.name == "VetHong" && mf.transform.root == pol.transform) v++;
        return $"police damaged {pol.Damage?.Damaged} decals {v} | player damaged {p.Damage?.Damaged}";
    }
}
// 21.325 — xe người chơi đụng cảnh quan: tìm vật cản theo hướng `side` (-1 trái, 1 phải, 0 thẳng) ở độ cao `h`, đặt xe cách
// 14 m quay thẳng vào nó, lao tới ở kmh. Báo vật bị tông, số lần móp do vật cản, hỏng của xe.
public static class Dent21325
{
    public static string Obstacle(float side, float h, float kmh, float after)
    {
        var p = Object.FindAnyObjectByType<OpenWorldDriver>(); var rb = p.GetComponent<Rigidbody>();
        var dir = side == 0 ? p.transform.forward : p.transform.right * side; dir.y = 0; dir.Normalize();
        var o = p.transform.position + Vector3.up * h;
        if (!Physics.Raycast(o, dir, out var hit, 60f, OpenWorldFences.WorldMask, QueryTriggerInteraction.Ignore)) return "không thấy vật cản";
        var start = hit.point - dir * 14f; start.y = p.transform.position.y;
        var rot = Quaternion.LookRotation(dir);
        rb.position = start; rb.rotation = rot; p.transform.SetPositionAndRotation(start, rot);
        var sp = typeof(OpenWorldDriver).GetField("<Speed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        sp.SetValue(p, kmh / 3.6f); rb.linearVelocity = dir * (kmh / 3.6f); p.testTouchUI = true; p.testPedal = 1;
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude); var masks = System.Array.ConvertAll(cams, cm => cm.cullingMask); foreach (var cm in cams) cm.cullingMask = 0;
        int d0 = p.ObstacleDents; float t0 = Time.time, hitAt = -1;
        while (Time.time - t0 < 4f)
        {
            if (hitAt < 0) sp.SetValue(p, kmh / 3.6f);
            EditorApplication.Step();
            if (hitAt < 0 && p.ObstacleDents != d0) { hitAt = Time.time; p.testPedal = 0; }
            if (hitAt > 0 && Time.time - hitAt > after) break;
        }
        for (int i = 0; i < cams.Length; i++) if (cams[i]) cams[i].cullingMask = masks[i];
        p.testPedal = 0;
        var cam = Camera.main; var f = p.transform.forward; f.y = 0; f.Normalize();
        cam.transform.position = p.transform.position + f * 3.8f - p.transform.right * 2.2f + Vector3.up * 2.0f; cam.transform.LookAt(p.transform.position + f * 1.2f + Vector3.up * 0.5f);
        return $"vật '{hit.collider.name}' cao {hit.collider.bounds.size.y:F1} m | móp {(hitAt > 0 ? (hitAt - t0).ToString("F2") + " s" : "KHÔNG")} lần {p.ObstacleDents - d0} | damaged {p.Damage?.Damaged} cracks {p.Damage?.GlassCracks} lamps {p.Damage?.LampsBroken} | tốc độ còn {p.Speed * 3.6f:F0} km/h crashed {p.Crashed}";
    }
}
