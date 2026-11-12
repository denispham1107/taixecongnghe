using System.Runtime.InteropServices;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  const int ExitCarChoice=6,FootGrenadeChoice=7,TakeCarChoice=8,ShopDoorChoice=9;
  int footPointer=int.MinValue;
  Vector2 footStick;
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void PhoVietSetFootMode(int enabled);
#endif
  static void NotifyFootBrowser(bool enabled)
  {
#if UNITY_WEBGL && !UNITY_EDITOR
   PhoVietSetFootMode(enabled?1:0);
#endif
  }

  public static Rect ExitVehicleLayout(Rect area,bool portraitView,float pixelRatio=1)
  {
   var gas=DrivePedalLayout(area,portraitView,false,pixelRatio);
   gas.y-=gas.height*1.12f;
   return gas;
  }
  public static Rect FootJoystickLayout(Rect area,bool portraitView,float pixelRatio=1)
  {
   Rect reference=PlayerMissileButtonLayout(area,portraitView,pixelRatio);
   float size=reference.width*1.55f;
   return new Rect(reference.x,reference.yMax-size,size,size);
  }
  static bool InsideControl(Rect bounds,Vector2 pixel)=>
   (pixel-bounds.center).sqrMagnitude<=bounds.width*bounds.width*.25f;

  int FootControlAt(Vector2 point)
  {
   Vector2 pixel=new Vector2(point.x,Screen.height-point.y);
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   if(CanExitVehicle&&InsideControl(ExitVehicleLayout(area,narrow,browserPixelRatio),pixel))return ExitCarChoice;
   if(State!=RideState.OnFoot)return -1;
   if(InsideControl(ExitVehicleLayout(area,narrow,browserPixelRatio),pixel)&&
      (InsideShop?shopEvaded:NearbyShop(ShopDoorReach)!=null))return ShopDoorChoice;
   if(InsideShop)return -1;
   if(InsideControl(DrivePedalLayout(area,narrow,true,browserPixelRatio),pixel))return FootGrenadeChoice;
   return NearbyTakeableVehicle()!=null&&InsideControl(DrivePedalLayout(area,narrow,false,browserPixelRatio),pixel)?TakeCarChoice:-1;
  }

  void DrawExitVehicleHud()
  {
   if(!CanExitVehicle)return;
   DrawFootButton(ExitVehicleLayout(GameGuiArea(),portrait,browserPixelRatio),ExitCarChoice,"NHẢY RA",Hex("F0AE56"),true);
  }
  void DrawFootHud()
  {
   var area=GameGuiArea();Rect stick=FootGuiRect(FootJoystickLayout(area,portrait,browserPixelRatio));
   Panel(stick,new Color(.05f,.13f,.21f,.75f),stick.width*.5f);
   float ring=stick.width*.12f;
   Panel(new Rect(stick.x+ring,stick.y+ring,stick.width-2*ring,stick.height-2*ring),new Color(.19f,.35f,.49f,.53f),stick.width*.5f);
   float knob=stick.width*.39f;
   Vector2 centre=stick.center+new Vector2(footStick.x,-footStick.y)*stick.width*.29f;
   Panel(new Rect(centre.x-knob*.5f,centre.y-knob*.5f,knob,knob),Hex("72C8F0"),knob*.5f);
   if(InsideShop||NearbyShop(ShopDoorReach)!=null)
    DrawFootButton(ExitVehicleLayout(area,portrait,browserPixelRatio),ShopDoorChoice,InsideShop?"RA PHỐ":"VÀO TIỆM",Hex("74DEBB"),!InsideShop||shopEvaded);
   if(InsideShop)return;
   DrawFootButton(DrivePedalLayout(area,portrait,true,browserPixelRatio),FootGrenadeChoice,"LỰU ĐẠN",Hex("F0AE56"),grenadeCooldown<.001f&&footJumpTime<=0);
   if(NearbyTakeableVehicle()!=null)
    DrawFootButton(DrivePedalLayout(area,portrait,false,browserPixelRatio),TakeCarChoice,"LẤY XE",Hex("74DEBB"),true);
  }
  Rect FootGuiRect(Rect r)
  {
   var area=GameGuiArea();return new Rect((r.x-area.x)/uiScale,(r.y-area.y)/uiScale,r.width/uiScale,r.height/uiScale);
  }
  void DrawFootButton(Rect pixels,int choice,string text,Color accent,bool enabled)
  {
   var r=FootGuiRect(pixels);float size=r.width;
   Panel(r,new Color(.04f,.10f,.18f,.93f),size*.5f);
   var ring=new Rect(r.x+size*.05f,r.y+size*.05f,size*.9f,size*.9f);
   Color tint=enabled?accent:Color.Lerp(accent,Hex("486178"),.65f);
   Panel(ring,tint,size*.5f);
   Panel(new Rect(r.x+size*.10f,r.y+size*.10f,size*.80f,size*.80f),Hex("142D47"),size*.5f);
   if(choice==FootGrenadeChoice)
   {
    Panel(new Rect(r.x+size*.36f,r.y+size*.31f,size*.26f,size*.31f),tint,size*.16f);
    Panel(new Rect(r.x+size*.43f,r.y+size*.22f,size*.14f,size*.12f),Cream,2);
   }
   else
   {
    StrokeIcon(r,new Vector2(.31f,.26f),new Vector2(.31f,.58f),tint);
    StrokeIcon(r,new Vector2(.31f,.26f),new Vector2(.53f,.26f),tint);
    StrokeIcon(r,new Vector2(.31f,.58f),new Vector2(.52f,.58f),tint);
    bool leaving=choice==ExitCarChoice||choice==ShopDoorChoice&&InsideShop;
    float from=leaving?.43f:.70f,to=leaving?.70f:.43f;
    StrokeIcon(r,new Vector2(from,.43f),new Vector2(to,.43f),Cream);
    float arm=leaving?-.09f:.09f;
    StrokeIcon(r,new Vector2(to,.43f),new Vector2(to+arm,.35f),Cream);
    StrokeIcon(r,new Vector2(to,.43f),new Vector2(to+arm,.51f),Cream);
   }
   Txt(text,r.x,r.y+size*.65f,size,size*.19f,Mathf.Max(8,Mathf.RoundToInt(size*.125f)),Cream,TextAnchor.MiddleCenter);
   bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
   if(enabled&&clicked&&Time.unscaledTime>=hudTouchSuppressUntil&&InsideControl(r,Event.current.mousePosition))HandleHudChoice(choice);
#endif
  }
  void StrokeIcon(Rect r,Vector2 a,Vector2 b,Color color)
  {
   a=r.position+a*r.width;b=r.position+b*r.width;
   Matrix4x4 matrix=GUI.matrix;
   GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
   Panel(new Rect(a.x,a.y-r.width*.018f,(b-a).magnitude,r.width*.036f),color,2);
   GUI.matrix=matrix;
  }

  void CancelFootPointer(){footPointer=int.MinValue;footStick=footMove=Vector2.zero;}
  void UpdateFootPointer(int id,int phase,Vector2 point)
  {
   if(State!=RideState.OnFoot||entryVehicle!=null)return;
   var joystick=FootJoystickLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio);
   Vector2 pixel=new Vector2(point.x,Screen.height-point.y);
   if(phase==0)
   {
    if(footPointer!=int.MinValue||!InsideControl(joystick,pixel))return;
    footPointer=id;
   }
   if(id!=footPointer)return;
   if(phase>=2){CancelFootPointer();return;}
   Vector2 delta=pixel-joystick.center;
   footStick=Vector2.ClampMagnitude(new Vector2(delta.x,-delta.y)/(joystick.width*.34f),1);
  }
  [UnityEngine.Scripting.Preserve]
  public void ApplyFootPointer(string json)
  {
   var input=JsonUtility.FromJson<BrowserGesture>(json);
   if(input==null||!Finite(input.x)||!Finite(input.y)||input.x<0||input.x>1||input.y<0||input.y>1)return;
   UpdateFootPointer(input.id,input.phase,new Vector2(input.x*Screen.width,(1-input.y)*Screen.height));
  }
  void ReadFootInput()
  {
#if !UNITY_WEBGL || UNITY_EDITOR
   for(int i=0;i<Input.touchCount;i++)
   {
    var t=Input.GetTouch(i);
    int phase=t.phase==TouchPhase.Began?0:t.phase==TouchPhase.Ended?2:t.phase==TouchPhase.Canceled?3:1;
    bool joystick=t.fingerId==footPointer;
    UpdateFootPointer(t.fingerId,phase,t.position);
    if(phase==2&&!joystick&&t.deltaPosition.sqrMagnitude<64)
    {
     int choice=HudButtonAt(t.position);if(choice>=0)HandleHudChoice(choice);
    }
    hudTouchSuppressUntil=Time.unscaledTime+.6f;
   }
   if(Input.touchCount==0&&Time.unscaledTime>=hudTouchSuppressUntil)
   {
    if(Input.GetMouseButtonDown(0))UpdateFootPointer(-1,0,Input.mousePosition);
    else if(Input.GetMouseButton(0))UpdateFootPointer(-1,1,Input.mousePosition);
    else UpdateFootPointer(-1,2,Input.mousePosition);
   }
#endif
   Vector2 keys=new Vector2(
    (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),
    (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
   footMove=Vector2.ClampMagnitude(keys+footStick,1);
   if(Input.GetKeyDown(KeyCode.Q))TryThrowFootGrenade();
   if(Input.GetKeyDown(KeyCode.E))
   {
    if(InsideShop)TryLeaveShop();
    else if(!TryEnterShop())TryTakeVehicle();
   }
  }
 }
}
