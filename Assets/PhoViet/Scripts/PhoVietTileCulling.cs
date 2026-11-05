using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 // 21.136: a road tile holds ~180 renderers and 47 LOD groups. Tiles that cannot appear inside the camera's
 // far distance are switched off, so they cost no bounds update or culling while the world scrolls every frame.
 public partial class PhoVietGame
 {
  public partial class TrafficVehicle
  {
   // 21.136: looked up once; this ran for every vehicle on every tick, and a failed lookup is the slow case.
   [System.NonSerialized] bool urbanRiderKnown,urbanRider;
   public bool IsUrbanRider{get{if(!urbanRiderKnown&&model!=null){urbanRider=model.GetComponent<UrbanRiderMotion>()!=null;urbanRiderKnown=true;}return urbanRider;}}
  }
  // 21.136: everything here used to be built on first use, mid-ride: 9 painted textures and 3.7 MB of garbage on the
  // first rear warning, a skinned dinosaur, fire pools. Doing it during loading moves those hitches off the road.
  void PrewarmRideResources()
  {
   if(!Application.isPlaying)return;
   CreateRearStyleTextures();if(rearWarningArrow==null)CreateRearWarningArrow();
   EnsureTankFx();EnsureThunderstorm();EnsureBoostSmoke();EnsurePlayerMissileHudIcon();
   for(int i=0;i<2;i++)shellPool.Push(PrewarmedShell());
   if(dinosaursEnabled&&dinosaurPrefab!=null)
    while(dinosaurs.Count<2)
    {
     var go=Instantiate(dinosaurPrefab,transform);go.SetActive(false);
     dinosaurs.Add(new Dinosaur{root=go.transform,visual=go.GetComponent<DinosaurVisual>()});
    }
   PrewarmContactFire();
   // Compile recorded variants behind the loading screen, not when each effect first appears (21.136).
   if(Application.platform==RuntimePlatform.WebGLPlayer)
   {
    var warmup=Resources.Load<ShaderVariantCollection>("PV_ShaderWarmup");
    if(warmup!=null&&!warmup.isWarmedUp)
    {
     var clock=System.Diagnostics.Stopwatch.StartNew();warmup.WarmUp();
     Debug.Log("[PhoViet] Shader warmup "+warmup.variantCount+" variants, "+clock.ElapsedMilliseconds+" ms");
    }
   }
  }
  Transform PrewarmedShell(){var shell=NewShellBody();shell.gameObject.SetActive(false);return shell;}
  const float TileCullMargin=14,SideRowReach=160,JunctionOpenDistance=66;
  [System.NonSerialized] public bool tileCullingEnabled=true;
  public int ActiveRoadTiles {get;private set;}
  struct TileBox {public Vector3 centre,extents;}
  readonly Dictionary<Transform,TileBox> tileBoxes=new Dictionary<Transform,TileBox>();
  int tileCullFrame;bool tileCullPending=true;
  TileBox BoxOfTile(Transform tile)
  {
   if(tileBoxes.TryGetValue(tile,out var box))return box;
   bool any=false;var bounds=new Bounds();var toLocal=tile.worldToLocalMatrix;
   // Includes street and bridge variants, so a tile never pops when its river state changes.
   foreach(var r in tile.GetComponentsInChildren<Renderer>(true))
   {
    var b=r.bounds;
    for(int i=0;i<8;i++)
    {
     var p=toLocal.MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
     if(!any){bounds=new Bounds(p,Vector3.zero);any=true;}else bounds.Encapsulate(p);
    }
   }
   if(!any)bounds=new Bounds(new Vector3(0,0,40),new Vector3(60,30,80));
   box=new TileBox{centre=bounds.center,extents=bounds.extents};tileBoxes[tile]=box;return box;
  }
  bool TileWanted(Transform tile,TileBox box,Vector3 eye,Vector3 forward,float reach,bool sideLimited)
  {
   if(sideLimited&&tile.localPosition.z>=SideRowReach)return false;
   Vector3 p=tile.InverseTransformPoint(eye),c=box.centre,e=box.extents;
   float dx=Mathf.Max(0,Mathf.Abs(p.x-c.x)-e.x),dy=Mathf.Max(0,Mathf.Abs(p.y-c.y)-e.y),dz=Mathf.Max(0,Mathf.Abs(p.z-c.z)-e.z);
   if(dx*dx+dy*dy+dz*dz>reach*reach)return false;
   // Wholly behind the camera plane.
   Vector3 centre=tile.TransformPoint(c);
   float span=Mathf.Abs(Vector3.Dot(tile.right,forward))*e.x+Mathf.Abs(Vector3.Dot(tile.up,forward))*e.y+Mathf.Abs(Vector3.Dot(tile.forward,forward))*e.z;
   return Vector3.Dot(centre-eye,forward)+span>=-TileCullMargin;
  }
  void UpdateTileCulling(bool immediate=false)
  {
   if(!RoutesReady||rideCamera==null)return;
   immediate|=tileCullPending;tileCullPending=false;
   if(!immediate&&!IsTurning&&(++tileCullFrame&3)!=0)return;
   var camera=rideCamera.transform;Vector3 eye=camera.position,forward=camera.forward;
   float reach=rideCamera.farClipPlane+TileCullMargin;bool junctionOpen=IsTurning||NextJunctionDistance<JunctionOpenDistance;
   int on=immediate?int.MaxValue:1,off=immediate?int.MaxValue:2,active=0;
   for(int r=0;r<roadRows.Count;r++)
   {
    var row=roadRows[r];
    if(!row.root.gameObject.activeSelf)continue;
    bool sideLimited=r!=mainRow&&!junctionOpen;
    for(int i=0;i<row.tiles.Count;i++)
    {
     var tile=row.tiles[i];
     bool want=!tileCullingEnabled||TileWanted(tile,BoxOfTile(tile),eye,forward,reach,sideLimited);
     if(tile.gameObject.activeSelf!=want)
     {
      if(want&&on>0){tile.gameObject.SetActive(true);on--;}
      else if(!want&&off>0){tile.gameObject.SetActive(false);off--;}
     }
     if(tile.gameObject.activeSelf)active++;
    }
   }
   ActiveRoadTiles=active;
  }
 }
}
