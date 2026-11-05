using System;using System.IO;using System.Reflection;using UnityEngine;using UnityEngine.Audio;using UnityEditor;
namespace PhoViet.Editor
{
 public static class PhoVietAudioUpgrade
 {
  const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
  public static object Install()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
   const string path="Assets/PhoViet/Resources/Audio/PV_Master.mixer";
   if(!AssetDatabase.IsValidFolder("Assets/PhoViet/Resources/Audio"))AssetDatabase.CreateFolder("Assets/PhoViet/Resources","Audio");
   var controller=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController",true);
   var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
   if(mixer==null)mixer=(AudioMixer)controller.GetMethod("CreateMixerControllerAtPath",F).Invoke(null,new object[]{path});
   var group=controller.GetProperty("masterGroup",F).GetValue(mixer);
   var snapshots=(Array)controller.GetProperty("snapshots",F).GetValue(mixer);
   var guid=group.GetType().GetMethod("GetGUIDForVolume",F).Invoke(group,null);
   foreach(var snapshot in snapshots)
   {
    group.GetType().GetMethod("SetValueForVolume",F).Invoke(group,new object[]{mixer,snapshot,PhoVietAudio.MasterDecibels});
    EditorUtility.SetDirty((UnityEngine.Object)snapshot);
   }
   var exposedType=controller.Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter",true);
   var parameter=Activator.CreateInstance(exposedType);exposedType.GetField("guid",F).SetValue(parameter,guid);exposedType.GetField("name",F).SetValue(parameter,PhoVietAudio.VolumeParameter);
   var exposed=Array.CreateInstance(exposedType,1);exposed.SetValue(parameter,0);controller.GetProperty("exposedParameters",F).SetValue(mixer,exposed);
   EditorUtility.SetDirty((UnityEngine.Object)group);EditorUtility.SetDirty(mixer);AssetDatabase.SaveAssets();
   var result=new{success=true,linearGain=PhoVietAudio.MasterGain,decibels=PhoVietAudio.MasterDecibels,snapshots=snapshots.Length,volumeOnly=true,sceneDirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};
   File.WriteAllText("Captures/audio2132-install.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
 }
}
