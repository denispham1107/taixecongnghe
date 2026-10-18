using UnityEngine;
namespace PhoViet
{
 public sealed class PassengerVisualAnimator:MonoBehaviour
 {
  public Animation animationPlayer;
  public void Pose(string clip,float normalized)
  {
   if(animationPlayer==null)return;var state=animationPlayer[clip];if(state==null)return;
   if(!animationPlayer.IsPlaying(clip))animationPlayer.Play(clip);
   state.enabled=true;state.weight=1;state.speed=0;state.wrapMode=WrapMode.ClampForever;state.time=Mathf.Clamp01(normalized)*state.length;animationPlayer.Sample();
  }
 }
}
