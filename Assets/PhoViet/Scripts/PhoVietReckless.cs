using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public Transform recklessBikeModel,recklessTruckModel;
        public int RecklessEvents {get;private set;}
        public int RecklessRamCount {get;private set;}
        public int OncomingRecklessEvents {get;private set;}
        public bool recklessTrafficEnabled=true;
        bool rearRecklessPending,rearRecklessTruck;
        int rearRecklessLane=-1;
        float rearRecklessWarning;
        public int RearRecklessWarningLane=>RearRecklessWarningVisible?rearRecklessLane:-1;
        public float RearRecklessWarningRemaining=>rearRecklessWarning;
        bool MovingOncomingDanger=>vehicles.Exists(v=>v.active&&v.isReckless&&v.baseTrafficSpeed<0&&!v.knocked&&!v.wallCrashed);
        bool MovingRearDanger=>vehicles.Exists(v=>v.active&&v.isReckless&&v.baseTrafficSpeed>0&&!v.knocked&&!v.wallCrashed);
        bool RearRecklessWarningVisible=>rearRecklessPending&&!MovingOncomingDanger&&!MovingRearDanger&&!IsTurning&&!ServiceStopped;

        void ResetReckless()
        {
            RecklessEvents=0;RecklessRamCount=0;OncomingRecklessEvents=0;
            rearRecklessPending=false;rearRecklessLane=-1;rearRecklessWarning=0;
            foreach(var v in vehicles)
            {
                v.isReckless=false;v.wallCrashed=false;v.willHitWall=false;v.knockFromPlayer=false;v.recklessAge=0;
                v.enteringFromBehind=false;
                StopRecklessFx(v);
            }
        }
        void MaybeSpawnReckless()
        {
            if(!recklessTrafficEnabled||State!=RideState.Riding||rng.NextDouble()>.75+Difficulty01*.15)return;
            bool truck=rng.Next(2)==0;
            bool oncoming=rng.Next(2)==0;
            // Join ordinary traffic waves. Never teleport away an existing dangerous driver.
            if(oncoming){if(SpawnReckless(truck,true)==null)SpawnReckless(!truck,true);}
            else QueueRearReckless(truck);
        }
        bool QueueRearReckless(bool truck)
        {
            if(rearRecklessPending||MovingRearDanger)return false;
            var candidate=vehicles.Find(v=>v.eventOnly&&v.model==(truck?recklessTruckModel:recklessBikeModel));
            if(candidate==null||candidate.active)
            {
                truck=!truck;candidate=vehicles.Find(v=>v.eventOnly&&v.model==(truck?recklessTruckModel:recklessBikeModel));
                if(candidate==null||candidate.active)return false;
            }
            rearRecklessPending=true;rearRecklessTruck=truck;rearRecklessLane=rng.Next(2,4);rearRecklessWarning=2;
            return true;
        }
        void TickRearRecklessWarning(float dt)
        {
            if(!rearRecklessPending||dt<=0)return;
            if(!recklessTrafficEnabled){rearRecklessPending=false;rearRecklessLane=-1;return;}
            if(!RearRecklessWarningVisible){rearRecklessWarning=2;return;}
            rearRecklessWarning=Mathf.Max(0,rearRecklessWarning-dt);
            if(rearRecklessWarning>0)return;
            // Keep the warned lane even when traffic temporarily occupies the spawn gap.
            var spawned=SpawnRecklessAtLane(rearRecklessTruck,false,rearRecklessLane);
            if(spawned!=null){rearRecklessPending=false;rearRecklessLane=-1;}
        }
        public TrafficVehicle SpawnReckless(bool truck,bool oncoming=false)
        {return SpawnRecklessAtLane(truck,oncoming,oncoming?-1:rng.Next(2,4));}
        TrafficVehicle SpawnRecklessAtLane(bool truck,bool oncoming,int rearLane)
        {
            if(!oncoming&&(MovingOncomingDanger||MovingRearDanger))return null;
            Transform model=truck?recklessTruckModel:recklessBikeModel;
            if(model==null)return null;
            TrafficVehicle v=vehicles.Find(t=>t.eventOnly&&t.model==model);
            if(v==null)return null;
            if(v.active)return null;
            int adjacent=oncoming?rng.Next(2):rearLane;
            float rearZ=Mathf.Min(-28,rideCamera.transform.localPosition.z-v.halfLength-12);
            Vector3 spawn=new Vector3(LaneX[adjacent],0,oncoming?105+(float)rng.NextDouble()*15:rearZ-(float)rng.NextDouble()*6);
            bool blocked=true;
            for(int attempt=0;attempt<6;attempt++)
            {
                blocked=false;
                foreach(var other in vehicles)
                {
                    if(!other.active||other.model.localPosition.y>2.5f)continue;
                    Vector2 half=VehicleFootprint(other);Vector3 p=other.model.localPosition;
                    if(Mathf.Abs(spawn.x-p.x)<v.halfWidth+half.x+.3f&&Mathf.Abs(spawn.z-p.z)<v.halfLength+half.y+1){blocked=true;break;}
                }
                if(!blocked&&IsChased&&policeBike!=null&&policeBike.gameObject.activeSelf)
                {
                    Vector3 p=policeBike.localPosition;Vector2 half=policeBody!=null?VehicleFootprint(policeBody):new Vector2(.5f,1.2f);
                    if(Mathf.Abs(spawn.x-p.x)<v.halfWidth+half.x+.3f&&Mathf.Abs(spawn.z-p.z)<v.halfLength+half.y+1)blocked=true;
                }
                if(!blocked)break;spawn.z+=oncoming?4:-4;
            }
            if(blocked)return null;
            v.active=true;v.isReckless=true;v.knocked=false;v.wallCrashed=false;v.crossing=false;
            v.knockAge=0;v.playerGrace=0;v.recklessAge=0;v.knockFromPlayer=false;
            v.speed=oncoming?-(truck?12:20):Mathf.Max(17,Speed)+(truck?9:12)+(float)rng.NextDouble()*3;v.baseTrafficSpeed=v.speed;
            v.willHitWall=rng.NextDouble()<.7;v.wallSide=rng.Next(2)==0?-1:1;
            v.wallCrashAt=oncoming?.9f+(float)rng.NextDouble()*.4f:2.8f+(float)rng.NextDouble()*1.2f;
            v.lane=adjacent;
            v.startPosition=spawn;
            v.rearStartX=spawn.x;if(!oncoming)v.startPosition.x=LaneX[adjacent];
            v.swerveDirection=oncoming?1:adjacent==3?-1:1;
            v.swerveFrequency=(truck?1.9f:2.6f)+(float)rng.NextDouble()*.4f;
            v.swerveAmplitude=oncoming?(5.4f-v.startPosition.x):3.4f+(float)rng.NextDouble()*1.1f;
            v.swerveSpeed=(truck?8:11)+(float)rng.NextDouble()*2;
            v.model.localPosition=spawn;v.previousPosition=v.model.localPosition;
            v.model.localRotation=Quaternion.Euler(0,oncoming?180:0,0);v.model.gameObject.SetActive(true);
            InitializeDriver(v);
            v.enteringFromBehind=!oncoming;
            StopRecklessFx(v);RecklessEvents++;
            if(oncoming)OncomingRecklessEvents++;
            notice=oncoming?(truck?"XE TẢI NGỦ GẬT NGƯỢC CHIỀU!":"QUÁI XẾ NGƯỢC CHIỀU LẤN LÀN!"):(truck?"XE TẢI NGỦ GẬT TỪ PHÍA SAU!":"QUÁI XẾ VƯỢT LÊN TỪ PHÍA SAU!");noticeTimer=1.5f;
            return v;
        }
        void TickRecklessVehicle(TrafficVehicle v,float dt)
        {
            bool truck=v.halfWidth>.7f;
            if(v.enteringFromBehind)
            {
                v.speed=Mathf.Max(v.speed,Speed+(truck?9:12));
                Vector3 previous=v.model.localPosition,entryPosition=previous;entryPosition.z+=(v.speed-Speed)*dt;
                float merge=Mathf.InverseLerp(truck?-10:-7,truck?-4:-2.6f,entryPosition.z);
                entryPosition.x=Mathf.Lerp(v.rearStartX,v.startPosition.x,Mathf.SmoothStep(0,1,merge));v.model.localPosition=entryPosition;
                float sideSpeed=dt>0?(entryPosition.x-previous.x)/dt:0;
                v.model.localRotation=Quaternion.Euler(0,Mathf.Atan2(sideSpeed,v.speed)*Mathf.Rad2Deg,truck?0:-sideSpeed*1.3f);
                if(v.model.localPosition.z>=1.5f){v.enteringFromBehind=false;v.recklessAge=0;}
                return;
            }
            v.recklessAge+=dt;
            if(v.baseTrafficSpeed<0&&v.speed>v.baseTrafficSpeed)v.speed=Mathf.MoveTowards(v.speed,v.baseTrafficSpeed,dt*20);
            if(v.speed>0)v.speed=Mathf.MoveTowards(v.speed,Speed+(truck?1.2f:3.2f),dt*7);
            float frequency=v.swerveFrequency>0?v.swerveFrequency:(truck?2.1f:2.8f);
            float weave=(1-Mathf.Cos(v.recklessAge*frequency))*.5f;
            float amplitude=v.swerveAmplitude>0?v.swerveAmplitude:v.speed<0?5.4f-v.startPosition.x:3.9f;
            float target=Mathf.Clamp(v.startPosition.x+v.swerveDirection*amplitude*weave,-5.8f,5.8f);
            if(v.willHitWall&&v.recklessAge>=v.wallCrashAt)target=v.wallSide*12;
            Vector3 p=v.model.localPosition;
            float x=Mathf.MoveTowards(p.x,target,dt*(v.swerveSpeed>0?v.swerveSpeed:truck?9:13));
            float sideways=dt>0?(x-p.x)/dt:0;
            p.x=x;p.z+=(v.speed-Speed)*dt;v.model.localPosition=p;
            float yaw=Mathf.Atan2(sideways,v.speed)*Mathf.Rad2Deg;
            v.model.localRotation=Quaternion.Euler(0,yaw,-sideways*(truck?.3f:1.5f));
        }
        void CheckBuildingImpact(TrafficVehicle v)
        {
            Vector3 p=v.model.localPosition;Vector2 half=VehicleFootprint(v);
            float route=Mathf.Repeat(RoadProgress+p.z,VietnamStreet.Length);
            // Shop rows stop before the junction: an empty cross street is never a wall.
            if(Mathf.Abs(p.x)+half.x<9.3f||route>58.4f)return;
            float side=Mathf.Sign(p.x);p.x=side*(9.3f-half.x);p.y=0;
            if(!v.isReckless){v.model.localPosition=p;v.knockVelocity.x=-v.knockVelocity.x*.25f;return;}
            v.model.GetComponent<CarVisualAnimator>()?.ReceiveImpact(v.model.InverseTransformDirection(transform.TransformDirection(new Vector3(-side,0,0))),15);
            v.model.localPosition=p;v.wallCrashed=true;v.knocked=true;v.knockAge=0;v.recovery=null;
            v.knockDirection=side;v.knockVelocity=Vector3.zero;
            v.model.localRotation=Quaternion.Euler(v.halfWidth>.7f?-8:0,v.model.localEulerAngles.y,-side*(v.halfWidth>.7f?24:78));
            if(v.crashEffect!=null)
            {
                v.crashEffect.localPosition=WreckFxPosition(v);v.crashEffect.localRotation=Quaternion.identity;
                v.crashEffect.gameObject.SetActive(true);if(v.smoke!=null)v.smoke.Play();if(v.fire!=null)v.fire.Play();
            }
            if(sound)fx?.PlayOneShot(crashClip,.7f);
        }
        void TickWallWreck(TrafficVehicle v,float dt)
        {
            v.knockAge+=dt;v.model.localPosition-=Vector3.forward*Speed*dt;
            if(v.crashEffect!=null)
            {
                v.crashEffect.localPosition=v.routeDetached?v.model.localPosition+v.routeEffectOffset:WreckFxPosition(v);
            }
        }
        Vector3 WreckFxPosition(TrafficVehicle v)
        {
            bool truck=v.halfWidth>.7f;Vector3 p=v.model.localPosition+v.model.localRotation*new Vector3(0,.7f,truck?2.2f:.4f);
            p.x-=Mathf.Sign(v.model.localPosition.x)*(truck?1.25f:.45f);p.y=Mathf.Max(.25f,p.y);return p;
        }
        void ResolveRecklessRams()
        {
            ResolveVehicleCollisions();
        }
        void StopRecklessFx(TrafficVehicle v)
        {
            if(v.smoke!=null)v.smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(v.fire!=null)v.fire.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(v.crashEffect!=null)v.crashEffect.gameObject.SetActive(false);
        }
        void RetireVehicle(TrafficVehicle v)
        {v.active=false;v.model.gameObject.SetActive(false);if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);StopRecklessFx(v);}
        void PauseRecklessFx(bool pause)
        {
            foreach(var v in vehicles)if(v.wallCrashed&&v.active)
            {
                if(v.smoke!=null){if(pause)v.smoke.Pause();else v.smoke.Play();}
                if(v.fire!=null){if(pause)v.fire.Pause();else v.fire.Play();}
            }
        }
        void RecklessHud()
        {
            if(State!=RideState.Riding&&State!=RideState.Paused)return;
            foreach(var v in vehicles)if(v.active&&v.isReckless&&!v.knocked)
            {
                float w=portrait?400:420,x=(uiW-w)/2,y=uiH-(portrait?160:125);
                Panel(new Rect(x,y,w,37),Red);
                string warning=v.speed<0?(v.halfWidth>.7f?"XE TẢI NGỦ GẬT · NGƯỢC CHIỀU LẤN LÀN":"QUÁI XẾ NGƯỢC CHIỀU · CẨN THẬN LẤN LÀN"):(v.halfWidth>.7f?"XE TẢI NGỦ GẬT · CẨN THẬN LẤN LÀN":"QUÁI XẾ · Q / CHẠM HAI LẦN ĐỂ TỰ VỆ");
                if(v.enteringFromBehind)warning=v.halfWidth>.7f?"XE TẢI NGỦ GẬT · ĐANG VƯỢT TỪ PHÍA SAU":"QUÁI XẾ · ĐANG VƯỢT TỪ PHÍA SAU";
                Txt(warning,x+8,y+9,w-16,24,portrait?11:12,Cream,TextAnchor.MiddleCenter);
                break;
            }
        }
        void UpdateDangerSigns()
        {
            if(rideCamera==null)return;
            foreach(var v in vehicles)
            {
                if(!v.active||!v.eventOnly)continue;
                var sign=v.model.Find("Biển xe nguy hiểm");if(sign==null)continue;
                bool front=Vector3.Dot(sign.forward,rideCamera.transform.position-sign.position)>0;
                var rear=sign.Find("Tên xe");var face=sign.Find("Tên xe mặt trước");
                if(rear!=null)rear.GetComponent<Renderer>().enabled=!front;
                if(face!=null)face.GetComponent<Renderer>().enabled=front;
            }
        }
    }
}
