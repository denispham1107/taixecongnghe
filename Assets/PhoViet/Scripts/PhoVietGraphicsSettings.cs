using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public const string GraphicsPreferenceKey="PhoViet.GraphicsQuality.v1";
        // Keep saved v1 IDs: the old High (0) is now VeryHigh; insert High without shifting Medium/Low.
        public enum GraphicsLevel { VeryHigh=0, Medium=1, Low=2, High=3 }
        static readonly GraphicsLevel[] GraphicsOrder={GraphicsLevel.VeryHigh,GraphicsLevel.High,GraphicsLevel.Medium,GraphicsLevel.Low};
        public static GraphicsLevel GraphicsLevelAt(int row)=>GraphicsOrder[Mathf.Clamp(row,0,3)];
        public static int GraphicsRank(GraphicsLevel level)=>level==GraphicsLevel.VeryHigh?0:level==GraphicsLevel.High?1:level==GraphicsLevel.Medium?2:3;
        public static string GraphicsName(GraphicsLevel level)=>level==GraphicsLevel.VeryHigh?"Rất cao":level==GraphicsLevel.High?"Cao":level==GraphicsLevel.Medium?"Trung bình":"Thấp";
        public GraphicsLevel SelectedGraphics {get;private set;}=GraphicsLevel.VeryHigh;
        public bool GraphicsSettingsOpen {get;private set;}
        bool graphicsInitialized;
        int originalAA,originalCascades,originalVSync;
        float originalShadowDistance,originalLodBias,originalReflectionScale,originalReflectionFar;
        ShadowQuality originalShadows;
        ShadowResolution originalShadowResolution;
        int originalReflectionSize,originalReflectionInterval;
        RiverReflection graphicsReflection;
        Texture2D graphicsGear;

        // Web bridge (Plugins/WebGL/PhoVietWeb.jslib): render pixel ratio and touch detection, including iPadOS desktop UA.
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void PhoVietSetPixelCap(float cap);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int PhoVietTouchDevice();
        static bool DeviceIsTouch=>Application.isMobilePlatform||PhoVietTouchDevice()!=0;
#else
        static void PhoVietSetPixelCap(float cap){}
        static bool DeviceIsTouch=>false;
#endif
        // 21.136: what one level actually renders. CPU cost on phones is ruled by draw calls and active renderers,
        // GPU cost by pixels and passes, so every level sets both instead of only trimming shadows and AA.
        public struct GraphicsPlan
        {
            public float pixelCap,pixelFloor,lodBias,shadowDistance,far,smallPropCull,reflectionScale,reflectionFar;
            public int antiAliasing,pixelLights,reflectionSize,reflectionInterval;
            public bool reflection,fogVolume,simpleStandard,sceneryShadows;
        }
        public const int SmallPropLayer=9;
        public static GraphicsPlan PlanFor(GraphicsLevel level)
        {
            switch(level)
            {
                case GraphicsLevel.VeryHigh:return new GraphicsPlan{pixelCap=2f,pixelFloor=1.5f,lodBias=.8f,shadowDistance=30,far=165,smallPropCull=110,antiAliasing=2,pixelLights=1,sceneryShadows=true,reflection=true,reflectionScale=.33f,reflectionSize=512,reflectionInterval=2,reflectionFar=100};
                case GraphicsLevel.High:return new GraphicsPlan{pixelCap=1.8f,pixelFloor=1.375f,lodBias=.6f,shadowDistance=25,far=150,smallPropCull=90,antiAliasing=2,pixelLights=1,sceneryShadows=true,reflection=true,reflectionScale=.29f,reflectionSize=448,reflectionInterval=2,reflectionFar=90};
                case GraphicsLevel.Medium:return new GraphicsPlan{pixelCap=1.6f,pixelFloor=1.25f,lodBias=.42f,shadowDistance=20,far=135,smallPropCull=70,antiAliasing=0,pixelLights=0,reflection=true,reflectionScale=.25f,reflectionSize=384,reflectionInterval=3,reflectionFar=75};
                default:return new GraphicsPlan{pixelCap=1.25f,pixelFloor=1f,lodBias=.3f,shadowDistance=0,far=115,smallPropCull=55,antiAliasing=0,pixelLights=0,simpleStandard=true};
            }
        }
        const string GraphicsAutoKey="PhoViet.GraphicsAutoDrop.v1";
        // Touch devices use the plan on every level; desktop keeps its own full baseline on VeryHigh.
        [System.NonSerialized] public bool forceTouchGraphics;
        bool TouchGraphics=>forceTouchGraphics||DeviceIsTouch;
        public GraphicsLevel EffectiveGraphics {get;private set;}=GraphicsLevel.VeryHigh;
        public float RenderPixelCap {get;private set;}
        float originalFar;int originalPixelLights;
        MorningFog graphicsFog;Shader standardShader;bool smallPropsTagged;

        // Snapshot the actual platform baseline, after the existing browser setup.
        void InitializeGraphicsSettings()
        {
            if(graphicsInitialized)return;
            originalAA=QualitySettings.antiAliasing;originalShadows=QualitySettings.shadows;originalVSync=QualitySettings.vSyncCount;
            originalShadowDistance=QualitySettings.shadowDistance;originalShadowResolution=QualitySettings.shadowResolution;
            originalCascades=QualitySettings.shadowCascades;originalLodBias=QualitySettings.lodBias;originalPixelLights=QualitySettings.pixelLightCount;
            graphicsReflection=rideCamera!=null?rideCamera.GetComponent<RiverReflection>():null;
            graphicsFog=rideCamera!=null?rideCamera.GetComponent<MorningFog>():null;
            standardShader=Shader.Find("Standard");
            if(rideCamera!=null)originalFar=rideCamera.farClipPlane;
            if(graphicsReflection!=null)
            {
                originalReflectionScale=graphicsReflection.resolution;originalReflectionSize=graphicsReflection.maxSize;
                originalReflectionInterval=graphicsReflection.interval;originalReflectionFar=graphicsReflection.farClip;
            }
            TagSmallProps();
            graphicsInitialized=true;
            int stored=PlayerPrefs.GetInt(GraphicsPreferenceKey,TouchGraphics?1:0);
            var chosen=stored>=0&&stored<=3?(GraphicsLevel)stored:GraphicsLevel.VeryHigh;
            int drop=chosen!=GraphicsLevel.VeryHigh&&(TouchGraphics||chosen==GraphicsLevel.High)?Mathf.Clamp(PlayerPrefs.GetInt(GraphicsAutoKey,0),0,3):0;
            SelectedGraphics=chosen;
            ApplyGraphicsLevel(GraphicsLevelAt(GraphicsRank(chosen)+drop));
        }
        // Signal lenses and street lamps are many small draws that are barely visible far away. Named meshes only:
        // the road, pavement and markings are combined into "Geometry_*" meshes and must never be culled early (21.138).
        void TagSmallProps()
        {
            if(smallPropsTagged)return;smallPropsTagged=true;
            foreach(var f in GetComponentsInChildren<MeshFilter>(true))
            {
                if(f.sharedMesh==null||f.gameObject.layer!=0)continue;string n=f.sharedMesh.name;
                if(n.StartsWith("PV_SignalLenses",System.StringComparison.Ordinal)||n.StartsWith("PV_LEDStreetLamp",System.StringComparison.Ordinal))f.gameObject.layer=SmallPropLayer;
            }
            foreach(var t in GetComponentsInChildren<TextMesh>(true))if(t.gameObject.layer==0)t.gameObject.layer=SmallPropLayer;
        }
        // Buildings and trees cast from their heaviest meshes: about a million triangles a frame. Vehicles keep their shadows.
        Renderer[] sceneryCasters;bool sceneryShadowsOn=true;
        void SetSceneryShadows(bool on)
        {
            if(on==sceneryShadowsOn||!RoutesReady)return;sceneryShadowsOn=on;
            if(sceneryCasters==null)
            {
                var found=new System.Collections.Generic.List<Renderer>();
                foreach(var row in roadRows)foreach(var r in row.root.GetComponentsInChildren<Renderer>(true))
                    if(r.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.On)found.Add(r);
                sceneryCasters=found.ToArray();
            }
            var mode=on?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
            foreach(var r in sceneryCasters)if(r!=null)r.shadowCastingMode=mode;
        }
        void RestoreGraphicsBaseline()
        {
            if(!graphicsInitialized)return;
            QualitySettings.antiAliasing=originalAA;QualitySettings.shadows=originalShadows;
            QualitySettings.shadowDistance=originalShadowDistance;QualitySettings.shadowResolution=originalShadowResolution;
            QualitySettings.shadowCascades=originalCascades;QualitySettings.lodBias=originalLodBias;QualitySettings.pixelLightCount=originalPixelLights;
            if(graphicsReflection!=null)
            {
                graphicsReflection.enabled=true;
                graphicsReflection.resolution=originalReflectionScale;graphicsReflection.maxSize=originalReflectionSize;
                graphicsReflection.interval=originalReflectionInterval;graphicsReflection.farClip=originalReflectionFar;
            }
            if(rideCamera!=null){rideCamera.farClipPlane=originalFar;rideCamera.layerCullDistances=new float[32];}
            SetSceneryShadows(true);
            if(graphicsFog!=null)graphicsFog.volumeAllowed=true;
            if(daylight!=null){daylight.SetFogReach(1);daylight.SetSingleDirectionalLight(false);}
            if(standardShader!=null)standardShader.maximumLOD=-1;
        }
        void ApplyGraphicsLevel(GraphicsLevel level)
        {
            RestoreGraphicsBaseline();EffectiveGraphics=level;
            if(level==GraphicsLevel.VeryHigh)QualitySettings.vSyncCount=originalVSync;
            if(level==GraphicsLevel.VeryHigh&&!TouchGraphics){RenderPixelCap=0;PhoVietSetPixelCap(0);UpdateTileCulling(true);return;}
            var plan=PlanFor(level);
            QualitySettings.antiAliasing=Mathf.Min(originalAA,plan.antiAliasing);
            bool shadows=plan.shadowDistance>0&&originalShadows!=ShadowQuality.Disable;
            QualitySettings.shadows=shadows?ShadowQuality.HardOnly:ShadowQuality.Disable;
            QualitySettings.shadowDistance=shadows?Mathf.Min(originalShadowDistance,plan.shadowDistance):0;
            QualitySettings.shadowResolution=(ShadowResolution)Mathf.Max(0,(int)originalShadowResolution-1);
            QualitySettings.shadowCascades=0;QualitySettings.lodBias=originalLodBias*plan.lodBias;
            QualitySettings.pixelLightCount=Mathf.Min(originalPixelLights,plan.pixelLights);
            SetSceneryShadows(plan.sceneryShadows);
            if(graphicsReflection!=null)
            {
                graphicsReflection.enabled=plan.reflection;
                graphicsReflection.resolution=Mathf.Min(originalReflectionScale,plan.reflectionScale);
                graphicsReflection.maxSize=Mathf.Min(originalReflectionSize,plan.reflectionSize);
                graphicsReflection.interval=Mathf.Max(originalReflectionInterval,plan.reflectionInterval);
                graphicsReflection.farClip=Mathf.Min(originalReflectionFar,plan.reflectionFar);
            }
            if(rideCamera!=null)
            {
                rideCamera.farClipPlane=Mathf.Min(originalFar,plan.far);
                var cull=new float[32];cull[SmallPropLayer]=plan.smallPropCull;rideCamera.layerCullDistances=cull;rideCamera.layerCullSpherical=true;
            }
            if(graphicsFog!=null)graphicsFog.volumeAllowed=plan.fogVolume;
            // The horizon fades into the sky colour exactly where the shorter view ends, so nothing pops in.
            if(daylight!=null&&rideCamera!=null){daylight.SetFogReach(Mathf.Min(1,rideCamera.farClipPlane/185f));daylight.SetSingleDirectionalLight(true);}
            if(standardShader!=null&&plan.simpleStandard)standardShader.maximumLOD=150;
            RenderPixelCap=plan.pixelCap;PhoVietSetPixelCap(RenderPixelCap);
            UpdateTileCulling(true);
        }
        public void SetGraphicsQuality(int level)
        {
            if(level<0||level>3)return;
            InitializeGraphicsSettings();SelectedGraphics=(GraphicsLevel)level;ApplyGraphicsLevel(SelectedGraphics);
            PlayerPrefs.SetInt(GraphicsPreferenceKey,level);PlayerPrefs.SetInt(GraphicsAutoKey,0);PlayerPrefs.Save();
            ResetGraphicsGovernor();
        }
        // 21.136: a fixed targetFrameRate drives the Web main loop from timers, which drifts against the display and
        // shows as stutter even at a high average. requestAnimationFrame stays locked to the screen.
        void ConfigureFramePacing()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.targetFrameRate=-1;QualitySettings.vSyncCount=1;
#else
            Application.targetFrameRate=60;
#endif
        }
        // Hold a steady frame rate on touch devices and on desktop High. VeryHigh stays fixed.
        // Lowers pixels first, then steps through the level plans,
        // and only comes back up after a long clean stretch. The player's own choice stays selected.
        const int GovernorWindow=45;const float GovernorSlow=.0235f,GovernorClean=.0185f;
        readonly float[] governorSamples=new float[GovernorWindow];
        int governorCount,governorSlowWindows;float governorElapsed,governorCleanTime,governorRaiseAfter=20,governorLastRaise=-1000;
        [System.NonSerialized] public bool forceGraphicsGovernor;
        public int GraphicsGovernorSteps {get;private set;}
        bool graphicsGovernorOff;
        // Fixed-settings measurements and tests can hold the level still.
        [UnityEngine.Scripting.Preserve] public void SetGraphicsGovernor(int enabled){graphicsGovernorOff=enabled==0;ResetGraphicsGovernor();}
        void ResetGraphicsGovernor(){governorCount=0;governorElapsed=0;governorSlowWindows=0;governorCleanTime=0;governorRaiseAfter=20;}
        void TickGraphicsGovernor()
        {
            // VeryHigh is fully fixed: no learned drop, dynamic pixels, levels or governor refresh changes.
            if(SelectedGraphics==GraphicsLevel.VeryHigh)return;
            bool adaptive=(TouchGraphics||forceGraphicsGovernor||SelectedGraphics==GraphicsLevel.High)&&!graphicsGovernorOff;
            if(!graphicsInitialized||(!adaptive&&Application.platform!=RuntimePlatform.WebGLPlayer))return;
            if(State!=RideState.Riding||GraphicsSettingsOpen||Time.timeScale<=0){governorCount=0;governorElapsed=0;return;}
            float dt=Time.unscaledDeltaTime;if(dt>.4f){governorCount=0;governorElapsed=0;return;} // Tab switch or a single load hitch.
            governorSamples[governorCount++]=dt;governorElapsed+=dt;
            if(governorCount<GovernorWindow&&(governorElapsed<1.2f||governorCount<8))return;
            int samples=governorCount;float window=governorElapsed;governorCount=0;governorElapsed=0;
            System.Array.Sort(governorSamples,0,samples);float median=governorSamples[samples/2];
            // A 120 Hz panel would double the load for no visible gain on this kind of game.
            if(median<.0095f&&QualitySettings.vSyncCount==1)QualitySettings.vSyncCount=2;
            if(!adaptive)return;
            if(median>GovernorSlow){governorCleanTime=0;if(++governorSlowWindows>=2||median>.05f){governorSlowWindows=0;GovernorStepDown();}return;}
            governorSlowWindows=0;
            if(median<GovernorClean){governorCleanTime+=window;if(governorCleanTime>=governorRaiseAfter){governorCleanTime=0;GovernorStepUp();}}
        }
        void GovernorStepDown()
        {
            if(SelectedGraphics==GraphicsLevel.VeryHigh)return;
            // Falling back soon after a raise means that raise was too much: wait longer next time.
            if(Time.unscaledTime-governorLastRaise<30)governorRaiseAfter=Mathf.Min(240,governorRaiseAfter*2);
            var plan=PlanFor(EffectiveGraphics);float next=RenderPixelCap*.86f;
            if(next>=plan.pixelFloor){RenderPixelCap=next;PhoVietSetPixelCap(next);GraphicsGovernorSteps++;return;}
            if(EffectiveGraphics!=GraphicsLevel.Low)
            {
                ApplyGraphicsLevel(GraphicsLevelAt(GraphicsRank(EffectiveGraphics)+1));GraphicsGovernorSteps++;
                PlayerPrefs.SetInt(GraphicsAutoKey,GraphicsRank(EffectiveGraphics)-GraphicsRank(SelectedGraphics));PlayerPrefs.Save();return;
            }
            // Nothing left to trim: a steady 30 FPS looks smoother than an uneven 25-40.
            if(RenderPixelCap>plan.pixelFloor){RenderPixelCap=plan.pixelFloor;PhoVietSetPixelCap(RenderPixelCap);GraphicsGovernorSteps++;}
            else if(QualitySettings.vSyncCount==1){QualitySettings.vSyncCount=2;GraphicsGovernorSteps++;}
        }
        void GovernorStepUp()
        {
            if(SelectedGraphics==GraphicsLevel.VeryHigh)return;
            if(QualitySettings.vSyncCount==2&&EffectiveGraphics==GraphicsLevel.Low&&RenderPixelCap<=PlanFor(GraphicsLevel.Low).pixelFloor)return; // Holding the steady 30.
            var plan=PlanFor(EffectiveGraphics);governorLastRaise=Time.unscaledTime;
            if(RenderPixelCap<plan.pixelCap-.001f){RenderPixelCap=Mathf.Min(plan.pixelCap,RenderPixelCap/.86f);PhoVietSetPixelCap(RenderPixelCap);return;}
            if(GraphicsRank(EffectiveGraphics)>GraphicsRank(SelectedGraphics))
            {
                ApplyGraphicsLevel(GraphicsLevelAt(GraphicsRank(EffectiveGraphics)-1));
                RenderPixelCap=PlanFor(EffectiveGraphics).pixelFloor;PhoVietSetPixelCap(RenderPixelCap);
                PlayerPrefs.SetInt(GraphicsAutoKey,GraphicsRank(EffectiveGraphics)-GraphicsRank(SelectedGraphics));PlayerPrefs.Save();
            }
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
            else if(Input.GetKeyDown(KeyCode.Alpha2))SetGraphicsQuality((int)GraphicsLevel.High);
            else if(Input.GetKeyDown(KeyCode.Alpha3))SetGraphicsQuality((int)GraphicsLevel.Medium);
            else if(Input.GetKeyDown(KeyCode.Alpha4))SetGraphicsQuality((int)GraphicsLevel.Low);
        }
        public static Rect WelcomeSettingsRect(Rect card)=>new Rect(card.xMax-60,card.y+8,44,44);
        public static Rect GraphicsWindowRect(float width,float height,bool narrow)
        {
            float w=Mathf.Min(narrow?432:560,width-32),h=Mathf.Min(500,height-32);
            return new Rect((width-w)/2,(height-h)/2,w,h);
        }
        public static Rect GraphicsChoiceRect(Rect window,int row)
        {float stride=Mathf.Min(76,(window.height-190)/4);return new Rect(window.x+22,window.y+86+row*stride,window.width-44,stride-10);}
        public static Rect GraphicsDoneRect(Rect window)=>new Rect(window.x+22,window.yMax-66,window.width-44,44);
        public static Rect GraphicsCloseRect(Rect window)=>new Rect(window.xMax-54,window.y+12,40,40);
        bool GraphicsTap(Vector2 point,float width,float height,bool narrow)
        {
            var window=GraphicsWindowRect(width,height,narrow);
            if(GraphicsDoneRect(window).Contains(point)||GraphicsCloseRect(window).Contains(point)||!window.Contains(point))CloseGraphicsSettings();
            else for(int i=0;i<4;i++)if(GraphicsChoiceRect(window,i).Contains(point)){SetGraphicsQuality((int)GraphicsLevelAt(i));break;}
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
            for(int i=0;i<4;i++)
            {
                var level=GraphicsLevelAt(i);bool selected=SelectedGraphics==level;var choice=GraphicsChoiceRect(r,i);bool compact=choice.height<50;
                if(HudButton("",choice,selected?Teal:new Color(.86f,.9f,.93f),Ink,18))SetGraphicsQuality((int)level);
                Panel(new Rect(choice.x+1.5f,choice.y+1.5f,choice.width-3,choice.height-3),selected?new Color(.89f,.96f,.96f):new Color(.96f,.975f,.985f),10);
                float cy=choice.center.y;
                Panel(new Rect(choice.x+17,cy-10,20,20),selected?Teal:new Color(.65f,.73f,.77f),10);
                Panel(new Rect(choice.x+19,cy-8,16,16),selected?Color.white:new Color(.96f,.975f,.985f),8);
                if(selected)Panel(new Rect(choice.x+23,cy-4,8,8),Teal,4);
                Txt(GraphicsName(level),choice.x+51,compact?cy-11:choice.y+6,choice.width-66,24,compact?16:19,Ink);
                if(!compact)Txt(i==0?"Đẹp nhất, giữ cố định, không tự giảm.":i==1?"Đẹp hơn Trung bình, tự giảm khi máy yếu.":i==2?"Cân bằng giữa độ đẹp và độ mượt.":"Mượt nhất, ưu tiên cho máy yếu.",choice.x+51,choice.y+32,choice.width-66,22,13,Teal);
            }
            Txt("Lựa chọn được lưu cho lần chơi sau.",r.x+22,r.yMax-100,r.width-44,24,13,Teal,TextAnchor.MiddleCenter);
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
