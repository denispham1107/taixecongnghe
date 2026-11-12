using System;
using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const float ShopHideSeconds=3f;
  const float ShopCameraZoom=.70f;
  const float ShopDoorReach=1.45f,ShopGuideRange=96f,ShopMarkerRange=36f;
  static readonly string[] ShopGuideArrows={"→","↘","↓","↙","←","↖","↑","↗"};
  static readonly Vector3 ShopInteriorOrigin=new Vector3(500,0,0);
  readonly Dictionary<Transform,List<Transform>> shopDoorsByTile=new Dictionary<Transform,List<Transform>>();
  Transform shopDoor,shopInterior,shopTow;
  float shopHideElapsed;
  bool shopEvaded;
  Vector3 shopTowCarStart,shopTowPoliceStart,shopTowHeading;
  Quaternion shopTowCarRotation;

  public bool InsideShop=>shopDoor!=null;
  public bool EvadedPoliceInShop=>shopEvaded;
  public float ShopHideRemaining=>InsideShop&&!shopEvaded?Mathf.Max(0,ShopHideSeconds-shopHideElapsed):0;
  bool AbandonedCarPresent=>rider!=null&&rider.gameObject.activeSelf;
  Vector3 FootStreetPosition=>InsideShop?ShopApproach(shopDoor):footActor.localPosition;
  const string ShopModelPath="ShopHideout/PV_ShopInterior";
  const string TowModelPath="ShopHideout/PV_PoliceTow";

  void ResetShopHideout()
  {
   shopDoor=null;shopHideElapsed=0;shopEvaded=false;
   if(shopInterior!=null)shopInterior.gameObject.SetActive(false);
   if(shopTow!=null)shopTow.gameObject.SetActive(false);
  }

  List<Transform> ShopDoors(Transform tile)
  {
   if(shopDoorsByTile.TryGetValue(tile,out var doors))return doors;
   doors=new List<Transform>();
   foreach(Transform child in tile)
    if(child.name.Equals("Blender • TẠP HÓA",StringComparison.Ordinal)||
       child.name.Equals("TẠP HÓA",StringComparison.Ordinal))doors.Add(child);
   shopDoorsByTile.Add(tile,doors);
   return doors;
  }

  Vector3 ShopApproach(Transform door)
  {
   if(door.name.StartsWith("Blender • ",StringComparison.Ordinal))
    return transform.InverseTransformPoint(door.TransformPoint(new Vector3(.45f,0,-4.47f)));
   var tilePosition=door.localPosition;
   return transform.InverseTransformPoint(door.parent.TransformPoint(
    new Vector3(Mathf.Sign(tilePosition.x)*8.73f,0,tilePosition.z)));
  }

  Vector3 ShopInward(Transform door)
  {
   if(door.name.StartsWith("Blender • ",StringComparison.Ordinal))
    return transform.InverseTransformDirection(door.forward);
   return transform.InverseTransformDirection(door.parent.right*Mathf.Sign(door.localPosition.x));
  }

  Transform NearbyShop(float reach,bool forGuide=false)
  {
   if(!PlayerOnFoot||InsideShop||shopEvaded||(!forGuide&&footJumpTime>0)||entryVehicle!=null||(!AbandonedCarPresent&&!riderWreckExploded))return null;
   Transform nearest=null;
   float best=reach*reach;
   if(RoutesReady)
   {
    foreach(var row in roadRows)
    {
     if(!row.root.gameObject.activeInHierarchy)continue;
     ScanShopTiles(row.tiles,footActor.localPosition,reach,ref best,ref nearest);
    }
   }
   else ScanShopTiles(blocks,footActor.localPosition,reach,ref best,ref nearest);
   return nearest;
  }

  void ScanShopTiles(List<Transform> tiles,Vector3 player,float reach,ref float best,ref Transform nearest)
  {
   Vector3 world=transform.TransformPoint(player);
   foreach(var tile in tiles)
   {
    Vector3 local=tile.InverseTransformPoint(world);
    if(Mathf.Abs(local.x)>9+reach||local.z< -reach||local.z>58+reach)continue;
    foreach(var door in ShopDoors(tile))
    {
     if(!door.gameObject.activeInHierarchy)continue;
     Vector3 approach=ShopApproach(door);
     float distance=Flat(approach-player).sqrMagnitude;
     if(distance>=best||OverRiver(approach))continue;
     nearest=door;best=distance;
    }
   }
  }

  public bool TryEnterShop()
  {
   if(footFallRemaining>0)return false;
   Transform door=NearbyShop(ShopDoorReach);
   return door!=null&&EnterShop(door);
  }

  bool TryWalkIntoShop(Vector3 previous,Vector3 next)
  {
   Transform door=NearbyShop(.95f);
   if(door==null)return false;
   Vector3 approach=ShopApproach(door),inward=ShopInward(door),right=Vector3.Cross(Vector3.up,inward);
   return Vector3.Dot(next-previous,inward)>.001f&&
          Mathf.Abs(Vector3.Dot(next-approach,right))<.64f&&
          Vector3.Dot(next-approach,inward)>.08f&&EnterShop(door);
  }

  bool EnterShop(Transform door)
  {
   if(!EnsureShopInterior())return false;
   shopDoor=door;shopHideElapsed=0;shopInterior.gameObject.SetActive(true);
   shopTowCarStart=rider.localPosition;
   shopTowCarRotation=rider.localRotation;
   shopTowHeading=Quaternion.Euler(0,rider.localEulerAngles.y,0)*Vector3.forward;
   if(policeBike!=null)shopTowPoliceStart=policeBike.localPosition;
   ResetCrashPoliceNavigation();HidePoliceOfficer();siren?.Stop();
   CancelFootPointer();footFacing=Vector3.forward;
   footActor.SetLocalPositionAndRotation(ShopInteriorOrigin+Vector3.forward*2.25f,Quaternion.identity);
   PoseFootCharacter(0,false);
   PositionShopCamera(true,0);
   return true;
  }

  bool EnsureShopInterior()
  {
   if(shopInterior!=null)return true;
   var prefab=Resources.Load<GameObject>(ShopModelPath);
   if(prefab==null)return false;
   shopInterior=Instantiate(prefab,transform,false).transform;
   shopInterior.name="Tạp hóa phố · nội thất Blender";
   shopInterior.localPosition=ShopInteriorOrigin;
   Text(shopInterior,"Bảng tạp hóa","TẠP HÓA PHỐ",new Vector3(0,2.46f,8.13f),.28f,Hex("FFF0CD"));
   foreach(float z in new[]{2.45f,5.15f,7.35f})
   {
    var lamp=Group("Ánh đèn cửa hàng",shopInterior,new Vector3(0,2.94f,z)).gameObject.AddComponent<Light>();
    lamp.type=LightType.Point;lamp.color=Hex("FFF0D2");lamp.intensity=1.45f;lamp.range=5.4f;
   }
   shopInterior.gameObject.SetActive(false);
   return true;
  }

  void TickShopHideout(float dt)
  {
   if(!shopEvaded)
   {
    shopHideElapsed=Mathf.Min(ShopHideSeconds,shopHideElapsed+dt);
    TickShopTow();
    if(shopHideElapsed>=ShopHideSeconds-.00001f)FinishShopEscape();
   }
   Vector3 p=footActor.localPosition-ShopInteriorOrigin;
   Vector3 move=new Vector3(footMove.x,0,footMove.y);
   Vector3 next=p+move*(FootSpeed*.63f*dt);
   if(shopEvaded&&next.z<.76f&&Mathf.Abs(next.x)<1.08f&&move.z<0&&TryLeaveShop())return;
   bool CanStep(Vector3 q) =>
    Mathf.Abs(q.x)<=1.94f&&q.z>=.72f&&q.z<=7.90f&&
    !(q.x>.70f&&q.z>3.22f&&q.z<5.70f)&&
    !(q.x<-.25f&&q.z>5.70f&&q.z<7.12f);
   if(CanStep(next))footActor.localPosition=ShopInteriorOrigin+next;
   else if(CanStep(new Vector3(next.x,0,p.z)))footActor.localPosition=ShopInteriorOrigin+new Vector3(next.x,0,p.z);
   else if(CanStep(new Vector3(p.x,0,next.z)))footActor.localPosition=ShopInteriorOrigin+new Vector3(p.x,0,next.z);
   if(move.sqrMagnitude>.005f)
    footActor.localRotation=Quaternion.RotateTowards(footActor.localRotation,Quaternion.LookRotation(move),dt*760);
   PoseFootCharacter(dt,(footActor.localPosition-ShopInteriorOrigin-p).sqrMagnitude>.000001f);
  }

  void TickShopTow()
  {
   float t=shopHideElapsed;
   if(policeBike!=null&&policeBike.gameObject.activeSelf&&!PoliceInRiver)
    policeBike.localPosition=shopTowPoliceStart+policeBike.localRotation*Vector3.forward*(t*7.5f);
   if(!AbandonedCarPresent)return;
   if(shopTow==null)
   {
    var prefab=Resources.Load<GameObject>(TowModelPath);
    if(prefab==null)return;
    shopTow=Instantiate(prefab,transform,false).transform;
    shopTow.name="Xe kéo của CSGT · Blender";
   }
   Vector3 ground=new Vector3(shopTowCarStart.x,0,shopTowCarStart.z);
   Quaternion heading=Quaternion.LookRotation(shopTowHeading);
   float bedLengthScale=Mathf.Max(1,PlayerHalfLength*2/4.25f);
   float bedOffset=PlayerHalfLength+2.1f;
   float approach=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.80f,t));
   float load=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.80f,1.65f,t));
   float departure=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.65f,ShopHideSeconds,t))*14;
   shopTow.gameObject.SetActive(true);
   shopTow.localScale=new Vector3(Mathf.Max(1,PlayerHalfWidth*2/2.22f),1,bedLengthScale);
   shopTow.SetLocalPositionAndRotation(ground+shopTowHeading*(bedOffset+7*(1-approach)+departure),heading);
   rider.localPosition=Vector3.Lerp(shopTowCarStart,ground+Vector3.up*.88f+shopTowHeading*(bedOffset-.72f*bedLengthScale),load)+shopTowHeading*departure;
   rider.localRotation=Quaternion.Slerp(shopTowCarRotation,heading,load);
  }

  void FinishShopEscape()
  {
   shopEvaded=true;IsChased=false;CrashDuringPursuit=false;PoliceSpeed=PoliceDistance=0;
   routePolice=false;chaseBoost=0;policeReplacementDelay=policeTakeoverRemaining=0;
   HidePoliceOfficer();ResetCrashPoliceNavigation();ResetPoliceDriving();siren?.Stop();
   if(policeBike!=null)policeBike.gameObject.SetActive(false);
   if(shopTow!=null)shopTow.gameObject.SetActive(false);
   rider.gameObject.SetActive(false);
  }

  public bool TryLeaveShop()
  {
   if(!PlayerOnFoot||!InsideShop||!shopEvaded)return false;
   Vector3 outside=ShopApproach(shopDoor),inward=ShopInward(shopDoor);
   Vector3 side=Vector3.Cross(Vector3.up,inward);
   if(!FootTrafficClear(outside,null)&&FootTrafficClear(outside+side*.75f,null))outside+=side*.75f;
   shopInterior.gameObject.SetActive(false);shopDoor=null;CancelFootPointer();
   footFacing=-inward;
   footActor.SetLocalPositionAndRotation(outside,Quaternion.LookRotation(footFacing));
   footFrameStart=outside;
   footRearCarTimer=0;
   PositionFootCamera(true,0);
   notice="XE ĐÃ BỊ KÉO · LẤY XE KHÁC ĐỂ ĐI TIẾP";noticeTimer=4;
   return true;
  }

  void PositionShopCamera(bool snap,float dt)
  {
   Vector3 p=footActor.localPosition-ShopInteriorOrigin;
   bool narrow=Screen.width<Screen.height;
   Vector3 target=ShopInteriorOrigin+new Vector3(p.x*.38f,narrow?2.98f:2.85f,Mathf.Max(.32f,p.z-3.65f));
   float blend=snap?1:1-Mathf.Exp(-dt*8);
   rideCamera.transform.localPosition=Vector3.Lerp(rideCamera.transform.localPosition,target,blend);
   Vector3 aim=ShopInteriorOrigin+new Vector3(p.x*.7f,1.14f,Mathf.Max(p.z+.9f,target.z-ShopInteriorOrigin.z+2.8f));
   rideCamera.transform.rotation=Quaternion.Slerp(rideCamera.transform.rotation,
    Quaternion.LookRotation(transform.TransformPoint(aim)-rideCamera.transform.position),blend);
   float halfAngle=(narrow?87:76)*Mathf.Deg2Rad*.5f;
   float zoomedFov=2*Mathf.Atan(Mathf.Tan(halfAngle)/ShopCameraZoom)*Mathf.Rad2Deg;
   rideCamera.fieldOfView=Mathf.Lerp(rideCamera.fieldOfView,zoomedFov,blend);
  }

  void DrawShopDoorMarker()
  {
   if(Event.current.type!=EventType.Repaint)return;
   Transform door=NearbyShop(ShopGuideRange,true);
   if(door==null||rideCamera==null)return;
   Vector3 approach=ShopApproach(door);
   if(Flat(approach-footActor.localPosition).sqrMagnitude>ShopMarkerRange*ShopMarkerRange)return;
   var marker=rideCamera.WorldToScreenPoint(transform.TransformPoint(approach+Vector3.up*1.65f));
   var area=GameGuiArea();
   if(marker.z<=0||!area.Contains(new Vector2(marker.x,Screen.height-marker.y)))return;
   float x=(marker.x-area.x)/uiScale,y=(Screen.height-marker.y-area.y)/uiScale;
   float hudScale=portrait?uiScale:LandscapeHudScale(area);
   float unit=hudScale/uiScale;
   if(y<(portrait?PortraitNoticeY:126)*unit)return;
   Color mint=Hex("A5FFCC");
   float left=Mathf.Clamp(x-65*unit,2*unit,uiW-132*unit);
   var guide=FootEscapeGuideRect(uiW*uiScale/hudScale);
   var guideInCurrentUnits=new Rect(guide.position*unit,guide.size*unit);
   if(new Rect(left,y-36*unit,130*unit,47*unit).Overlaps(guideInCurrentUnits))return;
   Panel(new Rect(left,y-36*unit,130*unit,28*unit),mint,6*unit);
   Panel(new Rect(left+2*unit,y-34*unit,126*unit,24*unit),Hex("103E34"),4*unit);
   Txt("TRỐN CSGT",left,y-35*unit,130*unit,26*unit,Mathf.RoundToInt(14*unit),mint,TextAnchor.MiddleCenter);
   DrawShopGuideArrow(new Rect(x-10*unit,y-9*unit,20*unit,20*unit),Vector2.up,mint);
  }

  static Vector2 ShopGuideBearing(Vector3 displacement,Vector3 cameraRight,Vector3 cameraForward)
  {
   Vector2 flat=Flat(displacement);
   return new Vector2(Vector2.Dot(flat,Flat(cameraRight).normalized),
                      -Vector2.Dot(flat,Flat(cameraForward).normalized)).normalized;
  }

  Rect FootEscapeGuideRect(float width)
  {
   float w=portrait?270:286,y=portrait?PortraitNoticeY+PortraitNoticeHeight+4:130;
   return new Rect((width-w)*.5f,y,w,52);
  }

  void DrawFootEscapeGuide()
  {
   if(Event.current.type!=EventType.Repaint)return;
   Rect card=FootEscapeGuideRect(uiW);
   float w=card.width,x=card.x,y=card.y;
   Color mint=Hex("A5FFCC");
   Panel(card,new Color(.035f,.17f,.14f,.94f),8);
   Panel(new Rect(x+7,y+8,34,34),Hex("206650"),8);
   Transform target=NearbyShop(ShopGuideRange,true);
   string label="TÌM CỬA TIỆM TẠP HÓA";
   if(target!=null)
   {
    Vector3 delta=ShopApproach(target)-footActor.localPosition;
    float metres=Flat(delta).magnitude;
    Vector2 bearing=ShopGuideBearing(transform.TransformVector(delta),rideCamera.transform.right,rideCamera.transform.forward);
    DrawShopGuideArrow(new Rect(x+7,y+8,34,34),bearing,mint);
    label=metres<=ShopDoorReach&&footJumpTime<=0?"CỬA TRỐN · VÀO TIỆM":"CỬA TRỐN · TẠP HÓA · "+Mathf.CeilToInt(metres)+" m";
   }
   else Txt("?",x+7,y+8,34,34,19,mint,TextAnchor.MiddleCenter);
   Txt(label,x+49,y+6,w-54,19,portrait?12:13,mint,TextAnchor.MiddleLeft);
   string time=footEscapeTime>0?"TÌM CHỖ TRỐN · CÒN "+Math.Ceiling(footEscapeTime)+" GIÂY":"CSGT CÓ THỂ BẮT";
   Txt(time,x+49,y+26,w-54,18,11,footEscapeTime>0?Cream:Gold,TextAnchor.MiddleLeft);
   if(footEscapeTime>0)Panel(new Rect(x+49,y+45,(w-60)*(float)(footEscapeTime/FootEscapeSeconds),2),mint,1);
  }

  void DrawShopGuideArrow(Rect bounds,Vector2 direction,Color color)
  {
   if(direction.sqrMagnitude<.001f)direction=Vector2.up;
   int sector=(Mathf.RoundToInt(Mathf.Atan2(direction.y,direction.x)*4/Mathf.PI)+8)%8;
   Txt(ShopGuideArrows[sector],bounds.x,bounds.y,bounds.width,bounds.height,
    Mathf.RoundToInt(bounds.width*.83f),color,TextAnchor.MiddleCenter);
  }

  static Rect ShopPausePixels(Rect area,bool portraitView)
  {
   float scale=portraitView?area.width/480f:LandscapeHudScale(area);
   return new Rect(area.xMax-52*scale,area.y+12*scale,40*scale,36*scale);
  }

  void DrawShopHud()
  {
   var area=GameGuiArea();
   float scale=portrait?area.width/480f:LandscapeHudScale(area);
   Rect card=FootGuiRect(new Rect(area.x+12*scale,area.y+12*scale,267*scale,58*scale));
   float unit=card.width/267;
   Panel(card,new Color(.07f,.19f,.21f,.92f),8*unit);
   Txt("TẠP HÓA PHỐ",card.x+11*unit,card.y+7*unit,card.width-22*unit,17*unit,
    Mathf.Max(10,Mathf.RoundToInt(14*unit)),Cream);
   string status=shopEvaded?"CSGT ĐÃ ĐI · XE ĐÃ BỊ KÉO":"ĐANG ẨN NÁU · "+Mathf.CeilToInt(ShopHideRemaining)+" GIÂY";
   Txt(status,card.x+11*unit,card.y+26*unit,card.width-22*unit,17*unit,
    Mathf.Max(9,Mathf.RoundToInt(12*unit)),shopEvaded?Hex("B5F0D2"):Gold);
   Panel(new Rect(card.x+11*unit,card.y+48*unit,(card.width-22*unit)*shopHideElapsed/ShopHideSeconds,3*unit),Hex("91D3B4"),2*unit);
   if(HudButton(State==RideState.Paused?"▶":"Ⅱ",FootGuiRect(ShopPausePixels(area,portrait)),Cream,Ink,18))TogglePause();
  }
 }
}
