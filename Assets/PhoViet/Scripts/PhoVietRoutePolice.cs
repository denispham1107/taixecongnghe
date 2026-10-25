using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        RoutePoint SampleRoute(float at)
        {
            if(routeTrail.Count==0)return new RoutePoint(at,new Vector3(rider.localPosition.x,0,at-Distance),0);
            for(int i=1;i<routeTrail.Count;i++)if(routeTrail[i].at>=at)
            {
                var a=routeTrail[i-1];var b=routeTrail[i];float t=Mathf.InverseLerp(a.at,b.at,at);
                return new RoutePoint(at,Vector3.Lerp(a.point,b.point,t),Mathf.LerpAngle(a.yaw,b.yaw,t));
            }
            var last=routeTrail[routeTrail.Count-1];return new RoutePoint(at,last.point+Quaternion.Euler(0,last.yaw,0)*Vector3.forward*(at-last.at),last.yaw);
        }
        const float PolicePathPadding=.08f;
        bool RoutePolicePathClear(Vector3 before,Vector3 target,float yaw,float dt)
        {
            float a=yaw*Mathf.Deg2Rad;Vector2 forward=new Vector2(Mathf.Sin(a),Mathf.Cos(a)),right=new Vector2(forward.y,-forward.x);
            foreach(var v in vehicles)
            {
                if(!v.active||v.model.localPosition.y>2.45f)continue;
                Vector3 next=v.model.localPosition+(BodyVelocity(v)-Vector3.forward*Speed)*Mathf.Max(dt,.15f);
                // Quét hai hộp xoay thật (SAT 4 trục) thay hộp AABB của xe xoay: container 12,8 m nằm
                // chéo 43° có AABB ~10,6×11 m phủ cả mặt đường trống, CSGT đứng ngoài thân xe vẫn bị
                // coi là kẹt bên trong và đứng im mãi (21.50). Đệm chỉ bù sai số ngoại suy của xe đang
                // chạy/xoay/đứng dậy; xe đứng yên dùng hình thật, vì xe bị hất trượt tới dừng cách CSGT
                // vài cm (nhỏ hơn đệm) sẽ khóa mọi hướng đi.
                Vector2 from=Flat(before-v.model.localPosition),delta=Flat(target-next)-from,vr=BodyRight(v),vf=BodyForward(v);
                float pad=BodyVelocity(v).sqrMagnitude>.0001f||v.angularVelocity.sqrMagnitude>.0001f||v.IsStandingUp?PolicePathPadding:0;
                float width=v.knocked?Mathf.Max(1.05f,v.halfWidth):v.halfWidth,enter=0,leave=1;bool overlap=true;
                for(int i=0;i<4&&overlap;i++)
                {
                    Vector2 axis=i==0?right:i==1?forward:i==2?vr:vf;
                    float radius=Mathf.Abs(Vector2.Dot(right,axis))*(.5f+pad)+Mathf.Abs(Vector2.Dot(forward,axis))*(1.2f+pad)+Mathf.Abs(Vector2.Dot(vr,axis))*width+Mathf.Abs(Vector2.Dot(vf,axis))*v.halfLength;
                    float p=Vector2.Dot(from,axis),d=Vector2.Dot(delta,axis);
                    overlap=ContactHalfSpace(p-radius,d,ref enter,ref leave)&&ContactHalfSpace(-p-radius,-d,ref enter,ref leave);
                }
                if(overlap)return false;
            }
            return true;
        }
        void TickRoutePolice(float dt)
        {
            if(policeBike==null||dt<=0)return;
            if(policeBody==null)policeBody=new TrafficVehicle{model=policeBike,halfWidth=.5f,halfLength=1.2f};
            Vector3 before=policeBike.localPosition;policeBody.previousPosition=before;policeBody.previousYaw=policeBike.localEulerAngles.y;policeBody.knocked=PoliceFallen;
            if(PoliceFallen)
            {
                policeBike.localPosition-=Vector3.forward*Speed*dt;ResolvePoliceContact(before,Vector3.zero,false);
                PoliceFallRemaining=Mathf.Max(0,PoliceFallRemaining-dt);PoliceSpeed=0;
                policeBike.localRotation=Quaternion.Euler(0,policeFallYaw,-policeFallSide*78*Mathf.Clamp01((1.2f-PoliceFallRemaining)/.16f));
                PoliceDistance=Distance-policeRouteAt;
                if(!PoliceFallen){policeBike.localRotation=Quaternion.Euler(0,policeFallYaw,0);policeBody.knocked=false;}
                return;
            }
            float nominal=PoliceDistance<=6.5f?cruiseSpeed-.65f:cruiseSpeed+15;
            if(IsTurning)nominal=PoliceDistance<=6.5f?TrafficPaceSpeed-.65f:TrafficPaceSpeed+2;
            float nextAt=Mathf.Min(Distance-.8f,policeRouteAt+Mathf.Max(0,nominal)*dt);var point=SampleRoute(nextAt);Quaternion yaw=Quaternion.Euler(0,point.yaw,0);
            Vector3 destination=point.point;bool clear=false;
            foreach(float side in new[]{-.9f,0,.9f})
            {
                var candidate=point.point+yaw*new Vector3(side,0,0);
                if(!RoutePolicePathClear(before,candidate,point.yaw,dt))continue;
                destination=candidate;clear=true;break;
            }
            Vector3 next=before-Vector3.forward*Speed*dt;
            if(clear)
            {
                // Bound lateral correction so avoidance never teleports across traffic.
                next=Vector3.MoveTowards(next,destination,Mathf.Max(0,nominal)*dt);PoliceSpeed=nominal;
                float advance=Mathf.Min(nextAt-policeRouteAt,Mathf.Max(0,nominal)*dt);policeRouteAt+=advance;
                policeBike.localRotation=Quaternion.Euler(0,point.yaw,0);
            }
            else {PoliceSpeed=Mathf.MoveTowards(PoliceSpeed,0,dt*45);PoliceAvoidances++;}
            policeBike.localPosition=next;ResolvePoliceContact(before,(next-before)/dt+Vector3.forward*Speed,true);
            if(PoliceFallen)policeFallYaw=policeBike.localEulerAngles.y;
            PoliceDistance=Distance-policeRouteAt;
            if(!PoliceFallen&&Vector3.Distance(policeBike.localPosition,rider.localPosition)<2.2f)Arrest();
            if(!IsTurning&&!PoliceFallen&&policeRouteAt>turnEndAt+1&&Mathf.Abs(Mathf.DeltaAngle(policeBike.localEulerAngles.y,0))<5)
            {routePolice=false;PoliceDistance=-policeBike.localPosition.z;}
        }
    }
}
