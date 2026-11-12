using UnityEngine;
namespace PhoViet
{
 public sealed class HelicopterVisual:MonoBehaviour
 {
  public Transform airframe,mainRotor,tailRotor;
  public AudioSource rotorAudio;
  public float MainAngle{get;private set;}public float TailAngle{get;private set;}
  PhoVietGame game;float clock,gain,rotorFactor=1;bool wrecked;
  static readonly Vector3[] BodySupport=VehicleWreckSupport.Points("Helicopter"),
   MainSupport=VehicleWreckSupport.Points("Main rotor"),TailSupport=VehicleWreckSupport.Points("Tail rotor");
  internal float WreckBottomY()=>Mathf.Min(VehicleWreckSupport.LowestY(airframe,BodySupport),
   Mathf.Min(VehicleWreckSupport.LowestY(mainRotor,MainSupport),VehicleWreckSupport.LowestY(tailRotor,TailSupport)));
  public bool Wrecked=>wrecked;
  public void SetWrecked(){wrecked=true;airframe.localPosition=Vector3.zero;airframe.localRotation=Quaternion.identity;}
  void Awake(){game=GetComponentInParent<PhoVietGame>();PhoVietAudio.Route(rotorAudio);}
  void LateUpdate(){Advance(Mathf.Min(Time.deltaTime,.05f));}
  public void Advance(float dt)
  {
   if(game==null)game=GetComponentInParent<PhoVietGame>();if(game==null||!gameObject.activeInHierarchy)return;
   if(game.State==PhoVietGame.RideState.Paused){if(rotorAudio!=null&&rotorAudio.isPlaying)rotorAudio.Pause();return;}
   clock+=dt;if(wrecked)rotorFactor=Mathf.MoveTowards(rotorFactor,0,dt*1.5f);MainAngle=Mathf.Repeat(MainAngle+2340*dt*rotorFactor,360);TailAngle=Mathf.Repeat(TailAngle+10800*dt*rotorFactor,360);
   mainRotor.localRotation=Quaternion.Euler(0,MainAngle,0);tailRotor.localRotation=Quaternion.Euler(TailAngle,0,0);
   if(!wrecked){airframe.localPosition=Vector3.up*(.10f*Mathf.Sin(clock*2.3f));airframe.localRotation=Quaternion.Euler(-3+.6f*Mathf.Sin(clock*1.7f),0,1.7f*Mathf.Sin(clock*1.2f));}
   if(rotorAudio!=null){gain=Mathf.MoveTowards(gain,1,dt*2);rotorAudio.volume=game.HelicopterSoundEnabled?gain*.38f*rotorFactor:0;rotorAudio.pitch=(1+.015f*Mathf.Sin(clock*.8f))*Mathf.Max(.35f,rotorFactor);if(!rotorAudio.isPlaying)rotorAudio.UnPause();}
  }
  public void ResetPose(){wrecked=false;rotorFactor=1;clock=0;gain=0;MainAngle=TailAngle=0;mainRotor.localRotation=tailRotor.localRotation=Quaternion.identity;if(rotorAudio!=null){rotorAudio.volume=0;rotorAudio.Play();}Advance(0);}
  void OnDisable(){if(rotorAudio!=null)rotorAudio.Stop();}
 }
}
