using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public bool trafficSurprises=true;
        public int SuddenBrakes {get;private set;}
        public int SuddenLaneChanges {get;private set;}
        public int IntersectionTurns {get;private set;}
        public int TrafficCollisions {get;private set;}
        float[,] contactUntil;

        void ResetTrafficDynamics()
        {
            SuddenBrakes=0;SuddenLaneChanges=0;IntersectionTurns=0;TrafficCollisions=0;RoadTrafficTurns=0;
            contactUntil=new float[vehicles.Count,vehicles.Count];
            foreach(var v in vehicles)InitializeDriver(v);
        }
        void InitializeDriver(TrafficVehicle v)
        {
            v.recovery=null;
            v.jobYieldLane=-1;
            v.routeDetached=false;v.routeVelocity=Vector3.zero;v.rearEntryBoost=0;
            v.roadTurning=false;v.roadDecisionAt=float.NaN;v.roadTurnAngle=0;
            v.knocked=false;v.wallCrashed=false;v.knockFromPlayer=false;v.knockVelocity=Vector3.zero;v.knockAge=0;v.playerGrace=0;
            v.enteringFromBehind=false;
            v.brakeRemaining=0;v.brakeCooldown=1.1f+(float)rng.NextDouble()*1.7f;
            v.laneCooldown=.65f+(float)rng.NextDouble()*1.4f;
            v.changingLane=false;v.changeProgress=0;v.turning=false;v.hasTurned=false;v.turnChoice=0;
            v.turnAngle=0;v.physicalCrash=false;v.angularVelocity=Vector3.zero;v.motionVelocity=Vector3.zero;
            v.previousPosition=v.model.localPosition;v.previousYaw=v.model.localEulerAngles.y;
            int index=vehicles.IndexOf(v);
            if(contactUntil!=null&&index>=0&&index<contactUntil.GetLength(0))
                for(int i=0;i<vehicles.Count;i++){contactUntil[index,i]=0;contactUntil[i,index]=0;}
            if(v.brakeLights==null)
            {
                var lights=Group("Đèn phanh động",v.model);v.brakeLights=lights;
                bool blenderBike=v.model.GetComponent<BikeVisualAnimator>()!=null;
                var blenderCar=v.model.GetComponent<CarVisualAnimator>();
                float z=v.halfLength>3?-3.26f:v.halfWidth>.7f?(v.model.name.Contains("tải")?-2.46f:-2.075f):-.88f;
                if(blenderBike)z=-1.015f;
                if(blenderCar!=null)z=blenderCar.tailZ;
                foreach(float side in new[]{-1f,1f})
                {
                    var lamp=Box("Đèn phanh đỏ",lights,new Vector3(side*(blenderCar!=null?blenderCar.tailX:blenderBike?.09f:v.halfWidth*.65f),blenderCar!=null?blenderCar.tailY:blenderBike?.79f:.86f,z),blenderBike?new Vector3(.08f,.06f,.018f):new Vector3(v.halfWidth>.7f?.3f:.17f,.12f,.018f),Red);
                    lamp.GetComponent<Renderer>().sharedMaterial=Mat(Hex("FF3929"),true);
                }
            }
            v.brakeLights.gameObject.SetActive(false);
        }
        public bool BeginSuddenBrake(TrafficVehicle v)
        {
            if(!v.active||v.knocked||v.crossing||v.isReckless||v.baseTrafficSpeed<=0||v.brakeRemaining>0)return false;
            v.brakeRemaining=.95f+(float)rng.NextDouble()*.65f;v.brakeCooldown=2+(float)rng.NextDouble()*2;
            if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(true);SuddenBrakes++;return true;
        }
        public bool BeginSuddenLaneChange(TrafficVehicle v,int lane)
        {
            if(!v.active||v.knocked||v.crossing||v.isReckless||v.roadTurning||v.routeDetached||v.baseTrafficSpeed==0||v.changingLane||lane<0||lane>3||lane==v.lane)return false;
            if(v.baseTrafficSpeed>0&&lane<2)return false;
            v.changeFromX=v.model.localPosition.x;v.targetX=LaneX[lane];v.changeProgress=0;
            v.changeDuration=v.halfLength>3?.9f:v.halfWidth>.7f?.7f:.52f;
            v.changingLane=true;v.lane=lane;v.laneCooldown=2.2f+(float)rng.NextDouble()*1.8f;SuddenLaneChanges++;return true;
        }
        void TickLongitudinalVehicle(TrafficVehicle v,float dt)
        {
            bool same=v.baseTrafficSpeed>0;Vector3 p=v.model.localPosition;
            if(v.enteringFromBehind&&!v.isReckless)
            {
                v.baseTrafficSpeed=Mathf.Max(v.baseTrafficSpeed,Speed+v.rearEntryBoost);
                if(p.z>4){v.enteringFromBehind=false;v.baseTrafficSpeed=7+(float)rng.NextDouble()*7;}
            }
            v.brakeCooldown-=dt;v.laneCooldown-=dt;
            if(TryBeginRoadTurn(v,dt)){TickRoadTurn(v,dt);return;}
            if(trafficSurprises&&!v.isReckless&&dt>0)
            {
                if(same&&v.brakeCooldown<=0&&p.z>-4&&p.z<60)
                {
                    if(rng.NextDouble()<.65+Difficulty01*.2)BeginSuddenBrake(v);
                    else v.brakeCooldown=.6f;
                }
                bool parallel=Mathf.Abs(p.z)<8&&Mathf.Abs(p.x-rider.localPosition.x)>.9f&&Mathf.Abs(p.x-rider.localPosition.x)<3.8f;
                if(v.laneCooldown<=0&&!v.changingLane&&p.z>-5&&p.z<(same?48:95))
                {
                    if(rng.NextDouble()<(parallel?.85:same?.35:.7))
                    {
                        int targetLane=same?(v.lane==2?3:2):Mathf.Clamp(v.lane+(v.lane==0?1:v.lane==3?-1:rng.Next(2)==0?-1:1),0,3);
                        BeginSuddenLaneChange(v,targetLane);
                    }
                    else v.laneCooldown=.4f;
                }
            }
            // Adjacent vehicles briefly match the rider, making cut-ins and defensive kicks readable.
            if(same&&(trafficSurprises||v.halfWidth<.7f))
            {
                if(!v.hasPaced&&p.z<=3&&p.z>=-2){v.hasPaced=true;v.rideAlongRemaining=1.6f;}
                v.rideAlongRemaining=Mathf.Max(0,v.rideAlongRemaining-dt);
            }
            bool braking=v.brakeRemaining>0;
            float target=braking?Mathf.Max(.7f,v.baseTrafficSpeed*.1f):v.rideAlongRemaining>0?Mathf.Max(v.baseTrafficSpeed,Speed-1.2f):v.baseTrafficSpeed;
            v.speed=trafficSurprises?Mathf.MoveTowards(v.speed,target,dt*(braking?38:6)):target;
            v.brakeRemaining=Mathf.Max(0,v.brakeRemaining-dt);
            if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(v.brakeRemaining>0);
            float previousX=p.x;
            if(v.changingLane)
            {
                v.changeProgress=Mathf.Min(1,v.changeProgress+dt/v.changeDuration);
                p.x=Mathf.Lerp(v.changeFromX,v.targetX,Mathf.SmoothStep(0,1,v.changeProgress));
                if(v.changeProgress>=1)v.changingLane=false;
            }
            p.z+=(v.speed-Speed)*dt;v.model.localPosition=p;
            float sideways=dt>0?(p.x-previousX)/dt:0;
            float yaw=Mathf.Atan2(sideways,v.speed)*Mathf.Rad2Deg;
            v.model.localRotation=Quaternion.Euler(0,yaw,v.halfWidth<.7f?-sideways*1.7f:0);
        }
        void TickIntersectionVehicle(TrafficVehicle v,float dt)
        {
            v.crossDelay-=dt;float moving=Mathf.Clamp(-v.crossDelay,0,dt);
            v.model.localPosition-=Vector3.forward*Speed*dt;
            float direction=Mathf.Sign(v.crossSpeed),velocity=Mathf.Abs(v.crossSpeed);
            if(!trafficSurprises)v.turnChoice=0;
            if(!v.turning&&v.turnChoice!=0&&moving>0)
            {
                float zSign=-direction*v.turnChoice;
                float endX=zSign>0?LaneX[direction>0?3:2]:LaneX[direction>0?1:0];
                float radius=v.halfWidth>.7f?3.1f:2.6f;
                float startX=endX-direction*radius;
                float toStart=(startX-v.model.localPosition.x)*direction;
                if(toStart<=velocity*moving)
                {
                    float before=Mathf.Clamp(toStart/velocity,0,moving);
                    v.model.localPosition+=Vector3.right*v.crossSpeed*before;moving-=before;
                    v.turning=true;v.turnAngle=0;v.turnStartX=startX;v.turnEndX=endX;v.turnRadius=radius;v.turnZSign=zSign;
                    v.turnOriginZ=Distance+v.model.localPosition.z;IntersectionTurns++;
                }
            }
            if(!v.turning){v.model.localPosition+=Vector3.right*v.crossSpeed*moving;return;}
            float remainingAngle=Mathf.PI*.5f-v.turnAngle;
            float turnTime=Mathf.Min(moving,remainingAngle*v.turnRadius/velocity);
            v.turnAngle=Mathf.Min(Mathf.PI*.5f,v.turnAngle+velocity*turnTime/v.turnRadius);
            float angle=v.turnAngle;
            v.model.localPosition=new Vector3(v.turnStartX+direction*v.turnRadius*Mathf.Sin(angle),0,v.turnOriginZ+v.turnZSign*v.turnRadius*(1-Mathf.Cos(angle))-Distance);
            v.model.localRotation=Quaternion.Euler(0,Mathf.Atan2(direction*Mathf.Cos(angle),v.turnZSign*Mathf.Sin(angle))*Mathf.Rad2Deg,v.halfWidth<.7f?-direction*v.turnZSign*12:0);
            if(angle>=Mathf.PI*.5f-.00001f)
            {
                v.turning=false;v.crossing=false;v.hasTurned=true;v.speed=v.turnZSign*velocity;v.baseTrafficSpeed=v.speed;
                v.lane=v.turnZSign>0?(v.turnEndX>3?3:2):(v.turnEndX<-3?0:1);
                v.model.localPosition+=Vector3.forward*v.speed*(moving-turnTime);
                v.model.localRotation=Quaternion.Euler(0,v.speed>0?0:180,0);
            }
        }
        void TrafficSurpriseHud()
        {
            if(State!=RideState.Riding)return;
            foreach(var v in vehicles)if(v.active&&v.isReckless&&!v.knocked)return;
            foreach(var v in vehicles)
            {
                if(!v.active||v.knocked||v.model.localPosition.z<-4||v.model.localPosition.z>35)continue;
                string warning=v.changingLane?"XE TẠT LÀN · CẨN THẬN VA CHẠM":v.brakeRemaining>0?"PHANH GẤP PHÍA TRƯỚC · ĐỔI LÀN NGAY":v.turning?"XE GIAO LỘ ĐANG RẼ VÀO ĐƯỜNG":"";
                if(warning.Length==0)continue;
                float w=portrait?400:420,x=(uiW-w)/2,y=uiH-(portrait?160:125);
                Panel(new Rect(x,y,w,37),Red);Txt(warning,x+8,y+9,w-16,24,portrait?11:12,Cream,TextAnchor.MiddleCenter);break;
            }
        }
    }
}
