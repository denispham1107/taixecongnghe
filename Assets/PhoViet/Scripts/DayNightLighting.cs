using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet
{
    // Blender-authored optics, with a bounded pool of real lights for WebGL/mobile.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(200)] // Follow the final bike suspension/jump pose from LateUpdate.
    public sealed class DayNightLighting : MonoBehaviour
    {
        [Serializable] public sealed class Halo { public Transform source; public Renderer glow; public bool rear; public float size=.7f; }
        [Serializable] public sealed class VehicleLamps { public Transform root,head; public Renderer roadLight; public Halo[] halos; public bool player; }
        [Serializable] public sealed class StreetLamp { public Transform source; public Renderer pool,glow; }
        [Serializable] sealed class Profile
        {
            public float halfCycleSeconds=30,sunIntensity=1.12f,moonIntensity=.24f;
            public string daySun="FFFFFF",nightMoon="A9C6FF",daySky="C3DFF9",dayHorizon="F0F7FF",nightSky="071329",nightHorizon="172846";
            public string dayAmbientSky="E4F1FF",dayAmbientEquator="D9E4EF",dayAmbientGround="ADB7C2",nightAmbientSky="263D67",nightAmbientEquator="182841",nightAmbientGround="0E172B";
        }
        public PhoVietGame game;
        public TextAsset authoredProfile;
        public Light sun,moon;
        public Material sky,streetLens;
        public Cubemap neutralReflection;
        public Material[] emissionMaterials=Array.Empty<Material>();
        public Renderer[] emissionUsers=Array.Empty<Renderer>();
        public VehicleLamps[] vehicles=Array.Empty<VehicleLamps>();
        public StreetLamp[] streets=Array.Empty<StreetLamp>();
        public Light[] lightPool=Array.Empty<Light>();
        public float CycleTime {get;private set;}
        public float NightLevel {get;private set;}
        public float LampLevel {get;private set;}
        public int ActiveRealtimeLights {get;private set;}
        public float CycleSeconds=>profile.halfCycleSeconds*2;
        Profile profile=new Profile();
        bool initialized,jailMode;
        Quaternion solarFrame=Quaternion.identity;
        Material runtimeSky,runtimeLens;
        Material[] managedEmission;
        readonly List<Material> owned=new List<Material>();
        MaterialPropertyBlock haloBlock,projectionBlock,streetBlock;
        Color daySky,dayHorizon,nightSky,nightHorizon,dayAmbientSky,dayAmbientEquator,dayAmbientGround,nightAmbientSky,nightAmbientEquator,nightAmbientGround,moonColor;
        readonly float[] scores=new float[6];
        readonly Vector3[] origins=new Vector3[6],directions=new Vector3[6];
        readonly bool[] streetKind=new bool[6];
        static readonly int Tint=Shader.PropertyToID("_Tint"),NightLights=Shader.PropertyToID("_PVNightLights");
        static readonly int ProjectHeadlight=Shader.PropertyToID("_ProjectHeadlight"),WorldToHead=Shader.PropertyToID("_WorldToHead"),HeadPosition=Shader.PropertyToID("_HeadPosition");

        public void Initialize(PhoVietGame owner)
        {
            if(initialized)return;game=owner;
            if(authoredProfile!=null)profile=JsonUtility.FromJson<Profile>(authoredProfile.text);
            profile.halfCycleSeconds=Mathf.Max(1,profile.halfCycleSeconds);
            daySky=C(profile.daySky);dayHorizon=C(profile.dayHorizon);nightSky=C(profile.nightSky);nightHorizon=C(profile.nightHorizon);
            dayAmbientSky=C(profile.dayAmbientSky);dayAmbientEquator=C(profile.dayAmbientEquator);dayAmbientGround=C(profile.dayAmbientGround);
            nightAmbientSky=C(profile.nightAmbientSky);nightAmbientEquator=C(profile.nightAmbientEquator);nightAmbientGround=C(profile.nightAmbientGround);moonColor=C(profile.nightMoon);
            runtimeSky=sky;runtimeLens=streetLens;managedEmission=emissionMaterials;haloBlock=new MaterialPropertyBlock();projectionBlock=new MaterialPropertyBlock();streetBlock=new MaterialPropertyBlock();
            if(Application.isPlaying)
            {
                var map=new Dictionary<Material,Material>();
                foreach(var material in emissionMaterials)if(material!=null&&!map.ContainsKey(material)){var copy=new Material(material){name=material.name};map.Add(material,copy);owned.Add(copy);}
                if(streetLens!=null){runtimeLens=new Material(streetLens);map[streetLens]=runtimeLens;owned.Add(runtimeLens);}
                foreach(var renderer in emissionUsers)
                {
                    if(renderer==null)continue;var materials=renderer.sharedMaterials;bool changed=false;
                    for(int i=0;i<materials.Length;i++)if(materials[i]!=null&&map.TryGetValue(materials[i],out var copy)){materials[i]=copy;changed=true;}
                    if(changed)renderer.sharedMaterials=materials;
                }
                managedEmission=new Material[emissionMaterials.Length];for(int i=0;i<managedEmission.Length;i++)managedEmission[i]=emissionMaterials[i]!=null?map[emissionMaterials[i]]:null;
                if(sky!=null){runtimeSky=new Material(sky);owned.Add(runtimeSky);}
            }
            initialized=true;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=neutralReflection;
            RenderSettings.skybox=runtimeSky;
        }
        static Color C(string hex)=>StreetArt.Hex(hex);
        public void ResetCycle()
        {
            Initialize(game!=null?game:GetComponent<PhoVietGame>());solarFrame=Quaternion.identity;jailMode=false;
            if(sun!=null)sun.enabled=true;if(moon!=null)moon.enabled=true;SetCycleTime(0);
        }
        public void Advance(float dt)
        {
            if(!initialized)Initialize(game!=null?game:GetComponent<PhoVietGame>());
            if(game==null||game.State!=PhoVietGame.RideState.Riding||game.JobOffersOpen||dt<=0)return;
            SetCycleTime(CycleTime+dt);
        }
        // Also used by the artist's day/dusk/night preview and deterministic captures.
        public void SetCycleTime(float seconds)
        {
            if(!initialized)Initialize(game!=null?game:GetComponent<PhoVietGame>());
            CycleTime=Mathf.Repeat(seconds,CycleSeconds);
            NightLevel=.5f-.5f*Mathf.Cos(CycleTime*Mathf.PI/profile.halfCycleSeconds);
            LampLevel=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.26f,.66f,NightLevel));ApplyEnvironment();UpdateLamps();
        }
        public void Rebase(Quaternion inverse,Vector3 origin)
        {
            solarFrame=inverse*solarFrame;
            foreach(var lamp in lightPool)if(lamp!=null){lamp.transform.localPosition=inverse*(lamp.transform.localPosition-origin);lamp.transform.localRotation=inverse*lamp.transform.localRotation;}
            ApplyEnvironment();UpdateLamps();
        }
        void ApplyEnvironment()
        {
            if(game==null)return;
            float twilight=Mathf.Sin(NightLevel*Mathf.PI);
            if(sun!=null)
            {
                sun.transform.localRotation=solarFrame*Quaternion.Euler(Mathf.Lerp(52,-12,NightLevel),-32,0);
                sun.color=Color.Lerp(C(profile.daySun),C("F2B9BF"),twilight*.35f);
                sun.intensity=profile.sunIntensity*(1-NightLevel)*(1-NightLevel);RenderSettings.sun=sun;
            }
            if(moon!=null){moon.transform.localRotation=solarFrame*Quaternion.Euler(48,118,0);moon.color=moonColor;moon.intensity=profile.moonIntensity*NightLevel;}
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=Color.Lerp(dayAmbientSky,nightAmbientSky,NightLevel);
            RenderSettings.ambientEquatorColor=Color.Lerp(dayAmbientEquator,nightAmbientEquator,NightLevel);
            RenderSettings.ambientGroundColor=Color.Lerp(dayAmbientGround,nightAmbientGround,NightLevel);
            RenderSettings.reflectionIntensity=Mathf.Lerp(.45f,.13f,NightLevel);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=Mathf.Lerp(85,65,NightLevel);RenderSettings.fogEndDistance=Mathf.Lerp(185,150,NightLevel);
            Color horizon=Color.Lerp(dayHorizon,nightHorizon,NightLevel);RenderSettings.fogColor=horizon;
            if(runtimeSky!=null)
            {
                runtimeSky.SetColor("_Zenith",Color.Lerp(daySky,nightSky,NightLevel));runtimeSky.SetColor("_Horizon",horizon);
                runtimeSky.SetFloat("_Night",NightLevel);runtimeSky.SetVector("_SunDirection",sun!=null?-sun.transform.forward:Vector3.up);
                runtimeSky.SetVector("_MoonDirection",moon!=null?-moon.transform.forward:Vector3.up);runtimeSky.SetMatrix("_SkyFrame",Matrix4x4.Rotate(Quaternion.Inverse(solarFrame)));
            }
            if(game.rideCamera!=null){game.rideCamera.backgroundColor=horizon;game.rideCamera.clearFlags=CameraClearFlags.Skybox;}
            Shader.SetGlobalFloat(NightLights,LampLevel);
            foreach(var material in managedEmission)if(material!=null){material.SetColor("_EmissionColor",Color.white*(LampLevel*2.1f));material.EnableKeyword("_EMISSION");}
            if(runtimeLens!=null){runtimeLens.SetColor("_EmissionColor",Color.white*(LampLevel*6));runtimeLens.EnableKeyword("_EMISSION");}
        }
        void LateUpdate()
        {
            if(!initialized||game==null)return;
            if(game.State==PhoVietGame.RideState.Jailed)
            {
                if(!jailMode)
                {
                    jailMode=true;if(sun!=null)sun.enabled=false;if(moon!=null)moon.enabled=false;
                    if(game.rideCamera!=null)game.rideCamera.clearFlags=CameraClearFlags.SolidColor;
                    UpdateLamps(); // Hide detached road spills/halos as well as the real lights.
                    RenderSettings.ambientSkyColor=C("CAD7E4");RenderSettings.ambientEquatorColor=C("AABACB");RenderSettings.ambientGroundColor=C("7C8793");
                }
                foreach(var light in lightPool)if(light!=null)light.enabled=false;ActiveRealtimeLights=0;return;
            }
            UpdateLamps();
        }
        public void UpdateLamps()
        {
            if(game==null||game.rideCamera==null)return;
            bool enabled=LampLevel>.001f&&game.State!=PhoVietGame.RideState.Jailed;
            for(int i=0;i<6;i++)scores[i]=float.PositiveInfinity;
            var camera=game.rideCamera.transform;Vector3 eye=camera.position,focus=game.rider.position;
            foreach(var vehicle in vehicles)
            {
                if(vehicle.root==null||vehicle.head==null)continue;
                bool on=enabled&&vehicle.root.gameObject.activeInHierarchy;
                if(vehicle.roadLight!=null)
                {
                    vehicle.roadLight.enabled=on;
                    if(on)
                    {
                        // The receiver remains on the road. Project the Blender cookie from the
                        // animated lamp, rather than pasting an unchanged patch beneath a jump.
                        var p=vehicle.head.position;
                        bool expanded=p.y-transform.position.y>1.5f||Vector3.Dot(vehicle.head.up,Vector3.up)<.98f;
                        if(!expanded){var forward=vehicle.head.forward;forward.y=0;if(forward.sqrMagnitude>.01f)p+=forward.normalized*12;}
                        p.y=transform.position.y+.035f;
                        vehicle.roadLight.transform.SetPositionAndRotation(p,Quaternion.identity);
                        // Keep ordinary traffic receivers small to limit mobile overdraw.
                        // The projection is in world space, so receiver orientation is immaterial.
                        if(!expanded){var forward=vehicle.head.forward;forward.y=0;if(forward.sqrMagnitude>.01f)vehicle.roadLight.transform.rotation=Quaternion.LookRotation(forward,Vector3.up);}
                        vehicle.roadLight.transform.localScale=new Vector3(expanded?64:32,1,expanded?64:32);
                        projectionBlock.SetFloat(ProjectHeadlight,1);
                        projectionBlock.SetMatrix(WorldToHead,Matrix4x4.TRS(vehicle.head.position,vehicle.head.rotation,Vector3.one).inverse);
                        projectionBlock.SetVector(HeadPosition,vehicle.head.position);
                        vehicle.roadLight.SetPropertyBlock(projectionBlock);
                    }
                }
                foreach(var halo in vehicle.halos)
                {
                    if(halo.source==null||halo.glow==null)continue;
                    Vector3 p=halo.source.position,toCamera=eye-p;
                    float facing=Mathf.Max(0,Vector3.Dot(halo.rear?-halo.source.forward:halo.source.forward,toCamera.normalized));
                    halo.glow.enabled=on&&facing>.06f;
                    if(halo.glow.enabled)
                    {
                        halo.glow.transform.SetPositionAndRotation(p,Quaternion.LookRotation(toCamera,Vector3.up));halo.glow.transform.localScale=Vector3.one*halo.size;
                        Color tint=halo.rear?C("FF2639"):C("EDF8FF");tint.a=Mathf.Pow(facing,2)*.5f;
                        haloBlock.SetColor(Tint,tint);halo.glow.SetPropertyBlock(haloBlock);
                    }
                }
                if(on)
                {
                    Vector3 direction=(vehicle.head.forward-vehicle.head.up*.085f).normalized;
                    float score=vehicle.player?-100000:(vehicle.head.position-focus).sqrMagnitude+25;
                    if(score<3000)Offer(score,vehicle.head.position,direction,false,0,3);
                }
            }
            foreach(var street in streets)
            {
                if(street.source==null)continue;bool on=enabled&&street.source.gameObject.activeInHierarchy;
                if(street.pool!=null)
                {
                    street.pool.enabled=on;
                    streetBlock.SetColor(Tint,new Color(.83f,.92f,1,.44f));street.pool.SetPropertyBlock(streetBlock);
                }
                if(street.glow!=null)
                {
                    street.glow.enabled=on;
                    if(on)
                    {
                        street.glow.transform.SetPositionAndRotation(street.source.position,Quaternion.LookRotation(eye-street.source.position,Vector3.up));
                        haloBlock.SetColor(Tint,new Color(.88f,.95f,1,.7f));street.glow.SetPropertyBlock(haloBlock);
                    }
                }
                float score=(street.source.position-focus).sqrMagnitude;
                // Reserve two sources so dense traffic cannot extinguish nearby street lamps.
                if(on&&score<2600)Offer(score,street.source.position,Vector3.down,true,4,5);
            }
            ActiveRealtimeLights=0;
            for(int i=0;i<lightPool.Length;i++)
            {
                var light=lightPool[i];if(light==null)continue;bool use=enabled&&i<6&&!float.IsPositiveInfinity(scores[i]);light.enabled=use;
                if(!use)continue;ActiveRealtimeLights++;
                light.transform.SetPositionAndRotation(origins[i],Quaternion.LookRotation(directions[i],streetKind[i]?Vector3.forward:Vector3.up));
                light.spotAngle=streetKind[i]?99:58;light.range=streetKind[i]?20:32;light.intensity=(streetKind[i]?3.6f:2.8f)*LampLevel;
                light.renderMode=i==0||streetKind[i]?LightRenderMode.ForcePixel:LightRenderMode.Auto;
                light.color=C("F1F7FF");light.shadows=i==0&&!streetKind[i]?LightShadows.Hard:LightShadows.None;
            }
        }
        void Offer(float score,Vector3 origin,Vector3 direction,bool street,int first,int last)
        {
            int at=last;if(score>=scores[at])return;
            while(at>first&&score<scores[at-1]){scores[at]=scores[at-1];origins[at]=origins[at-1];directions[at]=directions[at-1];streetKind[at]=streetKind[at-1];at--;}
            scores[at]=score;origins[at]=origin;directions[at]=direction;streetKind[at]=street;
        }
        void OnDestroy(){foreach(var material in owned)if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}}
    }
}
