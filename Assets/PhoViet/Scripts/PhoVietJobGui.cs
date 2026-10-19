using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  int jobTouchChoice=-1;Vector2 jobTouchStart;
  Rect JobChoiceRect(int index,float width,float height,bool narrow)
  {
   float w=narrow?448:790,h=narrow?530:475,x=(width-w)/2,y=Mathf.Max(125,(height-h)/2);
   return new Rect(x+w-143,y+126+index*(narrow?125:107)+14,113,narrow?86:70);
  }
  int JobChoiceAt(Vector2 screenPosition)
  {
   var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
   float scale=narrow?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f);
   Vector2 p=new Vector2((screenPosition.x-area.x)/scale,(Screen.height-screenPosition.y-area.y)/scale);
   for(int i=0;i<JobOffers.Count;i++)if(JobChoiceRect(i,area.width/scale,area.height/scale,narrow).Contains(p))return i;
   return -1;
  }
  void ReadJobTouch()
  {
   if(!JobOffersOpen){jobTouchChoice=-1;return;}if(Input.touchCount==0)return;
   var touch=Input.GetTouch(0);
   if(touch.phase==TouchPhase.Began){jobTouchChoice=JobChoiceAt(touch.position);jobTouchStart=touch.position;}
   if(touch.phase==TouchPhase.Ended)
   {
    int selected=jobTouchChoice;jobTouchChoice=-1;
    if(selected>=0&&JobChoiceAt(touch.position)==selected&&Vector2.Distance(jobTouchStart,touch.position)<Screen.width*.05f)AcceptJob(selected);
   }
   if(touch.phase==TouchPhase.Canceled)jobTouchChoice=-1;
  }
  void JobOffersGui()
  {
   FullscreenTint(new Color(.03f,.12f,.14f,.72f));float w=portrait?448:790,h=portrait?530:475,x=(uiW-w)/2,y=Mathf.Max(125,(uiH-h)/2);
   Panel(new Rect(x,y,w,h),StreetArt.Cream,18);Txt("CHỌN CHUYẾN XE",x+22,y+18,w-44,35,portrait?24:29,StreetArt.Ink);
   Txt("Game tạm dừng · Chỉ nhận Coin sau khi trả khách",x+22,y+58,w-44,32,13,StreetArt.Teal);
   Txt("SỐ DƯ: "+FormatCoins(Coins)+" COIN",x+22,y+92,w-44,25,14,StreetArt.Red);
   for(int i=0;i<JobOffers.Count;i++)
   {
    var j=JobOffers[i];float cy=y+126+i*(portrait?125:107);
    Panel(new Rect(x+16,cy,w-32,portrait?115:98),StreetArt.Ink,10);
    Txt((i+1)+". ĐÓN: "+city.Street(j.pickup),x+29,cy+9,w-176,23,portrait?14:17,StreetArt.Cream);
    Txt("TRẢ: "+city.Street(j.dropoff),x+29,cy+35,w-176,23,portrait?13:15,StreetArt.Cream);
    Txt("Đón "+Mathf.CeilToInt(j.pickupMetres)+" m · Chở "+Mathf.CeilToInt(j.tripMetres)+" m",x+29,cy+65,w-176,30,12,new Color(.7f,.85f,.8f));
    if(Btn(FormatCoins(j.reward)+" Coin\nNHẬN VIỆC",JobChoiceRect(i,uiW,uiH,portrait),StreetArt.Gold,StreetArt.Ink,14))AcceptJob(i);
   }
  }
  void JobNavigationGui()
  {
   if(portrait)return; // The portrait HUD owns one merged information/navigation bar.
   if(!jobsEnabled||ActiveJob==null||State==RideState.Ready||State==RideState.Jailed)return;
   float w=portrait?242:300,h=portrait?190:220,x=uiW-w-(portrait?16:36),y=portrait?145:125;
   Panel(new Rect(x,y,w,h),new Color(.07f,.18f,.2f,.96f),12);
   string stage=JobPhase==WorkPhase.Pickup?"ĐÓN KHÁCH":JobPhase==WorkPhase.Delivery?"ĐƯA KHÁCH":JobPhase==WorkPhase.Boarding?"KHÁCH ĐANG LÊN XE":"KHÁCH ĐANG XUỐNG XE";
   var route=NavigationRoute;float remaining=NavigationRemainingDistance;
   string instruction=JobNavigationInstruction();
   Txt(stage+" · "+Mathf.CeilToInt(remaining)+" m",x+12,y+9,w-24,20,12,StreetArt.Gold);
   Txt(instruction,x+12,y+31,w-24,23,14,StreetArt.Cream);
   DrawJobMap(new Rect(x+10,y+60,w-20,h-111));
   Txt(city.Street(CurrentWorkTarget),x+12,y+h-43,w-24,21,12,StreetArt.Cream);
   Txt(rerouteNotice>0?"ĐANG TÍNH LẠI LỘ TRÌNH":"THƯỞNG "+FormatCoins(ActiveJob.reward)+" COIN",x+12,y+h-22,w-24,18,10,rerouteNotice>0?StreetArt.Gold:new Color(.7f,.85f,.8f));
  }
  string JobNavigationInstruction()
  {
   var route=NavigationRoute;
   return ServiceStopped?"DỪNG TẠI ĐIỂM HẸN":IsTurning?"ĐANG RẼ "+(turnSide<0?"TRÁI":"PHẢI"):route!=null&&!route.direct?(route.nextTurn<0?"RẼ TRÁI":route.nextTurn>0?"RẼ PHẢI":"ĐI THẲNG")+" · "+Mathf.Max(0,Mathf.CeilToInt(NextJunctionDistance))+" m":CurrentWorkTarget!=null?"VÀO LỀ "+(TargetLane(CurrentWorkTarget)==3?"PHẢI":"TRÁI"):"";
  }
  void DrawJobMap(Rect rect,bool compact=false)
  {
   GUI.BeginGroup(rect);Vector2 player=NavigationMapPosition;var route=NavigationRoute;
   float radius=100;if(route!=null)foreach(var p in route.points)radius=Mathf.Max(radius,Mathf.Abs(p.x-player.x)+20,Mathf.Abs(p.y-player.y)+20);
   float scale=Mathf.Min(rect.width,rect.height)/(2*radius),cx=rect.width*.5f,cy=rect.height*.5f;
   System.Func<Vector2,Vector2> screen=p=>new Vector2(cx+(p.x-player.x)*scale,cy-(p.y-player.y)*scale);
   int grid=Mathf.Max(1,Mathf.CeilToInt(radius/80));
   for(int n=-grid;n<=grid;n++)
   {
    float gx=Mathf.Round(player.x/80)*80+n*80,gy=Mathf.Round(player.y/80)*80+n*80;
    Color gridColor=compact?new Color(.7f,.9f,.85f,.25f):new Color(.24f,.36f,.36f);float lineWidth=compact?1:3;
    MapLine(screen(new Vector2(gx,player.y-radius)),screen(new Vector2(gx,player.y+radius)),lineWidth,gridColor);
    MapLine(screen(new Vector2(player.x-radius,gy)),screen(new Vector2(player.x+radius,gy)),lineWidth,gridColor);
   }
   if(route!=null)for(int i=1;i<route.points.Count;i++)MapLine(screen(route.points[i-1]),screen(route.points[i]),compact?2:4,StreetArt.Gold);
   float dot=compact?3:4;
   if(CurrentWorkTarget!=null){Vector2 end=screen(CurrentWorkTarget.Point);Panel(new Rect(end.x-dot,end.y-dot,dot*2,dot*2),StreetArt.Red,dot);}
   Vector2 centre=screen(player),heading=NavigationMapDirection;Vector2 tip=centre+new Vector2(heading.x,-heading.y)*(compact?6:9);
   // Small dots support the smoothly rotating heading without changing the GUI
   // matrix, which would break clipping inside this map group on mobile.
   for(int i=0;i<=9;i++){var p=Vector2.Lerp(centre,tip,i/9f);float s=compact?1:2;Panel(new Rect(p.x-s,p.y-s,2*s,2*s),new Color(.25f,.85f,.9f),s);}
   Panel(new Rect(centre.x-dot,centre.y-dot,2*dot,2*dot),StreetArt.Cream,dot);GUI.EndGroup();
  }
  void MapLine(Vector2 a,Vector2 b,float width,Color color)
  {
   // Grid streets and route segments are axis-aligned. Keep the GUI transform
   // intact so the map's group clips every line inside its card on all screens.
   Panel(new Rect(Mathf.Min(a.x,b.x)-width*.5f,Mathf.Min(a.y,b.y)-width*.5f,
    Mathf.Max(width,Mathf.Abs(a.x-b.x)+width),Mathf.Max(width,Mathf.Abs(a.y-b.y)+width)),color,0);
  }
 }
}
