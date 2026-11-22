using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.303 — kiểm thử bờ biển: Main(x, z, yaw) bắt đầu lượt và đặt xe tại (x, z); Run(giây) chạy từng khung khi tạm dừng rồi báo;
// Leave(x, z) đặt xe vào phố (rời bờ biển).
public static class Coast21303
{
    static OpenWorldGame G => Object.FindAnyObjectByType<OpenWorldGame>();
    static OpenWorldHazards H => Object.FindAnyObjectByType<OpenWorldHazards>();
    static OpenWorldDriver P => Object.FindAnyObjectByType<OpenWorldDriver>();

    static void Put(float x, float z, float yaw)
    {
        var p = P; var rb = p.GetComponent<Rigidbody>();
        var pos = new Vector3(x, 0.4f, z); var rot = Quaternion.Euler(0, yaw, 0);
        rb.position = pos; rb.rotation = rot; p.transform.SetPositionAndRotation(pos, rot);
        rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
    }
    public static string Main(float x, float z, float yaw)
    {
        var g = G; if (g.Mode != OpenWorldGame.State.Riding) g.StartRun();
        Put(x, z, yaw);
        return "mode=" + g.Mode + " coast(pt)=" + OpenWorldHazards.IsCoast(new Vector3(x, 0, z));
    }
    public static string Leave(float x, float z, float yaw) { Put(x, z, yaw); return "ok"; }
    public static string Run(float seconds)
    {
        EditorApplication.isPaused = true;
        float t0 = Time.time; int n = 0;
        while (Time.time - t0 < seconds && n < 4000) { EditorApplication.Step(); n++; }
        var h = H;
        return $"t={Time.time - t0:F2}s frames={n} crashed={P.Crashed} | {h.Report()} | shells={h.BoatShells} {G.Report()}";
    }
    // vị trí máy bay + tàu biển (để đặt camera chụp)
    public static string Where()
    {
        var sb = new System.Text.StringBuilder("xe " + P.transform.position.ToString("F0") + " | ");
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.name == "MayBayCanhKep" || (t.name == "TauBien" && t.gameObject.activeInHierarchy)) sb.AppendLine(t.name + " " + t.position.ToString("F0") + " yaw " + t.eulerAngles.y.ToString("F0"));
        return sb.ToString();
    }
    // tắt bắn để chụp ảnh không bị hạ xe
    public static string Calm() { var h = H; h.maxShellsInFlight = 0; h.maxEnemyMissilesInFlight = 0; return "ok"; }
}
