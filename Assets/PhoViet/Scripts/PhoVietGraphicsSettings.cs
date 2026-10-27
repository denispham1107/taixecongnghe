using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public const string GraphicsPreferenceKey="PhoViet.GraphicsQuality.v1";
        public enum GraphicsLevel { High, Medium, Low }
        public GraphicsLevel SelectedGraphics {get;private set;}=GraphicsLevel.High;
        public bool GraphicsSettingsOpen {get;private set;}
        bool graphicsInitialized;
        int originalAA,originalCascades;
        float originalShadowDistance,originalLodBias,originalReflectionScale,originalReflectionFar;
        ShadowQuality originalShadows;
        ShadowResolution originalShadowResolution;
        int originalReflectionSize,originalReflectionInterval;
        RiverReflection graphicsReflection;
        Texture2D graphicsGear;

        // Snapshot the actual platform baseline, after the existing browser setup.
        void InitializeGraphicsSettings()
        {
            if(graphicsInitialized)return;
            originalAA=QualitySettings.antiAliasing;originalShadows=QualitySettings.shadows;
            originalShadowDistance=QualitySettings.shadowDistance;originalShadowResolution=QualitySettings.shadowResolution;
            originalCascades=QualitySettings.shadowCascades;originalLodBias=QualitySettings.lodBias;
            graphicsReflection=rideCamera!=null?rideCamera.GetComponent<RiverReflection>():null;
            if(graphicsReflection!=null)
            {
                originalReflectionScale=graphicsReflection.resolution;originalReflectionSize=graphicsReflection.maxSize;
                originalReflectionInterval=graphicsReflection.interval;originalReflectionFar=graphicsReflection.farClip;
            }
            graphicsInitialized=true;
            int stored=PlayerPrefs.GetInt(GraphicsPreferenceKey,0);
            ApplyGraphicsLevel(stored>=0&&stored<=2?(GraphicsLevel)stored:GraphicsLevel.High);
        }
        void RestoreGraphicsBaseline()
        {
            if(!graphicsInitialized)return;
            QualitySettings.antiAliasing=originalAA;QualitySettings.shadows=originalShadows;
            QualitySettings.shadowDistance=originalShadowDistance;QualitySettings.shadowResolution=originalShadowResolution;
            QualitySettings.shadowCascades=originalCascades;QualitySettings.lodBias=originalLodBias;
            if(graphicsReflection!=null)
            {
                graphicsReflection.resolution=originalReflectionScale;graphicsReflection.maxSize=originalReflectionSize;
                graphicsReflection.interval=originalReflectionInterval;graphicsReflection.farClip=originalReflectionFar;
            }
        }
        void ApplyGraphicsLevel(GraphicsLevel level)
        {
            RestoreGraphicsBaseline();SelectedGraphics=level;
            if(level==GraphicsLevel.High)return;
            bool low=level==GraphicsLevel.Low;
            QualitySettings.antiAliasing=low?0:Mathf.Min(originalAA,2);
            QualitySettings.shadows=low?ShadowQuality.Disable:originalShadows==ShadowQuality.Disable?ShadowQuality.Disable:ShadowQuality.HardOnly;
            QualitySettings.shadowDistance=low?0:originalShadowDistance*.65f;
            QualitySettings.shadowResolution=(ShadowResolution)Mathf.Max(0,(int)originalShadowResolution-1);
            QualitySettings.shadowCascades=0;QualitySettings.lodBias=originalLodBias*(low?.5f:.75f);
            if(graphicsReflection!=null)
            {
                graphicsReflection.resolution=originalReflectionScale*(low?.5f:.75f);
                graphicsReflection.maxSize=Mathf.Min(originalReflectionSize,low?384:512);
                graphicsReflection.interval=Mathf.Max(originalReflectionInterval,low?3:2);
                graphicsReflection.farClip=Mathf.Min(originalReflectionFar,low?75:100);
            }
        }
        public void SetGraphicsQuality(int level)
        {
            if(level<0||level>2)return;
            InitializeGraphicsSettings();ApplyGraphicsLevel((GraphicsLevel)level);
            PlayerPrefs.SetInt(GraphicsPreferenceKey,level);PlayerPrefs.Save();
        }
        public void OpenGraphicsSettings()
        {
            if(State!=RideState.Ready)return;
            GraphicsSettingsOpen=true;CancelSwipe();lastTapTime=-100;
        }
        public void CloseGraphicsSettings(){GraphicsSettingsOpen=false;CancelSwipe();lastTapTime=-100;}
        void ReadGraphicsSettingsKeys()
        {
            if(!GraphicsSettingsOpen)return;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P)||Input.GetKeyDown(KeyCode.Return))CloseGraphicsSettings();
            else if(Input.GetKeyDown(KeyCode.Alpha1))SetGraphicsQuality(0);
            else if(Input.GetKeyDown(KeyCode.Alpha2))SetGraphicsQuality(1);
            else if(Input.GetKeyDown(KeyCode.Alpha3))SetGraphicsQuality(2);
        }
        public static Rect WelcomeSettingsRect(Rect card)=>new Rect(card.xMax-60,card.y+8,44,44);
        public static Rect GraphicsWindowRect(float width,float height,bool narrow)
        {
            float w=Mathf.Min(narrow?432:560,width-32),h=Mathf.Min(440,height-32);
            return new Rect((width-w)/2,(height-h)/2,w,h);
        }
        public static Rect GraphicsChoiceRect(Rect window,int level)=>new Rect(window.x+22,window.y+86+level*76,window.width-44,64);
        public static Rect GraphicsDoneRect(Rect window)=>new Rect(window.x+22,window.yMax-66,window.width-44,44);
        public static Rect GraphicsCloseRect(Rect window)=>new Rect(window.xMax-54,window.y+12,40,40);
        bool GraphicsTap(Vector2 point,float width,float height,bool narrow)
        {
            var window=GraphicsWindowRect(width,height,narrow);
            if(GraphicsDoneRect(window).Contains(point)||GraphicsCloseRect(window).Contains(point)||!window.Contains(point))CloseGraphicsSettings();
            else for(int i=0;i<3;i++)if(GraphicsChoiceRect(window,i).Contains(point)){SetGraphicsQuality(i);break;}
            return true; // Every tap is consumed while the modal is open.
        }
        void GraphicsSettingsGui()
        {
            FullscreenTint(new Color(.03f,.10f,.16f,.60f));var r=GraphicsWindowRect(uiW,uiH,portrait);
            Panel(new Rect(r.x-2,r.y+6,r.width+4,r.height+2),new Color(.01f,.07f,.12f,.18f),21);
            Panel(r,WelcomeWhite,18);
            Txt("CÀI ĐẶT ĐỒ HỌA",r.x+22,r.y+19,r.width-90,30,24,Ink);
            Txt("Chọn mức phù hợp với thiết bị của bạn.",r.x+22,r.y+55,r.width-44,24,14,Teal);
            if(HudButton("×",GraphicsCloseRect(r),new Color(.1f,.35f,.4f,.07f),Ink,25))CloseGraphicsSettings();
            for(int i=0;i<3;i++)
            {
                bool selected=(int)SelectedGraphics==i;var choice=GraphicsChoiceRect(r,i);
                if(HudButton("",choice,selected?Teal:new Color(.86f,.9f,.93f),Ink,18))SetGraphicsQuality(i);
                Panel(new Rect(choice.x+1.5f,choice.y+1.5f,choice.width-3,choice.height-3),selected?new Color(.89f,.96f,.96f):new Color(.96f,.975f,.985f),10);
                Panel(new Rect(choice.x+17,choice.y+22,20,20),selected?Teal:new Color(.65f,.73f,.77f),10);
                Panel(new Rect(choice.x+19,choice.y+24,16,16),selected?Color.white:new Color(.96f,.975f,.985f),8);
                if(selected)Panel(new Rect(choice.x+23,choice.y+28,8,8),Teal,4);
                Txt(i==0?"Cao":i==1?"Trung bình":"Thấp",choice.x+51,choice.y+8,choice.width-66,27,19,Ink);
                Txt(i==0?"Chi tiết đầy đủ như hiện tại.":i==1?"Giảm bóng và độ chi tiết cảnh xa.":"Ít bóng, phản chiếu nhẹ hơn.",choice.x+51,choice.y+36,choice.width-66,22,13,Teal);
            }
            Txt("Lựa chọn được lưu cho lần chơi sau.",r.x+22,r.y+324,r.width-44,24,13,Teal,TextAnchor.MiddleCenter);
            if(HudButton("XONG",GraphicsDoneRect(r),Teal,Color.white,17))CloseGraphicsSettings();
#if !UNITY_WEBGL || UNITY_EDITOR
            if(Event.current.type==EventType.MouseDown&&!r.Contains(Event.current.mousePosition)){CloseGraphicsSettings();Event.current.Use();}
#endif
        }
        void DrawGraphicsGear(Rect rect)
        {
            if(graphicsGear==null)
            {
                const int n=96;var pixels=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++)
                {
                    float alpha=0;
                    for(int sy=0;sy<2;sy++)for(int sx=0;sx<2;sx++)
                    {
                        float px=(x+(sx+.5f)*.5f-n*.5f)/(n*.5f),py=(y+(sy+.5f)*.5f-n*.5f)/(n*.5f),radius=Mathf.Sqrt(px*px+py*py);
                        bool inside=radius<.72f;
                        for(int tooth=0;tooth<8&&!inside;tooth++)
                        {
                            float a=tooth*Mathf.PI*.25f,along=px*Mathf.Cos(a)+py*Mathf.Sin(a),across=-px*Mathf.Sin(a)+py*Mathf.Cos(a);
                            inside=along>.60f&&along<.94f&&Mathf.Abs(across)<.13f;
                        }
                        if(inside&&radius>.31f)alpha+=.25f;
                    }
                    pixels[y*n+x]=new Color(1,1,1,alpha);
                }
                graphicsGear=new Texture2D(n,n,TextureFormat.RGBA32,false){name="Graphics settings gear",filterMode=FilterMode.Bilinear,hideFlags=HideFlags.DontSave};
                graphicsGear.SetPixels(pixels);graphicsGear.Apply(false,true);
            }
            GUI.DrawTexture(rect,graphicsGear,ScaleMode.ScaleToFit,true,0,Teal,0,0);
        }
        void ShutdownGraphicsSettings()
        {
#if UNITY_EDITOR
            RestoreGraphicsBaseline(); // Play Mode must not leave the Editor quality lowered.
#endif
            if(graphicsGear!=null)Destroy(graphicsGear);
        }
    }
}
