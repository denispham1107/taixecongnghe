using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        public float PoliceFallRemaining {get;private set;}
        public bool PoliceFallen=>PoliceFallRemaining>0;
        public int PoliceFalls {get;private set;}
        public int PoliceAvoidances {get;private set;}
        public const float PoliceCruiserScale=.85f;
        public const float PoliceCruiserLength=4.4f*PoliceCruiserScale;
        const float PoliceHalfWidth=1.83f*PoliceCruiserScale*.5f,PoliceHalfLength=PoliceCruiserLength*.5f,PoliceHeight=1.605f*PoliceCruiserScale;
        const float PoliceOpeningDelay=3f,PoliceOpeningAcceleration=4f;
        TrafficVehicle policeBody;
        float[] policeContactUntil;
        float policeTargetX,policeOpeningRemaining,policeOpeningSpeedLimit=VehicleSpeedLimit;
        bool PoliceWaitingToStart=>policeOpeningRemaining>0;

        void ResetPoliceDriving(bool resetStats=true)
        {
            ResetPoliceOpening();
            PoliceFallRemaining=0;policeTargetX=0;
            dinosaurPoliceVelocity=Vector3.zero;
            if(resetStats)PoliceFalls=PoliceAvoidances=0;
            policeContactUntil=new float[vehicles.Count];
            if(policeBike!=null)
            {
                policeBody=CreatePoliceBody();
                policeBike.localRotation=Quaternion.identity;
            }
        }
        void ResetPoliceOpening()
        {
            policeOpeningRemaining=0;
            policeOpeningSpeedLimit=VehicleSpeedLimit;
        }
        float LimitPoliceOpeningSpeed(float speed)=>Mathf.Clamp(speed,0,policeOpeningSpeedLimit);
        TrafficVehicle CreatePoliceBody()=>new TrafficVehicle {model=policeBike,halfWidth=PoliceHalfWidth,halfLength=PoliceHalfLength};
        Vector3 PoliceRoadPosition()
        {
            Vector3 position=policeBike.localPosition;
            if(position.y!=0&&!PoliceInRiver)
            {
                position.y=0;
                policeBike.localPosition=position;
            }
            return position;
        }
        void TickPoliceRecovery(Vector3 before,float dt)
        {
            // Keep the sedan on the road throughout the recovery, including during a turn.
            policeBike.localPosition=before-Vector3.forward*RoadScrollSpeed*dt;
            TickDinosaurPolicePush(dt);
            ResolvePoliceContact(before,Vector3.zero,false);
            if(PoliceInRiver)return;
            PoliceFallRemaining=Mathf.Max(0,PoliceFallRemaining-dt);
            if(PoliceFallRemaining<.00001f)PoliceFallRemaining=0;
            PoliceSpeed=0;
            policeBike.localRotation=Quaternion.Euler(0,routePolice||PoliceFallen?policeFallYaw:0,0);
            PoliceDistance=routePolice?ChaseTrailDistance-policeRouteAt:-policeBike.localPosition.z;
            if(!PoliceFallen)
            {
                policeBody.knocked=false;
                if(!routePolice)policeArrived=PoliceDistance<=6.5f;
            }
        }
        bool PoliceCanReachRider()
        {
            return !PoliceFallen&&PoliceCanReachTarget(policeBike.localPosition,PoliceTargetPosition);
        }
        bool PoliceCanReachTarget(Vector3 position,Vector3 target)
        {
            if(target.y>=1.25f)return false;
            Vector3 relative=Quaternion.Inverse(policeBike.localRotation)*(target-position);
            float dx=Mathf.Max(0,Mathf.Abs(relative.x)-PoliceHalfWidth),dz=Mathf.Max(0,Mathf.Abs(relative.z)-PoliceHalfLength);
            return dx*dx+dz*dz<=1.3f*1.3f;
        }
        bool PolicePathClear(Vector3 from,float targetX,float roadSpeed,float horizon)
        {
            foreach(var v in vehicles)
            {
                if(!v.active||v.model.localPosition.y>2.45f)continue;
                Vector3 relativeVelocity=BodyVelocity(v)-Vector3.forward*RoadScrollSpeed;
                Vector3 police=from;float step=horizon/10;
                for(int i=0;i<10;i++)
                {
                    Vector3 next=police;next.x=Mathf.MoveTowards(police.x,targetX,8*step);next.z+=(roadSpeed-RoadScrollSpeed)*step;
                    float lateral=(next.x-police.x)/step,angle=Mathf.Atan2(lateral,Mathf.Max(.01f,roadSpeed));
                    float c=Mathf.Abs(Mathf.Cos(angle)),s=Mathf.Abs(Mathf.Sin(angle));
                    Vector2 policeHalf=new Vector2(PoliceHalfWidth*c+PoliceHalfLength*s+.12f,PoliceHalfLength*c+PoliceHalfWidth*s+.12f);
                    Vector3 a=v.model.localPosition+relativeVelocity*(i*step)-police,b=v.model.localPosition+relativeVelocity*((i+1)*step)-next;
                    if(SweptOverlap(Flat(a),Flat(b),VehicleFootprint(v)+policeHalf))return false;
                    police=next;
                }
            }
            return true;
        }
        bool ChoosePolicePath(Vector3 position,float roadSpeed,out float target)
        {
            float desired=Mathf.Clamp(rider.localPosition.x-1.1f,-5.5f,5.5f);
            target=position.x;float best=float.MaxValue;
            for(int i=-1;i<4;i++)
            {
                float candidate=i<0?desired:LaneX[i];
                if(!PolicePathClear(position,candidate,roadSpeed,.75f))continue;
                float score=Mathf.Abs(candidate-position.x)*.4f+Mathf.Abs(candidate-desired)*.6f;
                if(score<best){best=score;target=candidate;}
            }
            return best<float.MaxValue;
        }
        void TickPoliceDriving(float dt)
        {
            if(policeBike==null||dt<=0)return;
            if(policeBody==null)policeBody=CreatePoliceBody();
            if(policeContactUntil==null||policeContactUntil.Length!=vehicles.Count)policeContactUntil=new float[vehicles.Count];
            AdvanceGrenadeDrive(policeBody,dt);
            Vector3 before=PoliceRoadPosition();
            policeBody.previousPosition=before;policeBody.previousYaw=policeBike.localEulerAngles.y;policeBody.knocked=PoliceFallen;
            if(PoliceFallen)
            {
                TickPoliceRecovery(before,dt);
                return;
            }
            if(PoliceDistance<=6.5f)policeArrived=true;
            if(PoliceDistance>16)policeArrived=false;
            float nominal=policeArrived?cruiseSpeed-.65f:cruiseSpeed+15*OpeningTrafficPace;
            nominal=LimitPoliceOpeningSpeed(nominal)*GrenadeDriveFactor(policeBody);
            bool clear=ChoosePolicePath(before,nominal,out float target);
            if(!clear&&nominal>6)
            {
                // A blocked fast approach may still have room for a slower lane change.
                nominal=6;clear=ChoosePolicePath(before,nominal,out target);
            }
            if(clear)
            {
                PoliceSpeed=nominal;
                if(Mathf.Abs(target-(rider.localPosition.x-1.1f))>.8f&&Mathf.Abs(target-policeTargetX)>.7f)PoliceAvoidances++;
                policeTargetX=target;
            }
            else
            {
                PoliceSpeed=Mathf.MoveTowards(PoliceSpeed,0,dt*45);target=before.x;
            }
            Vector3 next=before;next.x=Mathf.MoveTowards(before.x,target,dt*8);next.z+=(PoliceSpeed-RoadScrollSpeed)*dt;
            policeBike.localPosition=next;
            float sideways=(next.x-before.x)/dt;
            policeBike.localRotation=Quaternion.Euler(0,Mathf.Atan2(sideways,Mathf.Max(PoliceSpeed,.01f))*Mathf.Rad2Deg,0);
            ApplyVehicleRoadStep(policeBody,before,dt);
            Vector3 roadVelocity=(policeBike.localPosition-before)/dt+Vector3.forward*RoadScrollSpeed;
            ResolvePoliceContact(before,roadVelocity,true);
            PoliceDistance=-policeBike.localPosition.z;
            if(PoliceCanReachRider())Arrest();
        }
        void ResolvePoliceContact(Vector3 before,Vector3 policeVelocity,bool canFall)
        {
            TrafficVehicle hit=null;int hitIndex=-1;float earliest=2,penetration=0;Vector2 normal=Vector2.zero;
            for(int i=0;i<vehicles.Count;i++)
            {
                var v=vehicles[i];if(!v.active)continue;
                if(VehicleContact(policeBody,v,out Vector2 n,out float at,out float depth)&&at<earliest)
                {hit=v;hitIndex=i;earliest=at;normal=n;penetration=depth;}
            }
            if(hit==null){BeginPoliceRiver(policeVelocity);return;}
            bool fresh=policeContactUntil[hitIndex]<=timeRiding;
            Vector3 otherVelocity=BodyVelocity(hit);
            float impactSpeed=-Vector2.Dot(Flat(otherVelocity-policeVelocity),normal);
            bool gentlePair=IsGentleCarPair(policeBody,hit,policeVelocity,otherVelocity,!fresh&&!PoliceFallen);
            if(fresh&&impactSpeed>.35f)
                ShowVehicleContact(policeBody,hit,normal,impactSpeed,earliest,gentlePair);
            if(gentlePair)
            {
                if(impactSpeed>.35f||penetration>.01f)
                    StopBodiesAtContact(policeBody,hit,normal,earliest,true,CanDisplaceRoadCar(hit));
                if(fresh&&impactSpeed>.35f)
                {
                    DisturbRoadContact(policeBody,-normal);
                    PoliceSpeed*=.8f;
                    DisturbRoadContact(hit,normal);
                    policeContactUntil[hitIndex]=timeRiding+.5f;
                    TrafficCollisions++;
                    if(sound&&fx!=null)fx.PlayOneShot(crashClip,.35f);
                }
                BeginPoliceRiver(policeVelocity);
                return;
            }
            if(IsCarBikeContact(policeBody,hit))
            {
                if(fresh&&impactSpeed>.35f)
                {
                    DisturbVehicle(policeBody,hit.model.localPosition,null);
                    PoliceSpeed*=.8f;
                    if(!hit.wallCrashed)
                        LaunchMotorcycleAfterContact(hit,normal,earliest,BodyVelocity(hit),impactSpeed,hit.knockFromPlayer);
                    policeContactUntil[hitIndex]=timeRiding+.5f;TrafficCollisions++;
                    if(sound&&fx!=null)fx.PlayOneShot(crashClip,.45f);
                }
                BeginPoliceRiver(policeVelocity);
                return;
            }
            Vector3 end=policeBike.localPosition;
            policeBike.localPosition=Vector3.Lerp(before,end,earliest);
            // Bộ lập đường CSGT khi người chơi té/khi cua (RoutePolicePathClear) chừa đệm
            // PolicePathPadding; dừng sát thân xe hơn khoảng đó thì mọi bước kế tiếp đều bị coi là
            // chồng lấn và CSGT đứng im mãi cạnh xe đứng yên (21.50). Đuổi trên đường thẳng giữ 2,5 cm.
            float clearance=RiderCrashActive||PlayerOnFoot||routePolice?PolicePathPadding+.02f:.025f;
            if(earliest==0&&penetration>0)policeBike.localPosition-=new Vector3(normal.x,0,normal.y)*(penetration+clearance);
            else if(clearance>.025f)policeBike.localPosition-=new Vector3(normal.x,0,normal.y)*clearance;
            if(fresh)
            {
                Vector3 velocity=BodyVelocity(hit);float closing=-Vector2.Dot(Flat(velocity-policeVelocity),normal);
                if(closing>.35f&&CanDisplaceRoadCar(hit))
                {
                    if(IsProtectedContainer(hit,policeBody))
                    {
                        if(!hit.knocked)DisturbRoadContact(hit,normal);
                        TrafficCollisions++;
                    }
                    else
                    {
                        float impulse=1.12f*closing/(1/VehicleMass(policeBody)+1/VehicleMass(hit));
                        Vector2 push=normal*impulse;
                        Vector3 after=velocity+new Vector3(push.x,0,push.y)/VehicleMass(hit);
                        Vector2 lever=Flat(policeBike.localPosition-hit.model.localPosition)*.5f;
                        ActivateCollisionBody(hit,velocity,after,push,lever,hit.knockFromPlayer,!hit.knocked,closing);
                        hit.trafficCrashTakeable=true;
                        TrafficCollisions++;
                    }
                }
                policeContactUntil[hitIndex]=timeRiding+.18f;
            }
            if(canFall)
            {
                PoliceFallRemaining=1.2f;PoliceFalls++;PoliceSpeed=0;
                policeFallYaw=policeBike.localEulerAngles.y;
                policeBody.knocked=true;
                notice="CSGT VA CHẠM · TẠM CHẬM LẠI";noticeTimer=1.2f;
                if(sound&&fx!=null)fx.PlayOneShot(crashClip,.55f);
            }
            BeginPoliceRiver(policeVelocity);
        }
    }
}
