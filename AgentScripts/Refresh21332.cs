using UnityEditor;
// 21.332 — nạp lại tệp đã sửa ngoài Unity (PhoVietWebBuild.cs đổi số phiên bản 1.92.0) và yêu cầu biên dịch lại; đặt PlayerSettings 1.92.0.
public static class Refresh21332
{
 public static string Run(){ AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); PlayerSettings.bundleVersion="1.92.0"; AssetDatabase.SaveAssets(); return "version "+PlayerSettings.bundleVersion+", compiling "+EditorApplication.isCompiling; }
 public static string Reimport(){ AssetDatabase.ImportAsset("Assets/PhoViet/Editor/PhoVietWebBuild.cs",ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport); UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(); return "requested, compiling "+EditorApplication.isCompiling; }
}
