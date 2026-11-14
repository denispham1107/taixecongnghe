using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        enum DrivePedalMode { Cruise, Brake, PullAway }
        enum ArcadeControl { Missile, Flight, Land, Gas, Brake }
        const float BrakeDeceleration=9f;
        const int GasHudChoice=4, BrakeHudChoice=5;
        DrivePedalMode drivePedal;
        Texture2D arcadeControls;
        bool DriveBraking=>drivePedal==DrivePedalMode.Brake;
        internal bool PlayerBrakeLampsOn=>(DriveBraking||IsPlayerCar&&driveNeutralBraking)&&(State==RideState.Riding||
            State==RideState.Paused&&pauseFromState==RideState.Riding);
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
            else if(DriveBraking)drivePedal=IsPlayerCar?DrivePedalMode.Cruise:DrivePedalMode.PullAway;
        }

        float DrivePedalSpeed(float target,float previous,float dt)
        {
            if(DriveBraking)return Mathf.MoveTowards(previous,0,BrakeDeceleration*dt);
            if(drivePedal==DrivePedalMode.PullAway)return Mathf.MoveTowards(previous,target,LaunchAcceleration*dt);
            return target;
        }

        public static Rect DrivePedalLayout(Rect area,bool narrow,bool brake,float pixelRatio=1)
        {
            var reference=ControlButtonBase(area,narrow,pixelRatio);
            float inset=(narrow?18:40)*(narrow?area.width/480f:LandscapeHudScale(area));
            reference.x=area.xMax-inset-reference.width;
            if(!brake)reference.y-=reference.height+Mathf.Max(6*pixelRatio,reference.width*.14f);
            return reference;
        }

        static Rect DriveActionLayout(Rect area,bool narrow,int choice,float pixelRatio)
        {
            Rect button=DrivePedalLayout(area,narrow,choice==BrakeHudChoice||choice==2,pixelRatio);
            float gap=Mathf.Max(6*pixelRatio,button.width*.14f);
            if(choice==2||choice==3)button.x-=button.width+gap;
            if(choice==3)button.y+=gap*.55f;
            if(choice==ExitCarChoice){button.y-=button.height+gap;button.x-=button.width*.32f;}
            return button;
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
            DrawArcadeControl(r,brake?ArcadeControl.Brake:ArcadeControl.Gas,selected?1:.72f);
            bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
#if !UNITY_WEBGL || UNITY_EDITOR
            if(clicked&&Time.unscaledTime>=hudTouchSuppressUntil&&
                (Event.current.mousePosition-r.center).sqrMagnitude<=r.width*r.width*.25f)SelectDrivePedal(brake);
#endif
        }

        void EnsureArcadeControls()
        {
            if(arcadeControls==null)arcadeControls=Resources.Load<Texture2D>("PV_ArcadeControls");
        }

        void DrawArcadeControl(Rect rect,ArcadeControl icon,float opacity)
        {
            if(Event.current.type!=EventType.Repaint)return;
            EnsureArcadeControls();
            if(arcadeControls==null)return;
            int index=(int)icon;
            // The texture has four columns and two rows of 256-pixel cells.
            var uv=new Rect(index%4*.25f,.5f-index/4*.5f,.25f,.5f);
            var saved=GUI.color;
            GUI.color=new Color(saved.r,saved.g,saved.b,saved.a*opacity);
            GUI.DrawTextureWithTexCoords(rect,arcadeControls,uv,true);
            GUI.color=saved;
        }
    }
}
