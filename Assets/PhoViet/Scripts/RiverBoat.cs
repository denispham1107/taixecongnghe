using UnityEngine;
namespace PhoViet
{
 // Thuyền neo dưới cầu nhấp nhô nhẹ theo sóng (chỉ chạy khi khối cầu đang bật).
 public sealed class RiverBoat:MonoBehaviour
 {
  public Vector3 restPosition;
  public float restYaw,phase;
  void OnEnable(){if(phase==0)phase=Random.value*6.28f;}
  void Update()
  {
   float t=Time.time+phase;
   transform.localPosition=restPosition+new Vector3(0,Mathf.Sin(t*1.25f)*.06f+Mathf.Sin(t*2.1f)*.02f,0);
   transform.localRotation=Quaternion.Euler(Mathf.Sin(t*.9f)*1.2f,restYaw+Mathf.Sin(t*.37f)*1.5f,Mathf.Sin(t*1.1f+1)*2.2f);
  }
 }
}
