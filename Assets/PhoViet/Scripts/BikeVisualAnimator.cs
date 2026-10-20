using System;
using System.Linq;
using UnityEngine;

namespace PhoViet
{
    // Animation only: traffic movement, hit detection, loss and police recovery stay in game logic.
    public sealed class BikeVisualAnimator : MonoBehaviour
    {
        public enum ActorRole { Player, Traffic, Police, Parked }
        public ActorRole role;
        public Transform visual;
        public Animation animationPlayer;
        public AnimationClip[] clips;
        public Transform frontWheel,rearWheel;
        public string AssetName;
        public string CurrentClip {get;private set;}="Idle";
        public float SampleTime {get;private set;}
        public float WheelAngle {get;private set;}
        [NonSerialized] PhoVietGame game;
        [NonSerialized] PhoVietGame.TrafficVehicle vehicle;
        Vector3 baseVisualPosition;
        Quaternion frontRest,rearRest;
        Quaternion baseVisualRotation;
        Transform leftFoot,rightFoot;
        float clock,kickAge=10,fallAge;
        int kickSide=1;
        BikeJumpPose jumpPose=new BikeJumpPose();
        float jumpBalancePitch,jumpPitchVelocity;
        Transform[] poseBones;Vector3[] restPositions,restScales;Quaternion[] restRotations;
        [NonSerialized] bool initialized;
        PhoVietGame.RideState previousState;

        void OnEnable()
        {
            initialized=false;game=null;vehicle=null;clock=0;kickAge=10;fallAge=0;WheelAngle=0;jumpBalancePitch=jumpPitchVelocity=0;
            if(visual!=null){visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;}
        }
        void Start(){Initialize();}
        public void Initialize()
        {
            if(initialized||visual==null||animationPlayer==null)return;
            game=GetComponentInParent<PhoVietGame>();
            if(game==null)return;
            vehicle=game.vehicles.Find(v=>v.model==transform);
            baseVisualPosition=visual.localPosition;
            baseVisualRotation=visual.localRotation;
            var bones=visual.GetComponentsInChildren<Transform>(true);
            if(poseBones==null)
            {
                poseBones=bones.Where(t=>t!=visual).ToArray();restPositions=poseBones.Select(t=>t.localPosition).ToArray();
                restRotations=poseBones.Select(t=>t.localRotation).ToArray();restScales=poseBones.Select(t=>t.localScale).ToArray();
            }
            leftFoot=bones.FirstOrDefault(t=>t.name=="LFoot");rightFoot=bones.FirstOrDefault(t=>t.name=="RFoot");
            // All exported wheel bones have identity local bind rotation. Never capture a
            // previous spawn's wheel spin or an editor hot-reload pose as the new bind pose.
            frontRest=Quaternion.identity;rearRest=Quaternion.identity;
            animationPlayer.playAutomatically=false;
            animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;
            animationPlayer.Stop();
            foreach(var clip in clips)if(clip!=null)animationPlayer.AddClip(clip,clip.name);
            foreach(AnimationState state in animationPlayer){state.enabled=false;state.weight=0;state.speed=0;}
            initialized=true;previousState=game.State;Sample("Idle",0);
            if(role==ActorRole.Player)jumpPose.Initialize(visual);
        }
        public void Kick(float direction){kickSide=direction<0?-1:1;kickAge=0;}
        void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
        public void Advance(float dt)
        {
            Initialize();if(!initialized||game==null)return;
            if(game.State==PhoVietGame.RideState.Paused)
            {RotateWheel(frontWheel,frontRest);RotateWheel(rearWheel,rearRest);return;}
            if(role==ActorRole.Traffic&&(vehicle==null||!vehicle.active))return;
            if(role==ActorRole.Parked)
            {clock+=dt;Sample("Idle",Mathf.Repeat(clock,2.4f)/2.4f);return;}
            if(game.State==PhoVietGame.RideState.Ready&&previousState!=game.State)
            {clock=0;kickAge=10;fallAge=0;WheelAngle=0;}
            previousState=game.State;clock+=dt;kickAge+=dt;
            bool recovering=role==ActorRole.Player&&game.State==PhoVietGame.RideState.Recovering||role==ActorRole.Traffic&&vehicle.IsStandingUp;
            bool falling=role==ActorRole.Player?(game.State==PhoVietGame.RideState.Falling||game.State==PhoVietGame.RideState.GameOver):
                role==ActorRole.Police?game.PoliceFallen:vehicle.knocked;
            float speed=role==ActorRole.Player?game.Speed:role==ActorRole.Police?game.PoliceSpeed:
                vehicle.crossing?vehicle.crossSpeed:vehicle.speed;
            string clip="Idle";float normalized=0;
            if(recovering){clip="Recover";normalized=role==ActorRole.Player?game.RiderRecoveryProgress:vehicle.StandUpProgress;speed=0;fallAge=0;}
            else if(falling)
            {
                if(role==ActorRole.Player){if(game.State==PhoVietGame.RideState.Falling)fallAge+=dt;}
                else fallAge=role==ActorRole.Police?1.2f-game.PoliceFallRemaining:vehicle.knockAge;
                float z=role==ActorRole.Player?game.RiderFallRoll:Mathf.DeltaAngle(0,transform.localEulerAngles.z);
                clip=z>0?"FallLeft":"FallRight";normalized=Mathf.Clamp01(fallAge/.75f);
                if(role==ActorRole.Police&&game.PoliceFallRemaining<.35f)
                {clip="Recover";normalized=1-Mathf.Clamp01(game.PoliceFallRemaining/.35f);}
                speed=0;
            }
            else
            {
                fallAge=0;
                if(role==ActorRole.Player&&kickAge<.55f&&game.State==PhoVietGame.RideState.Riding)
                {clip=kickSide<0?"KickLeft":"KickRight";normalized=kickAge/.55f;}
                else if(game.State==PhoVietGame.RideState.Riding&&Mathf.Abs(speed)>.1f)
                {clip="Ride";normalized=Mathf.Repeat(clock, .8f)/.8f;}
                else {clip="Idle";normalized=Mathf.Repeat(clock,2.4f)/2.4f;speed=0;}
            }
            Sample(clip,normalized);
            if(!falling&&!recovering)WheelAngle=Mathf.Repeat(WheelAngle+speed/.34f*Mathf.Rad2Deg*dt,360);
            RotateWheel(frontWheel,frontRest);RotateWheel(rearWheel,rearRest);
            // Physics owns the outer vehicle roll. Lift only its visual skin so fallen
            // handlebars, panniers and the rider settle above the road rather than through it.
            float recovery=role==ActorRole.Police&&falling?1-Mathf.Clamp01(game.PoliceFallRemaining/.35f):0;
            float signedRoll=Mathf.DeltaAngle(0,transform.localEulerAngles.z);
            visual.localRotation=baseVisualRotation*Quaternion.Euler(0,0,-signedRoll*recovery);
            visual.position=transform.TransformPoint(baseVisualPosition);
            if(role==ActorRole.Player&&game.State==PhoVietGame.RideState.Riding)game.PoseJumpPassenger(0,0);
            if(role==ActorRole.Player&&game.State==PhoVietGame.RideState.Riding&&game.RiderJumpActive)
            {
                float pitch=jumpPose.Apply(game.RiderJumpClip,game.RiderJumpPose);
                jumpBalancePitch=Mathf.SmoothDamp(jumpBalancePitch,pitch,ref jumpPitchVelocity,.045f,500,dt);
                pitch=jumpBalancePitch;
                Quaternion balance=Quaternion.Euler(pitch,0,0);
                Vector3 rearContact=new Vector3(0,0,-.73f);
                visual.localRotation=baseVisualRotation*balance;
                visual.localPosition=baseVisualPosition+rearContact-balance*rearContact;
                float lift=Mathf.Max(0,transform.position.y+.008f-LowestSupport());
                visual.position+=Vector3.up*lift;
                game.PoseJumpPassenger(pitch,visual.localPosition.y);
                CurrentClip=game.RiderJumpClip;
            }
            else {jumpBalancePitch=jumpPitchVelocity=0;}
            if(falling||recovering)visual.position+=Vector3.up*Mathf.Max(0,game.transform.position.y+.012f-LowestSupport());
        }
        void Sample(string name,float normalized)
        {
            var state=animationPlayer[name];if(state==null)return;
            // Some Blender actions key only moving joints. Restore unkeyed bones
            // before sampling so a recovery pose cannot leave raised arms on Ride.
            for(int i=0;i<poseBones.Length;i++)if(poseBones[i]!=null)
            {poseBones[i].localPosition=restPositions[i];poseBones[i].localRotation=restRotations[i];poseBones[i].localScale=restScales[i];}
            if(!animationPlayer.IsPlaying(name))animationPlayer.Play(name,PlayMode.StopAll);
            state.enabled=true;state.weight=1;state.speed=0;state.wrapMode=WrapMode.ClampForever;
            state.time=Mathf.Clamp01(normalized)*state.length;animationPlayer.Sample();
            CurrentClip=name;SampleTime=state.time;
        }
        void RotateWheel(Transform wheel,Quaternion rest)
        {
            if(wheel==null)return;
            Vector3 axis=wheel.parent.InverseTransformDirection(transform.right);
            wheel.localRotation=Quaternion.AngleAxis(WheelAngle,axis)*rest;
        }
        float LowestSupport()
        {
            float lowest=float.PositiveInfinity;
            // Tyre outlines follow the visual's actual roll/pitch, including the smooth
            // police recovery. This is visual ground contact; no gameplay body is moved.
            for(int wheel=0;wheel<2;wheel++)for(int i=0;i<8;i++)for(int side=-1;side<=1;side+=2)
            {
                float z=wheel==0?-.73f:.78f;
                float a=i*Mathf.PI*.25f;
                lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(side*.078f,.34f+.34f*Mathf.Sin(a),z+.34f*Mathf.Cos(a))).y);
            }
            for(int side=-1;side<=1;side+=2)
            {
                lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(side*.36f,.23f,-.95f)).y);
                lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(side*.50f,1.26f,.5f)).y);
                if(role==ActorRole.Police)lowest=Mathf.Min(lowest,visual.TransformPoint(new Vector3(side*.58f,.585f,-.95f)).y);
            }
            lowest=FootSupport(leftFoot,lowest);return FootSupport(rightFoot,lowest);
        }
        static float FootSupport(Transform foot,float lowest)
        {
            if(foot==null)return lowest;
            for(int x=-1;x<=1;x+=2)for(int y=0;y<2;y++)for(int z=0;z<2;z++)
                lowest=Mathf.Min(lowest,foot.TransformPoint(new Vector3(x*.09f,y==0?-.10f:.04f,z==0?-.10f:.25f)).y);
            return lowest;
        }
    }
}
