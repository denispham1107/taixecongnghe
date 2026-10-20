using System;
using System.Linq;
using UnityEngine;
namespace PhoViet
{
 // Sampled directly from the editable Blender jump actions. World translation remains in physics.
 public sealed class BikeJumpPose
 {
  [Serializable] public class Library { public string[] bones; public Clip[] clips; }
  [Serializable] public class Clip { public string name; public Pose[] samples; }
  [Serializable] public class Pose { public float pitch,crouch; public float[] angles; }
  static Library library;
  Transform[] bones;
  public bool Ready=>bones!=null&&library!=null;
  public void Initialize(Transform visual)
  {
   if(library==null){var data=Resources.Load<TextAsset>("PhoVietJump");if(data!=null)library=JsonUtility.FromJson<Library>(data.text);}
   if(library==null)return;
   var all=visual.GetComponentsInChildren<Transform>(true);
   bones=library.bones.Select(n=>all.FirstOrDefault(t=>t.name==n)).ToArray();
  }
  public float Apply(string name,float time)
  {
   if(!Ready)return 0;
   var clip=Array.Find(library.clips,c=>c.name==name);if(clip==null)return 0;
   float f=Mathf.Clamp01(time)*(clip.samples.Length-1);int i=Mathf.FloorToInt(f),j=Mathf.Min(i+1,clip.samples.Length-1);float t=f-i;
   var a=clip.samples[i];var b=clip.samples[j];
   for(int n=0;n<bones.Length;n++)if(bones[n]!=null)
    bones[n].localRotation=Quaternion.Euler(Mathf.Lerp(a.angles[n],b.angles[n],t),0,0);
   // Preserve the bind location supplied by the Ride/Idle clip and add suspension travel.
   if(bones[0]!=null)bones[0].localPosition+=Vector3.down*(.065f*Mathf.Lerp(a.crouch,b.crouch,t));
   return Mathf.Lerp(a.pitch,b.pitch,t);
  }
 }
}
