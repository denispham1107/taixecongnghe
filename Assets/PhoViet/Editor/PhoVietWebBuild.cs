using System;
using System.IO;
using System.IO.Compression;
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
            PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.geometricMemoryGrowthStep=.1f;
            PlayerSettings.WebGL.memoryGeometricGrowthCap=32;
            // GPU-compressed Web textures avoid the former RGB(A)32 expansion.
            PlayerSettings.WebGL.maximumMemorySize=512;
            PhoVietWebMemorySettings.Apply();
            PlayerSettings.runInBackground=false;
            PlayerSettings.bundleVersion="1.51.0";
            AssetDatabase.SaveAssets();
            Debug.Log("PHỐ VIỆT PWA configured: gzip fallback, single thread, Safari-compatible WASM");
        }

        // Can also run with Unity -batchmode -executeMethod PhoViet.Editor.PhoVietWebBuild.Build.
        [MenuItem("Phố Việt/Build Web PWA (mobile + desktop)")]
        public static void Build()
        {
            Configure();
            BuildSingle();
            CompleteTextureVariants();
        }

        // Resume only after a successful mobile build if output merging was interrupted.
        public static void CompleteTextureVariants()
        {
            const string root="Builds/WebGL";
            PruneRuntimeOutput(root);
            string mobileData=Directory.GetFiles(root+"/Build","*.data.unityweb").Single();
            byte[] mobileBytes=File.ReadAllBytes(mobileData);
            string dataName=Path.GetFileName(mobileData);
            var shared=new[]{"*.wasm.unityweb","*.framework.js.unityweb"}.ToDictionary(pattern=>pattern,pattern=>File.ReadAllBytes(Directory.GetFiles(root+"/Build",pattern).Single()));
            PhoVietWebMemorySettings.Apply(false);
            BuildSingle();
            foreach(var pair in shared)
                if(!Uncompress(pair.Value).SequenceEqual(Uncompress(File.ReadAllBytes(Directory.GetFiles(root+"/Build",pair.Key).Single()))))throw new Exception("Texture variants must share identical code: "+pair.Key);
            string desktopData=Directory.GetFiles(root+"/Build","*.data.unityweb").Single();
            if(Path.GetFileName(desktopData)==dataName)throw new Exception("Expected separate mobile and desktop texture data");
            File.WriteAllBytes(root+"/Build/"+dataName,mobileBytes);
            string htmlPath=root+"/index.html",html=File.ReadAllText(htmlPath);
            html=html.Replace("window.PHO_VIET_BUILD={","window.PHO_VIET_BUILD={\n      mobileDataUrl:\"Build/"+dataName+"\",");
            File.WriteAllText(htmlPath,html);
            RefreshWebShell();
        }

        static byte[] Uncompress(byte[] bytes)
        {
            using var input=new MemoryStream(bytes);
            using var gzip=new GZipStream(input,CompressionMode.Decompress);
            using var output=new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        static void BuildSingle()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/PhoViet/Scenes/PhoViet.unity"},
                target=BuildTarget.WebGL,
                locationPathName="Builds/WebGL",
                options=BuildOptions.None
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new Exception("Web build failed: "+report.summary.result);
            PruneRuntimeOutput("Builds/WebGL");
            FinalizeWebOutput("Builds/WebGL");
        }

        static void PruneRuntimeOutput(string root)
        {
            string directory=Path.GetFullPath(Path.Combine(root,"Build"));
            string expected=Path.GetFullPath(Path.Combine(Application.dataPath,"../Builds/WebGL/Build"));
            if(!string.Equals(directory,expected,StringComparison.OrdinalIgnoreCase))throw new Exception("Unexpected generated build directory");
            string html=File.ReadAllText(Path.Combine(root,"index.html"));
            var matches=System.Text.RegularExpressions.Regex.Matches(html,"(?:loaderUrl|dataUrl|frameworkUrl|codeUrl)\\s*:\\s*\"Build/([^\"]+)\"");
            var names=matches.Cast<System.Text.RegularExpressions.Match>().Select(m=>m.Groups[1].Value).ToArray();
            if(names.Length!=4||names.Distinct().Count()!=4||names.Any(n=>Path.GetFileName(n)!=n))throw new Exception("Expected four generated runtime URLs");
            foreach(string path in Directory.GetFiles(directory))
                if(!names.Contains(Path.GetFileName(path)))File.Delete(path);
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
            foreach(string file in new[]{"style.css","app.js","manifest.webmanifest","poster-morning.jpg","poster-morning-portrait.jpg","poster-night.jpg"})
                File.Copy("Assets/WebGLTemplates/PhoVietPWA/"+file,Path.Combine(root,file),true);
            Directory.CreateDirectory(Path.Combine(root,"icons"));
            foreach(string icon in Directory.GetFiles("Assets/WebGLTemplates/PhoVietPWA/icons"))
                if(!icon.EndsWith(".meta"))File.Copy(icon,Path.Combine(root,"icons",Path.GetFileName(icon)),true);
            // Replace the whole landing template while retaining the generated runtime
            // URLs, both texture variants and product version from the successful build.
            string htmlPath=Path.Combine(root,"index.html"),html=File.ReadAllText(htmlPath);
            string template=File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/index.html");
            const string configPattern=@"window\.PHO_VIET_BUILD=\{[\s\S]*?\n\s*\};";
            var config=System.Text.RegularExpressions.Regex.Match(html,configPattern);
            if(!config.Success||!System.Text.RegularExpressions.Regex.IsMatch(template,configPattern))throw new Exception("Missing generated runtime configuration");
            html=System.Text.RegularExpressions.Regex.Replace(template,configPattern,_=>config.Value);
            if(html.Contains("{{{"))throw new Exception("Unexpanded Unity template token");
            File.WriteAllText(htmlPath,html);
            string oldPoster=Path.Combine(root,"poster.png");if(File.Exists(oldPoster))File.Delete(oldPoster);
            FinalizeWebOutput(root);
        }

        static void FinalizeWebOutput(string root)
        {
            // Git checks out text with LF. Canonicalize before hashing so the
            // reconstructed Pages artifact has exactly the tested cache identity.
            foreach(string path in Directory.GetFiles(root,"*",SearchOption.AllDirectories))
            {
                string extension=Path.GetExtension(path).ToLowerInvariant();
                if(!new[]{".html",".css",".js",".json",".webmanifest",".svg",".txt"}.Contains(extension))continue;
                string value=File.ReadAllText(path).Replace("\r\n","\n").Replace("\r","\n");
                File.WriteAllText(path,value,new System.Text.UTF8Encoding(false));
            }
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
            File.WriteAllText(Path.Combine(root,"precache.json"),JsonUtility.ToJson(new CacheList{files=files,dataVariants=files.Where(f=>f.StartsWith("Build/",StringComparison.Ordinal)&&f.EndsWith(".data.unityweb",StringComparison.Ordinal)).ToArray()}));
            string workerPath=Path.Combine(root,"sw.js");
            File.WriteAllText(workerPath,File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/sw.js").Replace("__PHOVIET_CACHE__",digest));
            File.WriteAllText(Path.Combine(root,"build-info.json"),JsonUtility.ToJson(new BuildInfo {
                version=Application.version,cache=digest,unity=Application.unityVersion,utc=DateTime.UtcNow.ToString("o")
            },true));
            File.WriteAllText(Path.Combine(root,".nojekyll"),"");
            Debug.Log("PHỐ VIỆT PWA ready: "+files.Length+" offline files, cache "+digest);
        }
        [Serializable] private class CacheList { public string[] files,dataVariants; }
        [Serializable] private class BuildInfo { public string version,cache,unity,utc; }
    }
}
