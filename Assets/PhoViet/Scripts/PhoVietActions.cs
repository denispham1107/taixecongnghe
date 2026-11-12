using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public Transform policeBike,jailRoom,leftKickLeg,rightKickLeg;
        public Renderer redBeacon,blueBeacon;
        public static bool PolicePursuitReleased=>true;
        public bool IsChased {get;private set;}
        public float PoliceDistance {get;private set;}
        public float PoliceSpeed {get;private set;}
        public int KickCount {get;private set;}
        public int ChainCount {get;private set;}
        public float KickCooldown => kickCooldown;
        float kickCooldown,kickTime,kickDirection,chaseBoost,cruiseSpeed,collisionImmunity,arrestTime;
        float lastTapTime=-100,touchStarted;
        Vector2 lastTapPoint;
        bool policeArrived,roadFog;
        const float PoliceTakeoverDelay=3f;
        float policeReplacementDelay;
        float policeTakeoverRemaining;
        Color roadBackground;
        Texture2D jailWide,jailTall;
        AudioSource siren;AudioClip sirenClip;

        void InitializeActions()
        {
            roadFog=RenderSettings.fog;
            roadBackground=rideCamera.backgroundColor;
            siren=PhoVietAudio.Route(gameObject.AddComponent<AudioSource>());siren.loop=true;siren.volume=.12f;siren.playOnAwake=false;
            const int hz=22050;float[] wave=new float[hz*2];float phase=0;
            for(int i=0;i<wave.Length;i++)
            {float t=(float)i/hz;float frequency=650+200*Mathf.Sin(t*Mathf.PI*2);phase+=frequency/hz*Mathf.PI*2;wave[i]=Mathf.Sin(phase)*.35f;}
            sirenClip=AudioClip.Create("Còi mô tô tuần tra",wave.Length,1,hz,false);sirenClip.SetData(wave,0);siren.clip=sirenClip;
        }
        void ResetActions()
        {
            IsChased=false;PoliceDistance=0;PoliceSpeed=0;KickCount=0;ChainCount=0;
            kickCooldown=0;kickTime=0;chaseBoost=0;collisionImmunity=0;arrestTime=0;
            ResetAssistedKick();
            CancelChain();
            lastTapTime=-100;policeArrived=false;
            ResetPoliceDriving();
            ResetBikeImpact();
            policeReplacementDelay=policeTakeoverRemaining=0;
            if(siren!=null)siren.Stop();if(policeBike!=null)policeBike.gameObject.SetActive(false);
            if(jailRoom!=null)jailRoom.gameObject.SetActive(false);
            if(scenery!=null)scenery.gameObject.SetActive(true);if(rider!=null)rider.gameObject.SetActive(true);
            if(leftKickLeg!=null)leftKickLeg.localRotation=Quaternion.identity;if(rightKickLeg!=null)rightKickLeg.localRotation=Quaternion.identity;
            RenderSettings.fog=roadFog;
            if(rideCamera!=null)rideCamera.backgroundColor=roadBackground;
        }
        public bool TryKick()
        {
            return energyHover?TryBarrelRoll():TryThrowGrenades();
        }
        public bool RegisterTap(Vector2 position,float atTime)
        {
            if(State!=RideState.Riding)return false;
            // The portrait bottom is now gameplay space; retain safe-area and HUD exclusion.
            if(!GameplayPointerAllowed(position)){lastTapTime=-100;return false;}
            bool doubled=atTime-lastTapTime>=.06f&&atTime-lastTapTime<=.32f&&Vector2.Distance(position,lastTapPoint)<Screen.width*.13f;
            lastTapPoint=position;lastTapTime=atTime;
            if(!doubled)return false;
            lastTapTime=-100;return TryKick();
        }
        void KnockVehicle(TrafficVehicle v,float direction,bool chain,bool credited=true)
        {
            if(v.knocked||!v.active)return;
            RememberTrafficCourse(v);
            v.knocked=true;v.knockAge=0;v.knockDirection=Mathf.Sign(direction);
            v.physicalCrash=false;v.changingLane=false;v.turning=false;if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
            v.initialYaw=v.model.localEulerAngles.y;v.previousPosition=v.model.localPosition;v.playerGrace=.45f;
            v.knockFromPlayer=credited;
            bool car=v.halfWidth>.7f;
            v.knockVelocity=LimitRoadVelocity(new Vector3(v.knockDirection*(car?8:9),car?7.5f:2.2f,v.crossing?0:v.speed));
            if(chain&&credited){ChainCount++;notice="VA CHẠM DÂY CHUYỀN  ×"+ChainCount;noticeTimer=1.3f;}
            if(sound&&fx!=null)fx.PlayOneShot(crashClip,.4f);
        }
        void TickActionTimers(float dt)
        {
            kickCooldown=Mathf.Max(0,kickCooldown-dt);kickTime=Mathf.Max(0,kickTime-dt);
            grenadeCooldown=Mathf.Max(0,grenadeCooldown-dt);
            takeoverGrace=Mathf.Max(0,takeoverGrace-dt);
            collisionImmunity=Mathf.Max(0,collisionImmunity-dt);
            if(IsChased)chaseBoost=Mathf.Min(12,chaseBoost+dt*.28f);
        }
        void AnimateKick()
        {
            float swing=kickTime>0?Mathf.Sin(kickTime/.38f*Mathf.PI)*78:0;
            if(leftKickLeg!=null)leftKickLeg.localRotation=Quaternion.Euler(0,0,kickDirection<0?-swing:0);
            if(rightKickLeg!=null)rightKickLeg.localRotation=Quaternion.Euler(0,0,kickDirection>0?swing:0);
        }
        void TickKnockedVehicle(TrafficVehicle v,float dt)
        {
            if(v.tankDestroyed)return; // A burnt tank stays still, including the GameOver aftermath.
            if(v.physicalCrash){TickCollisionBody(v,dt);return;}
            v.knockAge+=dt;v.knockVelocity.y-=18*dt;
            Vector3 p=v.model.localPosition+(v.knockVelocity-Vector3.forward*Speed)*dt;
            if(p.y<0){p.y=0;v.knockVelocity.y=0;v.knockVelocity.x=Mathf.MoveTowards(v.knockVelocity.x,0,dt*5);}
            v.model.localPosition=p;
            bool car=v.halfWidth>.7f;float fall=Mathf.Clamp01(v.knockAge/.42f);
            v.model.localRotation=Quaternion.Euler(car?-fall*28:0,v.initialYaw+v.knockDirection*v.knockAge*(car?70:18),-v.knockDirection*fall*(car?65:82));
        }
        Vector2 VehicleFootprint(TrafficVehicle v)
        {
            float yaw=v.model.localEulerAngles.y*Mathf.Deg2Rad,c=Mathf.Abs(Mathf.Cos(yaw)),s=Mathf.Abs(Mathf.Sin(yaw));
            return new Vector2(Mathf.Max(v.knocked?1.05f:0,v.halfWidth*c+v.halfLength*s),v.halfLength*c+v.halfWidth*s);
        }
        void ResolveChainReactions()
        {
            ResolveVehicleCollisions();
        }
        void StartChase()
        {
            if(!PolicePursuitReleased||IsChased||policeTakeoverRemaining>0)return;
            if(PoliceInRiver)ResetPoliceDriving();
            IsChased=true;PoliceDistance=38;PoliceSpeed=Mathf.Min(VehicleSpeedLimit,(TrafficPaceSpeed+15)*OpeningTrafficPace);policeArrived=false;chaseBoost=0;
            if(policeBike!=null)
            {
                policeBike.gameObject.SetActive(true);
                var start=new Vector3(PoliceTargetPosition.x-1,0,PoliceTargetPosition.z-PoliceDistance);
                if(OverRiver(start))start.x=Mathf.Clamp(start.x,LaneX[1],LaneX[2]);
                policeBike.localPosition=start;
            }
            if(IsTurning&&policeBike!=null){routePolice=true;policeRouteAt=TravelDistance-PoliceDistance;policeBike.localPosition=SampleRoute(policeRouteAt).point;}
            if(sound)siren?.Play();
        }
        void ScheduleChaseAfterTakeover()
        {
            IsChased=false;PoliceSpeed=PoliceDistance=0;chaseBoost=0;
            policeReplacementDelay=0;policeTakeoverRemaining=PoliceTakeoverDelay;
            ResetPoliceDriving(false);siren?.Stop();
            if(policeBike!=null)policeBike.gameObject.SetActive(false);
        }
        void StopPursuitForFatalImpact()
        {
            IsChased=false;CrashDuringPursuit=false;routePolice=false;chaseBoost=0;exitAfterCollision=false;
            policeTakeoverRemaining=0;
            ResetCrashPoliceNavigation();siren?.Stop();
        }
        void TickChase(float dt)
        {
            if(State==RideState.Paused||InsideShop||shopEvaded&&State==RideState.OnFoot)return;
            if(policeTakeoverRemaining>0)
            {
                policeTakeoverRemaining-=dt;
                if(policeTakeoverRemaining>.00001f)return;
                policeTakeoverRemaining=0;
                StartChase();
                return;
            }
            if(!IsChased&&(CrashDuringPursuit&&State==RideState.Falling||State==RideState.OnFoot))
            {
                policeReplacementDelay-=dt;
                if(policeReplacementDelay<=0)StartChase();
            }
            if(!IsChased)return;
            if(policeBody!=null&&!OfficerOnFoot&&TickFootGrenadeStop(policeBody,policeBike.localPosition,dt))
            {PoliceSpeed=0;PoliceDistance=Vector2.Distance(Flat(policeBike.localPosition),Flat(PoliceTargetPosition));return;}
            if(RiderCrashActive||State==RideState.OnFoot)TickCrashPolice(dt);else if(routePolice)TickRoutePolice(dt);else TickPoliceDriving(dt);
            bool flash=(int)(timeRiding*8)%2==0;
            if(redBeacon!=null)redBeacon.sharedMaterial=Mat(flash?Hex("FF423D"):Hex("652F31"),true);
            if(blueBeacon!=null)blueBeacon.sharedMaterial=Mat(flash?Hex("284568"):Hex("4CBAFF"),true);
        }
        public void Arrest()
        {
            if((State!=RideState.Riding&&State!=RideState.Falling&&State!=RideState.OnFoot)||!IsChased)return;
            // A nearby officer waits for the physical fall to finish; no jail
            // transition can cut off the throw/landing animation.
            if(State==RideState.Falling&&(!RiderFallComplete||exitAfterCollision))return;
            if(State==RideState.OnFoot&&(InsideShop||footJumpTime>0||footEscapeTime>0))return;
            if(State==RideState.OnFoot&&deferFootArrest){footArrestRequested=true;return;}
            CancelVehicleEntry();CancelFootPointer();
            CancelAssistedKick(false);CancelBoost();State=RideState.Arrested;Speed=0;arrestTime=0;engine.Stop();siren?.Stop();
            if((int)Distance>BestDistance){BestDistance=(int)Distance;PlayerPrefs.SetInt(BestKey,BestDistance);PlayerPrefs.Save();}
        }
        void TickArrest(float dt)
        {
            arrestTime+=dt;
            if(arrestTime>=1.1f)
            {
                State=RideState.Jailed;HideDinosaurs();scenery.gameObject.SetActive(false);rider.gameObject.SetActive(false);
                HideFootActors();
                foreach(var v in vehicles){v.model.gameObject.SetActive(false);if(v.crashEffect!=null)v.crashEffect.gameObject.SetActive(false);}
                if(policeBike!=null)policeBike.gameObject.SetActive(false);
                if(jailWide==null)jailWide=Resources.Load<Texture2D>("Jail/JailWide");
                if(jailTall==null)jailTall=Resources.Load<Texture2D>("Jail/JailTall");
                if(jailRoom!=null&&jailWide==null&&jailTall==null)jailRoom.gameObject.SetActive(true);
                RenderSettings.fog=false;rideCamera.backgroundColor=Hex("394F51");PositionJailCamera(true);
            }
        }
        void PositionJailCamera(bool snap)
        {
            if(jailRoom==null)return;
            Vector3 origin=jailRoom.position;
            rideCamera.transform.position=origin+new Vector3(2.6f,2.7f,-7.2f);
            rideCamera.transform.LookAt(origin+new Vector3(-.6f,1.5f,.5f));rideCamera.fieldOfView=Screen.width<Screen.height?64:48;
        }
        void ChaseHud()
        {
            if(State==RideState.OnFoot&&!InsideShop&&!shopEvaded)
            {
                DrawFootEscapeGuide();
                return;
            }
            if(InsideShop||!IsChased||(State!=RideState.Riding&&State!=RideState.OnFoot))return;
            float w=portrait?216:255,x=(uiW-w)/2,y=portrait?PortraitNoticeY+PortraitNoticeHeight+4:130;
            Panel(new Rect(x,y,w,30),new Color(.07f,.19f,.21f,.82f),7);
            string label=OfficerOnFoot?"CSGT ĐUỔI BỘ":"CSGT ĐANG ĐUỔI";
            string status=PoliceFallen&&!OfficerOnFoot?"CSGT ĐANG CHẬM LẠI":label+"  ·  "+Mathf.Max(0,PoliceDistance).ToString("0.0")+" m";
            Txt(status,x+8,y+1,w-16,21,portrait?11:12,Cream,TextAnchor.MiddleCenter);
            Panel(new Rect(x+12,y+24,w-24,3),Hex("466363"),2);
            Panel(new Rect(x+12,y+24,(w-24)*Mathf.Clamp01(1-PoliceDistance/38),3),Red,2);
        }
        void JailScreen()
        {
            Texture2D backdrop=portrait?(jailTall!=null?jailTall:jailWide):(jailWide!=null?jailWide:jailTall);
            if(backdrop!=null)GUI.DrawTexture(FullscreenGuiRect(),backdrop,ScaleMode.ScaleAndCrop);
            float w=portrait?440:408,h=portrait?236:292,x=portrait?20:uiW-w-42,y=portrait?uiH-h-22:(uiH-h)/2;
            Panel(new Rect(x,y,w,h),Cream,18);
            Txt("PHỐ VIỆT  ·  TẠM GIỮ",x+26,y+20,w-52,22,13,Red);
            Txt("ĐÃ BỊ BẮT",x+24,y+45,w-48,52,portrait?34:40,Ink);
            Txt("CSGT đã đưa bạn vào phòng giam.",x+26,y+100,w-52,25,16,Ink);
            Txt("Quãng đường  "+Mathf.Max(0,Mathf.FloorToInt(Distance))+" m",x+26,y+129,w-52,28,17,Teal);
            if(Btn("CHƠI LẠI",new Rect(x+26,y+(portrait?166:176),w-52,48),Red,Cream,18))RestartRide();
            if(!portrait&&Btn("VỀ ĐẦU PHỐ",new Rect(x+26,y+238,w-52,32),Cream,Teal,13))ResetRide();
        }
    }
}
