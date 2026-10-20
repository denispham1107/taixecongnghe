using UnityEngine;
namespace PhoViet
{
 // Mantaflow is baked in Blender, never simulated on the phone.
 public sealed class WreckFireVisual:MonoBehaviour
 {
  public Light glow;float clock;PhoVietGame game;
  void OnEnable(){clock=0;game=GetComponentInParent<PhoVietGame>();}
  void LateUpdate()
  {if(game!=null&&game.State==PhoVietGame.RideState.Paused)return;clock+=Time.deltaTime;if(glow!=null)glow.intensity=1.6f+.3f*Mathf.Sin(clock*17)+.15f*Mathf.Sin(clock*31);}
 }
}
