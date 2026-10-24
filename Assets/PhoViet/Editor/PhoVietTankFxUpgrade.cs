using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhoViet.Editor
{
    // Cài hiệu ứng xe tăng bắn (25/10/2026, huongdan.md 21.38): nhập đạn/kết cấu render trong Blender
    // (pho_viet_tank_fx.py), tạo vật liệu hạt/vòng cảnh báo và gán vào PhoVietGame rồi lưu scene.
    public static class PhoVietTankFxUpgrade
    {
        public const string Folder="Assets/PhoViet/Art/TankFx/";
        [MenuItem("Phố Việt/Art/Install tank shell effects")]
        public static object Install()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode first");
            var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(g==null)throw new Exception("Open PhoViet scene first");
            AssetDatabase.Refresh();
            var spark=Folder+"PV_TankFx_Spark.png";
            if(!System.IO.File.Exists(spark))
            {
                var t=new Texture2D(64,64,TextureFormat.RGBA32,false);
                for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))/32;t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(Mathf.Pow(1-d,1.6f))));}
                System.IO.File.WriteAllBytes(spark,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(spark);
            }
            foreach(var file in new[]{"PV_TankFx_Muzzle.png","PV_TankFx_Explosion.png","PV_TankFx_Warning.png","PV_TankFx_Spark.png"})
            {
                var importer=AssetImporter.GetAtPath(Folder+file) as TextureImporter;if(importer==null)throw new Exception("Missing "+file);
                importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.sRGBTexture=true;importer.wrapMode=TextureWrapMode.Clamp;
                importer.mipmapEnabled=true;importer.maxTextureSize=1024;importer.SaveAndReimport();
            }
            var model=AssetImporter.GetAtPath(Folder+"PV_TankShell.fbx") as ModelImporter;if(model==null)throw new Exception("Missing PV_TankShell.fbx");
            model.materialImportMode=ModelImporterMaterialImportMode.None;model.importAnimation=false;model.importCameras=false;model.importLights=false;model.addCollider=false;model.bakeAxisConversion=false;model.SaveAndReimport();
            Material Mat(string name,string shader,string texture,int queue)
            {
                var path=Folder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
                m.shader=Shader.Find(shader);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+texture);m.renderQueue=queue;EditorUtility.SetDirty(m);return m;
            }
            g.tankShellPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PV_TankShell.fbx");
            g.tankShellMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhoViet/Art/Streets/PV_StreetAtlas.mat");
            g.tankMuzzleMaterial=Mat("PV_TankMuzzle","PhoViet/Soft Particle","PV_TankFx_Muzzle.png",3100);
            g.tankExplosionMaterial=Mat("PV_TankExplosion","PhoViet/Soft Particle","PV_TankFx_Explosion.png",3100);
            g.tankSparkMaterial=Mat("PV_TankSpark","PhoViet/Soft Particle","PV_TankFx_Spark.png",3100);
            g.tankWarningMaterial=Mat("PV_TankWarning","PhoViet/Tank Warning","PV_TankFx_Warning.png",3005);
            g.randomTanks=false;g.tankBarrageEnabled=true;
            EditorUtility.SetDirty(g);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
            return new{success=true,shell=g.tankShellPrefab!=null,muzzle=g.tankMuzzleMaterial.mainTexture!=null,explosion=g.tankExplosionMaterial.mainTexture!=null,warning=g.tankWarningMaterial.shader.name,jobs=g.jobsEnabled};
        }
    }
}
