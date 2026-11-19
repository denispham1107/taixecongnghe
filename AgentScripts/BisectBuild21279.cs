using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhoViet.Editor;

// 21.279: build WebGL (gói máy tính DXT) để đo bộ nhớ WebKit, bỏ bớt từng phần mới của 1.76 trong lúc build.
// args: [variant] — chữ cái: i = bỏ nội thất 3D, I = bỏ toàn bộ NoiThat, p = bỏ probe phản chiếu sông,
// b = bỏ tàu, l = bỏ MatLe, g = bỏ kính mặt tiền (renderer dùng shader ShopGlass), r = bỏ mặt sông mới.
public static class BisectBuild21279
{
 public static object Main(string variant)
 {
  string status="Builds/bisect-"+variant+".json";
  File.WriteAllText(status,"{\"state\":\"queued\"}");
  EditorApplication.delayCall+=()=>{
   var t0=DateTime.Now;int removed=0;
   PhoVietMobileSceneStrip.Extra=scene=>{
    foreach(var root in scene.GetRootGameObjects()){
     if(variant.Contains("p")&&root.name=="PhanChieuSong"){UnityEngine.Object.DestroyImmediate(root);removed++;continue;}
     if(variant.Contains("b")&&root.name=="TauTrenSong"){UnityEngine.Object.DestroyImmediate(root);removed++;continue;}
     if(variant.Contains("l")&&root.name=="MatLe"){UnityEngine.Object.DestroyImmediate(root);removed++;continue;}
     if(variant.Contains("r")&&root.name=="MatSong"){UnityEngine.Object.DestroyImmediate(root);removed++;continue;}
     foreach(var lg in root.GetComponentsInChildren<LODGroup>(true)){
      if(lg==null||lg.name!="NoiThat")continue;
      if(variant.Contains("I")){UnityEngine.Object.DestroyImmediate(lg.gameObject);removed++;continue;}
      if(variant.Contains("i")){var t3=lg.transform.Find("3D");var tx=lg.transform.Find("Xa");if(t3&&tx){lg.SetLODs(new[]{new LOD(0.02f,new Renderer[]{tx.GetComponent<Renderer>()})});UnityEngine.Object.DestroyImmediate(t3.gameObject);removed++;}}
     }
     if(variant.Contains("g"))foreach(var r in root.GetComponentsInChildren<Renderer>(true)){
      if(r==null)continue;var m=r.sharedMaterials;bool any=false;
      for(int k=0;k<m.Length;k++)if(m[k]&&m[k].shader&&m[k].shader.name.Contains("ShopGlass")){any=true;}
      if(any&&m.Length==1){UnityEngine.Object.DestroyImmediate(r.gameObject);removed++;}
     }
    }
   };
   string result;
   try{
    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{PhoVietWebBuild.Scene},target=BuildTarget.WebGL,locationPathName="Builds/WebGL-Bisect-"+variant,options=BuildOptions.CleanBuildCache});
    var data=Directory.GetFiles("Builds/WebGL-Bisect-"+variant+"/Build","*.data.unityweb").Select(f=>new FileInfo(f).Length).FirstOrDefault();
    result="{\"state\":\""+report.summary.result+"\",\"seconds\":"+(int)(DateTime.Now-t0).TotalSeconds+",\"removed\":"+removed+",\"data\":"+data+"}";
   }catch(Exception e){result="{\"state\":\"error\",\"msg\":\""+e.Message.Replace("\"","'")+"\"}";}
   finally{PhoVietMobileSceneStrip.Extra=null;}
   File.WriteAllText(status,result);
  };
  return "queued "+variant;
 }
}
