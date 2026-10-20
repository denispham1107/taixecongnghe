using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public Transform policeBike,jailRoom,leftKickLeg,rightKickLeg;
        public Renderer redBeacon,blueBeacon;
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
        Color roadBackground;
        AudioSource siren;AudioClip sirenClip;

        void InitializeActions()
        {
            roadFog=RenderSettings.fog;
            roadBackground=rideCamera.backgroundColor;
            siren=gameObject.AddComponent<AudioSource>();siren.loop=true;siren.volume=.12f;siren.playOnAwake=false;
            const int hz=22050;float[] wave=new float[hz*2];float phase=0;
            for(int i=0;i<wave.Length;i++)
            {float t=(float)i/hz;float frequency=650+200*Mathf.Sin(t*Mathf.PI*2);phase+=frequency/hz*Mathf.PI*2;wave[i]=Mathf.Sin(phase)*.35f;}
            sirenClip=AudioClip.Create("Còi mô tô tuần tra",wave.Length,1,hz,false);sirenClip.SetData(wave,0);siren.clip=sirenClip;
        }
        void ResetActions()
        {
            IsChased=false;PoliceDistance=0;PoliceSpeed=0;KickCount=0;ChainCount=0;
            kickCooldown=0;kickTime=0;chaseBoost=0;collisionImmunity=0;arrestTime=0;
            lastTapTime=-100;policeArrived=false;
            ResetPoliceDriving();
            if(siren!=null)siren.Stop();if(policeBike!=null)policeBike.gameObject.SetActive(false);
            if(jailRoom!=null)jailRoom.gameObject.SetActive(false);
            if(scenery!=null)scenery.gameObject.SetActive(true);if(rider!=null)rider.gameObject.SetActive(true);
            if(leftKickLeg!=null)leftKickLeg.localRotation=Quaternion.identity;if(rightKickLeg!=null)rightKickLeg.localRotation=Quaternion.identity;
            RenderSettings.fog=roadFog;
            if(rideCamera!=null)rideCamera.backgroundColor=roadBackground;
        }
        public bool TryKick()
        {
            if(State!=RideState.Riding||ServiceStopped||RiderJumpActive||RiderOnRoof||kickCooldown>0)return false;
            TrafficVehicle target=null;float closest=float.MaxValue;
            foreach(var v in vehicles)
            {
                if(!v.active||v.knocked||v.crossing||v.halfWidth>.7f)continue;
                Vector3 d=Quaternion.Inverse(Quaternion.Euler(0,IsTurning?TurnYaw:0,0))*(v.model.localPosition-rider.localPosition);
                // Oncoming bikes have a slightly longer reach window; dangerous riders take priority.
                if(Mathf.Abs(d.z)>(v.speed<0?3.4f:2)||Mathf.Abs(d.x)<.85f||Mathf.Abs(d.x)>3.65f)continue;
                float score=Mathf.Abs(d.z)+Mathf.Abs(d.x)*.12f-(v.isReckless?5:0);
                if(score<closest){closest=score;target=v;}
            }
            if(target==null){notice="CẦN XE MÁY Ở BÊN CẠNH TRONG TẦM ĐẠP";noticeTimer=1;return false;}
            kickDirection=Mathf.Sign((Quaternion.Inverse(Quaternion.Euler(0,IsTurning?TurnYaw:0,0))*(target.model.localPosition-rider.localPosition)).x);
            kickTime=.38f;kickCooldown=.8f;KickCount++;
            rider.GetComponent<BikeVisualAnimator>()?.Kick(kickDirection);
            bool selfDefense=target.isReckless;
            Vector3 targetVelocity=BodyVelocity(target);KnockVehicle(target,kickDirection,false);
            if(IsTurning){Vector3 side=Quaternion.Euler(0,TurnYaw,0)*Vector3.right;target.knockVelocity=targetVelocity+side*kickDirection*9;target.knockVelocity.y=2.2f;}
            if(!selfDefense)StartChase();
            notice=selfDefense?"TỰ VỆ: HẠ QUÁI XẾ · KHÔNG GỌI CSGT":kickDirection>0?"ĐẠP PHẢI!  CSGT ĐANG ĐẾN":"ĐẠP TRÁI!  CSGT ĐANG ĐẾN";noticeTimer=1.5f;
            return true;
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
            v.knocked=true;v.knockAge=0;v.knockDirection=Mathf.Sign(direction);
            v.physicalCrash=false;v.changingLane=false;v.turning=false;if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
            v.initialYaw=v.model.localEulerAngles.y;v.previousPosition=v.model.localPosition;v.playerGrace=.45f;
            v.knockFromPlayer=credited;
            bool car=v.halfWidth>.7f;
            v.knockVelocity=new Vector3(v.knockDirection*(car?8:9),car?7.5f:2.2f,v.crossing?0:v.speed);
            if(chain&&credited){ChainCount++;notice="VA CHẠM DÂY CHUYỀN  ×"+ChainCount;noticeTimer=1.3f;}
            if(sound&&fx!=null)fx.PlayOneShot(crashClip,.4f);
        }
        void TickActionTimers(float dt)
        {
            kickCooldown=Mathf.Max(0,kickCooldown-dt);kickTime=Mathf.Max(0,kickTime-dt);
            collisionImmunity=Mathf.Max(0,collisionImmunity-dt);
            recoveryTrafficYieldRemaining=Mathf.Max(0,recoveryTrafficYieldRemaining-dt);
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
            if(IsChased)return;
            IsChased=true;PoliceDistance=38;PoliceSpeed=Speed+15;policeArrived=false;chaseBoost=.5f;
            if(policeBike!=null){policeBike.gameObject.SetActive(true);policeBike.localPosition=new Vector3(rider.localPosition.x-1,0,-PoliceDistance);}
            if(IsTurning){routePolice=true;policeRouteAt=Distance-PoliceDistance;policeBike.localPosition=SampleRoute(policeRouteAt).point;}
            if(sound)siren?.Play();
        }
        void TickChase(float dt)
        {
            if(!IsChased)return;
            if(RiderCrashActive)TickCrashPolice(dt);else if(routePolice)TickRoutePolice(dt);else TickPoliceDriving(dt);
            bool flash=(int)(timeRiding*8)%2==0;
            if(redBeacon!=null)redBeacon.sharedMaterial=Mat(flash?Hex("FF423D"):Hex("652F31"),true);
            if(blueBeacon!=null)blueBeacon.sharedMaterial=Mat(flash?Hex("284568"):Hex("4CBAFF"),true);
        }
        public void Arrest()
        {
            if((State!=RideState.Riding&&State!=RideState.Falling&&State!=RideState.Recovering)||!IsChased)return;
            CancelBoost();State=RideState.Arrested;Speed=0;arrestTime=0;engine.Stop();siren?.Stop();
            if((int)Distance>BestDistance){BestDistance=(int)Distance;PlayerPrefs.SetInt(BestKey,BestDistance);PlayerPrefs.Save();}
        }
        void TickArrest(float dt)
        {
            arrestTime+=dt;
            if(policeBike!=null)
            {
                Vector3 from=policeBike.localPosition,to=Vector3.Lerp(from,rider.localPosition+Quaternion.Euler(0,IsTurning?TurnYaw:0,0)*new Vector3(-1.1f,0,-.7f),dt*5);bool clear=true;
                foreach(var v in vehicles)if(v.active&&v.model.localPosition.y<2.4f&&SweptOverlap(Flat(v.model.localPosition-from),Flat(v.model.localPosition-to),VehicleFootprint(v)+new Vector2(.5f,1.2f))){clear=false;break;}
                if(clear)policeBike.localPosition=to;
            }
            if(arrestTime>=1.1f)
            {
                State=RideState.Jailed;scenery.gameObject.SetActive(false);rider.gameObject.SetActive(false);
                foreach(var v in vehicles){v.model.gameObject.SetActive(false);if(v.crashEffect!=null)v.crashEffect.gameObject.SetActive(false);}
                if(policeBike!=null)policeBike.gameObject.SetActive(false);
                if(jailRoom!=null)jailRoom.gameObject.SetActive(true);
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
            if(!IsChased||State==RideState.Jailed)return;
            float w=portrait?300:360,x=(uiW-w)/2;
            Panel(new Rect(x,162,w,55),Ink);
            Txt(PoliceFallen?"CSGT NGÃ · DỰNG XE SAU "+PoliceFallRemaining.ToString("0.0")+" s":"CSGT ĐANG TRUY ĐUỔI   /   "+Mathf.Max(0,PoliceDistance).ToString("0.0")+" m",x+14,171,w-28,22,12,Cream);
            Panel(new Rect(x+14,201,w-28,4),Hex("466363"),2);
            Panel(new Rect(x+14,201,(w-28)*Mathf.Clamp01(1-PoliceDistance/38),4),Red,2);
        }
        void JailScreen()
        {
            float w=portrait?440:420,h=portrait?296:360,x=portrait?20:uiW-w-42,y=portrait?uiH-h-22:(uiH-h)/2;
            Panel(new Rect(x,y,w,h),Cream,18);
            Txt("CHUYẾN XE KẾT THÚC",x+26,y+24,w-52,24,12,Red);
            Txt("ĐÃ BỊ BẮT",x+24,y+62,w-48,62,portrait?34:40,Ink);
            Txt("Quãng đường  "+Mathf.Max(0,Mathf.FloorToInt(Distance))+" m\nĐạp xe  "+KickCount+"   ·   Dây chuyền  "+ChainCount,x+26,y+132,w-52,62,16,Teal);
            if(Btn("CHƠI LẠI   →",new Rect(x+26,y+(portrait?203:230),w-52,54),Red,Cream,18))RestartRide();
            if(!portrait&&Btn("VỀ ĐẦU PHỐ",new Rect(x+26,y+300,w-52,32),Cream,Teal,13))ResetRide();
        }
    }
}
