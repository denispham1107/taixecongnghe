using UnityEngine;
using static PhoViet.StreetArt;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        static readonly Color WelcomeWhite=new Color(.97f,.985f,1,.93f),WelcomeGlass=new Color(.97f,.985f,1,.82f);
        public static Rect WelcomeCardRect(float width,float height,bool narrow)
        {
            float w=narrow?432:360,h=narrow?258:276;
            float y=Mathf.Clamp(height*(narrow?.15f:.27f),105,Mathf.Max(105,height-h-95));
            return new Rect(narrow?(width-w)/2:48,y,w,h);
        }
        public static Rect WelcomeStartRect(Rect card)=>new Rect(card.x+22,card.y+card.height-70,card.width-44,44);
        bool WelcomeTap(Vector2 point)
        {
            var area=GameGuiArea();bool narrow=Screen.width<Screen.height;
            float scale=narrow?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f);
            var p=new Vector2((point.x-area.x)/scale,(Screen.height-point.y-area.y)/scale);
            if(!WelcomeStartRect(WelcomeCardRect(area.width/scale,area.height/scale,narrow)).Contains(p))return false;
            StartRide();return true;
        }
        void WelcomeCard()
        {
            Rect card=WelcomeCardRect(uiW,uiH,portrait);float x=card.x,y=card.y,w=card.width;
            Panel(new Rect(x-3,y+5,w+6,card.height+3),new Color(.04f,.13f,.20f,.14f),21);
            Panel(new Rect(x-1,y-1,w+2,card.height+2),new Color(1,1,1,.68f),19);Panel(card,WelcomeWhite,18);
            Panel(new Rect(x+22,y+20,5,13),Red,2);
            Txt("MỘT VÒNG VIỆT NAM",x+36,y+18,w-84,21,11,Teal);
            // A small road motif adds depth and an identity without another image asset.
            Panel(new Rect(x+w-50,y+17,27,27),new Color(.10f,.35f,.4f,.08f),8);
            Panel(new Rect(x+w-44,y+21,2,19),Teal,1);Panel(new Rect(x+w-31,y+21,2,19),Teal,1);
            for(int dash=0;dash<3;dash++)Panel(new Rect(x+w-37,y+22+dash*7,2,4),Red,1);
            Txt("LÁI QUA\nPHỐ QUEN.",x+20,y+48,w-40,108,portrait?39:41,Ink);
            Txt("Khám phá phố Việt, né xe\nvà nhận những chuyến đi mới.",x+23,y+(portrait?143:152),w-46,42,15,Hex("4C657A"));
            if(HudButton("LÊN XE THÔI   →",WelcomeStartRect(card),Red,Color.white,17))StartRide();
            Txt("KỶ LỤC  /  "+BestDistance.ToString("N0")+" m",x+23,y+card.height-20,w-46,17,10,Teal);
        }
    }
}
