using UnityEngine;
namespace PhoViet
{
 // Một khối phố 80 m có thể thành khối cầu vượt sông (24/10/2026, pho_viet_river_bridge.py).
 // Bật cầu: hiện mô hình cầu/sông/thuyền, ẩn nhà–cây–vỉa hè nằm trên lòng sông và thay
 // các mesh gộp của khối (mặt đường, vạch, dây điện) bằng bản đã cắt bỏ đoạn z Near–Far (4,8–55,2 từ 25/10/2026; trước đó 16,8–42).
 public sealed class RiverBlock:MonoBehaviour
 {
  // Mặt nước giữa hai bờ kè (tọa độ khối), mép mặt cầu tính rơi, cao độ mặt nước.
  // Mặt nước thấp hơn mặt đường 5 m; đáy sông −9,5 m (vật chìm dần xuống tới đáy).
  public const float Near=4.8f,Far=55.2f,DeckHalf=3.35f,WaterY=-5f,BedY=-9.5f;
  public GameObject bridge;
  public GameObject[] streetOnly=new GameObject[0];
  public MeshFilter[] clipFilters=new MeshFilter[0];
  public Mesh[] fullMeshes=new Mesh[0],clippedMeshes=new Mesh[0];
  [SerializeField] bool isBridge;
  // Số khối phố liên tiếp kết thúc ở khối này (0 nếu là cầu); dùng cho luật "đi một lúc là gặp cầu".
  [System.NonSerialized] public int streetRun;
  public bool IsBridge=>isBridge;
  public void SetBridge(bool on)
  {
   if(bridge==null)on=false;
   isBridge=on;streetRun=on?0:streetRun;
   if(bridge!=null&&bridge.activeSelf!=on)bridge.SetActive(on);
   foreach(var item in streetOnly)if(item!=null&&item.activeSelf==on)item.SetActive(!on);
   for(int i=0;i<clipFilters.Length;i++)if(clipFilters[i]!=null)clipFilters[i].sharedMesh=on?clippedMeshes[i]:fullMeshes[i];
  }
 }
}
