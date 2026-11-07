using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        enum DrivePedalMode { Cruise, Brake, PullAway }
        const float BrakeDeceleration=9f;
        const int GasHudChoice=4, BrakeHudChoice=5;
        DrivePedalMode drivePedal;
        Texture2D pedalChevron;
        bool DriveBraking=>drivePedal==DrivePedalMode.Brake;
        bool DriveStopped=>DriveBraking&&Speed==0;

        void SelectDrivePedal(bool brake)
        {
            if(State!=RideState.Riding||ServiceStopped||JobOffersOpen)return;
            lastTapTime=-100;
            if(brake)
            {
                if(!DriveBraking)CancelAssistedKick();
                drivePedal=DrivePedalMode.Brake;
            }
            else if(DriveBraking)drivePedal=DrivePedalMode.PullAway;
        }

        float DrivePedalSpeed(float target,float previous,float dt)
        {
            if(DriveBraking)return Mathf.MoveTowards(previous,0,BrakeDeceleration*dt);
            if(drivePedal==DrivePedalMode.PullAway)return Mathf.MoveTowards(previous,target,LaunchAcceleration*dt);
            return target;
        }

        public static Rect DrivePedalLayout(Rect area,bool narrow,bool brake,float pixelRatio=1)
        {
            var reference=brake?PlayerMissileButtonLayout(area,narrow,pixelRatio):FlightButtonLayout(area,narrow,pixelRatio);
            float inset=(narrow?18:40)*(narrow?area.width/480f:LandscapeHudScale(area));
            reference.x=area.xMax-inset-reference.width;
            return reference;
        }

        int DrivePedalAt(Vector2 position)
        {
            if(State!=RideState.Riding)return -1;
            var area=GameGuiArea();
            var point=new Vector2(position.x,Screen.height-position.y);
            bool narrow=Screen.width<Screen.height;
            var gas=DrivePedalLayout(area,narrow,false,browserPixelRatio);
            var brake=DrivePedalLayout(area,narrow,true,browserPixelRatio);
            if((point-gas.center).sqrMagnitude<=gas.width*gas.width*.25f)return GasHudChoice;
            return (point-brake.center).sqrMagnitude<=brake.width*brake.width*.25f?BrakeHudChoice:-1;
        }

        void DrawDrivePedal(bool brake)
        {
            if(State!=RideState.Riding)return;
            var area=GameGuiArea();
            var p=DrivePedalLayout(area,portrait,brake,browserPixelRatio);
            var r=new Rect((p.x-area.x)/uiScale,(p.y-area.y)/uiScale,p.width/uiScale,p.height/uiScale);
            bool selected=brake==DriveBraking;
            Color accent=brake?StreetArt.Hex("FF836E"):StreetArt.Hex("63E6C3");
            Panel(r,new Color(accent.r,accent.g,accent.b,selected?1:.52f),r.width*.5f);
            float ring=selected?3:2;
            var inner=new Rect(r.x+ring,r.y+ring,r.width-2*ring,r.height-2*ring);
            Panel(inner,brake?new Color(.22f,.09f,.14f,.94f):new Color(.02f,.17f,.19f,.94f),inner.width*.5f);
            DrawDrivePedalGlyph(r,accent,brake);
            Txt(brake?"ĐẠP PHANH":"ĐẠP GA",r.x,r.y+r.height*.65f,r.width,r.height*.22f,
                Mathf.RoundToInt(r.width*(brake?.123f:.145f)),Color.white,TextAnchor.MiddleCenter);
            bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
            if(clicked&&Time.unscaledTime>=hudTouchSuppressUntil&&
                (Event.current.mousePosition-r.center).sqrMagnitude<=r.width*r.width*.25f)SelectDrivePedal(brake);
#endif
        }

        void DrawDrivePedalGlyph(Rect r,Color accent,bool brake)
        {
            float s=r.width;
            // Side profile with the nose pointing right, below an acceleration arrow or stop mark.
            Panel(new Rect(r.x+.24f*s,r.y+.46f*s,.51f*s,.095f*s),Color.white,.035f*s);
            Panel(new Rect(r.x+.34f*s,r.y+.39f*s,.28f*s,.10f*s),Color.white,.045f*s);
            Panel(new Rect(r.x+.39f*s,r.y+.413f*s,.18f*s,.044f*s),accent,.02f*s);
            Panel(new Rect(r.x+.30f*s,r.y+.50f*s,.11f*s,.11f*s),accent,.055f*s);
            Panel(new Rect(r.x+.61f*s,r.y+.50f*s,.11f*s,.11f*s),accent,.055f*s);
            if(brake)
            {
                Panel(new Rect(r.x+.41f*s,r.y+.18f*s,.18f*s,.16f*s),accent,.046f*s);
                Panel(new Rect(r.x+.45f*s,r.y+.243f*s,.10f*s,.034f*s),StreetArt.Hex("431827"),.017f*s);
                return;
            }
            if(pedalChevron==null)pedalChevron=CreatePedalChevron();
            GUI.DrawTexture(new Rect(r.x+.36f*s,r.y+.165f*s,.17f*s,.18f*s),pedalChevron,ScaleMode.StretchToFill,true,0,accent,0,0);
            GUI.DrawTexture(new Rect(r.x+.51f*s,r.y+.165f*s,.17f*s,.18f*s),pedalChevron,ScaleMode.StretchToFill,true,0,accent,0,0);
        }

        static Texture2D CreatePedalChevron()
        {
            const int size=48;
            var pixels=new Color32[size*size];
            var tip=new Vector2(35,24);
            var start=new Vector2(13,5);
            var leg=tip-start;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                var point=new Vector2(x+.5f,Mathf.Min(y+.5f,size-y-.5f));
                var nearest=start+leg*Mathf.Clamp01(Vector2.Dot(point-start,leg)/leg.sqrMagnitude);
                byte alpha=(byte)Mathf.RoundToInt(255*Mathf.Clamp01(4.5f-Vector2.Distance(point,nearest)));
                pixels[y*size+x]=new Color32(255,255,255,alpha);
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Drive pedal chevron",wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            return texture;
        }
    }
}
