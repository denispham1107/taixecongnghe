using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
 public partial class PhoVietGame
 {
  Transform policeOfficer,officerLeftLeg,officerRightLeg;
  float policeBlockedTime,officerStride;
  bool OfficerOnFoot=>policeOfficer!=null&&policeOfficer.gameObject.activeSelf;

  void HidePoliceOfficer()
  {
   policeBlockedTime=officerStride=0;
   if(policeOfficer!=null)policeOfficer.gameObject.SetActive(false);
  }

  void TickBlockedPoliceOfficer(Vector3 previous,Vector3 goal,float dt)
  {
   if(!IsChased||PoliceInRiver||PoliceCanReachRider())return;
   Vector3 oldRoadPosition=previous-Vector3.forward*(Speed*dt);
   float progress=Vector2.Distance(Flat(oldRoadPosition),Flat(goal))-Vector2.Distance(Flat(policeBike.localPosition),Flat(goal));
   policeBlockedTime=progress>dt*.5f?0:policeBlockedTime+dt;
   if(policeBlockedTime<1.65f)return;
   Vector3 start=policeBike.localPosition;
   for(int i=0;i<8;i++)
   {
    float angle=i*45f;
    Vector3 step=policeBike.localRotation*Quaternion.Euler(0,angle,0)*Vector3.forward*2.25f;
    Vector3 door=new Vector3(start.x+step.x,0,start.z+step.z);
    if(!FootGround(door,9.15f,out _,out _)||!FootTrafficClear(door,null,true))continue;
    EnsurePoliceOfficer();policeOfficer.localPosition=door;policeOfficer.gameObject.SetActive(true);
    ResetCrashPoliceNavigation();PoliceSpeed=0;return;
   }
  }

  void TickPoliceOfficer(float dt)
  {
   Vector3 start=policeOfficer.localPosition-Vector3.forward*(Speed*dt);
   policeBike.localPosition-=Vector3.forward*(Speed*dt);
   Vector3 goal=PoliceTargetPosition;
   float gap=Vector2.Distance(Flat(start),Flat(goal));
   if(gap>1.85f&&CrashPoliceWaypoint(start,dt,out Vector3 target,true))
   {
    var next=Vector3.MoveTowards(start,target,6.5f*dt);
    Vector3 delta=next-start;
    if(delta.sqrMagnitude>.000001f)policeOfficer.localRotation=Quaternion.LookRotation(delta,Vector3.up);
    start=next;officerStride+=dt*19;
   }
   policeOfficer.localPosition=start;
   officerLeftLeg.localRotation=Quaternion.Euler(Mathf.Sin(officerStride)*29,0,0);
   officerRightLeg.localRotation=Quaternion.Euler(-Mathf.Sin(officerStride)*29,0,0);
   PoliceDistance=Vector2.Distance(Flat(start),Flat(goal));
   if(PoliceDistance<=1.85f&&goal.y<1.25f)Arrest();
  }

  void EnsurePoliceOfficer()
  {
   if(policeOfficer!=null)return;
   Color uniform=Hex("CBB779"),belt=Hex("353837"),skin=Hex("B98363"),cap=Hex("8D5038");
   policeOfficer=Group("CSGT tiếp cận đi bộ",transform);
   Shape("Áo",policeOfficer,PrimitiveType.Capsule,new Vector3(0,1.20f,0),new Vector3(.40f,.27f,.24f),uniform);
   Box("Thắt lưng",policeOfficer,new Vector3(0,.96f,0),new Vector3(.31f,.07f,.22f),belt);
   Sphere("Đầu",policeOfficer,new Vector3(0,1.57f,0),new Vector3(.23f,.28f,.22f),skin);
   Shape("Mũ",policeOfficer,PrimitiveType.Cylinder,new Vector3(0,1.70f,0),new Vector3(.29f,.04f,.27f),cap);
   Box("Lưỡi trai",policeOfficer,new Vector3(0,1.67f,.11f),new Vector3(.24f,.025f,.14f),belt);
   officerLeftLeg=Group("Chân trái",policeOfficer,new Vector3(-.09f,.92f,0));
   officerRightLeg=Group("Chân phải",policeOfficer,new Vector3(.09f,.92f,0));
   foreach(var leg in new[]{officerLeftLeg,officerRightLeg})
   {
    Shape("Quần",leg,PrimitiveType.Capsule,new Vector3(0,-.37f,0),new Vector3(.14f,.40f,.15f),uniform);
    Box("Giày",leg,new Vector3(0,-.86f,.05f),new Vector3(.16f,.11f,.28f),belt);
   }
   for(int side=-1;side<=1;side+=2)
   {
    Shape("Tay áo",policeOfficer,PrimitiveType.Capsule,new Vector3(side*.22f,1.17f,.055f),new Vector3(.13f,.20f,.13f),uniform,new Vector3(-28,0,side*13));
    Sphere("Bàn tay",policeOfficer,new Vector3(side*.24f,1.01f,.14f),new Vector3(.095f,.13f,.095f),skin);
   }
  }
 }
}
