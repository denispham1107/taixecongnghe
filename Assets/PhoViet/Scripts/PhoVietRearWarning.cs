using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  Texture2D rearWarningArrow;
  void RearRecklessWarningHud()
  {
   if(rideCamera==null||(State!=RideState.Riding&&State!=RideState.Paused))return;
   for(int lane=2;lane<=3;lane++)if(RearLaneWarningVisible(lane))DrawRearLaneWarning(lane);
  }
  bool RearLaneWarningVisible(int lane)=>previewRearWarnings||(RearRecklessWarningVisible&&rearRecklessLane==lane)||OrdinaryRearWarningVisible(lane);
  Vector3 RearWarningScreenPosition(int lane,float z,float pixels,Rect area)
  {
   var point=rideCamera.WorldToScreenPoint(transform.TransformPoint(new Vector3(LaneX[lane],.03f,z)));
   float min=area.xMin+pixels*.6f,max=area.xMax-pixels*.6f;
   point.x=Mathf.Clamp(point.x,min,max);
   int other=lane==2?3:2;
   if(RearLaneWarningVisible(other))
   {
    var adjacent=rideCamera.WorldToScreenPoint(transform.TransformPoint(new Vector3(LaneX[other],.03f,z)));
    float x=Mathf.Clamp(adjacent.x,min,max),gap=pixels*1.35f;
    // Offscreen lanes can clamp to the same edge; retain two distinct ordered indicators.
    if(adjacent.z>0&&Mathf.Abs(point.x-x)<gap)point.x=Mathf.Clamp(x+(lane==2?-gap:gap),min,max);
   }
   return point;
  }
  void DrawRearLaneWarning(int lane)
  {
   var area=GameGuiArea();float pixels=Mathf.Clamp(area.height*.07f,30,46),bottom=area.yMax-pixels*.5f-9;
   var ray=rideCamera.ScreenPointToRay(new Vector3(Screen.width*.5f,Screen.height-bottom,0));
   var plane=new Plane(transform.up,transform.position+transform.up*.03f);
   if(!plane.Raycast(ray,out float distance))return;
   float z=transform.InverseTransformPoint(ray.GetPoint(distance)).z;
   var screen=RearWarningScreenPosition(lane,z,pixels,area);
   if(screen.z<=0)return;
   float size=pixels/uiScale,x=(Mathf.Clamp(screen.x,area.xMin+pixels*.6f,area.xMax-pixels*.6f)-area.x)/uiScale;
   float y=(bottom-area.y)/uiScale,pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*Mathf.PI*7);
   // Mẫu mới (21.42) thay toàn bộ khung/mũi tên/vạch cũ.
   if(rearWarningStyle>=0){DrawRearWarningStyle(rearWarningStyle,x,y,size,pulse);return;}
   Panel(new Rect(x-size*.58f,y-size*.54f,size*1.16f,size*1.1f),new Color(1,.24f,.05f,.12f+pulse*.16f),size*.4f);
   Panel(new Rect(x-size*.44f,y-size*.45f,size*.88f,size*.95f),new Color(.07f,.17f,.18f,.86f),size*.2f);
   if(rearWarningArrow==null)CreateRearWarningArrow();
   var color=GUI.color;GUI.color=new Color(1,1,1,.72f+pulse*.28f);
   GUI.DrawTexture(new Rect(x-size*.5f,y-size*.53f,size,size),rearWarningArrow);
   GUI.color=color;
   Panel(new Rect(x-size*.24f,y+size*.40f,size*.48f,size*.06f),StreetArt.Gold,size*.03f);
  }
  void CreateRearWarningArrow()
  {
   const int size=128;rearWarningArrow=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Rear danger arrow • gold rim",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   var pixels=new Color[size*size];
   System.Func<float,float,bool> inside=(x,y)=>y>-.86f&&y<.88f&&Mathf.Abs(x)<(y>.12f?(.88f-y)*.9f:.25f);
   for(int y=0;y<size;y++)for(int x=0;x<size;x++)
   {
    float px=(x+.5f)/size*2-1,py=(y+.5f)/size*2-1;
    if(!inside(px,py))continue;
    bool edge=!inside(px-.055f,py)||!inside(px+.055f,py)||!inside(px,py-.055f)||!inside(px,py+.055f);
    pixels[y*size+x]=edge?StreetArt.Gold:px<-.12f?new Color(1,.42f,.23f):new Color(.91f,.10f,.09f);
   }
   rearWarningArrow.SetPixels(pixels);rearWarningArrow.Apply(false,true);
  }
 }
}
