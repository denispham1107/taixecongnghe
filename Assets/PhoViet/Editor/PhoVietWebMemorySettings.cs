using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhoViet.Editor
{
 public static class PhoVietWebMemorySettings
 {
  // Recovery after an interrupted platform-wide texture import. The startup
  // uncompressed override is temporary; restore normal imports before building.
  public static void RecoverImportSettings()
  {
   EditorUserBuildSettings.webGLBuildSubtarget=WebGLTextureSubtarget.Generic;
   EditorUserBuildSettings.overrideTextureCompression=UnityEditor.Build.OverrideTextureCompression.NoOverride;
   AssetDatabase.Refresh();
  }
  // Keep source pixels, mipmaps, filtering, colour space and all geometry.
  // GPU compression must match mobile WebGL: DXT is expanded on iPhone, and
  // the player/rider's former RGB(A)32 maps alone consumed ~169 MiB.
  public static object Apply(bool mobile=true)
  {
   if(Application.isPlaying)throw new InvalidOperationException("Exit Play before configuring Web textures");
   if(EditorUserBuildSettings.overrideTextureCompression!=UnityEditor.Build.OverrideTextureCompression.NoOverride)
    throw new InvalidOperationException("Clear the temporary import compression override before a release build");
   // Explicit per-texture formats avoid reimporting unused historic art when
   // the global Web subtarget changes. Only release dependencies are touched.
   EditorUserBuildSettings.webGLBuildSubtarget=WebGLTextureSubtarget.Generic;
   var changes=new List<object>();
   // Importing one texture can refresh other cached importers. Re-read the
   // dependency settings until stable, instead of building a mixed GPU format.
   for(int pass=0;pass<3;pass++)
   {
   int before=changes.Count;
   foreach(string path in AssetDatabase.GetDependencies("Assets/PhoViet/Scenes/PhoViet.unity",true))
   {
    var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture==null||texture.width*texture.height<65536)continue;
    var web=importer.GetPlatformTextureSettings("WebGL");
    // Both region channels are scalar masks, not colours. EAC keeps them
    // separate and does not borrow the colour block's shared endpoints.
    TextureImporterFormat format=path.EndsWith("PV_UrbanRider_Regions.png",StringComparison.Ordinal)?TextureImporterFormat.EAC_RG:
     importer.textureType==TextureImporterType.NormalMap||importer.DoesSourceTextureHaveAlpha()?TextureImporterFormat.ETC2_RGBA8:TextureImporterFormat.ETC2_RGB4;
    if(!mobile)format=importer.textureType==TextureImporterType.NormalMap||importer.DoesSourceTextureHaveAlpha()||path.EndsWith("PV_UrbanRider_Regions.png",StringComparison.Ordinal)?TextureImporterFormat.DXT5:TextureImporterFormat.DXT1;
    if(web.overridden&&web.format==format&&web.compressionQuality==50)continue;
    web.name="WebGL";web.maxTextureSize=web.overridden?web.maxTextureSize:importer.maxTextureSize;
    web.overridden=true;web.format=format;web.textureCompression=TextureImporterCompression.CompressedHQ;
    web.compressionQuality=50;web.crunchedCompression=false;
    importer.SetPlatformTextureSettings(web);AssetDatabase.WriteImportSettingsIfDirty(path);
    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
    changes.Add(new{path,format=format.ToString(),size=web.maxTextureSize});
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   if(changes.Count==before)break;
   }
   foreach(string path in AssetDatabase.GetDependencies("Assets/PhoViet/Scenes/PhoViet.unity",true))
   {
    var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture==null||texture.width*texture.height<65536)continue;
    var format=importer.GetPlatformTextureSettings("WebGL").format;
    bool valid=mobile?(format==TextureImporterFormat.ETC2_RGB4||format==TextureImporterFormat.ETC2_RGBA8||format==TextureImporterFormat.EAC_RG):(format==TextureImporterFormat.DXT1||format==TextureImporterFormat.DXT5);
    if(!valid)throw new InvalidOperationException("Web texture imports did not stabilize: "+path);
   }
   var report=new{mobile,changed=changes.Count,changes,subtarget=EditorUserBuildSettings.webGLBuildSubtarget.ToString()};
   Directory.CreateDirectory("Captures");File.WriteAllText(mobile?"Captures/web-memory-texture-settings.json":"Captures/web-memory-desktop-settings.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
   return report;
  }
 }
}
