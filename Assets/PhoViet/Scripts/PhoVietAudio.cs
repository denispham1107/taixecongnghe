using UnityEngine;
using UnityEngine.Audio;
namespace PhoViet
{
 // One volume-only master bus: preserves each source's balance, fades and mute.
 // AudioMixer group volume is supported by Unity's Web Audio backend.
 public static class PhoVietAudio
 {
  public const float MasterGain=1.3f;
  public const string MixerResource="Audio/PV_Master",VolumeParameter="MasterVolume";
  public static float MasterDecibels=>20*Mathf.Log10(MasterGain);
  static AudioMixerGroup output;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void Reset(){output=null;}
  public static AudioSource Route(AudioSource source)
  {
   if(source==null)return null;
   if(output==null)
   {
    var mixer=Resources.Load<AudioMixer>(MixerResource);
    if(mixer!=null){var groups=mixer.FindMatchingGroups("Master");if(groups.Length>0)output=groups[0];}
   }
   source.outputAudioMixerGroup=output;return source;
  }
 }
}
