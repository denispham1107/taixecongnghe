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
        public UrbanRiderMotion urbanMotion;
        float rearWheelAngle;
        public string CurrentClip {get;private set;}="Idle";
        public float SampleTime {get;private set;}
        public float WheelAngle {get;private set;}
        [NonSerialized] PhoVietGame game;
        [NonSerialized] PhoVietGame.TrafficVehicle vehicle;
        Vector3 baseVisualPosition;
        Quaternion frontRest,rearRest;
        Quaternion baseVisualRotation;
        Transform leftFoot,rightFoot;
        Vector3[][] kickFeet;
        public bool HasKickContactPose {get{Initialize();return kickFeet!=null;}}
        public Vector3 KickFootLocal(float side,float normalized,bool toe)
        {
            Initialize();if(kickFeet==null)return Vector3.zero;
            var points=kickFeet[(side<0?0:2)+(toe?1:0)];float at=Mathf.Clamp01(normalized)*(points.Length-1);
            int i=Mathf.Min(points.Length-2,(int)at);return Vector3.Lerp(points[i],points[i+1],at-i);
        }
        float clock,fallAge;
        int kickSide=1;
        BikeJumpPose jumpPose=new BikeJumpPose();
        float jumpBalancePitch,jumpPitchVelocity;
        Transform[] poseBones;Vector3[] restPositions,restScales;Quaternion[] restRotations;
        [NonSerialized] bool initialized;
        PhoVietGame.RideState previousState;

        void OnEnable()
        {
            initialized=false;game=null;vehicle=null;clock=0;fallAge=0;WheelAngle=0;jumpBalancePitch=jumpPitchVelocity=0;
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
            if(urbanMotion!=null)urbanMotion.Initialize();
            if(role==ActorRole.Player)
            {
                // The chased player stays detailed even in the wider portrait view.
                var lod=visual.GetComponent<LODGroup>();if(lod!=null)lod.ForceLOD(0);
                jumpPose.Initialize(visual);
                if(kickFeet==null&&leftFoot!=null&&rightFoot!=null&&animationPlayer["KickLeft"]!=null&&animationPlayer["KickRight"]!=null)
                {
                    kickFeet=new Vector3[4][];for(int i=0;i<4;i++)kickFeet[i]=new Vector3[65];
                    for(int side=0;side<2;side++)for(int i=0;i<65;i++)
                    {
                        Sample(side==0?"KickLeft":"KickRight",i/64f);var foot=side==0?leftFoot:rightFoot;
                        for(int toe=0;toe<2;toe++)kickFeet[side*2+toe][i]=transform.InverseTransformPoint(foot.TransformPoint(new Vector3(0,-.03f,toe==1?.23f:-.06f)));
                    }
                    Sample("Idle",0);
                }
            }
        }
        public void Kick(float direction){kickSide=direction<0?-1:1;}
        void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
        public void Advance(float dt)
        {
            Initialize();if(!initialized||game==null)return;
            if(game.State==PhoVietGame.RideState.Paused)
            {RotateWheels();return;}
            if(role==ActorRole.Traffic&&(vehicle==null||!vehicle.active))return;
            if(role==ActorRole.Parked)
            {clock+=dt;Sample("Idle",Mathf.Repeat(clock,2.4f)/2.4f);if(urbanMotion!=null)urbanMotion.Advance(dt,false);return;}
            if(game.State==PhoVietGame.RideState.Ready&&previousState!=game.State)
            {clock=0;fallAge=0;WheelAngle=0;}
            previousState=game.State;clock+=dt;
            bool recovering=role==ActorRole.Traffic&&vehicle.IsStandingUp;
            bool falling=role==ActorRole.Player?(game.State==PhoVietGame.RideState.Falling||game.State==PhoVietGame.RideState.GameOver||game.State==PhoVietGame.RideState.Arrested&&game.CrashDuringPursuit):
                role==ActorRole.Police?(game.PoliceFallen||game.PoliceInRiver):vehicle.knocked;
            float speed=role==ActorRole.Player?game.Speed:role==ActorRole.Police?game.PoliceSpeed:
                vehicle.crossing?vehicle.crossSpeed:vehicle.speed;
            string clip="Idle";float normalized=0;
            if(recovering){clip="Recover";normalized=vehicle.StandUpProgress;speed=0;fallAge=0;}
            else if(falling)
            {
                if(role==ActorRole.Player){if(game.State==PhoVietGame.RideState.Falling)fallAge+=dt;}
                else fallAge=role==ActorRole.Police?(game.PoliceInRiver?game.PoliceRiverAge:1.2f-game.PoliceFallRemaining):vehicle.knockAge;
                float z=role==ActorRole.Player?game.RiderFallRoll:Mathf.DeltaAngle(0,transform.localEulerAngles.z);
                clip=z>0?"FallLeft":"FallRight";normalized=Mathf.Clamp01(fallAge/.75f);
                if(role==ActorRole.Police&&!game.PoliceInRiver&&game.PoliceFallRemaining<.35f)
                {clip="Recover";normalized=1-Mathf.Clamp01(game.PoliceFallRemaining/.35f);}
                speed=0;
            }
            else
            {
                fallAge=0;
                if(role==ActorRole.Player&&game.KickSwingActive&&game.State==PhoVietGame.RideState.Riding)
                {clip=kickSide<0?"KickLeft":"KickRight";normalized=game.KickSwingPose;}
                else if(game.State==PhoVietGame.RideState.Riding&&Mathf.Abs(speed)>.1f)
                {clip="Ride";normalized=Mathf.Repeat(clock, .8f)/.8f;}
                else {clip="Idle";normalized=Mathf.Repeat(clock,2.4f)/2.4f;speed=0;}
            }
            Sample(clip,normalized);
            if(!falling&&!recovering)
            {
                float spinSpeed=urbanMotion!=null?Mathf.Abs(speed):speed;
                WheelAngle=Mathf.Repeat(WheelAngle+spinSpeed/(urbanMotion!=null?urbanMotion.frontRadius:.34f)*Mathf.Rad2Deg*dt,360);
                rearWheelAngle=Mathf.Repeat(rearWheelAngle+spinSpeed/(urbanMotion!=null?urbanMotion.rearRadius:.34f)*Mathf.Rad2Deg*dt,360);
            }
            // Physics owns the outer vehicle roll. Lift only its visual skin so fallen
            // handlebars, panniers and the rider settle above the road rather than through it.
            float recovery=role==ActorRole.Police&&falling&&!game.PoliceInRiver?1-Mathf.Clamp01(game.PoliceFallRemaining/.35f):0;
            float signedRoll=Mathf.DeltaAngle(0,transform.localEulerAngles.z);
            // Người chơi đổi làn: thân xe quay theo hướng lái quanh trục đứng của xe (chỉ phần hình).
            Quaternion steer=role==ActorRole.Player?Quaternion.Euler(0,game.RiderSteerYaw,0):Quaternion.identity;
            visual.localRotation=steer*baseVisualRotation*Quaternion.Euler(0,0,-signedRoll*recovery);
            visual.position=transform.TransformPoint(baseVisualPosition);
            if(role==ActorRole.Player&&game.State==PhoVietGame.RideState.Riding)game.PoseJumpPassenger(0,0);
            if(role==ActorRole.Player&&game.State==PhoVietGame.RideState.Riding&&game.RiderJumpActive)
            {
                float pitch=jumpPose.Apply(game.RiderJumpClip,game.RiderJumpPose);
                jumpBalancePitch=Mathf.SmoothDamp(jumpBalancePitch,pitch,ref jumpPitchVelocity,.045f,500,dt);
                pitch=jumpBalancePitch;
                Quaternion balance=Quaternion.Euler(pitch,0,0);
                Vector3 rearContact=new Vector3(0,0,-.73f);
                visual.localRotation=steer*baseVisualRotation*balance;
                visual.localPosition=baseVisualPosition+steer*(rearContact-balance*rearContact);
                float lift=Mathf.Max(0,transform.position.y+.008f-LowestSupport());
                visual.position+=Vector3.up*lift;
                game.PoseJumpPassenger(pitch,visual.localPosition.y);
                CurrentClip=game.RiderJumpClip;
            }
            else {jumpBalancePitch=jumpPitchVelocity=0;}
            if(urbanMotion!=null)urbanMotion.Advance(dt,falling||recovering);
            RotateWheels();
            // Rơi/chìm dưới sông thì không nâng hình lên mặt đường (trước đây làm xe/người như nổi ngang mặt đường).
            bool inRiver=role==ActorRole.Player?game.RiderInRiver:(vehicle!=null&&vehicle.riverFalling)||game.InRiverWater(transform);
            if((falling||recovering||urbanMotion!=null)&&!inRiver)visual.position+=Vector3.up*Mathf.Max(0,game.transform.position.y+.012f-LowestSupport());
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
        void RotateWheels()
        {
            if(urbanMotion!=null)
            {
                frontWheel.localRotation=Quaternion.AngleAxis(WheelAngle,urbanMotion.FrontSpinAxis)*urbanMotion.FrontRest;
                rearWheel.localRotation=Quaternion.AngleAxis(rearWheelAngle,urbanMotion.RearSpinAxis)*urbanMotion.RearRest;
            }
            else {RotateWheel(frontWheel,frontRest);RotateWheel(rearWheel,rearRest);}
        }
        void RotateWheel(Transform wheel,Quaternion rest)
        {
            if(wheel==null)return;
            Vector3 axis=wheel.parent.InverseTransformDirection(transform.right);
            wheel.localRotation=Quaternion.AngleAxis(WheelAngle,axis)*rest;
        }
        float LowestSupport()
        {
            float lowest=urbanMotion!=null?urbanMotion.LowestWheelSupport():float.PositiveInfinity;
            // Tyre outlines follow the visual's actual roll/pitch, including the smooth
            // police recovery. This is visual ground contact; no gameplay body is moved.
            for(int wheel=0;wheel<(urbanMotion!=null?0:2);wheel++)for(int i=0;i<8;i++)for(int side=-1;side<=1;side+=2)
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
