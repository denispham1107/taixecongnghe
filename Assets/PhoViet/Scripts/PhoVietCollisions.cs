using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        // Pursuit never grants immunity: every actual traffic contact immediately loses.
        bool ResolveRiderContacts(float oldX,float dt)
        {
            if(!IsChased&&collisionImmunity>0)return true;
            foreach(var v in vehicles)
            {
                if(!v.active||(!IsChased&&v.playerGrace>0)||v.model.localPosition.y>=2.4f)continue;
                Vector2 from=new Vector2(v.previousPosition.x-oldX,v.previousPosition.z);
                Vector2 to=new Vector2(v.model.localPosition.x-rider.localPosition.x,v.model.localPosition.z);
                if(!SweptOverlap(from,to,VehicleFootprint(v)+new Vector2(.33f,.91f)))continue;
                Crash();return false;
            }
            return true;
        }
        public static float VehicleMass(TrafficVehicle v)
        {return v.halfLength>3?4500:v.halfWidth>.7f?(v.model.name.Contains("tải")?2200:1350):220;}
        static Vector2 Flat(Vector3 p)=>new Vector2(p.x,p.z);
        static float BodyInertia(TrafficVehicle v)=>VehicleMass(v)*(4*v.halfLength*v.halfLength+4*v.halfWidth*v.halfWidth)/12;
        static float LeverCross(Vector2 lever,Vector2 direction)=>lever.y*direction.x-lever.x*direction.y;
        static Vector2 SpinVelocity(TrafficVehicle v,Vector2 lever)
        {float omega=v.physicalCrash?v.angularVelocity.y*Mathf.Deg2Rad:0;return new Vector2(omega*lever.y,-omega*lever.x);}
        Vector3 BodyVelocity(TrafficVehicle v)
        {
            if(v.wallCrashed)return Vector3.zero;
            if(v.knocked)return v.knockVelocity;
            if(v.motionVelocity.sqrMagnitude>.0001f)return v.motionVelocity;
            return v.crossing?new Vector3(v.crossDelay>0?0:v.crossSpeed,0,0):new Vector3(0,0,v.speed);
        }
        static Vector2 BodyForward(TrafficVehicle v)
        {float a=v.model.localEulerAngles.y*Mathf.Deg2Rad;return new Vector2(Mathf.Sin(a),Mathf.Cos(a));}
        static Vector2 BodyRight(TrafficVehicle v)
        {var f=BodyForward(v);return new Vector2(f.y,-f.x);}
        static float BodyWidth(TrafficVehicle v)=>v.knocked&&v.halfWidth<.7f?1.05f:v.halfWidth;
        static float ProjectedRadius(TrafficVehicle v,Vector2 axis)
        {return Mathf.Abs(Vector2.Dot(BodyRight(v),axis))*BodyWidth(v)+Mathf.Abs(Vector2.Dot(BodyForward(v),axis))*v.halfLength;}

        // Swept separating-axis test: contact must overlap on every body axis at the same instant.
        // The scroll velocity cancels in relative coordinates; impulses use actual road velocities.
        bool VehicleContact(TrafficVehicle a,TrafficVehicle b,out Vector2 normal,out float at,out float penetration)
        {
            normal=Vector2.zero;at=0;penetration=0;
            Vector2 from=Flat(b.previousPosition-a.previousPosition),to=Flat(b.model.localPosition-a.model.localPosition),delta=to-from;
            float enter=0,leave=1,minDepth=float.MaxValue;Vector2 entryNormal=Vector2.zero,minNormal=Vector2.zero;
            for(int i=0;i<4;i++)
            {
                Vector2 axis=i==0?BodyRight(a):i==1?BodyForward(a):i==2?BodyRight(b):BodyForward(b);
                float radius=ProjectedRadius(a,axis)+ProjectedRadius(b,axis),p=Vector2.Dot(from,axis),d=Vector2.Dot(delta,axis);
                float depth=radius-Mathf.Abs(p);
                if(depth<minDepth){minDepth=depth;minNormal=axis*(p>=0?1:-1);}
                if(Mathf.Abs(d)<.000001f){if(Mathf.Abs(p)>radius)return false;continue;}
                float t1=(-radius-p)/d,t2=(radius-p)/d,near=Mathf.Min(t1,t2),far=Mathf.Max(t1,t2);
                if(near>enter){enter=near;entryNormal=axis*(p+d*near>=0?1:-1);}
                leave=Mathf.Min(leave,far);if(enter>leave)return false;
            }
            if(leave<0||enter>1)return false;
            at=Mathf.Clamp01(enter);normal=entryNormal.sqrMagnitude>.5f?entryNormal:minNormal;
            float ay=Mathf.Lerp(a.previousPosition.y,a.model.localPosition.y,at),by=Mathf.Lerp(b.previousPosition.y,b.model.localPosition.y,at);
            float ah=a.halfLength>3?3.2f:a.halfWidth>.7f?1.9f:a.knocked?.6f:2.4f;
            float bh=b.halfLength>3?3.2f:b.halfWidth>.7f?1.9f:b.knocked?.6f:2.4f;
            if(ay+ah<by||by+bh<ay)return false;
            penetration=Mathf.Max(0,ProjectedRadius(a,normal)+ProjectedRadius(b,normal)-Mathf.Abs(Vector2.Dot(to,normal)));
            return true;
        }
        void ResolveVehicleCollisions()
        {
            if(contactUntil==null||contactUntil.GetLength(0)!=vehicles.Count)contactUntil=new float[vehicles.Count,vehicles.Count];
            for(int i=0;i<vehicles.Count;i++)for(int j=i+1;j<vehicles.Count;j++)
            {
                var a=vehicles[i];var b=vehicles[j];
                if(!a.active||!b.active||(a.wallCrashed&&b.wallCrashed)||contactUntil[i,j]>timeRiding)continue;
                if(!VehicleContact(a,b,out Vector2 normal,out float at,out float depth))continue;
                Vector2 impactA=Flat(Vector3.Lerp(a.previousPosition,a.model.localPosition,at)),impactB=Flat(Vector3.Lerp(b.previousPosition,b.model.localPosition,at));
                Vector2 contact=(impactA+impactB)*.5f,leverA=contact-impactA,leverB=contact-impactB;
                Vector3 av=BodyVelocity(a),bv=BodyVelocity(b);
                Vector2 relative=Flat(bv-av)+SpinVelocity(b,leverB)-SpinVelocity(a,leverA);float closing=-Vector2.Dot(relative,normal);
                float invA=a.wallCrashed?0:1/VehicleMass(a),invB=b.wallCrashed?0:1/VehicleMass(b),sum=invA+invB;
                float invIA=a.wallCrashed?0:1/BodyInertia(a),invIB=b.wallCrashed?0:1/BodyInertia(b);
                if(sum<=0)continue;
                if(closing<.35f)
                {
                    if(depth>0&&at==0)SeparateBodies(a,b,normal,Mathf.Min(depth,.5f),invA/sum,invB/sum);
                    continue;
                }
                // Low restitution and Coulomb friction dissipate energy instead of adding random launches.
                const float restitution=.18f,friction=.28f;
                float normalA=LeverCross(leverA,normal),normalB=LeverCross(leverB,normal);
                float impulse=(1+restitution)*closing/(sum+normalA*normalA*invIA+normalB*normalB*invIB);
                Vector2 tangent=new Vector2(-normal.y,normal.x);
                float tangentA=LeverCross(leverA,tangent),tangentB=LeverCross(leverB,tangent);
                float drag=Mathf.Clamp(-Vector2.Dot(relative,tangent)/(sum+tangentA*tangentA*invIA+tangentB*tangentB*invIB),-impulse*friction,impulse*friction);
                Vector2 push=normal*impulse+tangent*drag;
                Vector3 aAfter=av-new Vector3(push.x,0,push.y)*invA,bAfter=bv+new Vector3(push.x,0,push.y)*invB;
                bool credited=a.knockFromPlayer||b.knockFromPlayer;
                bool newA=!a.knocked,newB=!b.knocked;
                if((a.isReckless&&newB)||(b.isReckless&&newA))RecklessRamCount++;
                if(!a.wallCrashed)ActivateCollisionBody(a,av,aAfter,-push,contact-impactA,credited,newA,closing);
                if(!b.wallCrashed)ActivateCollisionBody(b,bv,bAfter,push,contact-impactB,credited,newB,closing);
                if(at>0)
                {
                    // Restore the impact position for a swept hit, preventing vehicles passing through each other.
                    if(!a.wallCrashed)a.model.localPosition=Vector3.Lerp(a.previousPosition,a.model.localPosition,at);
                    if(!b.wallCrashed)b.model.localPosition=Vector3.Lerp(b.previousPosition,b.model.localPosition,at);
                }
                else if(depth>0)SeparateBodies(a,b,normal,Mathf.Min(depth,2),invA/sum,invB/sum);
                contactUntil[i,j]=timeRiding+.12f;TrafficCollisions++;
                if(sound&&fx!=null&&Mathf.Min(Mathf.Abs(a.model.localPosition.z),Mathf.Abs(b.model.localPosition.z))<50)fx.PlayOneShot(crashClip,Mathf.Clamp01(closing/25)*.45f);
            }
        }
        static void SeparateBodies(TrafficVehicle a,TrafficVehicle b,Vector2 normal,float depth,float shareA,float shareB)
        {
            Vector3 separation=new Vector3(normal.x,0,normal.y)*(depth+.01f);
            if(!a.wallCrashed)a.model.localPosition-=separation*shareA;
            if(!b.wallCrashed)b.model.localPosition+=separation*shareB;
        }
        void ActivateCollisionBody(TrafficVehicle v,Vector3 before,Vector3 after,Vector2 impulse,Vector2 lever,bool credited,bool newlyHit,float closing)
        {
            float deltaV=Flat(after-before).magnitude;
            v.model.GetComponent<CarVisualAnimator>()?.ReceiveImpact(v.model.InverseTransformDirection(transform.TransformDirection(new Vector3(impulse.x,0,impulse.y))),closing);
            if(!v.physicalCrash)
            {
                v.crashEuler=v.model.localEulerAngles;
                v.crashEuler.x=Mathf.DeltaAngle(0,v.crashEuler.x);v.crashEuler.z=Mathf.DeltaAngle(0,v.crashEuler.z);
                v.angularVelocity=Vector3.zero;
            }
            v.knocked=true;v.physicalCrash=true;v.changingLane=false;v.turning=false;v.knockAge=newlyHit?0:v.knockAge;
            if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(false);
            v.knockDirection=credited&&v.knockFromPlayer?v.knockDirection:Mathf.Abs(impulse.x)>.01f?Mathf.Sign(impulse.x):v.model.localPosition.x>=0?1:-1;
            if(newlyHit){v.playerGrace=0;if(credited){ChainCount++;notice="VA CHẠM DÂY CHUYỀN  ×"+ChainCount;noticeTimer=1.3f;}}
            v.knockFromPlayer=credited;
            float inertia=BodyInertia(v);
            float torque=LeverCross(lever,impulse);
            v.angularVelocity.y=Mathf.Clamp(v.angularVelocity.y+torque/inertia*Mathf.Rad2Deg,-150,150);
            float lateral=Vector2.Dot(Flat(after-before),BodyRight(v));
            v.angularVelocity.z=Mathf.Clamp(v.angularVelocity.z-lateral*(v.halfWidth<.7f?12:3),-130,130);
            v.angularVelocity.x=Mathf.Clamp(v.angularVelocity.x+Vector2.Dot(Flat(after-before),BodyForward(v))*1.5f,-40,40);
            // Ground reaction can bounce a hard impact; a motorcycle cannot launch a much heavier truck.
            float hop=v.halfWidth<.7f?Mathf.Clamp(deltaV*.12f,0,2.8f):Mathf.Clamp((deltaV-6)*.08f,0,v.halfLength>3?.5f:1.2f);
            after.y=Mathf.Max(before.y,closing>6?hop:0);v.knockVelocity=after;v.motionVelocity=after;
        }
        void TickCollisionBody(TrafficVehicle v,float dt)
        {
            v.knockAge+=dt;v.knockVelocity.y-=9.81f*dt;
            Vector3 p=v.model.localPosition+(v.knockVelocity-Vector3.forward*Speed)*dt;
            bool grounded=p.y<=0;
            if(grounded)
            {
                p.y=0;v.knockVelocity.y=v.knockVelocity.y<-2?-v.knockVelocity.y*.12f:0;
                Vector2 velocity=Vector2.MoveTowards(Flat(v.knockVelocity),Vector2.zero,dt*4.8f);
                v.knockVelocity.x=velocity.x;v.knockVelocity.z=velocity.y;
            }
            v.crashEuler+=v.angularVelocity*dt;v.angularVelocity*=Mathf.Exp(-dt*(grounded?2.8f:.8f));
            if(v.halfWidth<.7f)v.crashEuler.z=Mathf.MoveTowards(v.crashEuler.z,-v.knockDirection*78,dt*150);
            else
            {
                v.crashEuler.z=Mathf.Clamp(v.crashEuler.z,-65,65);v.crashEuler.x=Mathf.Clamp(v.crashEuler.x,-30,30);
                if(grounded&&Mathf.Abs(v.crashEuler.z)<35)v.crashEuler.z=Mathf.MoveTowards(v.crashEuler.z,0,dt*20);
                if(grounded)v.crashEuler.x=Mathf.MoveTowards(v.crashEuler.x,0,dt*20);
            }
            v.model.localPosition=p;v.model.localRotation=Quaternion.Euler(v.crashEuler);
        }
    }
}
