using System;
using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame : MonoBehaviour
    {
        public enum RideState { Ready, Riding, Paused, Falling, GameOver, Arrested, Jailed }
        [Serializable] public partial class TrafficVehicle
        {
            public Transform model;
            public int lane;
            public float speed, halfWidth, halfLength;
            public bool active;
            public Vector3 startPosition;
            public bool crossing;
            public float crossSpeed, crossDelay;
            public bool knocked;
            public Vector3 knockVelocity, previousPosition;
            public float knockAge, knockDirection, initialYaw, playerGrace;
            public float baseTrafficSpeed,rideAlongRemaining;
            public bool hasPaced;
            public bool eventOnly,isReckless,wallCrashed,willHitWall,knockFromPlayer;
            public float recklessAge,wallSide,wallCrashAt,swerveDirection;
            public bool enteringFromBehind;
            public float swerveFrequency,swerveAmplitude,swerveSpeed,rearStartX;
            public Transform crashEffect;
            public ParticleSystem smoke,fire;
            public float brakeRemaining,brakeCooldown,laneCooldown,changeProgress,changeDuration,changeFromX,targetX;
            public bool changingLane,turning,hasTurned,physicalCrash;
            public int turnChoice;
            public float turnStartX,turnOriginZ,turnRadius,turnAngle,turnEndX,turnZSign;
            public Vector3 motionVelocity,crashEuler,angularVelocity;
            public float previousYaw;
            public bool routeDetached;
            public Vector3 routeVelocity;
            public Vector3 routeEffectOffset;
            public float rearEntryBoost;
            public int jobYieldLane=-1;
            public bool roadTurning;
            public float roadTurnAngle,roadTurnRadius,roadTurnSpeed,roadDecisionAt;
            public Vector3 roadTurnStart,roadTurnForward,roadTurnSide;
            [NonSerialized] public Transform brakeLights;
        }
        public Font vietnameseFont;
        public Material worldTextMaterial;
        public Transform scenery, rider;
        public Camera rideCamera;
        public List<Transform> blocks=new List<Transform>();
        public List<Renderer> trafficLights=new List<Renderer>();
        public List<TrafficVehicle> vehicles=new List<TrafficVehicle>();
        public RideState State { get; private set; }=RideState.Ready;
        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public int CurrentLane { get; private set; }=2;
        public int PassedVehicles { get; private set; }
        public int BestDistance { get; private set; }
        public int Frames {get;private set;}
        public int ScheduledIntersections {get;private set;}
        public const float DifficultyMultiplier=3f;
        public float DifficultyTime => timeRiding*DifficultyMultiplier;
        public float Difficulty01 => Mathf.Clamp01(DifficultyTime/110f);
        public bool OpeningTrafficReduced => timeRiding<10f;
        float TrafficDensity => OpeningTrafficReduced?.5f:1f;
        int TrafficCount(int count)=>Mathf.CeilToInt(count*TrafficDensity);
        public bool sound=true;
        public string CurrentStreet => RouteStreet;
        public static readonly float[] LaneX={-4.65f,-1.55f,1.55f,4.65f};
        float spawnTimer, fallTime, timeRiding, lastX, lean, streak, noticeTimer;
        float nextIntersectionDistance;
        Vector2 swipeStart; bool dragging;
        System.Random rng=new System.Random();
        public int RunSeed {get;private set;}
        AudioSource engine,fx; AudioClip crashClip;
        GUIStyle label,button; bool stylesReady;
        float uiW,uiH,uiScale; bool portrait;
        string notice=""; int signalState=-1;
        const string BestKey="PhoViet.BestMetres.v1";

        void Awake()
        {
            StreetArt.ConfigureWorldText(vietnameseFont,worldTextMaterial); Application.targetFrameRate=60; Application.runInBackground=true;
            BestDistance=PlayerPrefs.GetInt(BestKey,0);
            CreateSound(); InitializeActions(); ResetRide();
        }
        public void BuildWorld()
        {
            StreetArt.Font=vietnameseFont;
            scenery=Group("PHỐ VIỆT • thành phố",transform);
            for(int i=0;i<7;i++)blocks.Add(VietnamStreet.Build(scenery,i,trafficLights));
            rider=Motorcycle(transform,"Tài xế • áo đỏ sao vàng",Red,Red,true);
            rider.localPosition=new Vector3(LaneX[2],0,0);
            // A fixed pool avoids instantiation, destruction and per-frame garbage during a run.
            Color[] colors={Teal,Hex("C45740"),Hex("E4C05C"),Hex("7F9FA8"),Cream,Hex("5C6C8A")};
            for(int i=0;i<24;i++)
            {
                bool car=i%3==0;var t=car?Car(transform,colors[i%6],i%6==0,i%9==3):Motorcycle(transform,"Xe máy lưu thông",colors[i%6],colors[(i+2)%6],false);
                t.gameObject.SetActive(false);
                vehicles.Add(new TrafficVehicle {model=t,halfWidth=car?.94f:.43f,halfLength=car?2.45f:1.1f});
            }
            var cameraGo=new GameObject("Main Camera");cameraGo.transform.SetParent(transform,false);rideCamera=cameraGo.AddComponent<Camera>();cameraGo.tag="MainCamera";
            cameraGo.AddComponent<AudioListener>();rideCamera.fieldOfView=55;rideCamera.nearClipPlane=.15f;rideCamera.farClipPlane=190;rideCamera.backgroundColor=Hex("BCD8D5");
            rideCamera.clearFlags=CameraClearFlags.SolidColor;rideCamera.allowHDR=true;
            PositionCamera(true);
        }
        void CreateSound()
        {
            engine=gameObject.AddComponent<AudioSource>();engine.loop=true;engine.volume=.08f;engine.playOnAwake=false;
            int hz=22050;float[] wave=new float[hz];
            for(int i=0;i<wave.Length;i++){float t=(float)i/hz;wave[i]=Mathf.Sin(t*2*Mathf.PI*60)*.45f+Mathf.Sin(t*2*Mathf.PI*120)*.17f+Mathf.Sin(t*2*Mathf.PI*180)*.08f;}
            var clip=AudioClip.Create("Động cơ xe máy",wave.Length,1,hz,false);clip.SetData(wave,0);engine.clip=clip;
            fx=gameObject.AddComponent<AudioSource>();fx.playOnAwake=false;fx.volume=.3f;
            float[] hit=new float[hz/3];var noise=new System.Random(4);
            for(int i=0;i<hit.Length;i++)hit[i]=((float)noise.NextDouble()*2-1)*Mathf.Exp(-i/(hz*.07f));
            crashClip=AudioClip.Create("Va chạm",hit.Length,1,hz,false);crashClip.SetData(hit,0);
        }
        public void ResetRide()
        {
            CancelSwipe();
            int seed;do{seed=Guid.NewGuid().GetHashCode()&int.MaxValue;}while(seed==RunSeed);
            RunSeed=seed;rng=new System.Random(seed);
            State=RideState.Ready;Distance=0;Speed=0;CurrentLane=2;PassedVehicles=0;timeRiding=0;fallTime=0;streak=0;lean=0;noticeTimer=0;
            nextIntersectionDistance=VietnamStreet.IntersectionCenter;ScheduledIntersections=0;
            ResetCityJobs();
            ResetRoutes();
            ResetActions();
            ResetReckless();
            ResetTrafficDynamics();
            ResetRiderImpact();
            ResetJump();
            ResetBoost();
            if(rider!=null){rider.localPosition=new Vector3(LaneX[2],0,0);rider.localRotation=Quaternion.identity;}
            lastX=LaneX[2];
            for(int i=0;i<blocks.Count;i++)blocks[i].localPosition=new Vector3(0,0,(i-1)*VietnamStreet.Length);
            foreach(var v in vehicles){v.active=false;v.crossing=false;v.crossSpeed=0;v.crossDelay=0;v.knocked=false;v.knockAge=0;v.playerGrace=0;v.model.gameObject.SetActive(false);}
            float openingZ=30+(float)rng.NextDouble()*10;
            int openingCount=TrafficCount(rng.Next(5,8));
            for(int i=0;i<openingCount;i++){SpawnTraffic(rng.Next(4),openingZ);openingZ+=11+(float)rng.NextDouble()*10;}
            spawnTimer=.7f+(float)rng.NextDouble()*1.1f;if(engine!=null)engine.Stop();ResetDaylight();PositionCamera(true);
        }
        public void StartRide() { if(State==RideState.Ready){State=RideState.Riding;Speed=17;engine?.Play();} }
        public void RestartRide(){ResetRide();StartRide();}
        public void MoveLane(int direction)
        {
            if(State!=RideState.Riding||ServiceStopped)return;
            if(SelectJunctionTurn(direction))return;
            CurrentLane=Mathf.Clamp(CurrentLane+Math.Sign(direction),0,3);
        }
        public void TogglePause()
        {
            if(JobOffersOpen)return;
            CancelSwipe();
            if(State==RideState.Riding||State==RideState.Falling){pauseFromState=State;State=RideState.Paused;engine.Pause();if(siren!=null)siren.Pause();navigationAudio?.Pause();PauseRecklessFx(true);}
            else if(State==RideState.Paused){State=pauseFromState;PauseRecklessFx(false);if(sound){if(State==RideState.Riding){engine.UnPause();navigationAudio?.UnPause();}if(IsChased)siren?.UnPause();}}
        }
        public TrafficVehicle SpawnTraffic(int lane,float z,int preferred=-1)
        {
            TrafficVehicle v=null;
            if(preferred>=0 && preferred<vehicles.Count && !vehicles[preferred].active&&!vehicles[preferred].eventOnly&&!RearExitReserved(vehicles[preferred]))v=vehicles[preferred];
            if(v==null){int count=0;foreach(var t in vehicles)if(!t.active&&!t.eventOnly&&!RearExitReserved(t)&&rng.Next(++count)==0)v=t;}
            if(v==null)return null;
            v.crossing=false;v.crossSpeed=0;v.crossDelay=0;v.knocked=false;v.knockAge=0;v.playerGrace=0;v.hasPaced=false;v.rideAlongRemaining=0;
            v.lane=lane;v.active=true;v.speed=lane<2?-rng.Next(10,17):rng.Next(7,12);
            v.baseTrafficSpeed=v.speed;
            v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(LaneX[lane],0,z);v.model.localRotation=Quaternion.Euler(0,lane<2?180:0,0);
            InitializeDriver(v);
            return v;
        }
        public TrafficVehicle SpawnCrossTraffic(int direction,float z,bool car,float crossSpeed,float delay=0)
        {
            TrafficVehicle v=null;
            // Reuse the existing saved models, selecting the correct vehicle silhouette and bounds.
            int candidates=0;
            foreach(var candidate in vehicles)
                if(!candidate.active && !candidate.eventOnly&&!RearExitReserved(candidate) && (candidate.halfWidth>.7f)==car&&rng.Next(++candidates)==0)v=candidate;
            if(v==null)return null;
            direction=direction>=0?1:-1;
            v.active=true;v.crossing=true;v.lane=-1;v.speed=0;v.knocked=false;v.knockAge=0;v.playerGrace=0;
            v.crossSpeed=direction*Mathf.Abs(crossSpeed);v.crossDelay=Mathf.Max(0,delay);
            v.model.localPosition=new Vector3(-direction*16,0,z);
            v.model.localRotation=Quaternion.Euler(0,direction*90,0);v.model.gameObject.SetActive(true);
            InitializeDriver(v);v.turnChoice=rng.NextDouble()<.75?(rng.Next(2)==0?-1:1):0;
            return v;
        }
        float TimeToIntersection(float metres)
        {
            float acceleration=.12f*DifficultyMultiplier+(IsChased?.28f:0),cap=30+(IsChased?chaseBoost:0);
            float speed=Mathf.Max(Speed,17);
            if(BoostActive)
            {
                float boostedSpeed=Mathf.Max(Speed,17*BoostMultiplier),boostedAcceleration=acceleration*BoostMultiplier,boostedCap=cap*BoostMultiplier;
                float fastTime=TravelTimeToIntersection(metres,boostedSpeed,boostedAcceleration,boostedCap);
                if(fastTime<=boostRemaining)return fastTime;
                float toCap=Mathf.Max(0,(boostedCap-boostedSpeed)/boostedAcceleration),ramp=Mathf.Min(toCap,boostRemaining);
                float travelled=boostedSpeed*ramp+.5f*boostedAcceleration*ramp*ramp+boostedCap*(boostRemaining-ramp);
                float afterSpeed=Mathf.Min(boostedCap,boostedSpeed+boostedAcceleration*boostRemaining)/BoostMultiplier;
                return boostRemaining+TravelTimeToIntersection(Mathf.Max(0,metres-travelled),afterSpeed,acceleration,cap);
            }
            return TravelTimeToIntersection(metres,speed,acceleration,cap);
        }
        static float TravelTimeToIntersection(float metres,float speed,float acceleration,float cap)
        {
            float toCap=Mathf.Max(0,(cap-speed)/acceleration);
            float beforeCap=speed*toCap+.5f*acceleration*toCap*toCap;
            if(metres>=beforeCap)return toCap+(metres-beforeCap)/cap;
            return (Mathf.Sqrt(speed*speed+2*acceleration*Mathf.Max(0,metres))-speed)/acceleration;
        }
        void ScheduleIntersection()
        {
            // Absolute route distance makes each real intersection fire once, including recycled blocks.
            while(nextIntersectionDistance-Distance<=90)
            {
                float centre=nextIntersectionDistance-Distance;
                int count=TrafficCount(DifficultyTime<18?2:DifficultyTime<60?3:5);
                int firstDirection=rng.Next(2)==0?1:-1;
                float targetX=LaneX[rng.Next(4)];
                for(int i=0;i<count;i++)
                {
                    int direction=i%2==0?firstDirection:-firstDirection;
                    // Two actual cross-street lanes, kept inside its 14 metre width.
                    float z=centre+(direction>0?-2.8f:2.8f);
                    float velocity=(Mathf.Lerp(6.5f,11f,Difficulty01)+i*.35f)*(.95f+(float)rng.NextDouble()*.15f);
                    float travel=(16+direction*targetX)/velocity;
                    float delay=Mathf.Max(0,TimeToIntersection(z)-travel)+(i>=2?.7f:0);
                    SpawnCrossTraffic(direction,z,rng.NextDouble()<Mathf.Lerp(.2f,.7f,Difficulty01),velocity,delay);
                }
                ScheduledIntersections++;nextIntersectionDistance+=VietnamStreet.Length;
            }
        }
        public static bool SweptOverlap(Vector2 from,Vector2 to,Vector2 halfSize)
        {
            float enter=0,leave=1;
            for(int axis=0;axis<2;axis++)
            {
                float d=to[axis]-from[axis],p=from[axis],extent=halfSize[axis];
                if(Mathf.Abs(d)<.000001f){if(Mathf.Abs(p)>extent)return false;continue;}
                float a=(-extent-p)/d,b=(extent-p)/d;
                enter=Mathf.Max(enter,Mathf.Min(a,b));leave=Mathf.Min(leave,Mathf.Max(a,b));
                if(enter>leave)return false;
            }
            return true;
        }
        void Update()
        {
            Frames++;
            ReadHudTouch();
            bool wasRiding=State==RideState.Riding;
            if(State==RideState.Ready && (Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)))StartRide();
            else if((State==RideState.GameOver||State==RideState.Jailed) && (Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.Space)))RestartRide();
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P))TogglePause();
            if(State==RideState.Riding)
            {
                if(wasRiding&&!ServiceStopped&&Input.GetKeyDown(KeyCode.Q))TryKick();
                if(wasRiding&&Input.GetKeyDown(KeyCode.Space))TryJump();
                if(wasRiding&&Input.GetKeyDown(KeyCode.W))TryBoost();
                if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))MoveLane(-1);
                if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))MoveLane(1);
                ReadSwipe(); TickRide(Mathf.Min(Time.deltaTime,.05f));
            }
            UpdateWork(Mathf.Min(Time.deltaTime,.05f));
            if(State==RideState.Falling)TickRiderCrash(Mathf.Min(Time.deltaTime,.05f));
            if(noticeTimer>0)noticeTimer-=Time.deltaTime;
            if(State==RideState.Arrested)TickArrest(Time.deltaTime);
            AnimateLights();PositionCamera(false);
            UpdateJobMarker();
            UpdateDangerSigns();
            if(engine!=null){engine.mute=!sound;engine.pitch=.85f+Speed/45f;}
            if(siren!=null)siren.mute=!sound;
        }
        void TickRide(float dt)
        {
            AdvanceDaylight(dt);
            TickWorkMotion(dt);
            TickActionTimers(dt);
            TickJump(dt);
            TickBoost(dt);
            if(!ServiceStopped)timeRiding+=dt;cruiseSpeed=Mathf.Min(17+DifficultyTime*.12f,30)+(IsChased?chaseBoost:0);
            Speed=cruiseSpeed;
            if(jumpFlying&&!ServiceStopped)Speed+=3;
            if(BoostActive)Speed*=BoostMultiplier;
            Speed=WorkSpeedLimit(Speed);
            Speed=JobAcceptanceSpeedLimit(Speed);
            Distance+=Speed*dt;ScrollRoutes(Speed*dt);
            float oldX=rider.localPosition.x;
            oldRiderZ=rider.localPosition.z;
            oldRiderYaw=IsTurning?TurnYaw:0;
            float x=ServiceStopped?oldX:Mathf.MoveTowards(oldX,LaneX[CurrentLane],dt*15);
            lean=Mathf.Lerp(lean,(oldX-x)*-35,dt*10);
            if(!IsTurning){rider.localPosition=new Vector3(x,jumpHeight,0);rider.localRotation=Quaternion.Euler(0,0,lean);}
            BeginOrContinueTurn(Speed*dt);
            {var p=rider.localPosition;p.y=jumpHeight;rider.localPosition=p;}
            lastX=x;
            riderRoadVelocity=dt>0?new Vector3((rider.localPosition.x-oldX)/dt,0,Speed+(rider.localPosition.z-oldRiderZ)/dt):Quaternion.Euler(0,TurnYaw,0)*Vector3.forward*Speed;
            riderVelocityValid=true;
            TickTrafficMotion(dt);
            ResolveJumpSurface(dt);
            TickRearRecklessWarning(dt);
            TickOrdinaryRearWarnings(dt);
            if(!ResolveRiderContacts(oldX,dt))return;
            if(!ServiceStopped&&Speed>.1f)rearDepartureProtection=Mathf.Max(0,rearDepartureProtection-dt);
            x=rider.localPosition.x;lastX=x;
            RetireTraffic(true);
            TickChase(dt);AnimateKick();
            if(State!=RideState.Riding)return;
            EndRouteFrame(dt);
            if(IsTurning)return;
            TickExitTraffic(dt);
            ScheduleIntersection();
            // The ordinary wave also owns reckless entries, so both thin out together.
            spawnTimer-=dt*TrafficDensity;
            if(spawnTimer<=0)
            {
                int a=rng.Next(4);float z=105+rng.Next(0,14);SpawnTraffic(a,z);
                if(DifficultyTime>12){int b=(a+rng.Next(1,4))%4;SpawnTraffic(b,z+rng.Next(5,13));}
                MaybeSpawnReckless();
                spawnTimer=Mathf.Lerp(2.5f,1.35f,Difficulty01)*(.8f+(float)rng.NextDouble()*.4f);
            }
        }
        void TickTrafficMotion(float dt)
        {
            foreach(var v in vehicles)
            {
                if(!v.active)continue;
                Vector3 previous=v.model.localPosition;
                v.previousPosition=previous;v.previousYaw=v.model.localEulerAngles.y;v.playerGrace=Mathf.Max(0,v.playerGrace-dt);
                ScrollTrafficRecovery(v,Vector3.forward*Speed*dt);
                if(v.wallCrashed)TickWallWreck(v,dt);
                else if(TickTrafficRecovery(v,dt)){}
                else if(v.knocked)TickKnockedVehicle(v,dt);
                else if(HoldCrossTrafficForService(v,dt)){}
                else if(v.roadTurning)TickRoadTurn(v,dt);
                else if(v.routeDetached)v.model.localPosition+=(v.routeVelocity-Vector3.forward*Speed)*dt;
                else if(!v.crossing&&TickJobTrafficYield(v,dt)){}
                else if(v.isReckless)TickRecklessVehicle(v,dt);
                else if(v.crossing)
                {
                    TickIntersectionVehicle(v,dt);
                }
                else
                {
                    TickLongitudinalVehicle(v,dt);
                }
                YieldRearForService(v);
                BrakeBeforeStoppedRider(v);
                if((v.isReckless||v.knocked)&&!v.wallCrashed&&!v.routeDetached)CheckBuildingImpact(v);
                if(dt>0)v.motionVelocity=v.wallCrashed?Vector3.zero:v.knocked?v.knockVelocity:(v.model.localPosition-previous)/dt+Vector3.forward*Speed;
            }
            ResolveVehicleCollisions();
            if(ServiceRearProtected)foreach(var vehicle in vehicles)YieldRearForService(vehicle);
            if(ServiceStopped)foreach(var vehicle in vehicles)BrakeBeforeStoppedRider(vehicle);
        }
        void RetireTraffic(bool countPasses)
        {
            foreach(var v in vehicles)
            {
                if(!v.active)continue;
                Vector3 previous=v.previousPosition;
                Vector3 p=v.model.localPosition;
                if(countPasses&&previous.z>0 && p.z<=0){PassedVehicles++;if(Mathf.Abs(p.x-rider.localPosition.x)<1.7f){streak++;notice="NÉ ĐẸP!  +"+(10*(int)streak);noticeTimer=1.4f;}}
                if(v.recovery!=null)continue;
                if((p.z<rider.localPosition.z-(IsTurning?25:18)&&(!v.enteringFromBehind||v.knocked)) || (v.enteringFromBehind&&!v.isReckless&&p.z< -95) || Mathf.Abs(p.x)>(IsTurning||v.routeDetached?36:24) || (v.knocked&&v.knockAge>8) || (!IsTurning&&!v.routeDetached&&v.crossing&&!v.knocked&&p.x*Mathf.Sign(v.crossSpeed)>19))RetireVehicle(v);
            }
        }
        public void Crash()
        {
            if(State==RideState.Riding)BeginRiderImpact(null,Vector2.up,rider.localPosition+Vector3.forward*.8f,Vector3.forward*Speed);
        }
        void AnimateLights()
        {
            int active=Time.time%13<6?2:Time.time%13<8?1:0;
            if(active==signalState)return;signalState=active;
            for(int i=0;i<trafficLights.Count;i++) if(trafficLights[i]!=null)
                trafficLights[i].sharedMaterial=Mat(i%3==active?(active==0?Hex("FF6453"):active==1?Gold:Hex("64E5A3")):Hex("354B47"),true);
        }
        void PositionCamera(bool snap)
        {PositionRideCamera(snap,Mathf.Min(Time.unscaledDeltaTime,.05f));}
        void PositionRideCamera(bool snap,float cameraDt)
        {
            if(rideCamera==null)return;
            if(State==RideState.Jailed){PositionJailCamera(snap);return;}
            if(!snap&&State==RideState.Paused&&IsTurning)return;
            bool narrow=Screen.width<Screen.height;
            float x=rider!=null?rider.localPosition.x:LaneX[2];
            Quaternion heading=Quaternion.Euler(0,IsTurning?TurnYaw:0,0);
            bool followsCrash=RiderCrashActive||State==RideState.GameOver;
            Vector3 center=IsTurning||followsCrash?rider.localPosition-heading*new Vector3(LaneX[CurrentLane],0,0):Vector3.zero;
            if(!followsCrash)center.y=jumpHeight*.45f;
            float lateral=IsTurning||followsCrash?LaneX[CurrentLane]:x;
            Vector3 target=center+heading*new Vector3(lateral*(narrow?.85f:.25f),IsChased?7.8f:7.15f,IsChased?-14f:-9.6f);
            // Keep a similar following distance at every riding speed. A fixed
            // time lag let a boosted bike leave the frame during a short corner.
            float cornerTracking=IsTurning||cameraTurnBlend>0?Mathf.Max(1,Speed/17):1;
            rideCamera.transform.localPosition=snap?target:Vector3.Lerp(rideCamera.transform.localPosition,target,1-Mathf.Exp(-cameraDt*5*cornerTracking));
            Vector3 look=center+heading*new Vector3(lateral*(narrow?.85f:.18f),1.2f,8);
            Quaternion rotation=Quaternion.LookRotation(transform.TransformPoint(look)-rideCamera.transform.position,Vector3.up);
            rideCamera.transform.rotation=!snap&&(IsTurning||cameraTurnBlend>0)?Quaternion.Slerp(rideCamera.transform.rotation,rotation,1-Mathf.Exp(-cameraDt*9*cornerTracking)):rotation;
            // Perspective zoom: tan(FOV/2) scales inversely with apparent size.
            rideCamera.fieldOfView=narrow?2*Mathf.Atan(Mathf.Tan(65*Mathf.Deg2Rad*.5f)/PortraitWorldScale)*Mathf.Rad2Deg:55+Mathf.Max(0,Speed-17)*.13f;
        }
        public string Telemetry()=>JsonUtility.ToJson(new RunTelemetry {state=State.ToString(),distance=Distance,speed=Speed,lane=CurrentLane,x=rider.localPosition.x,frames=Frames,passed=PassedVehicles,street=CurrentStreet,chased=IsChased,policeGap=PoliceDistance,policeSpeed=PoliceSpeed,kicks=KickCount,chain=ChainCount});
        [Serializable] class RunTelemetry{public string state,street;public float distance,speed,x,policeGap,policeSpeed;public int lane,frames,passed,kicks,chain;public bool chased;}

        void InitStyles()
        {
            if(stylesReady)return;
            label=new GUIStyle {font=vietnameseFont,richText=false,wordWrap=true};label.normal.textColor=Ink;
            button=new GUIStyle(label) {alignment=TextAnchor.MiddleCenter};button.normal.textColor=Cream;button.hover.textColor=Gold;
            stylesReady=true;
        }
        void Panel(Rect r,Color c,float radius=12)
        {GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,c,0,radius);}
        void Txt(string value,float x,float y,float w,float h,int size,Color c,TextAnchor align=TextAnchor.UpperLeft)
        {
            if(value=="★"){DrawGuiStar(new Rect(x,y,w,h),c);return;}
            label.fontSize=size;label.normal.textColor=c;label.alignment=align;GUI.Label(new Rect(x,y,w,h),PortableGuiText(value),label);
        }
        bool Btn(string value,Rect r,Color bg,Color fg,int size=17)
        {Panel(r,bg);button.fontSize=size;button.normal.textColor=fg;return GUI.Button(r,PortableGuiText(value),button);}
        void Line(float x,float y,float w,Color color){Panel(new Rect(x,y,w,1),color,0);}
        void OnGUI()
        {
            InitStyles();portrait=Screen.width<Screen.height;
            var area=GameGuiArea();
            uiScale=portrait?area.width/480f:Mathf.Min(area.width/1440f,area.height/900f);
            uiW=area.width/uiScale;uiH=area.height/uiScale;
            GUI.matrix=Matrix4x4.TRS(new Vector3(area.x,area.y,0),Quaternion.identity,Vector3.one*uiScale);
            if(State==RideState.Jailed)JailScreen();else if(State==RideState.Ready)ReadyScreen();else Hud();
            if(State==RideState.Paused&&!JobOffersOpen)EndCard(true);
            if(JobOffersOpen)JobOffersGui();
            if(State==RideState.GameOver)EndCard(false);
            if(State==RideState.Falling&&fallTime<.2f)FullscreenTint(new Color(.7f,.08f,.05f,.1f));
            if(State==RideState.Arrested){FullscreenTint(new Color(.1f,.18f,.4f,.23f));Txt("CSGT ĐÃ ĐUỔI KỊP",0,uiH*.33f,uiW,90,portrait?29:44,Cream,TextAnchor.MiddleCenter);}
            GUI.matrix=Matrix4x4.identity;
        }
        void Brand(float x,float y)
        {
            Panel(new Rect(x-10,y-8,259,57),new Color(.97f,.985f,1,.83f),14);
            Panel(new Rect(x,y,36,36),Red,10);Txt("★",x,y-1,36,36,27,Gold,TextAnchor.MiddleCenter);
            Txt("PHỐ VIỆT",x+48,y-3,200,30,24,Ink);Txt("CHUYẾN XE KHÔNG NGỪNG",x+49,y+27,198,17,9,Teal);
        }
        void ReadyScreen()
        {
            Brand(portrait?24:48,portrait?24:38);
            if(!portrait)
            {
                Panel(new Rect(uiW-277,40,229,56),WelcomeGlass);Txt("ĐỊA ĐIỂM HÔM NAY",uiW-257,48,200,18,9,Teal);Txt(CurrentStreet,uiW-257,67,200,25,16,Ink);
            }
            WelcomeCard();
            if(!portrait)
            {
                float by=uiH-82;Panel(new Rect(48,by,754,43),WelcomeGlass,12);
                Txt("← → / A D: ĐỔI LÀN     Q: ĐẠP     SPACE: NHẢY     W: TĂNG TỐC     P: TẠM DỪNG",64,by+13,724,23,11,Ink);
            }
            else {Panel(new Rect(18,uiH-54,uiW-36,30),WelcomeGlass,10);Txt("VUỐT: ĐỔI LÀN / NHẢY / TĂNG TỐC  ·  CHẠM 2 LẦN: ĐẠP",25,uiH-46,uiW-50,21,9,Ink,TextAnchor.MiddleCenter);}
        }
        void Hud()
        {
            if(portrait)PortraitHud();else LandscapeHud();
            RearRecklessWarningHud();
        }
        void EndCard(bool paused)
        {
            FullscreenTint(new Color(.04f,.12f,.14f,.63f));
            float w=portrait?432:480,h=paused?340:465,x=(uiW-w)/2,y=(uiH-h)/2;
            Panel(new Rect(x,y,w,h),Cream,18);
            Txt(paused?"NGHỈ MỘT CHÚT":"KẾT THÚC CHUYẾN XE",x+28,y+28,w-56,25,12,Red);
            Txt(paused?"Dừng chân bên phố.":"Chuyến đi kết thúc!",x+27,y+69,w-54,70,portrait?31:36,Ink);
            if(paused)
            {
                Txt("Phố vẫn đợi. Sẵn sàng thì mình đi tiếp nhé.",x+28,y+140,w-56,50,16,Teal);
                if(Btn("ĐI TIẾP   →",new Rect(x+28,y+215,w-56,58),Teal,Cream,18))TogglePause();
            }
            else
            {
                Txt("Bạn đã đi được",x+28,y+137,w-56,27,15,Teal);
                Txt(Mathf.Max(0,Mathf.FloorToInt(Distance)).ToString("N0")+" m",x+24,y+172,w-48,75,58,Ink);
                Line(x+28,y+267,w-56,Hex("D8D6BF"));
                Txt(PassedVehicles+" XE ĐÃ NÉ",x+28,y+286,200,27,13,Teal);Txt("KỶ LỤC  "+BestDistance+" m",x+w-225,y+286,200,27,13,Red,TextAnchor.UpperRight);
                if(Btn("THỬ MỘT CHUYẾN NỮA   →",new Rect(x+28,y+334,w-56,58),Red,Cream,portrait?15:18))RestartRide();
                if(Btn("VỀ ĐẦU PHỐ",new Rect(x+28,y+404,w-56,34),Cream,Teal,12))ResetRide();
            }
        }
        void OnDestroy()
        {
            if(engine!=null&&engine.clip!=null)Destroy(engine.clip);if(crashClip!=null)Destroy(crashClip);
            if(sirenClip!=null)Destroy(sirenClip);
        }
    }
}
