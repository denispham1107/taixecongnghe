using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  Texture2D rearWarningArrow;
  void RearRecklessWarningHud()
  {
   if(!RearRecklessWarningVisible||rideCamera==null||(State!=RideState.Riding&&State!=RideState.Paused))return;
   var area=GameGuiArea();float pixels=Mathf.Clamp(area.height*.07f,30,46),bottom=area.yMax-pixels*.5f-9;
   var ray=rideCamera.ScreenPointToRay(new Vector3(Screen.width*.5f,Screen.height-bottom,0));
   var plane=new Plane(transform.up,transform.position+transform.up*.03f);
   if(!plane.Raycast(ray,out float distance))return;
   float z=transform.InverseTransformPoint(ray.GetPoint(distance)).z;
   var screen=rideCamera.WorldToScreenPoint(transform.TransformPoint(new Vector3(LaneX[rearRecklessLane],.03f,z)));
   if(screen.z<=0)return;
   float size=pixels/uiScale,x=(Mathf.Clamp(screen.x,area.xMin+pixels*.6f,area.xMax-pixels*.6f)-area.x)/uiScale;
   float y=(bottom-area.y)/uiScale,pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*Mathf.PI*7);
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
