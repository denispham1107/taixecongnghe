using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  sealed class MissilePickupTile
  {public Transform tile;public readonly Transform[] icons=new Transform[2];public readonly int[] lanes=new int[2];public readonly bool[] collected=new bool[2];}
  readonly List<MissilePickupTile> missilePickupTiles=new List<MissilePickupTile>();
  readonly Dictionary<Transform,MissilePickupTile> missilePickupByTile=new Dictionary<Transform,MissilePickupTile>();
  GameObject missilePickupPrefab;System.Random missilePickupRng;float missilePickupClock;
  public int MissilePickupsCollected{get;private set;}
  public int MissilePickupPoolCount=>missilePickupTiles.Count*2;
  void ResetMissilePickups()
  {
   MissilePickupsCollected=0;missilePickupClock=0;missilePickupRng=new System.Random(RunSeed^0x1f78943);
   if(!Application.isPlaying)return;
   if(missilePickupPrefab==null)missilePickupPrefab=Resources.Load<GameObject>("PlayerMissile/PV_MissilePickup");
   if(missilePickupPrefab==null)return;
   foreach(var row in roadRows)foreach(var tile in row.tiles)
   {
    if(!missilePickupByTile.TryGetValue(tile,out var entry))
    {
     entry=new MissilePickupTile{tile=tile};missilePickupByTile.Add(tile,entry);missilePickupTiles.Add(entry);
     for(int i=0;i<2;i++){entry.icons[i]=Instantiate(missilePickupPrefab,tile,false).transform;entry.icons[i].name="Nạp tên lửa +1 • "+i;}
    }
    RefreshMissilePickupTile(tile);
   }
  }
  void RefreshMissilePickupTile(Transform tile)
  {
   if(missilePickupRng==null||!missilePickupByTile.TryGetValue(tile,out var e))return;
   bool bridge=RiverOf(tile)!=null&&RiverOf(tile).IsBridge;
   int a=bridge?1+missilePickupRng.Next(2):missilePickupRng.Next(4),b=bridge?3-a:(a+missilePickupRng.Next(1,4))%4;
   for(int i=0;i<2;i++)
   {
    e.lanes[i]=i==0?a:b;e.collected[i]=false;
    float z=(bridge?(i==0?20:43):(i==0?27:51))+(float)missilePickupRng.NextDouble()*7;
    // Keep the blue energy coin and the rocket individually legible if their
    // random lanes happen to match, without changing the energy RNG stream.
    if(energyByTile.TryGetValue(tile,out var energy))for(int j=0;j<GroundPickupsPerTile;j++)if(energy.lanes[j]==e.lanes[i]&&Mathf.Abs(energy.icons[j].localPosition.z-z)<3)z+=4;
    e.icons[i].localPosition=new Vector3(LaneX[e.lanes[i]],1.15f,z);e.icons[i].localRotation=Quaternion.identity;e.icons[i].gameObject.SetActive(true);
   }
  }
  void TickMissilePickups(float dt,Vector3 previousRider)
  {
   if(missilePickupPrefab==null||State!=RideState.Riding||PlayerMissiles>=MaximumPlayerMissiles)return;
   Quaternion inverse=Quaternion.Inverse(Quaternion.Euler(0,rider.localEulerAngles.y,0));
   Vector3 half=new Vector3(PlayerHalfWidth+.38f,PlayerHeight*.5f+.5f,PlayerHalfLength+.3f);
   foreach(var e in missilePickupTiles)
   {
    if(!e.tile.gameObject.activeInHierarchy)continue;
    for(int i=0;i<2;i++)
    {
     if(e.collected[i]||PlayerMissiles>=MaximumPlayerMissiles)continue;
     var icon=e.icons[i];
     Vector3 at=transform.InverseTransformPoint(icon.position),center=Vector3.up*(PlayerHeight*.5f);
     Vector3 from=inverse*(previousRider+center-at-Vector3.forward*(RoadScrollSpeed*dt)),to=inverse*(rider.localPosition+center-at);
     if(!EnergySweptContact(from,to,half))continue;
     e.collected[i]=true;icon.gameObject.SetActive(false);PlayerMissiles=Mathf.Min(MaximumPlayerMissiles,PlayerMissiles+1);MissilePickupsCollected++;
     notice="NHẶT TÊN LỬA +1 · "+PlayerMissiles+"/"+MaximumPlayerMissiles;noticeTimer=1.2f;
    }
   }
  }
  void PoseMissilePickups(float dt)
  {
   if(rideCamera==null||!(State==RideState.Riding||PlayerOnFoot&&!InsideShop))return;
   missilePickupClock+=dt;
   foreach(var e in missilePickupTiles)
   {
    if(!e.tile.gameObject.activeInHierarchy)continue;
    for(int i=0;i<2;i++)
    {
     if(e.collected[i])continue;
     var icon=e.icons[i];
     var p=icon.localPosition;p.y=1.15f+Mathf.Sin(missilePickupClock*2.6f+i*2)*.08f;icon.localPosition=p;
     Vector3 view=rideCamera.transform.position-icon.position;
     if(!PlayerOnFoot)view.y=0;
     if(view.sqrMagnitude>.01f)icon.rotation=Quaternion.LookRotation(view,Vector3.up);
    }
   }
  }
 }
}
