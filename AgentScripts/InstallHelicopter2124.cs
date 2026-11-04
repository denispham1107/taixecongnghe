using System;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;using PhoViet;
public static class InstallHelicopter2124
{
 const string Art="Assets/PhoViet/Art/Helicopter/",Res="Assets/PhoViet/Resources/Helicopter/";
 static GameObject Part(string name,Transform parent){var o=new GameObject(name);o.transform.SetParent(parent,false);return o;}
 static Mesh Disc(float radius,string path)
 {
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
  int n=64;float[] rings={.12f,.35f,.85f,1};float[] alpha={0,.065f,.055f,0};var verts=new Vector3[n*4];var colors=new Color[n*4];var tris=new int[n*3*6];
  for(int r=0;r<4;r++)for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;verts[r*n+i]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius*rings[r];colors[r*n+i]=r<2?new Color(.17f,.20f,.22f,alpha[r]):new Color(.55f,.16f,.16f,alpha[r]);}
  int k=0;for(int r=0;r<3;r++)for(int i=0;i<n;i++){int a=r*n+i,b=r*n+(i+1)%n,c=a+n,d=b+n;foreach(int v in new[]{a,c,b,b,c,d})tris[k++]=v;}
  mesh=new Mesh{name="Rotor motion exposure"};mesh.vertices=verts;mesh.colors=colors;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  Directory.CreateDirectory(Res);AssetDatabase.Refresh();
  var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"PV_Helicopter.fbx");importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=false;importer.SaveAndReimport();
  foreach(string suffix in new[]{"Palette","Normal","Surface"})
  {
   string path=Art+"PV_Helicopter_"+suffix+".png";var t=(TextureImporter)AssetImporter.GetAtPath(path);t.maxTextureSize=2048;t.mipmapEnabled=true;t.isReadable=false;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.sRGBTexture=suffix=="Palette";t.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
   var w=t.GetPlatformTextureSettings("WebGL");w.name="WebGL";w.overridden=true;w.maxTextureSize=2048;w.format=suffix=="Palette"?TextureImporterFormat.DXT1:TextureImporterFormat.DXT5;w.textureCompression=TextureImporterCompression.CompressedHQ;w.compressionQuality=50;t.SetPlatformTextureSettings(w);t.SaveAndReimport();
  }
  var audio=(AudioImporter)AssetImporter.GetAtPath(Art+"PV_HelicopterRotor.wav");audio.forceToMono=true;var sample=audio.defaultSampleSettings;sample.loadType=AudioClipLoadType.DecompressOnLoad;sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.quality=.65f;audio.defaultSampleSettings=sample;audio.SaveAndReimport();
  string matPath=Art+"PV_Helicopter.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,matPath);}
  mat.color=Color.white;mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PV_Helicopter_Palette.png");mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PV_Helicopter_Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PV_Helicopter_Surface.png"));mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICGLOSSMAP");mat.SetFloat("_GlossMapScale",.45f);mat.SetFloat("_BumpScale",.65f);mat.enableInstancing=true;
  var blurShader=Shader.Find("PhoViet/Rotor Blur");if(blurShader==null||ShaderUtil.ShaderHasError(blurShader))throw new Exception("Rotor shader failed");var blur=AssetDatabase.LoadAssetAtPath<Material>(Art+"PV_RotorBlur.mat");if(blur==null){blur=new Material(blurShader);AssetDatabase.CreateAsset(blur,Art+"PV_RotorBlur.mat");}
  var meshes=AssetDatabase.LoadAllAssetsAtPath(Art+"PV_Helicopter.fbx").OfType<Mesh>().ToArray();if(meshes.Length!=9)throw new Exception("Expected three parts x three LODs");
  var data=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Art+"PV_Helicopter.json"));var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Trực thăng • Redline Rescue");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
  try
  {
   var visual=root.AddComponent<HelicopterVisual>();visual.airframe=Part("Airframe",root.transform).transform;visual.mainRotor=Part("Main rotor",visual.airframe).transform;visual.tailRotor=Part("Tail rotor",visual.airframe).transform;
   foreach(var pair in new[]{Tuple.Create("MainRotor",visual.mainRotor),Tuple.Create("TailRotor",visual.tailRotor)}){var p=data["pivotsUnity"][pair.Item1];pair.Item2.localPosition=new Vector3((float)p[0],(float)p[1],(float)p[2]);}
   var lods=new LOD[3];
   for(int lod=0;lod<3;lod++)
   {
    var rs=new Renderer[3];int j=0;
    foreach(string part in new[]{"Body","MainRotor","TailRotor"})
    {
     var mesh=meshes.Single(m=>m.name=="Heli_"+part+"_LOD"+lod);var go=Part(mesh.name,part=="Body"?visual.airframe:part=="MainRotor"?visual.mainRotor:visual.tailRotor);go.transform.localRotation=Quaternion.Euler(-90,0,0);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=lod==0?ShadowCastingMode.On:ShadowCastingMode.Off;rs[j++]=r;
    }
    lods[lod]=new LOD(lod==0?.12f:lod==1?.05f:.009f,rs);
   }
   var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
   foreach(var pair in new[]{Tuple.Create("Main",visual.mainRotor,2.55f),Tuple.Create("Tail",visual.tailRotor,.55f)}){var go=Part(pair.Item1+" exposure blur",pair.Item2);if(pair.Item1=="Tail")go.transform.localRotation=Quaternion.Euler(0,0,90);go.AddComponent<MeshFilter>().sharedMesh=Disc(pair.Item3,Art+"PV_"+pair.Item1+"RotorBlur.asset");var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=blur;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
   var a=Part("Rotor audio",visual.airframe).AddComponent<AudioSource>();a.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Art+"PV_HelicopterRotor.wav");a.loop=true;a.playOnAwake=false;a.spatialBlend=.9f;a.dopplerLevel=.7f;a.minDistance=6;a.maxDistance=140;a.rolloffMode=AudioRolloffMode.Linear;a.volume=0;visual.rotorAudio=a;
   PrefabUtility.SaveAsPrefabAsset(root,Res+"PV_Helicopter.prefab");AssetDatabase.SaveAssets();
   var report=new{success=true,parts=meshes.Select(m=>new{name=m.name,triangles=m.triangles.Length/3}).ToArray(),mainPivot=new[]{visual.mainRotor.localPosition.x,visual.mainRotor.localPosition.y,visual.mainRotor.localPosition.z},tailPivot=new[]{visual.tailRotor.localPosition.x,visual.tailRotor.localPosition.y,visual.tailRotor.localPosition.z},audio=a.clip!=null,previewOnly=true};File.WriteAllText("Captures/heli2124-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }finally{UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);}
 }
}
