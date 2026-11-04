using System;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;using PhoViet;
public static class InstallHelicopterMissile2126
{
 const string Art="Assets/PhoViet/Art/Helicopter/Missile/",Res="Assets/PhoViet/Resources/Helicopter/";
 static Material Mat(string name,string shader){string path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.enableInstancing=true;return m;}
 static GameObject Child(string name,Transform parent){var o=new GameObject(name);o.transform.SetParent(parent,false);return o;}
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"PV_Missile.fbx");importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=false;importer.SaveAndReimport();
  var ti=(TextureImporter)AssetImporter.GetAtPath(Art+"PV_MissileSmoke.png");ti.textureType=TextureImporterType.Default;ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;ti.mipmapEnabled=true;ti.filterMode=FilterMode.Trilinear;ti.wrapMode=TextureWrapMode.Clamp;ti.maxTextureSize=1024;ti.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="WebGL",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.DXT5});ti.SaveAndReimport();
  foreach(string name in new[]{"PhoViet/Missile Surface","PhoViet/Missile Exhaust","PhoViet/Soft Particle"}){var shader=Shader.Find(name);if(shader==null||ShaderUtil.ShaderHasError(shader))throw new Exception("Shader error: "+name);}
  Material surface=Mat("PV_MissileBody","PhoViet/Missile Surface"),exhaust=Mat("PV_MissileExhaust","PhoViet/Missile Exhaust"),smoke=Mat("PV_MissileSmoke","PhoViet/Soft Particle");smoke.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PV_MissileSmoke.png");
  var meshes=AssetDatabase.LoadAllAssetsAtPath(Art+"PV_Missile.fbx").OfType<Mesh>().ToArray();var data=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Art+"PV_Missile.json"));
  var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Tên lửa trực thăng • Blender");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
  try
  {
   var visual=root.AddComponent<MissileFlightVisual>();var lods=new LOD[3];
   for(int i=0;i<3;i++)
   {var mesh=meshes.Single(m=>m.name=="PV_MissileBody_LOD"+i);if(mesh.colors32.Length!=mesh.vertexCount||mesh.subMeshCount!=1)throw new Exception("Expected vertex colors/single surface");var o=Child(mesh.name,root.transform);o.transform.localRotation=Quaternion.Euler(-90,0,0);o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=surface;r.shadowCastingMode=ShadowCastingMode.Off;lods[i]=new LOD(i==0?.035f:i==1?.012f:.002f,new[]{r});}
   var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
   var f=Child("Authored plume core",root.transform);visual.flame=f.transform;f.transform.localPosition=new Vector3(0,0,-.64f);f.transform.localRotation=Quaternion.Euler(-90,0,0);f.AddComponent<MeshFilter>().sharedMesh=meshes.Single(m=>m.name=="PV_MissileExhaust");var fr=f.AddComponent<MeshRenderer>();fr.sharedMaterial=exhaust;fr.shadowCastingMode=ShadowCastingMode.Off;fr.receiveShadows=false;
   var pulse=(Newtonsoft.Json.Linq.JArray)data["plumePulse"];visual.plumePeriod=(float)data["plumePeriod"];visual.plumePulse=new AnimationCurve(pulse.Select((v,i)=>new Keyframe((float)i/(pulse.Count-1),(float)v)).ToArray());
   var go=Child("Blender exhaust smoke",root.transform);go.transform.localPosition=new Vector3(0,0,-.65f);go.transform.localRotation=Quaternion.Euler(0,180,0);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);visual.smoke=ps;
   var main=ps.main;main.playOnAwake=false;main.loop=true;main.maxParticles=(int)data["smokeMaxParticles"];main.startLifetime=(float)data["smokeLifetime"];main.startSpeed=3;main.startSize=new ParticleSystem.MinMaxCurve(.12f,.2f);main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);main.simulationSpace=ParticleSystemSimulationSpace.Custom;
   var emission=ps.emission;emission.rateOverTime=(float)data["smokeRate"];var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=7;shape.radius=.035f;
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.6f,1,4));var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.72f,.4f),0),new GradientColorKey(new Color(.85f,.88f,.9f),.18f),new GradientColorKey(new Color(.6f,.64f,.68f),1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(.65f,.2f),new GradientAlphaKey(0,1)});color.color=gradient;
   var sheet=ps.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=4;sheet.numTilesY=1;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,1));
   var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=smoke;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
   PhoViet.Editor.PhoVietMissileTrailSetup.Configure(root);
   PrefabUtility.SaveAsPrefabAsset(root,Res+"PV_HelicopterMissile.prefab");AssetDatabase.SaveAssets();
   var report=new{success=true,meshes=meshes.Select(m=>new{m.name,triangles=m.triangles.Length/3,submeshes=m.subMeshCount,colors=m.colors32.Length}).ToArray(),texture=new[]{512,128},previewSceneOnly=true};File.WriteAllText("Captures/missile2126-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
  }finally{UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);}
 }
}
