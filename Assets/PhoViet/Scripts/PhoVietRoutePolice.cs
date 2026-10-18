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
        bool RoutePolicePathClear(Vector3 before,Vector3 target,float yaw,float dt)
        {
            float a=yaw*Mathf.Deg2Rad,c=Mathf.Abs(Mathf.Cos(a)),s=Mathf.Abs(Mathf.Sin(a));
            var half=new Vector2(.5f*c+1.2f*s+.08f,1.2f*c+.5f*s+.08f);
            foreach(var v in vehicles)
            {
                if(!v.active||v.model.localPosition.y>2.45f)continue;
                Vector3 next=v.model.localPosition+(BodyVelocity(v)-Vector3.forward*Speed)*Mathf.Max(dt,.15f);
                if(SweptOverlap(Flat(v.model.localPosition-before),Flat(next-target),VehicleFootprint(v)+half))return false;
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
            if(IsTurning)nominal=PoliceDistance<=6.5f?Speed-.65f:Speed+2;
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
