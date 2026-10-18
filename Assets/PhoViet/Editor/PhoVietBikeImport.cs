using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PhoViet.Editor
{
    public sealed class PhoVietBikeImport : AssetPostprocessor
    {
        const string Folder="Assets/PhoViet/Art/Bikes/";
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Folder,StringComparison.Ordinal))return;
            Configure((ModelImporter)assetImporter);
        }
        static void Configure(ModelImporter importer)
        {
            importer.animationType=ModelImporterAnimationType.Legacy;
            importer.importAnimation=true;importer.optimizeGameObjects=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.None;
            importer.importVisibility=false;importer.importCameras=false;importer.importLights=false;
            importer.addCollider=false;importer.isReadable=false;importer.globalScale=1;
            importer.meshCompression=ModelImporterMeshCompression.Low;
            importer.animationCompression=ModelImporterAnimationCompression.KeyframeReduction;
            importer.animationRotationError=.15f;importer.animationPositionError=.002f;importer.animationScaleError=.01f;
        }
        [MenuItem("Phố Việt/Art/Configure Blender bike imports")]
        public static void ConfigureAll()
        {
            AssetDatabase.Refresh();
            foreach(string guid in AssetDatabase.FindAssets("t:Model",new[]{Folder.TrimEnd('/')}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                if(importer==null)continue;Configure(importer);
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips)
                {
                    string[] names={"KickLeft","KickRight","FallLeft","FallRight","Recover","Idle","Ride"};
                    string name=names.FirstOrDefault(n=>clip.name.EndsWith(n,StringComparison.OrdinalIgnoreCase));
                    if(name==null)continue;clip.name=name;
                    clip.loopTime=name=="Idle"||name=="Ride";clip.wrapMode=clip.loopTime?WrapMode.Loop:WrapMode.ClampForever;
                }
                importer.clipAnimations=clips;importer.SaveAndReimport();
                Debug.Log(path+" imported clips: "+string.Join(",",AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).Select(c=>c.name)));
            }
        }
    }
}
