using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  Texture2D playerMissileHudIcon;
  public Rect PlayerMissileButtonPixels()
  {
   return PlayerMissileButtonLayout(GameGuiArea(),Screen.width<Screen.height,browserPixelRatio);
  }
  public static Rect PlayerMissileButtonLayout(Rect area,bool narrow,float pixelRatio=1)
  {
   float scale=narrow?area.width/480f:LandscapeHudScale(area);
   // Keep a real56CSS-pixel touch target on short landscape phones, including
   // dense WebGL buffers; this does not change the game's render resolution.
   // Góc dưới bên trái theo yêu cầu người dùng (21.134), không còn ở bên phải.
   float size=Mathf.Max(76*scale,56*pixelRatio);return new Rect(area.x+18*scale,area.yMax-size-24*scale,size,size);
  }
  bool PlayerMissileButtonAt(Vector2 position)
  {
   if(State!=RideState.Riding&&State!=RideState.Paused)return false;
   var r=PlayerMissileButtonPixels();var p=new Vector2(position.x,Screen.height-position.y);
   return (p-r.center).sqrMagnitude<=r.width*r.width*.25f;
  }
  void EnsurePlayerMissileHudIcon(){if(playerMissileHudIcon==null)playerMissileHudIcon=Resources.Load<Texture2D>("PlayerMissile/PV_MissileHud");}
  void DrawPlayerMissileAmmo(Rect r)
  {
   EnsurePlayerMissileHudIcon();if(playerMissileHudIcon!=null)GUI.DrawTexture(new Rect(r.x,r.y,20,20),playerMissileHudIcon,ScaleMode.ScaleToFit,true);
   Txt(PlayerMissiles+" / "+MaximumPlayerMissiles,r.x+26,r.y+1,r.width-26,20,13,PlayerMissiles>0?StreetArt.Cream:StreetArt.Gold);
  }
  void DrawPlayerMissileButton()
  {
   if(State!=RideState.Riding&&State!=RideState.Paused)return;
   EnsurePlayerMissileHudIcon();var pixels=PlayerMissileButtonPixels();var area=GameGuiArea();
   var r=new Rect((pixels.x-area.x)/uiScale,(pixels.y-area.y)/uiScale,pixels.width/uiScale,pixels.height/uiScale);
   bool ready=PlayerCanFire;
   Panel(r,new Color(.25f,.72f,1,ready?.8f:.38f),r.width*.5f);
   var inner=new Rect(r.x+2,r.y+2,r.width-4,r.height-4);Panel(inner,new Color(.025f,.12f,.22f,ready?.87f:.67f),inner.width*.5f);
   var old=GUI.color;GUI.color=ready?Color.white:new Color(.64f,.72f,.8f,.75f);
   if(playerMissileHudIcon!=null)GUI.DrawTexture(new Rect(r.x+r.width*.23f,r.y+r.height*.13f,r.width*.54f,r.height*.54f),playerMissileHudIcon,ScaleMode.ScaleToFit,true);GUI.color=old;
   Txt(PlayerMissiles+"/"+MaximumPlayerMissiles,r.x,r.y+r.height*.63f,r.width,r.height*.25f,Mathf.RoundToInt(r.height*.17f),StreetArt.Cream,TextAnchor.MiddleCenter);
   bool click=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
   if(click&&Time.unscaledTime>=hudTouchSuppressUntil&&(Event.current.mousePosition-r.center).sqrMagnitude<=r.width*r.width*.25f)TryFirePlayerMissile();
#endif
  }
  void HandleHudChoice(int choice)
  {if(choice==0)TogglePause();else if(choice==1)sound=!sound;else if(choice==2){lastTapTime=-100;TryFirePlayerMissile();}}
 }
}
