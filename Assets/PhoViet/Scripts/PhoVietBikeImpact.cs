using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        float bikeImpactSlowdown,bikeImpactOffset,bikeImpactYaw;
        Vector3 bikeAppliedOffset;
        float[] riderSoftImpactUntil;

        void ResetBikeImpact()
        {
            bikeImpactSlowdown=bikeImpactOffset=bikeImpactYaw=0;
            bikeAppliedOffset=Vector3.zero;
            riderSoftImpactUntil=new float[vehicles.Count];
        }

        bool IsMotorcycleContact(TrafficVehicle vehicle)=>!vehicle.armored&&vehicle.halfWidth<=.7f;
        bool IsCarBikeContact(TrafficVehicle car,TrafficVehicle bike)=>!car.armored&&car.halfWidth>.7f&&IsMotorcycleContact(bike);

        void AdvanceBikeImpact(float dt)
        {
            bikeImpactSlowdown=Mathf.MoveTowards(bikeImpactSlowdown,0,dt*.32f);
            bikeImpactOffset=Mathf.MoveTowards(bikeImpactOffset,0,dt*.9f);
            bikeImpactYaw=Mathf.MoveTowards(bikeImpactYaw,0,dt*21);
        }

        void ApplyBikeImpactOffset()
        {
            bikeAppliedOffset=Quaternion.Euler(0,IsTurning?TurnYaw:0,0)*Vector3.right*bikeImpactOffset;
            rider.localPosition+=bikeAppliedOffset;
            rider.localRotation=Quaternion.Euler(0,bikeImpactYaw,0)*rider.localRotation;
        }

        void DeflectRiderFromMotorcycle(TrafficVehicle bike,int index,Vector2 normal,float at,Vector3 before)
        {
            var other=BodyVelocity(bike);
            float closing=Mathf.Max(0,Vector2.Dot(Flat(before-other),normal));
            ApplyRiderSoftImpact(index,normal,closing);
            KnockMotorcycleFromContact(bike,normal,at,other,closing);
            notice="VA XE MÁY · XE CHỆCH HƯỚNG";
            noticeTimer=1.2f;
            if(sound&&fx!=null)fx.PlayOneShot(crashClip,.5f);
        }

        void ApplyRiderSoftImpact(int index,Vector2 normal,float closing)
        {
            float severity=Mathf.InverseLerp(0,38,closing);
            float side=-Vector2.Dot(normal,Flat(Quaternion.Euler(0,IsTurning?TurnYaw:0,0)*Vector3.right));
            if(Mathf.Abs(side)<.12f)side=CurrentLane>=2?-1:1;
            side=Mathf.Sign(side);
            float loss=Mathf.Lerp(.12f,.38f,severity);
            bikeImpactSlowdown=Mathf.Max(bikeImpactSlowdown,loss);
            Speed*=1-loss;
            rider.localRotation=Quaternion.Euler(0,-bikeImpactYaw,0)*rider.localRotation;
            bikeImpactOffset=side*Mathf.Lerp(.12f,.48f,severity);
            bikeImpactYaw=side*Mathf.Lerp(3,8,severity);
            riderSoftImpactUntil[index]=timeRiding+.5f;
            LastRiderImpactSpeed=closing;
            rider.localPosition-=bikeAppliedOffset;
            ApplyBikeImpactOffset();
        }

        void KnockMotorcycleFromContact(TrafficVehicle bike,Vector2 normal,float at,Vector3 other,float closing)
        {
            ShowVehicleContact(riderBody,bike,normal,closing,at);
            if(!bike.wallCrashed)LaunchMotorcycleAfterContact(bike,normal,at,other,closing,false);
            TrafficCollisions++;
        }

        void LaunchMotorcycleAfterContact(TrafficVehicle bike,Vector2 normal,float at,Vector3 before,float closing,bool credited)
        {
            bike.model.localPosition=Vector3.Lerp(bike.previousPosition,bike.model.localPosition,at);
            bike.footStopRemaining=0;
            float severity=Mathf.InverseLerp(0,38,closing);
            Vector3 push=new Vector3(normal.x,0,normal.y)*Mathf.Lerp(2.2f,8.5f,severity);
            ActivateCollisionBody(bike,before,before+push,Flat(push)*VehicleMass(bike),Vector2.zero,credited,!bike.knocked,closing);
        }
    }
}
