using UnityEngine;
using UnityEngine.Rendering;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  Transform markerArrow,markerGlow,markerPlate,markerGroundRing;TextMesh markerLabel;Material markerGlowMaterial,markerLabelMaterial;
  static readonly Color WaypointGreen=new Color(.08f,1f,.56f,1);
  public bool JobMarkerVisible=>jobMarker!=null&&jobMarker.gameObject.activeSelf;
  public string JobMarkerLabel=>markerLabel==null?"":markerLabel.text;
  Material WaypointMaterial(Color color,bool solid=false)
  {
   var material=new Material(Resources.Load<Shader>(solid?"PhoVietWaypointSolid":"PhoVietWaypoint"));material.color=color;return material;
  }
  Transform MarkerPart(string name,Transform parent,Mesh mesh,Color color,bool solid,Vector3 position,float scale=1)
  {
   var part=StreetArt.Group(name,parent,position);part.localScale=Vector3.one*scale;
   part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=part.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=WaypointMaterial(color,solid);
   renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return part;
  }
  void CreateJobMarker()
  {
   jobMarker=StreetArt.Group("Mũi tên xanh 3D • Điểm hẹn",transform);
   markerArrow=StreetArt.Group("Mũi tên 3D",jobMarker);
   markerGlow=MarkerPart("Quầng sáng xanh",markerArrow,WaypointMesh.FlatArrow(),new Color(.1f,1f,.54f,.18f),false,new Vector3(0,.85f,.38f),1.4f);
   markerGlowMaterial=markerGlow.GetComponent<Renderer>().sharedMaterial;
   var solid=WaypointMesh.BeveledArrow();
   MarkerPart("Viền tương phản đậm",markerArrow,solid,new Color(.02f,.18f,.13f),true,new Vector3(.035f,.82f,.18f),1.24f);
   MarkerPart("Viền sáng",markerArrow,solid,new Color(.9f,1f,.98f),true,new Vector3(0,.85f,.07f),1.15f);
   MarkerPart("Mũi tên xanh lá",markerArrow,solid,WaypointGreen,true,new Vector3(0,.85f,-.035f));
   var shine=new Mesh{name="Waypoint polished face highlight"};shine.vertices=new[]{new Vector3(-.17f,.64f,0),new Vector3(-.09f,.64f,0),new Vector3(-.09f,.04f,0),new Vector3(-.17f,-.01f,0)};shine.triangles=new[]{0,1,2,0,2,3};shine.RecalculateBounds();
   MarkerPart("Vệt sáng mặt ngọc",markerArrow,shine,new Color(.65f,1f,.85f,.6f),false,new Vector3(0,.85f,-.30f)).GetComponent<Renderer>().sharedMaterial.renderQueue=4002;
   markerGroundRing=MarkerPart("Vòng sáng điểm hẹn",jobMarker,WaypointMesh.GroundRing(),new Color(.1f,1f,.58f,.65f),false,Vector3.zero);
   var plate=new Mesh{name="Waypoint label plate"};plate.vertices=new[]{new Vector3(-1.35f,-.23f,0),new Vector3(1.35f,-.23f,0),new Vector3(1.35f,.23f,0),new Vector3(-1.35f,.23f,0)};plate.triangles=new[]{0,1,2,0,2,3};plate.RecalculateBounds();
   markerPlate=MarkerPart("Nền nhãn điểm hẹn",jobMarker,plate,new Color(.02f,.14f,.12f,.9f),false,new Vector3(0,2.25f,-.38f));markerPlate.GetComponent<Renderer>().sharedMaterial.renderQueue=4002;
   StreetArt.Text(jobMarker,"Nhãn điểm hẹn","",new Vector3(0,2.25f,-.40f),.32f,Color.white);
   markerLabel=jobMarker.GetComponentInChildren<TextMesh>();markerLabelMaterial=WaypointMaterial(Color.white);markerLabelMaterial.renderQueue=4003;markerLabel.GetComponent<Renderer>().sharedMaterial=markerLabelMaterial;
  }
  void UpdateJobMarker()
  {
   if(!jobsEnabled||ActiveJob==null||rideCamera==null||State==RideState.Ready||State==RideState.Jailed||State==RideState.GameOver||State==RideState.Arrested)
   {if(jobMarker!=null)jobMarker.gameObject.SetActive(false);return;}
   var target=CurrentWorkTarget;
   Vector2 delta=target.Point+target.CurbDirection*7.6f-NavigationPosition;
   var local=new Vector3(Vector2.Dot(delta,CityNavigation.Directions[(navHeading+1)%4]),.45f,Vector2.Dot(delta,CityNavigation.Directions[navHeading]));
   Vector3 world=transform.TransformPoint(local),view=rideCamera.WorldToViewportPoint(world);
   bool visible=view.z>rideCamera.nearClipPlane&&view.z<150&&view.x>=0&&view.x<=1&&view.y>=0&&view.y<=1;
   if(!visible){if(jobMarker!=null)jobMarker.gameObject.SetActive(false);return;}
   if(jobMarker==null)CreateJobMarker();jobMarker.gameObject.SetActive(true);
   jobMarker.localPosition=local;jobMarker.rotation=rideCamera.transform.rotation;
   float size=Mathf.Clamp(view.z*.032f,.75f,3.5f);jobMarker.localScale=Vector3.one*size;
   float phase=Time.unscaledTime,pulse=.5f+.5f*Mathf.Sin(phase*2.4f),bob=.10f+.10f*pulse;
   markerArrow.localPosition=Vector3.up*bob;markerArrow.localRotation=Quaternion.Euler(-7,-17+5*Mathf.Sin(phase*.7f),0);
   markerGlow.localScale=Vector3.one*(1.38f+pulse*.1f);markerGlowMaterial.color=new Color(.1f,1f,.54f,.14f+pulse*.08f);
   markerGroundRing.position=transform.TransformPoint(new Vector3(local.x,.26f,local.z));markerGroundRing.rotation=transform.rotation;markerGroundRing.localScale=Vector3.one*(.9f+pulse*.14f);
   markerPlate.localPosition=new Vector3(0,2.25f+bob,-.38f);markerLabel.transform.localPosition=new Vector3(0,2.25f+bob,-.4f);
   markerLabel.text=JobPhase==WorkPhase.Pickup||JobPhase==WorkPhase.Boarding?"ĐÓN KHÁCH":"TRẢ KHÁCH";
   if(StreetArt.Font!=null)markerLabelMaterial.mainTexture=StreetArt.Font.material.mainTexture;
  }
 }
}
