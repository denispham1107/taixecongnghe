using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class VerifyPlayerMissileScene2128
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  check(g!=null&&!g.gameObject.scene.isDirty&&!EditorApplication.isCompiling&&!BuildPipeline.isBuildingPlayer,"Edit Mode, clean authored scene, no build/compile");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Jobs remain locked");check(Application.version=="1.54.0","Version not bumped without a release");
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PhoViet/Resources/PlayerMissile/PV_MissilePickup.prefab");check(prefab!=null,"Runtime pickup resource exists");var group=prefab.GetComponent<LODGroup>();var lods=group.GetLODs();check(lods.Length==3,"Pickup has three LODs");
  int[] expected={868,504,252};for(int i=0;i<3;i++){check(lods[i].renderers.Length==1,"One pickup draw surface per LOD"+i);var mesh=lods[i].renderers[0].GetComponent<MeshFilter>().sharedMesh;check(mesh.triangles.Length/3==expected[i]&&mesh.colors32.Length==mesh.vertexCount,"Expected triangles/vertex colors LOD"+i);check(mesh.bounds.size.x<2&&mesh.bounds.size.y<2,"Pickup fits within one lane LOD"+i);check(!ShaderUtil.ShaderHasError(lods[i].renderers[0].sharedMaterial.shader),"Pickup shader valid LOD"+i);var colors=mesh.colors;var vertices=mesh.vertices;float y=0;int red=0;for(int j=0;j<colors.Length;j++)if(colors[j].r>.1f&&colors[j].r>colors[j].g*3&&colors[j].r>colors[j].b*3){y+=vertices[j].y;red++;}check(red>0&&y/red>.4f,"Red nose points upward within pickup ring LOD"+i);}
  check(prefab.GetComponentsInChildren<ParticleSystem>(true).Length==0&&prefab.GetComponentsInChildren<Light>(true).Length==0&&prefab.GetComponentsInChildren<Collider>(true).Length==0,"No pickup particles/lights/colliders");
  var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/PlayerMissile/PV_MissileHud.png");var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(icon));check(icon!=null&&icon.width==256&&icon.height==256,"HUD vector raster is256square");check(importer.alphaIsTransparency&&!importer.mipmapEnabled&&importer.filterMode==FilterMode.Bilinear,"HUD alpha/filter settings preserve small silhouette");
  foreach(var viewport in new[]{new Vector2(320,568),new Vector2(390,844),new Vector2(430,932),new Vector2(568,320),new Vector2(844,390),new Vector2(932,430),new Vector2(768,1024),new Vector2(1024,768),new Vector2(1440,900),new Vector2(1920,1080)})foreach(float dpr in new[]{1f,2f,3f})
  {
   var area=new Rect(12*dpr,24*dpr,(viewport.x-24)*dpr,(viewport.y-48)*dpr);var rect=PhoVietGame.PlayerMissileButtonLayout(area,viewport.x<viewport.y,dpr);
   string label=viewport+" DPR"+dpr;check(area.Contains(rect.min)&&area.Contains(rect.max),"Button fits safe viewport "+label);check(rect.width==rect.height&&rect.width/dpr>=55.99f,"Round target at least56CSSpixels "+label);check(rect.yMin>area.yMin+110*dpr,"Button never overlaps top HUD "+label);
  }
  string html=File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/index.html");check(html.Contains("<b>F</b>")&&html.Contains("<b>Tên lửa của bạn:</b>")&&html.Contains("tối đa 3"),"Landing/help explain F and ammo without old rules");
  string js=File.ReadAllText("Assets/WebGLTemplates/PhoVietPWA/app.js");check(js.Contains("pixelRatio:unityInstance.Module.devicePixelRatio"),"Browser reports existing render ratio for touch sizing");
  var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/player2128-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
 }
}
