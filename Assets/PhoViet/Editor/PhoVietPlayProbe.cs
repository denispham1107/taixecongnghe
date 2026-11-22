using System.IO;
using PhoViet.OpenWorld;
using UnityEditor;
using UnityEngine;

namespace PhoViet.Editor
{
    // 21.316c — bộ ghi số liệu khi NGƯỜI DÙNG tự bấm Play trong Editor (chỉ Editor, không vào bản build; chỉ chạy khi có tệp cờ
    // Captures/steer21316/probe.on). Mỗi khung hình camera chính vẽ xong ghi 1 dòng CSV vào Captures/steer21316/play-<giờ>.csv:
    // thời gian thực, deltaTime, số bước vật lý trong khung, vị trí/hướng xe, hướng/vị trí camera, góc lái ảo, tốc độ, nguồn lái
    // (phím Horizontal, nút cảm ứng Trái/Phải, số ngón chạm). Dùng để đo đúng cảm giác lái/khựng hình trên máy người dùng.
    [InitializeOnLoad]
    static class PhoVietPlayProbe
    {
        const string Flag = "Captures/steer21316/probe.on";
        static StreamWriter w; static OpenWorldDriver drv; static float lastReal;

        static PhoVietPlayProbe()
        {
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.EnteredPlayMode) Begin();
                else if (s == PlayModeStateChange.ExitingPlayMode) End();
            };
            if (EditorApplication.isPlaying) Begin();
        }

        static void Begin()
        {
            if (w != null || !File.Exists(Flag)) return;
            Directory.CreateDirectory("Captures/steer21316");
            w = new StreamWriter("Captures/steer21316/play-" + System.DateTime.Now.ToString("HHmmss") + ".csv");
            w.WriteLine("real,dt,fixed,carX,carZ,carYaw,camYaw,camX,camZ,steer,speed,axisH,left,right,touches,mouse,moveYaw,roll,fps");
            Camera.onPostRender += Post;
            lastReal = Time.realtimeSinceStartup;
        }


        static void Post(Camera cam)
        {
            if (w == null || cam != Camera.main) return;
            if (!drv) drv = Object.FindAnyObjectByType<OpenWorldDriver>();
            if (!drv) return;
            float real = Time.realtimeSinceStartup;
            var p = drv.transform.position; var c = cam.transform.position;
            float moveYaw = drv.MoveDir.sqrMagnitude > 0.5f ? Mathf.Atan2(drv.MoveDir.x, drv.MoveDir.z) * Mathf.Rad2Deg : 0f;
            w.WriteLine(string.Join(",", new[]
            {
                real.ToString("F4"), Time.deltaTime.ToString("F4"), (Time.fixedTime / Time.fixedDeltaTime).ToString("F0"),
                p.x.ToString("F3"), p.z.ToString("F3"), drv.transform.eulerAngles.y.ToString("F3"), cam.transform.eulerAngles.y.ToString("F3"),
                c.x.ToString("F3"), c.z.ToString("F3"), drv.SteerValue.ToString("F3"), drv.Speed.ToString("F2"),
                Input.GetAxisRaw("Horizontal").ToString("F0"), drv.LeftHeld ? "1" : "0", drv.RightHeld ? "1" : "0",
                Input.touchCount.ToString(), Input.GetMouseButton(0) ? "1" : "0", moveYaw.ToString("F3"), drv.BodyRoll.ToString("F2"),
                (1f / Mathf.Max(1e-4f, real - lastReal)).ToString("F0")
            }));
            lastReal = real;
        }

        static void End()
        {
            if (w == null) return;
            Camera.onPostRender -= Post;
            w.Flush(); w.Close(); w = null; drv = null;
        }
    }
}
