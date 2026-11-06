using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  // Temporarily hidden by the user (21.142). Keep the implementation for an explicit re-enable.
  public static bool BoostReleased=>false;
  public bool BoostActive=>BoostReleased&&boostRemaining>0;
  public float BoostRemaining=>boostRemaining;
  public float BoostCooldown=>boostCooldown;
  public const float BoostMultiplier=2.25f,BoostDuration=4,BoostRecovery=2;
  float boostRemaining,boostCooldown;
  BoostSmokeVisual boostSmoke;
  public static bool IsBoostSwipe(Vector2 delta,float width)=>delta.y< -width*.12f&&-delta.y>=Mathf.Abs(delta.x)*SwipeVerticalRatio;
  public bool TryBoost()
  {
   if(!BoostReleased||State!=RideState.Riding||ServiceStopped||BoostActive||boostCooldown>0)return false;
   CancelAssistedKick(false);boostRemaining=BoostDuration;
   notice="TĂNG TỐC +125% · 4 GIÂY";noticeTimer=1.3f;
   EnsureBoostSmoke();return true;
  }
  void EnsureBoostSmoke()
  {
   if(boostSmoke!=null||rider==null)return;
   boostSmoke=rider.GetComponent<BoostSmokeVisual>()??rider.gameObject.AddComponent<BoostSmokeVisual>();boostSmoke.Initialize(this);
  }
  void ResetBoost()
  {boostRemaining=boostCooldown=0;if(boostSmoke!=null)boostSmoke.Clear();}
  void CancelBoost()
  {
   if(BoostActive){boostRemaining=0;boostCooldown=BoostRecovery;}
   if(boostSmoke!=null)boostSmoke.Clear();
  }
  void TickBoost(float dt)
  {
   if(!BoostReleased){if(boostRemaining>0||boostCooldown>0)ResetBoost();return;}
   if(State!=RideState.Riding&&State!=RideState.Falling)return;
   if(ServiceStopped||State!=RideState.Riding){CancelBoost();boostCooldown=Mathf.Max(0,boostCooldown-dt);return;}
   if(boostRemaining>0)
   {
    float after=dt-boostRemaining;
    boostRemaining=Mathf.Max(0,boostRemaining-dt);
    if(boostRemaining==0)boostCooldown=Mathf.Max(0,BoostRecovery-Mathf.Max(0,after));
   }
   else boostCooldown=Mathf.Max(0,boostCooldown-dt);
  }
 }
}
