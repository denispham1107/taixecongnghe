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
  readonly Texture2D[] footGlyphs=new Texture2D[4];
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
   Rect opposite=DrivePedalLayout(area,portraitView,true,pixelRatio);
   float size=opposite.width*1.55f*1.20f*1.10f;
   float inset=area.xMax-opposite.xMax;
   return new Rect(area.x+inset,opposite.yMax-size,size,size);
  }
  static bool InsideControl(Rect bounds,Vector2 pixel)=>
   (pixel-bounds.center).sqrMagnitude<=bounds.width*bounds.width*.25f;

  int FootControlAt(Vector2 point)
  {
   Vector2 pixel=new Vector2(point.x,Screen.height-point.y);
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   if(CanExitVehicle&&InsideControl(DriveActionLayout(area,narrow,ExitCarChoice,browserPixelRatio),pixel))return ExitCarChoice;
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
   DrawFootButton(DriveActionLayout(GameGuiArea(),portrait,ExitCarChoice,browserPixelRatio),ExitCarChoice,"NHẢY RA",Hex("F0AE56"),true);
  }
  void DrawFootHud()
  {
   DrawJoystick();
   var area=GameGuiArea();
   if(InsideShop||NearbyShop(ShopDoorReach)!=null)
    DrawFootButton(ExitVehicleLayout(area,portrait,browserPixelRatio),ShopDoorChoice,InsideShop?"RA PHỐ":"VÀO TIỆM",Hex("74DEBB"),!InsideShop||shopEvaded);
   if(InsideShop)return;
   DrawFootButton(DrivePedalLayout(area,portrait,true,browserPixelRatio),FootGrenadeChoice,"LỰU ĐẠN",Hex("F0AE56"),footGrenadeCooldown<=.00001f&&footJumpTime<=0&&footFallRemaining<=0&&entryVehicle==null);
   if(NearbyTakeableVehicle()!=null)
    DrawFootButton(DrivePedalLayout(area,portrait,false,browserPixelRatio),TakeCarChoice,"LẤY XE",Hex("74DEBB"),true);
  }
  void DrawJoystick()
  {
   Rect stick=FootGuiRect(FootJoystickLayout(GameGuiArea(),portrait,browserPixelRatio));
   Panel(stick,new Color(.05f,.13f,.21f,.75f),stick.width*.5f);
   float ring=stick.width*.12f;
   Panel(new Rect(stick.x+ring,stick.y+ring,stick.width-2*ring,stick.height-2*ring),new Color(.19f,.35f,.49f,.53f),stick.width*.5f);
   float knob=stick.width*.39f;
   Vector2 centre=stick.center+new Vector2(footStick.x,-footStick.y)*stick.width*.29f;
   Panel(new Rect(centre.x-knob*.5f,centre.y-knob*.5f,knob,knob),Hex("72C8F0"),knob*.5f);
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
   else if(Event.current.type==EventType.Repaint)
   {
    int glyph=choice==ExitCarChoice?0:choice==TakeCarChoice?3:InsideShop?2:1;
    var texture=footGlyphs[glyph]??(footGlyphs[glyph]=CreateFootGlyph(glyph));
    GUI.DrawTexture(new Rect(r.x+size*.21f,r.y+size*.12f,size*.58f,size*.52f),texture,
     ScaleMode.ScaleToFit,true,0,enabled?Cream:tint,0,0);
   }
   Txt(text,r.x,r.y+size*.65f,size,size*.19f,Mathf.Max(8,Mathf.RoundToInt(size*.125f)),Cream,TextAnchor.MiddleCenter);
   bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
   if(enabled&&clicked&&Time.unscaledTime>=hudTouchSuppressUntil&&InsideControl(r,Event.current.mousePosition))HandleHudChoice(choice);
#endif
  }
  static Texture2D CreateFootGlyph(int glyph)
  {
   const int size=96;
   var pixels=new Color32[size*size];
   void Stroke(float ax,float ay,float bx,float by,float radius=.044f)
   {
    var a=new Vector2(ax,ay);
    var segment=new Vector2(bx,by)-a;
    float lengthSquared=segment.sqrMagnitude;
    for(int y=0;y<size;y++)for(int x=0;x<size;x++)
    {
     Vector2 delta=new Vector2((x+.5f)/size,1-(y+.5f)/size)-a;
     float along=lengthSquared>0?Mathf.Clamp01(Vector2.Dot(delta,segment)/lengthSquared):0;
     var index=y*size+x;
     byte opacity=(byte)Mathf.RoundToInt(255*Mathf.Clamp01((radius-(delta-segment*along).magnitude)*size+.5f));
     if(opacity>pixels[index].a)pixels[index]=new Color32(255,255,255,opacity);
    }
   }
   void Arrow(float start,float y,float tipX,float tipY)
   {
    Stroke(start,y,tipX,tipY,.051f);
    var back=(new Vector2(start,y)-new Vector2(tipX,tipY)).normalized;
    var wing=new Vector2(-back.y,back.x)*.115f;
    var basePoint=new Vector2(tipX,tipY)+back*.15f;
    Stroke(tipX,tipY,basePoint.x+wing.x,basePoint.y+wing.y);
    Stroke(tipX,tipY,basePoint.x-wing.x,basePoint.y-wing.y);
   }
   if(glyph==0||glyph==3)
   {
    Stroke(.09f,.75f,.09f,.61f);
    Stroke(.09f,.61f,.22f,.55f);
    Stroke(.22f,.55f,.33f,.40f);
    Stroke(.33f,.40f,.55f,.40f);
    Stroke(.55f,.40f,.68f,.55f);
    Stroke(.68f,.55f,.77f,.60f);
    Stroke(.77f,.60f,.77f,.75f);
    Stroke(.09f,.75f,.77f,.75f);
    Stroke(.24f,.79f,.24f,.79f,.086f);
    Stroke(.63f,.79f,.63f,.79f,.086f);
    if(glyph==0)Arrow(.54f,.34f,.88f,.10f);
    else Arrow(.93f,.12f,.60f,.30f);
   }
   else
   {
    Stroke(.19f,.88f,.19f,.12f);
    Stroke(.19f,.12f,.59f,.12f);
    Stroke(.59f,.12f,.59f,.34f);
    Stroke(.19f,.88f,.59f,.88f);
    Stroke(.59f,.88f,.59f,.66f);
    if(glyph==1)Arrow(.88f,.5f,.39f,.5f);
    else Arrow(.39f,.5f,.88f,.5f);
   }
   var texture=new Texture2D(size,size,TextureFormat.RGBA32,false)
    {name="Foot HUD glyph "+glyph,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   texture.SetPixels32(pixels);
   texture.Apply(false,true);
   return texture;
  }

  void CancelFootPointer(){footPointer=int.MinValue;footStick=footMove=driveInputAtTurn=Vector2.zero;}
  bool acceptsJoystick=>!PortraitGameBlocked&&(State==RideState.OnFoot&&entryVehicle==null||
   State==RideState.Riding&&IsPlayerCar&&!JobOffersOpen&&!ServiceStopped);
  bool JoystickAt(Vector2 point)=>acceptsJoystick&&InsideControl(
   FootJoystickLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio),new Vector2(point.x,Screen.height-point.y));

  void UpdateFootPointer(int id,int phase,Vector2 point)
  {
   if(id==footPointer&&phase>=2){CancelFootPointer();return;}
   if(!acceptsJoystick)return;
   var joystick=FootJoystickLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio);
   Vector2 pixel=new Vector2(point.x,Screen.height-point.y);
   if(phase==0)
   {
    if(footPointer!=int.MinValue||!InsideControl(joystick,pixel))return;
    footPointer=id;
   }
   if(id!=footPointer)return;
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
   footMove=Vector2.ClampMagnitude(DirectionalKeys(true)+footStick,1);
   if(Input.GetKeyDown(KeyCode.Q))TryThrowFootGrenade();
   if(Input.GetKeyDown(KeyCode.E))
   {
    if(InsideShop)TryLeaveShop();
    else if(!TryEnterShop())TryTakeVehicle();
   }
  }
  static Vector2 DirectionalKeys(bool wasd)=>new Vector2(
   (Input.GetKey(KeyCode.RightArrow)||wasd&&Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.LeftArrow)||wasd&&Input.GetKey(KeyCode.A)?1:0),
   (Input.GetKey(KeyCode.UpArrow)||wasd&&Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.DownArrow)||wasd&&Input.GetKey(KeyCode.S)?1:0));
 }
}
