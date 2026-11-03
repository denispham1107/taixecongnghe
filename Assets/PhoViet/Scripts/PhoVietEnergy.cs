using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public const int MaximumEnergy=150,JumpEnergyCost=5,PickupEnergy=15,FlightEnergyCost=10;
  public const float FlightEnergyInterval=.5f,BarrelRollDuration=.9f;
  public int Energy {get;private set;}=MaximumEnergy;
  public int EnergyPickupsCollected {get;private set;}
  public bool RiderFlying=>energyHover;
  public bool RiderRolling=>barrelRolling;
  public float RiderBarrelRoll=>barrelRolling?360*SmoothRoll(barrelClock/BarrelRollDuration):0;
  public int EnergyPickupPoolCount=>energyTiles.Count*2;
  bool energyHover,barrelRolling;float flightEnergyClock,barrelClock,energyClock;
  Texture2D energyHudIcon;GameObject energyPickupPrefab;
  System.Random energyRng;
  sealed class EnergyTile
  {
   public Transform tile;public Transform[] icons=new Transform[2];
   public readonly int[] lanes=new int[2];public readonly bool[] collected=new bool[2];
  }
  readonly List<EnergyTile> energyTiles=new List<EnergyTile>();
  readonly Dictionary<Transform,EnergyTile> energyByTile=new Dictionary<Transform,EnergyTile>();
  static float SmoothRoll(float t){t=Mathf.Clamp01(t);return t*t*t*(10+t*(-15+6*t));}
  bool SpendEnergy(int cost)
  {if(Energy<cost)return false;Energy-=cost;return true;}
  void ResetEnergy()
  {
   Energy=MaximumEnergy;EnergyPickupsCollected=0;energyClock=0;CancelEnergyFlight();
   energyRng=new System.Random(RunSeed^0x274ea231);
   if(energyPickupPrefab==null)energyPickupPrefab=Resources.Load<GameObject>("Energy/PV_EnergyPickup");
   if(energyHudIcon==null)energyHudIcon=Resources.Load<Texture2D>("Energy/PV_EnergyHaloHud");
   if(energyPickupPrefab==null)return;
   foreach(var row in roadRows)foreach(var tile in row.tiles)
   {
    if(!energyByTile.TryGetValue(tile,out var entry))
    {
     entry=new EnergyTile{tile=tile};energyByTile.Add(tile,entry);energyTiles.Add(entry);
     for(int i=0;i<2;i++){entry.icons[i]=Instantiate(energyPickupPrefab,tile,false).transform;entry.icons[i].name="Năng lượng C • "+i;}
    }
    RefreshEnergyTile(tile);
   }
  }
  void RefreshEnergyTile(Transform tile)
  {
   if(energyRng==null||!energyByTile.TryGetValue(tile,out var e))return;
   bool bridge=RiverOf(tile)!=null&&RiverOf(tile).IsBridge;
   int a=bridge?1+energyRng.Next(2):energyRng.Next(4);
   int b=bridge?3-a:(a+energyRng.Next(1,4))%4;
   for(int i=0;i<2;i++)
   {
    e.lanes[i]=i==0?a:b;e.collected[i]=false;
    e.icons[i].localPosition=new Vector3(LaneX[e.lanes[i]],1.15f,(i==0?13:36)+(float)energyRng.NextDouble()*13);
    e.icons[i].localRotation=Quaternion.identity;e.icons[i].gameObject.SetActive(true);
   }
  }
  bool ToggleEnergyFlight()
  {
   if(energyHover){energyHover=false;jumpVelocity=0;return true;}
   if(!jumpFlying||jumpHeight<=.08f||Energy<=0)return false;
   // Carry an unpaid fraction until landing; rapid toggles cannot give free flight.
   CancelAssistedKick(false);energyHover=true;jumpVelocity=0;return true;
  }
  void CancelEnergyFlight(){energyHover=false;barrelRolling=false;flightEnergyClock=barrelClock=0;}
  public bool TryBarrelRoll()
  {
   if(State!=RideState.Riding||ServiceStopped||!energyHover||barrelRolling)return false;
   barrelRolling=true;barrelClock=0;return true;
  }
  // Returns the part of this step left for gravity after the last paid half-second.
  float TickEnergyFlight(float dt)
  {
   if(barrelRolling){barrelClock=Mathf.Min(BarrelRollDuration,barrelClock+dt);if(barrelClock>=BarrelRollDuration)barrelRolling=false;}
   if(!energyHover)return dt;
   float left=dt;
   while(energyHover&&left>0)
   {
    float step=Mathf.Min(left,FlightEnergyInterval-flightEnergyClock);
    flightEnergyClock+=step;left-=step;
    if(flightEnergyClock>=FlightEnergyInterval-1e-6f)
    {
     flightEnergyClock=0;Energy=Mathf.Max(0,Energy-FlightEnergyCost);
     if(Energy==0){energyHover=false;jumpVelocity=0;}
    }
   }
   return energyHover?0:left;
  }
  void TickEnergyPickups(float dt,Vector3 previousRider)
  {
   energyClock+=dt;if(energyPickupPrefab==null)return;
   Quaternion inverse=Quaternion.Inverse(Quaternion.Euler(0,rider.localEulerAngles.y,0));
   Vector3 half=new Vector3(PlayerHalfWidth+.43f,PlayerHeight*.5f+.52f,PlayerHalfLength+.25f);
   foreach(var e in energyTiles)
   {
    if(!e.tile.gameObject.activeInHierarchy)continue;
    for(int i=0;i<2;i++)
    {
     if(e.collected[i])continue;var icon=e.icons[i];
     var local=icon.localPosition;local.y=1.15f+Mathf.Sin(energyClock*2.5f+i*2)*.07f;icon.localPosition=local;
     Vector3 world=icon.position,view=rideCamera.transform.position-world;view.y=0;
     if(view.sqrMagnitude>.01f)icon.rotation=Quaternion.LookRotation(view,Vector3.up);
     Vector3 at=transform.InverseTransformPoint(world);
     // Tiles/branches scrolled by the same delta. Test relative swept motion,
     // including fast W travel and the car's width during a junction curve.
     Vector3 from=inverse*(previousRider+Vector3.up*(PlayerHeight*.5f)-at-Vector3.forward*(Speed*dt));
     Vector3 to=inverse*(rider.localPosition+Vector3.up*(PlayerHeight*.5f)-at);
     if(!EnergySweptContact(from,to,half))continue;
     e.collected[i]=true;icon.gameObject.SetActive(false);Energy=Mathf.Min(MaximumEnergy,Energy+PickupEnergy);EnergyPickupsCollected++;
    }
   }
  }
  static bool EnergySweptContact(Vector3 from,Vector3 to,Vector3 half)
  {
   float enter=0,exit=1;
   for(int axis=0;axis<3;axis++)
   {
    float d=to[axis]-from[axis];if(Mathf.Abs(d)<1e-7f){if(Mathf.Abs(from[axis])>half[axis])return false;continue;}
    float a=(-half[axis]-from[axis])/d,b=(half[axis]-from[axis])/d;
    enter=Mathf.Max(enter,Mathf.Min(a,b));exit=Mathf.Min(exit,Mathf.Max(a,b));if(enter>exit)return false;
   }
   return true;
  }
  void DrawEnergyBar(Rect rect)
  {
   if(energyHudIcon==null)energyHudIcon=Resources.Load<Texture2D>("Energy/PV_EnergyHaloHud");
   if(energyHudIcon!=null)GUI.DrawTexture(new Rect(rect.x,rect.y-2,24,24),energyHudIcon,ScaleMode.ScaleToFit,true);
   Color color=Energy<20?StreetArt.Gold:new Color(.35f,.86f,1,1);
   Txt(Energy+"/"+MaximumEnergy,rect.x+29,rect.y,rect.width-29,18,12,StreetArt.Cream,TextAnchor.UpperRight);
   Rect bar=new Rect(rect.x+29,rect.y+20,rect.width-29,7);
   Panel(bar,new Color(.01f,.08f,.14f,.72f),3);
   if(Energy>0)Panel(new Rect(bar.x,bar.y,bar.width*Energy/MaximumEnergy,bar.height),color,3);
  }
 }
}
