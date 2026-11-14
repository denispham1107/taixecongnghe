using UnityEngine;
namespace PhoViet
{
 // Tàu neo nhấp nhô tại chỗ, tàu chạy quay đầu theo vòng dọc bờ.
 public sealed class RiverBoat:MonoBehaviour
 {
  public Vector3 restPosition;
  public float restYaw,phase;
  const float PatrolTurnWidth=1.4f,PatrolRadiansPerSecond=.22f;
  bool patrolling;
  Vector3 patrolAxis,patrolSide;
  float patrolAngle;
  void OnEnable(){if(phase==0)phase=Random.value*6.28f;}
  public void SetPatrol(Vector3 red,Vector3 green)
  {
   restPosition=(red+green)*.5f;
   patrolAxis=(green-red)*.5f;
   patrolSide=Vector3.Cross(Vector3.up,patrolAxis.normalized)*PatrolTurnWidth;
   patrolAngle=0;patrolling=true;
   UpdatePose(Time.time+phase);
  }
  void Update()
  {
   if(patrolling)patrolAngle=Mathf.Repeat(patrolAngle+Time.deltaTime*PatrolRadiansPerSecond,2*Mathf.PI);
   UpdatePose(Time.time+phase);
  }
  void UpdatePose(float t)
  {
   Vector3 p=restPosition;
   p.y+=Mathf.Sin(t*1.25f)*.06f+Mathf.Sin(t*2.1f)*.02f;
   float yaw=restYaw+Mathf.Sin(t*.37f)*1.5f;
   if(patrolling)
   {
    float sin=Mathf.Sin(patrolAngle),cos=Mathf.Cos(patrolAngle);
    p+=-cos*patrolAxis+sin*patrolSide;
    // The imported fishing boat's bow points along local +X.
    Vector3 heading=sin*patrolAxis+cos*patrolSide;
    yaw=Mathf.Atan2(-heading.z,heading.x)*Mathf.Rad2Deg;
   }
   transform.SetLocalPositionAndRotation(p,Quaternion.Euler(Mathf.Sin(t*.9f)*1.2f,yaw,Mathf.Sin(t*1.1f+1)*2.2f));
  }
 }
}
