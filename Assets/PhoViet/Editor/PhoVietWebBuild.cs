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
        // 1.74.0 (21.264): bản web chạy bản đồ mở; scene phố cuộn cũ PhoViet.unity đã xóa khỏi dự án.
        public const string Scene="Assets/PhoViet/Scenes/PhoVietOpenWorld.unity";

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
            // 21.280: heap ban đầu 384 MiB (gói điện thoại nạp xong ~366 MiB) → không nở heap lúc nạp/chơi; Safari iOS có thể phải
            // chép cả heap mỗi lần nở (tạm thời gấp đôi). Trang chưa dùng không tốn bộ nhớ thật.
            PlayerSettings.WebGL.initialMemorySize=384;
            PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.geometricMemoryGrowthStep=.1f;
            PlayerSettings.WebGL.memoryGeometricGrowthCap=32;
            // Growth remains on demand. 1.74.0 (21.264): bản đồ mở nặng hơn phố cuộn cũ (1.73 cần 640 MiB) → trần 1024 MiB.
            PlayerSettings.WebGL.maximumMemorySize=1024;
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
            PhoVietWebMemorySettings.Apply();
            PlayerSettings.runInBackground=false;
            // 21.136: cascaded directional shadows make Unity draw the whole scene once more into a screen-space depth
            // pass before the real one. Without them the forward pass samples the shadow map directly.
            foreach(UnityEngine.Rendering.GraphicsTier tier in Enum.GetValues(typeof(UnityEngine.Rendering.GraphicsTier)))
            {
                var settings=UnityEditor.Rendering.EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.WebGL,tier);
                if(settings.cascadedShadowMaps){settings.cascadedShadowMaps=false;UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.WebGL,tier,settings);}
            }
            // 21.281: mã WebAssembly tối ưu kích thước. Trước đây «BuildTimes» (chế độ build nhanh khi phát triển, không tối ưu):
            // wasm 22 MB → Safari iOS tốn nhiều bộ nhớ biên dịch và biên dịch lại khi chơi. Nội dung game không đổi.
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL,ManagedStrippingLevel.Medium);
            PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.WebGL,UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.WebGL,Il2CppCompilerConfiguration.Master);
            SetWasmCodeOptimization("DiskSizeLTO");
            PlayerSettings.bundleVersion="1.95.3";
            AssetDatabase.SaveAssets();
            Debug.Log("PHỐ VIỆT PWA configured: gzip fallback, single thread, Safari-compatible WASM");
        }

        // UnityEditor.WebGL.UserBuildSettings.codeOptimization lưu theo máy (Library), nên đặt lại mỗi lần build qua reflection.
        static void SetWasmCodeOptimization(string value)
        {
            Type t=null;
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies()){t=a.GetType("UnityEditor.WebGL.UserBuildSettings");if(t!=null)break;}
            var p=t?.GetProperty("codeOptimization",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);
            if(p==null)throw new Exception("WebGL codeOptimization setting not found");
            p.SetValue(null,Enum.Parse(p.PropertyType,value));
            if(p.GetValue(null).ToString()!=value)throw new Exception("WebGL codeOptimization not applied: "+p.GetValue(null));
        }

        // Can also run with Unity -batchmode -executeMethod PhoViet.Editor.PhoVietWebBuild.Build.
        [MenuItem("Phố Việt/Build Web PWA (mobile + desktop)")]
        public static void Build()
        {
            Configure();
            // 21.277: gói điện thoại bỏ nội thất 3D (chỉ ảnh xa) cho Safari iPhone/iPad không sập vì hết bộ nhớ
            PhoVietMobileSceneStrip.Active=true;
            try{BuildSingle();}finally{PhoVietMobileSceneStrip.Active=false;}
            if(PhoVietMobileSceneStrip.LastStripped<1000)throw new Exception("Mobile interior strip did not run: "+PhoVietMobileSceneStrip.LastStripped);
            // 21.282: phải có dữ liệu occlusion culling (menu Bản đồ mở/12) — thiếu thì mỗi khung vẽ mọi thứ trong tầm 1.500 m.
            if(!System.IO.File.Exists("Assets/PhoViet/Scenes/PhoVietOpenWorld/OcclusionCullingData.asset"))throw new Exception("Missing occlusion culling data: run Phố Việt/Bản đồ mở/12");
            // 21.279: probe phản chiếu sông phải tắt (bật = Unity trộn cho mọi renderer → +560 MB, Safari iPhone sập).
            foreach(var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include))
                if(probe.enabled&&probe.gameObject.scene.path==Scene)throw new Exception("Reflection probe must stay disabled: "+probe.name);
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
            // 21.279: build sạch mỗi lần — build tăng dần có thể bỏ qua bước xử lý scene (gói điện thoại/máy tính lẫn nhau).
            PhoVietMobileSceneStrip.Processed=0;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{Scene},
                target=BuildTarget.WebGL,
                locationPathName="Builds/WebGL",
                options=BuildOptions.CleanBuildCache
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new Exception("Web build failed: "+report.summary.result);
            if(PhoVietMobileSceneStrip.Processed<1)throw new Exception("Scene was not processed in this build (stale build cache)");
            PruneRuntimeOutput("Builds/WebGL");
            PruneUnusedPosters("Builds/WebGL");
            FinalizeWebOutput("Builds/WebGL");
        }

        // 21.295: trang chủ chỉ còn MỘT ảnh (poster-home-mobile.jpg). Ảnh poster*.jpg/png khác trong template (lịch sử) không được
        // index.html tham chiếu → xóa khỏi bản phát hành trước khi tính cache offline.
        static void PruneUnusedPosters(string root)
        {
            string html=File.ReadAllText(Path.Combine(root,"index.html"));
            foreach(string path in Directory.GetFiles(root,"poster*"))
                if(!html.Contains(Path.GetFileName(path)))File.Delete(path);
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
            foreach(string file in new[]{"style.css","app.js","account.js","manifest.webmanifest","poster-home-mobile.jpg","poster-home-lazer.jpg","poster-home-khien.jpg"})
                File.Copy("Assets/WebGLTemplates/PhoVietPWA/"+file,Path.Combine(root,file),true);
            // 21.331: trang chủ mẫu C có dải 8 nút điều khiển (controls/btn-*.png)
            foreach(string dir in new[]{"icons","controls","install"})   // 21.358: ảnh hướng dẫn cài (install/)
            {
                Directory.CreateDirectory(Path.Combine(root,dir));
                foreach(string f in Directory.GetFiles("Assets/WebGLTemplates/PhoVietPWA/"+dir))
                    if(!f.EndsWith(".meta"))File.Copy(f,Path.Combine(root,dir,Path.GetFileName(f)),true);
            }
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
            PruneUnusedPosters(root);
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
