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
        TrafficVehicle policeBody;
        float[] policeContactUntil;
        float policeFallSide=1,policeTargetX;

        void ResetPoliceDriving()
        {
            PoliceFallRemaining=0;PoliceFalls=0;PoliceAvoidances=0;policeFallSide=1;policeTargetX=0;
            policeContactUntil=new float[vehicles.Count];
            if(policeBike!=null)
            {
                policeBody=CreatePoliceBody();
                policeBike.localRotation=Quaternion.identity;
            }
        }
        TrafficVehicle CreatePoliceBody()=>new TrafficVehicle {model=policeBike,halfWidth=PoliceHalfWidth,halfLength=PoliceHalfLength};
        bool PoliceCanReachRider()
        {
            if(PoliceFallen||rider.localPosition.y>=1.25f)return false;
            Vector3 relative=Quaternion.Inverse(policeBike.localRotation)*(rider.localPosition-policeBike.localPosition);
            float dx=Mathf.Max(0,Mathf.Abs(relative.x)-PoliceHalfWidth),dz=Mathf.Max(0,Mathf.Abs(relative.z)-PoliceHalfLength);
            return dx*dx+dz*dz<=1.3f*1.3f;
        }
        bool PolicePathClear(Vector3 from,float targetX,float roadSpeed,float horizon)
        {
            foreach(var v in vehicles)
            {
                if(!v.active||v.model.localPosition.y>2.45f)continue;
                Vector3 relativeVelocity=BodyVelocity(v)-Vector3.forward*Speed;
                Vector3 police=from;float step=horizon/10;
                for(int i=0;i<10;i++)
                {
                    Vector3 next=police;next.x=Mathf.MoveTowards(police.x,targetX,8*step);next.z+=(roadSpeed-Speed)*step;
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
            Vector3 before=policeBike.localPosition;
            policeBody.previousPosition=before;policeBody.previousYaw=policeBike.localEulerAngles.y;policeBody.knocked=PoliceFallen;
            if(PoliceFallen)
            {
                // Keep the patrol car in place on the road as the route scrolls past it.
                policeBike.localPosition=before-Vector3.forward*Speed*dt;
                TickDinosaurPolicePush(dt);
                ResolvePoliceContact(before,Vector3.zero,false);
                PoliceFallRemaining=Mathf.Max(0,PoliceFallRemaining-dt);if(PoliceFallRemaining<.00001f)PoliceFallRemaining=0;PoliceSpeed=0;
                float fall=Mathf.Clamp01((1.2f-PoliceFallRemaining)/.16f);
                policeBike.localRotation=Quaternion.Euler(0,policeFallYaw,-policeFallSide*8*fall);
                PoliceDistance=-policeBike.localPosition.z;
                if(!PoliceFallen)
                {
                    policeBike.localRotation=Quaternion.identity;policeBody.knocked=false;policeArrived=PoliceDistance<=6.5f;
                }
                return;
            }
            if(PoliceDistance<=6.5f)policeArrived=true;
            if(PoliceDistance>16)policeArrived=false;
            float nominal=policeArrived?cruiseSpeed-.65f:cruiseSpeed+15*OpeningTrafficPace;
            if(OpeningTrafficPace<1)nominal=Mathf.Max(0,nominal);
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
            Vector3 next=before;next.x=Mathf.MoveTowards(before.x,target,dt*8);next.z+=(PoliceSpeed-Speed)*dt;
            policeBike.localPosition=next;
            float sideways=(next.x-before.x)/dt;
            policeBike.localRotation=Quaternion.Euler(0,Mathf.Atan2(sideways,Mathf.Max(PoliceSpeed,.01f))*Mathf.Rad2Deg,-sideways*.35f);
            Vector3 roadVelocity=(next-before)/dt+Vector3.forward*Speed;
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
            float impactSpeed=-Vector2.Dot(Flat(BodyVelocity(hit)-policeVelocity),normal);
            if(fresh&&impactSpeed>.35f)
                ShowVehicleContact(policeBody,hit,normal,impactSpeed,earliest);
            Vector3 end=policeBike.localPosition;
            policeBike.localPosition=Vector3.Lerp(before,end,earliest);
            // Bộ lập đường CSGT khi người chơi té/khi cua (RoutePolicePathClear) chừa đệm
            // PolicePathPadding; dừng sát thân xe hơn khoảng đó thì mọi bước kế tiếp đều bị coi là
            // chồng lấn và CSGT đứng im mãi cạnh xe đứng yên (21.50). Đuổi trên đường thẳng giữ 2,5 cm.
            float clearance=RiderCrashActive||routePolice?PolicePathPadding+.02f:.025f;
            if(earliest==0&&penetration>0)policeBike.localPosition-=new Vector3(normal.x,0,normal.y)*(penetration+clearance);
            else if(clearance>.025f)policeBike.localPosition-=new Vector3(normal.x,0,normal.y)*clearance;
            if(fresh)
            {
                Vector3 velocity=BodyVelocity(hit);float closing=-Vector2.Dot(Flat(velocity-policeVelocity),normal);
                if(closing>.35f&&!hit.wallCrashed&&!hit.armored)
                {
                    float impulse=1.12f*closing/(1/VehicleMass(policeBody)+1/VehicleMass(hit));
                    Vector2 push=normal*impulse;
                    Vector3 after=velocity+new Vector3(push.x,0,push.y)/VehicleMass(hit);
                    Vector2 lever=Flat(policeBike.localPosition-hit.model.localPosition)*.5f;
                    ActivateCollisionBody(hit,velocity,after,push,lever,hit.knockFromPlayer,!hit.knocked,closing);
                    TrafficCollisions++;
                }
                policeContactUntil[hitIndex]=timeRiding+.18f;
            }
            if(canFall)
            {
                PoliceFallRemaining=1.2f;PoliceFalls++;PoliceSpeed=0;
                policeFallYaw=policeBike.localEulerAngles.y;
                policeFallSide=normal.x<0?-1:1;policeBody.knocked=true;
                notice="CSGT VA CHẠM · TẠM CHẬM LẠI";noticeTimer=1.2f;
                if(sound&&fx!=null)fx.PlayOneShot(crashClip,.55f);
            }
            BeginPoliceRiver(policeVelocity);
        }
    }
}
