using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        public int RoadTrafficTurns {get;private set;}
        bool TryBeginRoadTurn(TrafficVehicle v,float dt)
        {
            if(!trafficSurprises||dt<=0||v.hasTurned||v.changingLane||v.enteringFromBehind||v.isReckless||v.knocked)return false;
            float direction=Mathf.Sign(v.baseTrafficSpeed),nearest=float.PositiveInfinity,centre=0;
            foreach(var block in blocks)
            {
                float z=block.localPosition.z+VietnamStreet.IntersectionCenter;
                float ahead=(z-v.model.localPosition.z)*direction;
                if(ahead>=-1&&ahead<nearest){nearest=ahead;centre=z;}
            }
            if(nearest>18)return false;
            float key=Distance+centre;
            if(float.IsNaN(v.roadDecisionAt)||Mathf.Abs(v.roadDecisionAt-key)>1)
            {
                v.roadDecisionAt=key;
                v.turnChoice=rng.NextDouble()<.68?(rng.Next(2)==0?-1:1):0;
            }
            if(v.turnChoice==0)return false;
            float radius=4.2f+(v.halfWidth>.7f?1.2f:0);
            if(nearest>radius+Mathf.Abs(v.speed)*dt||nearest<radius-2)return false;
            v.roadTurning=true;v.changingLane=false;v.brakeRemaining=0;v.roadTurnAngle=0;v.roadTurnRadius=radius;
            v.roadTurnStart=v.model.localPosition;v.roadTurnForward=Vector3.forward*direction;
            v.roadTurnSide=Vector3.right*direction*v.turnChoice;v.roadTurnSpeed=Mathf.Min(Mathf.Abs(v.speed),v.halfWidth>.7f?7:9);
            RoadTrafficTurns++;IntersectionTurns++;return true;
        }
        void TickRoadTurn(TrafficVehicle v,float dt)
        {
            v.roadTurnStart-=Vector3.forward*Speed*dt;
            float previous=v.roadTurnAngle;
            float angle=Mathf.Min(Mathf.PI*.5f,previous+v.roadTurnSpeed*dt/v.roadTurnRadius);v.roadTurnAngle=angle;
            v.model.localPosition=v.roadTurnStart+v.roadTurnRadius*(v.roadTurnForward*Mathf.Sin(angle)+v.roadTurnSide*(1-Mathf.Cos(angle)));
            Vector3 heading=v.roadTurnForward*Mathf.Cos(angle)+v.roadTurnSide*Mathf.Sin(angle);
            float yaw=Mathf.Atan2(heading.x,heading.z)*Mathf.Rad2Deg;
            v.model.localRotation=Quaternion.Euler(0,yaw,v.halfWidth<.7f?-v.turnChoice*12*Mathf.Sin(angle*2):0);
            if(angle<Mathf.PI*.5f-.00001f)return;
            v.roadTurning=false;v.hasTurned=true;v.routeDetached=true;v.routeVelocity=v.roadTurnSide*v.roadTurnSpeed;
            float used=(angle-previous)*v.roadTurnRadius/v.roadTurnSpeed;
            v.model.localPosition+=v.routeVelocity*Mathf.Max(0,dt-used);
            v.model.localRotation=Quaternion.Euler(0,yaw,0);
        }
    }
}
