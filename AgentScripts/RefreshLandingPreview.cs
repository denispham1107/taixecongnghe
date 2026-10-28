using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RefreshLandingPreview
{
 public static object Main()
 {
  if(Application.isPlaying)throw new Exception("Edit Mode required");
  const string path="Builds/WebGL/index.html";
  Func<string,string[]> urls=html=>Regex.Matches(html,"(?:loaderUrl|mobileDataUrl|dataUrl|frameworkUrl|codeUrl)\\s*:\\s*\"Build/([^\"]+)\"").Cast<Match>().Select(m=>m.Value).OrderBy(x=>x).ToArray();
  var before=urls(File.ReadAllText(path));
  PhoViet.Editor.PhoVietWebBuild.RefreshWebShell();
  var html=File.ReadAllText(path);var after=urls(html);
  if(before.Length!=5||!before.SequenceEqual(after))throw new Exception("Generated runtime/texture URLs changed");
  if(html.Contains("{{{")||!html.Contains("poster-morning.jpg")||html.Contains("Áo đỏ sao vàng"))throw new Exception("Landing template was not expanded correctly");
  var report=new{success=true,checks=3,runtimeUrls=after,version=Application.version,previewOnly=true};
  File.WriteAllText("Captures/landing-refresh-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}
