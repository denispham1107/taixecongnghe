using System.Collections.Generic;
using UnityEngine;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  // Match the road-to-head height of the seated white Urban Rider in the scene.
  const float FootStandingHeight=2.15f,FootSourceStandingHeight=1.70f;

  struct FootBonePose
  {
   public Transform bone;
   public Vector3 position;
   public Quaternion rotation;
  }

  FootBonePose[] footStandingPose;
  float footRunBlend;
  bool footActionPoseApplied;
  Transform footSpine;
  FootLimb footRightArm,footLeftArm,footRightLeg,footLeftLeg;

  struct FootLimb
  {
   public Transform upper,lower,end;
  }

  void PrepareFootStandingPose(SkinnedMeshRenderer skin)
  {
   var bones=skin.bones;
   var bindPoses=skin.sharedMesh.bindposes;
   var worldBind=new Dictionary<Transform,Matrix4x4>(bones.Length);
   Matrix4x4 skinToWorld=skin.transform.localToWorldMatrix;
   for(int i=0;i<bones.Length;i++)
    if(bones[i]!=null)worldBind[bones[i]]=skinToWorld*bindPoses[i].inverse;

   // The FBX opens on a running frame. Its skin bind pose has planted feet
   // and an upright torso. Lower the arms to turn the bind pose into a stance.
   var hierarchy=footSkin.GetComponentsInChildren<Transform>(true);
   foreach(var bone in hierarchy)
    if(worldBind.TryGetValue(bone,out Matrix4x4 bind))
     bone.SetPositionAndRotation(bind.GetColumn(3),bind.rotation);
   footSpine=FindJoint("Spine2");
   footRightArm=FindLimb("Right","Arm","ForeArm","Hand");
   footLeftArm=FindLimb("Left","Arm","ForeArm","Hand");
   footRightLeg=FindLimb("Right","UpLeg","Leg","Foot");
   footLeftLeg=FindLimb("Left","UpLeg","Leg","Foot");
   RelaxFootArm("Left",-1);
   RelaxFootArm("Right",1);

   var pose=new List<FootBonePose>(worldBind.Count);
   foreach(var bone in hierarchy)
    if(worldBind.ContainsKey(bone))
     pose.Add(new FootBonePose{bone=bone,position=bone.localPosition,rotation=bone.localRotation});
   footStandingPose=pose.ToArray();

   Transform FindJoint(string name)=>System.Array.Find(hierarchy,b=>b.name=="mixamorig:"+name);
   FootLimb FindLimb(string side,string upper,string lower,string end)=>new FootLimb
   {upper=FindJoint(side+upper),lower=FindJoint(side+lower),end=FindJoint(side+end)};

   void RelaxFootArm(string name,int side)
   {
    Transform upper=System.Array.Find(hierarchy,b=>b.name=="mixamorig:"+name+"Arm");
    Transform elbow=System.Array.Find(hierarchy,b=>b.name=="mixamorig:"+name+"ForeArm");
    Transform hand=System.Array.Find(hierarchy,b=>b.name=="mixamorig:"+name+"Hand");
    if(upper==null||elbow==null||hand==null)return;
    upper.rotation=Quaternion.FromToRotation(elbow.position-upper.position,
     footActor.TransformDirection(new Vector3(side*.18f,-1,-.09f)))*upper.rotation;
    elbow.rotation=Quaternion.FromToRotation(hand.position-elbow.position,
     footActor.TransformDirection(new Vector3(-side*.08f,-1,.16f)))*elbow.rotation;
   }
  }

  Vector3 FootGrenadeHandPosition()
  {
   if(footRightArm.end!=null)
    return transform.InverseTransformPoint(footRightArm.end.position)+footActor.localRotation*new Vector3(0,.035f,.07f);
   return footActor.localPosition+footActor.localRotation*new Vector3(.35f,1.4f,.25f);
  }

  void ApplyFootThrowPose()
  {
   float age=FootThrowDuration-footThrowPose;
   float windUp=FootThrowRelease*.57f;
   float cock=Mathf.SmoothStep(0,1,age/windUp);
   float swing=Mathf.SmoothStep(0,1,(age-windUp)/(FootThrowRelease-windUp));
   float follow=Mathf.SmoothStep(0,1,(age-FootThrowRelease)/.14f);
   float weight=cock*(1-Mathf.SmoothStep(0,1,(age-.34f)/(FootThrowDuration-.34f)));
   if(footSpine!=null)
   {
    float twist=Mathf.Lerp(-26,24,swing)*weight;
    footSpine.rotation=Quaternion.AngleAxis(twist,footActor.up)*footSpine.rotation;
   }
   Vector3 upper=Vector3.Lerp(new Vector3(.55f,.74f,-.62f),new Vector3(.19f,.47f,1),swing);
   Vector3 lower=Vector3.Lerp(new Vector3(-.20f,1,-.67f),new Vector3(-.12f,.38f,1),swing);
   AimFootLimb(footRightArm,Vector3.Lerp(upper,new Vector3(.14f,-.62f,.85f),follow),
    Vector3.Lerp(lower,new Vector3(-.26f,-.67f,.8f),follow),weight);
   AimFootLimb(footLeftArm,new Vector3(-.58f,-.65f,.36f),new Vector3(.17f,-.75f,.68f),weight*.7f);
   footActionPoseApplied=true;
  }

  void ApplyFootFallPose()
  {
   float age=FootFallSeconds-footFallRemaining;
   float hit=Mathf.SmoothStep(0,1,age/(footFallLandingTime*.86f));
   float recover=Mathf.SmoothStep(0,1,(age-1.04f)/.46f);
   float down=hit*(1-recover);
   float brace=Mathf.Sin(Mathf.PI*Mathf.Clamp01(age/(footFallLandingTime+.28f)));
   Vector3 away=Quaternion.Inverse(footActor.localRotation)*footFallDirection;
   float side=Mathf.Abs(away.x)>.12f?Mathf.Sign(away.x):1;
   float legTuck=down*Mathf.Lerp(.45f,1,brace);
   AimFootLimb(footRightLeg,new Vector3(.2f,-.57f,.75f),new Vector3(.04f,-.8f,-.58f),legTuck*(side>0?1:.65f));
   AimFootLimb(footLeftLeg,new Vector3(-.24f,-.8f,.55f),new Vector3(-.12f,-.75f,-.55f),legTuck*(side<0?1:.65f));
   AimFootLimb(footRightArm,new Vector3(.58f,-.22f,.88f),new Vector3(-.22f,.56f,.82f),Mathf.Max(down*.75f,brace));
   AimFootLimb(footLeftArm,new Vector3(-.72f,-.16f,.6f),new Vector3(.32f,.58f,.8f),Mathf.Max(down*.78f,brace*.82f));
   if(footSpine!=null)
    footSpine.rotation=Quaternion.AngleAxis(19*down,footActor.right)*footSpine.rotation;

   // Tip around the hips and bring them down to the pavement as the torso lands.
   Quaternion tip=Quaternion.AngleAxis(86*down,Vector3.Cross(Vector3.up,away));
   Vector3 hips=Vector3.up*1.16f;
   footSkin.localPosition=footSkinPosition+Vector3.up*Mathf.Lerp(hips.y,.38f,down)-tip*hips;
   footSkin.localRotation=tip*footSkinRotation;
   footActionPoseApplied=true;
  }

  void AimFootLimb(FootLimb limb,Vector3 upperDirection,Vector3 lowerDirection,float weight)
  {
   if(limb.upper==null||limb.lower==null||limb.end==null)return;
   Aim(limb.upper,limb.lower,upperDirection);
   Aim(limb.lower,limb.end,lowerDirection);

   void Aim(Transform joint,Transform next,Vector3 direction)
   {
    Quaternion turn=Quaternion.FromToRotation(next.position-joint.position,footActor.TransformDirection(direction));
    joint.rotation=Quaternion.Slerp(Quaternion.identity,turn,weight)*joint.rotation;
   }
  }

  void AdvanceFootPose(float dt,bool moving)
  {
   if(footRunClip==null||footRunClip.length<=0)return;
   if(footFallRemaining>0)
   {
    if(footRunBlend>0)ApplyFootStandingPose(0);
    footRunBlend=0;
    return;
   }
   if(moving)footRunClock+=dt;
   float nextBlend=Mathf.MoveTowards(footRunBlend,moving?1:0,dt/(moving?.12f:.16f));
   if(nextBlend>0)
   {
    footRunClip.SampleAnimation(footSkin.gameObject,Mathf.Repeat(footRunClock,footRunClip.length));
    if(nextBlend<1)ApplyFootStandingPose(Mathf.SmoothStep(0,1,nextBlend));
   }
   else if(footRunBlend>0)ApplyFootStandingPose(0);
   footRunBlend=nextBlend;
  }

  void ApplyFootStandingPose(float runWeight)
  {
   if(footStandingPose==null)return;
   foreach(var joint in footStandingPose)
   {
    var bone=joint.bone;
    bone.SetLocalPositionAndRotation(Vector3.Lerp(joint.position,bone.localPosition,runWeight),
     Quaternion.Slerp(joint.rotation,bone.localRotation,runWeight));
   }
  }
 }
}
