using UnityEngine;
namespace PhoViet
{
 // Các mẫu biểu tượng “xe phía sau sắp chạy lên” (25/10/2026, huongdan.md 21.42) để người dùng chọn.
 // Vẽ bằng hàm khoảng cách (SDF) có khử răng cưa, đúng bảng màu HUD: Mực 203C43, Kem F6F9FE, Đỏ D83833, Teal 177E80, Vàng FFD35A.
 // rearWarningStyle: -1 = mẫu cũ (khung tối + mũi tên đỏ viền vàng), 0–3 = mẫu mới A–D.
 public partial class PhoVietGame
 {
  public int rearWarningStyle=-1;
  public bool previewRearWarnings;   // chỉ để chụp ảnh duyệt mẫu: hiện cả hai làn
  public static readonly string[] RearWarningStyleNames={"A · Huy hiệu tròn đỏ","B · Biển báo tam giác","C · Thẻ kính teal","D · Ba mũi tên chạy lên"};
  Texture2D[] rearStyleTextures;Texture2D rearChevron,rearGlow;

  static float Capsule(Vector2 p,Vector2 a,Vector2 b,float r)
  {
   Vector2 pa=p-a,ba=b-a;float h=Mathf.Clamp01(Vector2.Dot(pa,ba)/Vector2.Dot(ba,ba));return (pa-ba*h).magnitude-r;
  }
  static float RoundBox(Vector2 p,Vector2 half,float r){Vector2 q=new Vector2(Mathf.Abs(p.x),Mathf.Abs(p.y))-half+Vector2.one*r;return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-r;}
  // Tam giác đều đỉnh lên (iq), bo góc r.
  static float Triangle(Vector2 p,float size,float r)
  {
   const float k=1.7320508f;p.x=Mathf.Abs(p.x)-size;p.y=p.y+size/k;
   if(p.x+k*p.y>0)p=new Vector2(p.x-k*p.y,-k*p.x-p.y)/2;
   p.x-=Mathf.Clamp(p.x,-2*size,0);return -p.magnitude*Mathf.Sign(p.y)-r;
  }
  static float Chevron(Vector2 p,float y,float w,float h,float t)=>Mathf.Min(Capsule(p,new Vector2(-w,y-h),new Vector2(0,y),t),Capsule(p,new Vector2(w,y-h),new Vector2(0,y),t));
  static float Arrow(Vector2 p,float t)=>Mathf.Min(Capsule(p,new Vector2(0,-.5f),new Vector2(0,.46f),t),Chevron(p,.5f,.4f,.4f,t));
  delegate Color Shade(Vector2 p,float px);
  static Texture2D Paint(string name,int size,Shade shade)
  {
   var tex=new Texture2D(size,size,TextureFormat.RGBA32,true){name=name,filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};
   var pixels=new Color[size*size];float px=2f/size;
   for(int y=0;y<size;y++)for(int x=0;x<size;x++)pixels[y*size+x]=shade(new Vector2((x+.5f)*px-1,(y+.5f)*px-1),px);
   tex.SetPixels(pixels);tex.Apply(true,true);return tex;
  }
  // Phủ màu c theo độ phủ của hình (khoảng cách d, mép mềm theo cỡ điểm ảnh).
  static Color Over(Color under,Color c,float d,float px,float soft=1)
  {
   float a=Mathf.Clamp01(.5f-d/(px*soft))*c.a;float outA=a+under.a*(1-a);
   if(outA<=1e-5f)return new Color(0,0,0,0);
   return new Color((c.r*a+under.r*under.a*(1-a))/outA,(c.g*a+under.g*under.a*(1-a))/outA,(c.b*a+under.b*under.a*(1-a))/outA,outA);
  }
  void CreateRearStyleTextures()
  {
   if(rearStyleTextures!=null)return;
   rearStyleTextures=new Texture2D[4];
   Color shadow=new Color(.05f,.1f,.11f,.45f),ink=StreetArt.Hex("203C43"),cream=StreetArt.Hex("F6F9FE"),red=StreetArt.Hex("D83833"),redDeep=StreetArt.Hex("A9231F"),teal=StreetArt.Hex("177E80"),gold=StreetArt.Hex("FFD35A");
   // A · huy hiệu tròn đỏ, viền kem, hai mũi tên kép trắng hướng lên.
   rearStyleTextures[0]=Paint("Rear warning A • red badge",256,(p,px)=>
   {
    Color c=new Color(0,0,0,0);
    c=Over(c,shadow,(p+new Vector2(0,.03f)).magnitude-.86f,px,14);
    var q=p-new Vector2(0,.05f);
    c=Over(c,cream,q.magnitude-.86f,px);
    c=Over(c,Color.Lerp(red,redDeep,Mathf.Clamp01(-q.y*.6f+.3f)),q.magnitude-.74f,px);
    c=Over(c,cream,Chevron(q,.36f,.34f,.3f,.075f),px);
    c=Over(c,new Color(cream.r,cream.g,cream.b,.75f),Chevron(q,.02f,.34f,.3f,.075f),px);
    c=Over(c,new Color(1,1,1,.18f),(q-new Vector2(-.18f,.32f)).magnitude-.34f,px,22);
    return c;
   });
   // B · biển báo tam giác vàng viền đỏ bo tròn, mũi tên mực đậm.
   rearStyleTextures[1]=Paint("Rear warning B • triangle sign",256,(p,px)=>
   {
    Color c=new Color(0,0,0,0);var q=p-new Vector2(0,.08f);
    c=Over(c,shadow,Triangle(q+new Vector2(0,.05f),.8f,.1f),px,14);
    c=Over(c,red,Triangle(q,.8f,.1f),px);
    c=Over(c,gold,Triangle(q,.6f,.07f),px);
    var a=(q-new Vector2(0,-.13f))*1.85f;
    c=Over(c,ink,Arrow(a,.13f)/1.85f,px);
    return c;
   });
   // C · thẻ kính teal bo góc như bảng HUD, mũi tên kem nét tròn, chấm đỏ cảnh báo.
   rearStyleTextures[2]=Paint("Rear warning C • teal glass card",256,(p,px)=>
   {
    Color c=new Color(0,0,0,0);
    c=Over(c,shadow,RoundBox(p+new Vector2(0,.05f),new Vector2(.78f,.84f),.28f),px,14);
    c=Over(c,new Color(teal.r,teal.g,teal.b,.94f),RoundBox(p,new Vector2(.76f,.82f),.26f),px);
    c=Over(c,new Color(1,1,1,.22f),Mathf.Abs(RoundBox(p,new Vector2(.7f,.76f),.21f))-.018f,px);
    var a=(p-new Vector2(0,.14f))*1.75f;
    c=Over(c,cream,Arrow(a,.15f)/1.75f,px);
    c=Over(c,red,(p-new Vector2(.5f,.56f)).magnitude-.13f,px);c=Over(c,cream,Mathf.Abs((p-new Vector2(.5f,.56f)).magnitude-.13f)-.025f,px);
    return c;
   });
   // D · ba mũi tên (vẽ riêng 1 mũi để chạy lần lượt) + quầng đỏ mềm.
   rearChevron=Paint("Rear warning D • chevron",128,(p,px)=>
   {
    Color c=new Color(0,0,0,0);
    c=Over(c,new Color(.25f,.04f,.03f,.5f),Chevron(p-new Vector2(0,-.06f),.42f,.78f,.62f,.2f),px,10);
    c=Over(c,cream,Chevron(p,.42f,.78f,.62f,.2f),px);
    c=Over(c,red,Chevron(p,.42f,.78f,.62f,.12f),px);
    return c;
   });
   rearGlow=Paint("Rear warning D • glow",128,(p,px)=>new Color(1,.3f,.16f,Mathf.Clamp01(1-p.magnitude)*Mathf.Clamp01(1-p.magnitude)*.8f));
   rearStyleTextures[3]=rearChevron;
  }
  // Vẽ một biểu tượng tại tâm (x,y) cỡ size (đơn vị GUI), pulse 0..1.
  void DrawRearWarningStyle(int style,float x,float y,float size,float pulse)
  {
   CreateRearStyleTextures();var color=GUI.color;
   switch(style)
   {
    case 0:
    {
     float s=size*(1+.07f*pulse);
     GUI.color=new Color(1,.3f,.2f,.25f+.3f*pulse);GUI.DrawTexture(new Rect(x-s*.8f,y-s*.8f,s*1.6f,s*1.6f),rearGlow ?? rearStyleTextures[0]);
     GUI.color=Color.white;GUI.DrawTexture(new Rect(x-s*.55f,y-s*.55f,s*1.1f,s*1.1f),rearStyleTextures[0]);break;
    }
    case 1:
    {
     float s=size*1.12f;float bob=-pulse*size*.05f;
     GUI.color=Color.white;GUI.DrawTexture(new Rect(x-s*.55f,y-s*.6f+bob,s*1.1f,s*1.1f),rearStyleTextures[1]);
     GUI.color=new Color(1,1,1,.35f+.45f*pulse);GUI.DrawTexture(new Rect(x-s*.55f,y-s*.6f+bob,s*1.1f,s*1.1f),rearStyleTextures[1]);break;
    }
    case 2:
    {
     float s=size*1.1f;
     GUI.color=Color.white;GUI.DrawTexture(new Rect(x-s*.55f,y-s*.6f,s*1.1f,s*1.1f),rearStyleTextures[2]);
     GUI.color=color;Txt("XE SAU",x-s*.5f,y+s*.13f,s,s*.3f,Mathf.Max(7,Mathf.RoundToInt(s*.17f)),new Color(.97f,.98f,1,.95f),TextAnchor.MiddleCenter);
     break;
    }
    default:
    {
     // Ba mũi tên sáng lần lượt từ dưới lên như đang có xe lao tới.
     float t=Time.unscaledTime*2.4f;
     GUI.color=new Color(1,1,1,.55f);GUI.DrawTexture(new Rect(x-size*.75f,y-size*.85f,size*1.5f,size*1.6f),rearGlow);
     for(int i=0;i<3;i++)
     {
      float phase=Mathf.Repeat(t-i*.33f,1f),a=Mathf.Lerp(.35f,1,Mathf.Clamp01(1-Mathf.Abs(phase-.5f)*2.2f));
      float cy=y+size*(.32f-i*.36f),w=size*(.86f-i*.08f);
      GUI.color=new Color(1,1,1,a);GUI.DrawTexture(new Rect(x-w*.5f,cy-w*.42f,w,w*.84f),rearChevron);
     }
     break;
    }
   }
   GUI.color=color;
  }
 }
}
