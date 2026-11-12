using System;
using UnityEngine;

namespace PhoViet
{
    // The physics/pool root is never animated. Only the Blender skin, wheels and lamps move.
    public sealed class CarVisualAnimator : MonoBehaviour
    {
        public Transform visual,body,bumperFront,bumperRear;
        public Transform[] wheels,steering;
        public Animation animationPlayer;
        public AnimationClip[] clips;
        public string AssetName;
        public float wheelRadius=.38f,tailZ=-2.13f,tailY=.94f,tailX=.64f;
        public string CurrentClip {get;private set;}="Idle";
        public float WheelAngle {get;private set;}
        public float SampleTime {get;private set;}
        public float SteeringAngle {get;private set;}
        [NonSerialized] PhoVietGame game;
        [NonSerialized] PhoVietGame.TrafficVehicle vehicle;
        [NonSerialized] bool initialized;
        float clock,impactAge=10,lastYaw;
        WheelRolling wheelRolling;
        Vector3[] wreckPoints;
        string impactClip="ImpactFront";
        bool wasKnocked;

        void OnEnable(){initialized=false;game=null;vehicle=null;clock=0;impactAge=10;WheelAngle=0;SteeringAngle=0;wheelRolling=default;wasKnocked=false;}
        public void ResetVisualBinding(){OnEnable();}
        public void Initialize()
        {
            if(initialized||visual==null||animationPlayer==null)return;
            game=GetComponentInParent<PhoVietGame>();if(game==null)return;
            vehicle=game.vehicles.Find(v=>v.model==transform);if(vehicle==null)return;
            animationPlayer.playAutomatically=false;animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;animationPlayer.Stop();
            foreach(var clip in clips)animationPlayer.AddClip(clip,clip.name);
            foreach(AnimationState state in animationPlayer){state.enabled=false;state.weight=0;state.speed=0;}
            var variant=GetComponent<MeshyCarVariants>();
            wreckPoints=VehicleWreckSupport.Points(variant!=null?variant.ModelAsset:AssetName);
            if(variant!=null)CarWheelDetail.Bind(variant.ModelAsset,visual,wheels,wheelRadius);
            lastYaw=transform.localEulerAngles.y;initialized=true;Sample("Idle",0);
        }
        // localPush is the physical impulse applied to this vehicle, in its own axes.
        public void ReceiveImpact(Vector3 localPush,float closing)
        {
            if(closing<.35f)return;
            impactClip=Mathf.Abs(localPush.x)>Mathf.Abs(localPush.z)?(localPush.x>0?"ImpactLeft":"ImpactRight"):
                (localPush.z<0?"ImpactFront":"ImpactRear");
            impactAge=0;
        }
        void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
        public void Advance(float dt)
        {
            Initialize();if(!initialized||!vehicle.active&&!vehicle.playerClaimed)return;
            if(game.State==PhoVietGame.RideState.Paused){PoseWheels();return;}
            clock+=dt;
            if(vehicle.knocked&&!wasKnocked&&impactAge>1)ReceiveImpact(new Vector3(-vehicle.knockDirection,0,-1),10);
            if(!vehicle.knocked&&wasKnocked)SteeringAngle=0;
            wasKnocked=vehicle.knocked;
            bool trafficMoving=game.State==PhoVietGame.RideState.Riding||game.PlayerOnFoot||game.RiderCrashActive;
            float speed=vehicle.playerClaimed?(game.State==PhoVietGame.RideState.Riding?game.Speed:0):trafficMoving?game.TrafficWheelSpeed(vehicle):0;
            bool wreck=vehicle.playerClaimed?game.RiderWreckOnRoad:vehicle.knocked||vehicle.IsStandingUp;
            bool coasting=wreck||vehicle.riverFalling;
            impactAge=vehicle.knocked?impactAge+dt:10;
            string clip;float normalized;
            if(vehicle.knocked){clip=impactClip;normalized=Mathf.Clamp01(impactAge/.8f);}
            else if(vehicle.brakeRemaining>0){clip="Brake";normalized=Mathf.Repeat(clock,.8f)/.8f;}
            else if(speed>.1f){clip="Drive";normalized=Mathf.Repeat(clock,1)/1;}
            else{clip="Idle";normalized=0;}
            Sample(clip,normalized);
            float yaw=transform.localEulerAngles.y;
            if(dt>0&&!vehicle.knocked)
            {
                float yawRate=Mathf.DeltaAngle(lastYaw,yaw)/dt;
                float target=Mathf.Clamp(yawRate*2.72f/Mathf.Max(speed,4),-28,28);
                // Junction arcs and weaving have real changes in heading; front wheels follow those headings.
                SteeringAngle=Mathf.MoveTowards(SteeringAngle,target,dt*100);
            }
            lastYaw=yaw;
            WheelAngle=WheelRolling.Rotate(WheelAngle,wheelRolling.Advance(dt,speed,coasting),wheelRadius);
            PoseWheels();
            visual.position=transform.position;
            // Xe rơi/chìm dưới sông không bị nâng hình lên mặt đường.
            if(wreck&&!vehicle.riverFalling&&(vehicle.playerClaimed||!game.InRiverWater(transform)))
                visual.position+=Vector3.up*Mathf.Max(0,game.transform.position.y+.055f-LowestSupport());
            // Dynamic brake overlays follow animated body suspension and impact rotation.
            if(vehicle.brakeLights!=null&&body!=null)
            {
                vehicle.brakeLights.localPosition=body.localPosition-body.localRotation*new Vector3(0,.75f,0);
                vehicle.brakeLights.localRotation=body.localRotation;
            }
        }
        float LowestSupport()
        {
            if(wreckPoints!=null)return VehicleWreckSupport.LowestY(visual,wreckPoints);
            float lowest=float.PositiveInfinity;
            foreach(var wheel in wheels)for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;
                lowest=Mathf.Min(lowest,wheel.TransformPoint(new Vector3(0,wheelRadius*Mathf.Sin(a),wheelRadius*Mathf.Cos(a))).y);
            }
            var meshy=GetComponent<MeshyCarVariants>();
            bool heavy=wheelRadius>.45f;float width=meshy!=null?vehicle.halfWidth:heavy?1.14f:.90f,top=meshy!=null?meshy.Height:heavy?3.18f:AssetName=="PV_DeliveryVan"?2.14f:1.73f,length=meshy!=null?vehicle.halfLength:heavy?3.23f:2.15f;
            for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)
            {
                // Include the lower body edges as well as the roof when a wreck rolls or pitches.
                lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(s*width,top,z*length)).y);
                lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(s*width,wheelRadius*.35f,z*length)).y);
            }
            return lowest;
        }
        void PoseWheels()
        {
            if(wheels==null||steering==null)return;
            for(int i=0;i<wheels.Length;i++)
            {
                steering[i].localRotation=Quaternion.Euler(0,i<2?SteeringAngle:0,0);
                wheels[i].localRotation=Quaternion.Euler(WheelAngle,0,0);
            }
        }
        void Sample(string name,float normalized)
        {
            var state=animationPlayer[name];if(state==null)return;
            if(!animationPlayer.IsPlaying(name))animationPlayer.Play(name,PlayMode.StopAll);
            state.enabled=true;state.weight=1;state.speed=0;state.wrapMode=WrapMode.ClampForever;state.time=Mathf.Clamp01(normalized)*state.length;
            animationPlayer.Sample();CurrentClip=name;SampleTime=state.time;
        }
    }
}
