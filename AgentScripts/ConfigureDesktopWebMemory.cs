using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public static class ConfigureDesktopWebMemory
{
 public static object Main()
 {
  if(Application.isPlaying||EditorUserBuildSettings.overrideTextureCompression!=UnityEditor.Build.OverrideTextureCompression.NoOverride)throw new Exception("Saved Edit Mode without import overrides required");
  return PhoViet.Editor.PhoVietWebMemorySettings.Apply(false);
 }
}
