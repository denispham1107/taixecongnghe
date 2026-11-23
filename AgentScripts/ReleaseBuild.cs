using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using PhoViet.Editor;
// 21.333 — script build phát hành dùng cho MỌI bản sau 1.92.0 (thay QueueRelease213xx từng bản).
// Lý do: 1.92.0 build lần 1 ra nhầm 1.91.0 vì PhoVietWebBuild.cs được sửa ngoài Unity ngay trước khi build, Unity chưa biên dịch
// lại (MCP recompile còn báo «up_to_date») → Build() ghi số cũ. Script này chạy bằng run_script (biên dịch mới từ tệp mỗi lần) nên
// tự đọc số phiên bản trong mã nguồn và CHẶN build khi dll Editor chưa có số đó.
// Cách dùng (Unity ở chế độ Edit):
//   1. Sửa số phiên bản trong PhoVietWebBuild.cs (PlayerSettings.bundleVersion="x.y.z") + ProjectSettings.asset.
//   2. run_script ReleaseBuild.Prepare — trả «ready x.y.z» thì sang bước 3; trả «compiling …» thì đợi biên dịch xong rồi gọi lại.
//   3. run_script ReleaseBuild.BuildNow — lệnh MCP hết giờ sau 60 s nhưng build vẫn chạy; theo dõi Builds/release-x.y.z-status.json
//      ({"state":"done",…} hoặc "error"). Sau khi xong, script tự kiểm Builds/WebGL/build-info.json mang đúng x.y.z.
public static class ReleaseBuild
{
    const string Src = "Assets/PhoViet/Editor/PhoVietWebBuild.cs";
    const string Dll = "Library/ScriptAssemblies/Assembly-CSharp-Editor.dll";

    public static string SourceVersion()
    {
        var m = Regex.Match(File.ReadAllText(Src), "PlayerSettings\\.bundleVersion\\s*=\\s*\"([0-9]+\\.[0-9]+\\.[0-9]+)\"");
        if (!m.Success) throw new Exception("Không tìm thấy PlayerSettings.bundleVersion trong " + Src);
        return m.Groups[1].Value;
    }

    // Chuỗi hằng trong dll .NET lưu dạng UTF-16.
    public static bool DllHas(string version)
    {
        byte[] d = File.ReadAllBytes(Dll), n = Encoding.Unicode.GetBytes(version);
        for (int i = 0; i + n.Length <= d.Length; i++)
        {
            int k = 0; while (k < n.Length && d[i + k] == n[k]) k++;
            if (k == n.Length) return true;
        }
        return false;
    }

    static string Status(string v) => "Builds/release-" + v + "-status.json";
    static string Esc(string m) => m.Replace("\"", "'").Replace("\n", " ").Replace("\r", " ");

    public static string Prepare()
    {
        string v = SourceVersion();
        if (EditorApplication.isPlaying) return "đang Play — dừng trước";
        if (DllHas(v) && !EditorApplication.isCompiling)
        {
            PlayerSettings.bundleVersion = v; AssetDatabase.SaveAssets();
            return "ready " + v;
        }
        AssetDatabase.ImportAsset(Src, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        CompilationPipeline.RequestScriptCompilation();
        return "compiling for " + v + " — đợi biên dịch xong rồi gọi lại Prepare tới khi «ready " + v + "»";
    }

    public static string BuildNow()
    {
        string v = SourceVersion(), status = Status(v);
        Directory.CreateDirectory("Builds");
        if (EditorApplication.isPlaying) { File.WriteAllText(status, "{\"state\":\"error\",\"msg\":\"đang Play\"}"); return "đang Play — dừng trước"; }
        if (EditorApplication.isCompiling || !DllHas(v))
        {
            File.WriteAllText(status, "{\"state\":\"error\",\"msg\":\"dll Editor chưa có " + v + " — chạy ReleaseBuild.Prepare trước\"}");
            return "CHẶN: dll Editor chưa có " + v + " (Unity chưa biên dịch lại PhoVietWebBuild.cs). Chạy ReleaseBuild.Prepare trước.";
        }
        EditorSceneManager.SaveOpenScenes();
        File.WriteAllText(status, "{\"state\":\"building-now\",\"version\":\"" + v + "\"}");
        var t0 = DateTime.Now;
        try
        {
            PhoVietWebBuild.Build();
            string info = File.Exists("Builds/WebGL/build-info.json") ? File.ReadAllText("Builds/WebGL/build-info.json") : "";
            if (!info.Contains("\"" + v + "\"")) throw new Exception("build-info.json không mang " + v + ": " + info);
            File.WriteAllText(status, "{\"state\":\"done\",\"version\":\"" + v + "\",\"seconds\":" + (int)(DateTime.Now - t0).TotalSeconds + ",\"stripped\":" + PhoVietMobileSceneStrip.LastStripped + "}");
        }
        catch (Exception e) { File.WriteAllText(status, "{\"state\":\"error\",\"version\":\"" + v + "\",\"msg\":\"" + Esc(e.Message) + "\"}"); }
        return File.ReadAllText(status);
    }
}
