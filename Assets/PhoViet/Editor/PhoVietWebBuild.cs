using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PhoViet.Editor
{
    public sealed class PhoVietWebBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;

        [MenuItem("Phố Việt/Cấu hình bản Web PWA")]
        public static void Configure()
        {
            PlayerSettings.WebGL.template="PROJECT:PhoVietPWA";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.nameFilesAsHashes=true;
            PlayerSettings.WebGL.dataCaching=false; // One versioned offline cache owns all web files.
            PlayerSettings.WebGL.threadsSupport=false; // GitHub Pages cannot supply isolation headers.
            PlayerSettings.WebGL.wasm2023=false; // Keep compatibility with Safari 15+.
            PlayerSettings.WebGL.initialMemorySize=128;
            PlayerSettings.WebGL.maximumMemorySize=512;
            PlayerSettings.runInBackground=false;
            PlayerSettings.bundleVersion="1.13.0";
            AssetDatabase.SaveAssets();
            Debug.Log("PHỐ VIỆT PWA configured: gzip fallback, single thread, Safari-compatible WASM");
        }

        // Can also run with Unity -batchmode -executeMethod PhoViet.Editor.PhoVietWebBuild.Build.
        public static void Build()
        {
            Configure();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/PhoViet/Scenes/PhoViet.unity"},
                target=BuildTarget.WebGL,
                locationPathName="Builds/WebGL",
                options=BuildOptions.None
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new Exception("Web build failed: "+report.summary.result);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if(report.summary.platform!=BuildTarget.WebGL)return;
            FinalizeWebOutput(report.summary.outputPath);
        }

        // Frontend-only changes can refresh a successful build without relinking unchanged WASM.
        public static void RefreshWebShell()
        {
            const string root="Builds/WebGL";
            if(!File.Exists(Path.Combine(root,"index.html")))throw new Exception("Build WebGL first");
            foreach(string file in new[]{"style.css","app.js","manifest.webmanifest"})
                File.Copy("Assets/WebGLTemplates/PhoVietPWA/"+file,Path.Combine(root,file),true);
            FinalizeWebOutput(root);
        }

        static void FinalizeWebOutput(string root)
        {
            // Hash file contents, including the shell, so each release has an atomic offline cache.
            var files=Directory.GetFiles(root,"*",SearchOption.AllDirectories)
                .Where(p=>!p.EndsWith(".meta")&&!p.EndsWith(".map")&&!p.EndsWith(".br"))
                .Select(p=>p.Substring(root.Length+1).Replace('\\','/'))
                .Where(p=>p!="sw.js"&&p!="precache.json"&&p!="build-info.json"&&p!=".nojekyll")
                .OrderBy(p=>p,StringComparer.Ordinal).ToArray();
            using var hash=SHA256.Create();
            foreach(string file in files)
            {
                byte[] bytes=File.ReadAllBytes(Path.Combine(root,file));
                hash.TransformBlock(bytes,0,bytes.Length,null,0);
            }
            hash.TransformFinalBlock(Array.Empty<byte>(),0,0);
            string digest=BitConverter.ToString(hash.Hash).Replace("-","").Substring(0,16).ToLowerInvariant();
            File.WriteAllText(Path.Combine(root,"precache.json"),JsonUtility.ToJson(new CacheList{files=files}));
            string workerPath=Path.Combine(root,"sw.js");
            File.WriteAllText(workerPath,File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/sw.js").Replace("__PHOVIET_CACHE__",digest));
            File.WriteAllText(Path.Combine(root,"build-info.json"),JsonUtility.ToJson(new BuildInfo {
                version=Application.version,cache=digest,unity=Application.unityVersion,utc=DateTime.UtcNow.ToString("o")
            },true));
            File.WriteAllText(Path.Combine(root,".nojekyll"),"");
            Debug.Log("PHỐ VIỆT PWA ready: "+files.Length+" offline files, cache "+digest);
        }
        [Serializable] private class CacheList { public string[] files; }
        [Serializable] private class BuildInfo { public string version,cache,unity,utc; }
    }
}
