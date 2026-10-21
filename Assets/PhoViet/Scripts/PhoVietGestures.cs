using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  bool swipeConsumed;int swipePointerId;int swipeWidth,swipeHeight;
  void CancelSwipe(){swipeCanSteer=false;swipeConsumed=true;dragging=false;}
  // Match the original portrait distances in either orientation (and at every DPR).
  public static int SwipeAction(Vector2 delta,float shortSide)
  {
   if(IsJumpSwipe(delta,shortSide))return 2;
   if(IsBoostSwipe(delta,shortSide))return 3;
   return Mathf.Abs(delta.x)>shortSide*.075f&&Mathf.Abs(delta.x)>=Mathf.Abs(delta.y)?(delta.x>0?1:-1):0;
  }
  void BeginSwipe(Vector2 position,float at,int id)
  {
   swipeStart=position;touchStarted=at;swipePointerId=id;swipeConsumed=false;
   swipeWidth=Screen.width;swipeHeight=Screen.height;
   swipeCanSteer=State==RideState.Riding&&!JobOffersOpen&&GameplayPointerAllowed(position);
  }
  void TriggerSwipe(int action)
  {
   if(!swipeCanSteer||swipeConsumed||swipeWidth!=Screen.width||swipeHeight!=Screen.height||State!=RideState.Riding||JobOffersOpen)return;
   if(action!= -1&&action!=1&&action!=2&&action!=3)return;
   swipeConsumed=true;lastTapTime=-100;
   if(action==2)TryJump();else if(action==3)TryBoost();else MoveLane(action);
  }
  void ContinueSwipe(Vector2 position,bool ended,float at,bool allowTap)
  {
   if(!swipeCanSteer||swipeConsumed)return;
   TriggerSwipe(SwipeAction(position-swipeStart,Mathf.Min(swipeWidth,swipeHeight)));
   if(ended&&!swipeConsumed&&allowTap&&swipeWidth==Screen.width&&swipeHeight==Screen.height&&Vector2.Distance(position,swipeStart)<Mathf.Min(swipeWidth,swipeHeight)*.035f&&at>=touchStarted&&at-touchStarted<.24f)
    RegisterTap(position,at);
  }
  void ReadSwipe()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
   // The DOM pointer bridge owns web gestures. Unity polling can miss a complete
   // touch between frames and its compatibility mouse events must not fire twice.
   return;
#else
   if(Input.touchCount>0)
   {
    var t=Input.GetTouch(0);
    if(t.phase==TouchPhase.Began)BeginSwipe(t.position,Time.unscaledTime,t.fingerId);
    if(t.fingerId!=swipePointerId)return;
    if(t.phase==TouchPhase.Moved||t.phase==TouchPhase.Ended)ContinueSwipe(t.position,t.phase==TouchPhase.Ended,Time.unscaledTime,true);
    if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)swipeCanSteer=false;
   }
   else
   {
    if(Input.GetMouseButtonDown(0)){BeginSwipe(Input.mousePosition,Time.unscaledTime,-1);dragging=true;}
    if(dragging)ContinueSwipe(Input.mousePosition,Input.GetMouseButtonUp(0),Time.unscaledTime,false);
    if(Input.GetMouseButtonUp(0)){dragging=false;swipeCanSteer=false;}
   }
#endif
  }
  [System.Serializable]class BrowserGesture{public int phase,id,action;public float x,y,at;}
  [UnityEngine.Scripting.Preserve]
  public void ApplyBrowserGesture(string json)
  {
   var p=JsonUtility.FromJson<BrowserGesture>(json);
   if(p==null||!Finite(p.x)||!Finite(p.y)||!Finite(p.at)||p.x<0||p.x>1||p.y<0||p.y>1)return;
   var position=new Vector2(p.x*Screen.width,(1-p.y)*Screen.height);
   if(p.phase==0){BeginSwipe(position,p.at,p.id);return;}
   if(p.id!=swipePointerId)return;
   if(p.phase==1)TriggerSwipe(p.action);
   else if(p.phase==2){ContinueSwipe(position,true,p.at,true);swipeCanSteer=false;}
   else if(p.phase==3)swipeCanSteer=false;
  }
  static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
 }
}
