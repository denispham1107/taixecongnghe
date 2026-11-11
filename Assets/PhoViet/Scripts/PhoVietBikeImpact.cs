using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        float bikeImpactSlowdown,bikeImpactOffset,bikeImpactYaw;
        Vector3 bikeAppliedOffset;
        float[] bikeImpactUntil;

        void ResetBikeImpact()
        {
            bikeImpactSlowdown=bikeImpactOffset=bikeImpactYaw=0;
            bikeAppliedOffset=Vector3.zero;
            bikeImpactUntil=new float[vehicles.Count];
        }

        bool IsMotorcycleContact(TrafficVehicle vehicle)=>!vehicle.armored&&vehicle.halfWidth<=.7f;

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
            float severity=Mathf.InverseLerp(0,38,closing);
            float side=-Vector2.Dot(normal,Flat(Quaternion.Euler(0,IsTurning?TurnYaw:0,0)*Vector3.right));
            if(Mathf.Abs(side)<.12f)side=CurrentLane>=2?-1:1;
            side=Mathf.Sign(side);
            float loss=Mathf.Lerp(.12f,.65f,severity);
            bikeImpactSlowdown=Mathf.Max(bikeImpactSlowdown,loss);
            Speed*=1-loss;
            rider.localRotation=Quaternion.Euler(0,-bikeImpactYaw,0)*rider.localRotation;
            bikeImpactOffset=side*Mathf.Lerp(.16f,.67f,severity);
            bikeImpactYaw=side*Mathf.Lerp(4,17,severity);
            bikeImpactUntil[index]=timeRiding+.5f;
            LastRiderImpactSpeed=closing;
            KnockMotorcycleFromContact(bike,normal,at,other,closing,severity);

            rider.localPosition-=bikeAppliedOffset;
            ApplyBikeImpactOffset();
            notice="VA XE MÁY · XE CHỆCH HƯỚNG";
            noticeTimer=1.2f;
            if(sound&&fx!=null)fx.PlayOneShot(crashClip,.5f);
        }

        void KnockMotorcycleFromContact(TrafficVehicle bike,Vector2 normal,float at,Vector3 other,float closing,float severity)
        {
            ShowVehicleContact(riderBody,bike,normal,closing,at);
            bike.model.localPosition=Vector3.Lerp(bike.previousPosition,bike.model.localPosition,at);
            Vector3 push=new Vector3(normal.x,0,normal.y)*Mathf.Lerp(2.2f,8.5f,severity);
            ActivateCollisionBody(bike,other,other+push,Flat(push)*VehicleMass(bike),Vector2.zero,false,!bike.knocked,closing);
            TrafficCollisions++;
        }
    }
}
