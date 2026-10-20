using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  public enum WorkPhase {None,Pickup,Boarding,Delivery,Alighting}
  [Serializable] public sealed class RideJob
  {
   public RideAddress pickup,dropoff;public int reward;public float pickupMetres,tripMetres;public bool female;
  }
  public bool jobsEnabled=true;
  public Transform malePassenger,femalePassenger;
  public AudioClip navigationLeft,navigationRight,navigationStraight;
  public int Coins {get;private set;}
  public WorkPhase JobPhase {get;private set;}
  public bool JobOffersOpen {get;private set;}
  public readonly List<RideJob> JobOffers=new List<RideJob>();
  public RideJob ActiveJob {get;private set;}
  public CityRoute ActiveRoute {get;private set;}
  public int Reroutes {get;private set;}
  public float RearDepartureProtection=>rearDepartureProtection;
  public Vector2Int NavigationNextNode=>navNext;
  public int NavigationHeading=>navHeading;
  public Vector2 NavigationPosition=>(Vector2)navNext*80-(Vector2)CityNavigation.Directions[navHeading]*NextJunctionDistance;
  // Preview the committed exit immediately, without advancing the authoritative city
  // node until the road geometry has been rebased at the end of the curve.
  CityRoute turnNavigationRoute;
  public CityRoute NavigationRoute=>IsTurning&&turnNavigationRoute!=null?turnNavigationRoute:ActiveRoute;
  public Vector2 NavigationMapPosition=>IsTurning?NavigationPosition+(Vector2)CityNavigation.Directions[navHeading]*rider.localPosition.z+(Vector2)CityNavigation.Directions[(navHeading+1)%4]*rider.localPosition.x:NavigationPosition;
  public Vector2 NavigationMapDirection=>((Vector2)CityNavigation.Directions[navHeading])*Mathf.Cos(TurnYaw*Mathf.Deg2Rad)+(Vector2)CityNavigation.Directions[(navHeading+1)%4]*Mathf.Sin(TurnYaw*Mathf.Deg2Rad);
  float TurnExitAdvance=>Vector2.Dot(NavigationMapPosition-(Vector2)navNext*80,CityNavigation.Directions[(navHeading+turnSide+4)%4]);
  public float NavigationRemainingDistance=>NavigationRoute==null?0:Mathf.Max(0,IsTurning&&turnNavigationRoute!=null?turnNavigationRoute.distance-Mathf.Max(0,TurnExitAdvance):ActiveRoute.distance-Mathf.Max(0,Distance-routePlanDistance));
  void PreviewJunctionNavigation()
  {
   if(!jobsEnabled||ActiveJob==null)return;
   int heading=(navHeading+turnSide+4)%4;
   turnNavigationRoute=city.Plan((Vector2)navNext*80,navNext+CityNavigation.Directions[heading],heading,80,CurrentWorkTarget);
  }
  public bool ServiceStopped=>jobsEnabled&&(JobPhase==WorkPhase.Boarding||JobPhase==WorkPhase.Alighting);
  public bool ServiceRearProtected=>ServiceStopped||rearDepartureProtection>0;
  const string CoinsKey="PhoViet.Coins.v1";
  static readonly float[] ShopOffsets={14,22.4f,30.8f,39.2f,47.6f,56,64.4f};
  CityNavigation city;System.Random jobRandom;
  Vector2Int navNext;int navHeading;float offersDelay,serviceTime,rearDepartureProtection,rerouteNotice;
  Transform customer,jobMarker;AudioSource navigationAudio;
  string spokenKey="";Vector3 boardingStart;bool streetTargetMissed,pendingReroute;float routePlanDistance;
  int navigationCueStage;
  public static string FormatCoins(int value)=>Math.Max(0,value).ToString("N0",CultureInfo.InvariantCulture);
  void ResetCityJobs()
  {
   Coins=Math.Max(0,PlayerPrefs.GetInt(CoinsKey,0));navHeading=0;navNext=new Vector2Int(0,1);
   city=new CityNavigation(RouteNames,RunSeed^17391);jobRandom=new System.Random(RunSeed^93517);
   ActiveJob=null;ActiveRoute=null;turnNavigationRoute=null;JobPhase=WorkPhase.None;JobOffersOpen=false;JobOffers.Clear();Reroutes=0;
   offersDelay=2;serviceTime=0;rearDepartureProtection=0;jobTrafficYieldRemaining=0;rerouteNotice=0;spokenKey="";navigationCueStage=0;streetTargetMissed=false;pendingReroute=false;
   if(malePassenger!=null)malePassenger.gameObject.SetActive(false);if(femalePassenger!=null)femalePassenger.gameObject.SetActive(false);
   customer=null;if(jobMarker!=null)jobMarker.gameObject.SetActive(false);navigationAudio?.Stop();
  }
  string GridJunctionStreet()=>city.Street(navNext,(navHeading+1)%4);
  void PassCityJunction(){pendingReroute=ActiveRoute!=null&&!ActiveRoute.direct&&ActiveRoute.nextTurn!=0;navNext+=CityNavigation.Directions[navHeading];}
  void TurnCityJunction(int side){pendingReroute=ActiveRoute!=null&&(ActiveRoute.direct||ActiveRoute.nextTurn!=side);navHeading=(navHeading+side+4)%4;navNext+=CityNavigation.Directions[navHeading];}
  void CityJunctionPrepared(){if(ActiveJob!=null)ReplanJob(pendingReroute);pendingReroute=false;}
  RideAddress CurrentWorkTarget=>ActiveJob==null?null:JobPhase==WorkPhase.Pickup||JobPhase==WorkPhase.Boarding?ActiveJob.pickup:ActiveJob.dropoff;
  public void ShowJobOffers()
  {
   if(!jobsEnabled||ActiveJob!=null||State!=RideState.Riding||IsTurning)return;
   JobOffers.Clear();Vector2Int around=new Vector2Int(Mathf.FloorToInt(NavigationPosition.x/80),Mathf.FloorToInt(NavigationPosition.y/80));
   for(int i=0;i<3;i++)
   {
    RideAddress pickup,drop;CityRoute pickupRoute,tripRoute;int attempts=0;
    do
    {
     int range=i==0?1:i==1?3:5;
     pickup=new RideAddress{node=around+new Vector2Int(jobRandom.Next(-range,range+1),jobRandom.Next(-range,range+1)),axis=jobRandom.Next(2),offset=ShopOffsets[jobRandom.Next(ShopOffsets.Length)],curb=jobRandom.Next(2)==0?-1:1};
     if(i==0&&attempts<2)
     {
      // At least one practical short pickup along the current approach when possible.
      var direction=CityNavigation.Directions[navHeading];var before=navNext-direction;
      pickup.axis=navHeading%2;pickup.node=navHeading<2?before:navNext;
      float desired=navHeading<2?80-NextJunctionDistance+24:NextJunctionDistance-24;float best=float.PositiveInfinity;
      foreach(float offset in ShopOffsets)if(Mathf.Abs(offset-desired)<best){best=Mathf.Abs(offset-desired);pickup.offset=offset;}
     }
     drop=new RideAddress{node=pickup.node+new Vector2Int(jobRandom.Next(-i-2,i+3),jobRandom.Next(-i-2,i+3)),axis=jobRandom.Next(2),offset=ShopOffsets[jobRandom.Next(ShopOffsets.Length)],curb=jobRandom.Next(2)==0?-1:1};
     pickupRoute=city.Plan(NavigationPosition,navNext,navHeading,NextJunctionDistance,pickup);
     int arrival=FindArrivalHeading(pickupRoute,pickup);
     Vector2Int next=arrival==pickup.axis?pickup.node+CityNavigation.Directions[pickup.axis]:pickup.node;
     float remaining=arrival==pickup.axis?80-pickup.offset:pickup.offset;
     tripRoute=city.Plan(pickup.Point,next,arrival,remaining,drop);attempts++;
    }while(attempts<30&&(tripRoute.distance<60||JobOffers.Exists(j=>(j.pickup.Point-pickup.Point).sqrMagnitude<100)));
    int reward=20+5*Mathf.FloorToInt(Mathf.Max(0,pickupRoute.distance+tripRoute.distance-160)/80);
    var job=new RideJob{pickup=pickup,dropoff=drop,reward=reward,pickupMetres=pickupRoute.distance,tripMetres=tripRoute.distance,female=jobRandom.Next(2)==0};
    JobOffers.Add(job);city.Street(pickup);city.Street(drop);
   }
   JobOffers.Sort((a,b)=>(a.pickupMetres+a.tripMetres).CompareTo(b.pickupMetres+b.tripMetres));
   int previousReward=15;foreach(var job in JobOffers){job.reward=Math.Max(job.reward,previousReward+5);previousReward=job.reward;}
   JobOffersOpen=true;State=RideState.Paused;engine?.Pause();siren?.Pause();PauseRecklessFx(true);navigationAudio?.Pause();
  }
  static int FindArrivalHeading(CityRoute route,RideAddress target)
  {
   Vector2 d=route.points[route.points.Count-1]-route.points[route.points.Count-2];
   if(Mathf.Abs(d.x)>Mathf.Abs(d.y))return d.x>=0?1:3;return d.y>=0?0:2;
  }
  public bool AcceptJob(int index)
  {
   if(!JobOffersOpen||index<0||index>=JobOffers.Count)return false;
   ActiveJob=JobOffers[index];JobPhase=WorkPhase.Pickup;JobOffersOpen=false;serviceTime=0;
   State=RideState.Riding;PauseRecklessFx(false);if(sound){engine?.UnPause();if(IsChased)siren?.UnPause();}
   BeginJobTrafficYield();ReplanJob(false);UpdateNavigationVoice();notice="ĐÃ NHẬN CHUYẾN · "+FormatCoins(ActiveJob.reward)+" COIN";noticeTimer=2;return true;
  }
  void ReplanJob(bool afterJunction)
  {
   if(!jobsEnabled||ActiveJob==null||ServiceStopped)return;
   ActiveRoute=city.Plan(NavigationPosition,navNext,navHeading,Mathf.Max(0,NextJunctionDistance),CurrentWorkTarget);
   routePlanDistance=Distance;
   spokenKey="";streetTargetMissed=false;
   if(afterJunction){Reroutes++;rerouteNotice=2.5f;}
  }
  void UpdateWork(float dt)
  {
   if(!jobsEnabled)return;
   ReadJobTouch();
   if(State==RideState.Riding&&ActiveJob==null&&!JobOffersOpen){offersDelay-=dt;if(offersDelay<=0)ShowJobOffers();}
   if(navigationAudio!=null){navigationAudio.mute=!sound;if(State!=RideState.Riding)navigationAudio.Pause();}
   if(State!=RideState.Riding||ActiveJob==null)return;
   rerouteNotice=Mathf.Max(0,rerouteNotice-dt);
   UpdateNavigationVoice();
  }
  bool TurnSoonAfterStraight()
  {
   float travelled=Mathf.Max(0,Distance-routePlanDistance),along=0;
   var points=ActiveRoute.points;
   for(int i=1;i<points.Count-1;i++)
   {
    along+=Vector2.Distance(points[i-1],points[i]);
    Vector2 incoming=points[i]-points[i-1],outgoing=points[i+1]-points[i];
    if(incoming.sqrMagnitude>.01f&&outgoing.sqrMagnitude>.01f&&Vector2.Dot(incoming.normalized,outgoing.normalized)<.5f)
     return Mathf.Max(0,along-travelled)/Mathf.Max(2,Speed)<=4f;
   }
   return false;
  }
  void UpdateNavigationVoice()
  {
   if(IsTurning||ServiceStopped||ActiveRoute==null||State!=RideState.Riding)return;
   string key=navNext+":"+navHeading+":"+ActiveRoute.nextTurn+":"+JobPhase+":"+ActiveRoute.direct;
   if(key!=spokenKey){spokenKey=key;navigationCueStage=0;}
   // Never queue stale speech: only the next junction can issue a turn instruction.
   float metres=ActiveRoute.direct?ActiveRoute.distance-Mathf.Max(0,Distance-routePlanDistance):NextJunctionDistance;
   float seconds=Mathf.Max(0,metres)/Mathf.Max(2,Speed);
   if(metres<=0)return;
   int turn=ActiveRoute.direct?0:ActiveRoute.nextTurn;
   if(turn==0&&(TurnSoonAfterStraight()||(ActiveRoute.direct&&seconds<4)))return;
   int stage=seconds<=2?2:seconds<=4?1:0;
   if(stage>navigationCueStage){navigationCueStage=stage;SpeakNavigation(turn);}
  }
  void SpeakNavigation(int turn)
  {
   if(navigationAudio==null){navigationAudio=gameObject.AddComponent<AudioSource>();navigationAudio.playOnAwake=false;navigationAudio.volume=.9f;}
   var clip=turn<0?navigationLeft:turn>0?navigationRight:navigationStraight;
   if(sound&&clip!=null){navigationAudio.clip=clip;navigationAudio.Play();}
  }
  void TickWorkMotion(float dt)
  {
   if(!jobsEnabled)return;
   jobTrafficYieldRemaining=Mathf.Max(0,jobTrafficYieldRemaining-dt);
   if(!ServiceStopped)rearDepartureProtection=Mathf.Max(0,rearDepartureProtection-dt);
   if(ActiveJob==null||IsTurning)return;
   if(ServiceStopped){TickService(dt);return;}
   var target=CurrentWorkTarget;Vector2 delta=target.Point-NavigationPosition,heading=CityNavigation.Directions[navHeading];
   float ahead=Vector2.Dot(delta,heading),across=Vector2.Dot(delta,(Vector2)CityNavigation.Directions[(navHeading+1)%4]);
   if(Mathf.Abs(across)<.1f&&target.axis%2==navHeading%2)
   {
    int lane=TargetLane(target);
    if(ahead<=1.2f&&ahead>=-2.5f&&CurrentLane==lane&&Mathf.Abs(rider.localPosition.x-LaneX[lane])<.4f)
    {BeginService();return;}
    if(ahead< -4&&!streetTargetMissed){streetTargetMissed=true;Reroutes++;rerouteNotice=3;ActiveRoute=city.Plan(NavigationPosition,navNext,navHeading,Mathf.Max(0,NextJunctionDistance),target);spokenKey="";}
   }
  }
  int TargetLane(RideAddress target)=>Vector2.Dot(target.CurbDirection,CityNavigation.Directions[(navHeading+1)%4])>0?3:0;
  float WorkSpeedLimit(float desired)
  {
   if(ServiceStopped)return 0;
   if(!jobsEnabled||ActiveJob==null||IsTurning)return desired;
   var target=CurrentWorkTarget;var delta=target.Point-NavigationPosition;float z=Vector2.Dot(delta,CityNavigation.Directions[navHeading]);
   if(target.axis%2==navHeading%2&&Mathf.Abs(Vector2.Dot(delta,CityNavigation.Directions[(navHeading+1)%4]))<.1f&&CurrentLane==TargetLane(target)&&z>0&&z<22)
    return Mathf.Min(desired,Mathf.Max(2,z*.7f));
   return desired;
  }
  void BeginService()
  {
   bool pickup=JobPhase==WorkPhase.Pickup;JobPhase=pickup?WorkPhase.Boarding:WorkPhase.Alighting;serviceTime=0;Speed=0;engine?.Pause();navigationAudio?.Stop();
   foreach(var v in vehicles)v.jobYieldLane=-1;
   if(pickup)
   {
    customer=ActiveJob.female?femalePassenger:malePassenger;
    if(customer!=null){customer.SetParent(transform,false);boardingStart=new Vector3(Mathf.Sign(LaneX[CurrentLane])*10.2f,.25f,1);customer.localPosition=boardingStart;customer.localRotation=Quaternion.Euler(0,CurrentLane==3?-90:90,0);customer.gameObject.SetActive(true);}
   }
   else if(customer!=null){customer.SetParent(transform,true);boardingStart=customer.localPosition;}
  }
  void TickService(float dt)
  {
   serviceTime+=dt;bool pickup=JobPhase==WorkPhase.Boarding;float length=pickup?3.6f:3.2f;
   if(customer!=null)
   {
    var motion=customer.GetComponent<PassengerVisualAnimator>();
    if(pickup)
    {
     float walk=Mathf.Clamp01(serviceTime/2.3f);Vector3 side=rider.localPosition+new Vector3(CurrentLane==3?.75f:-.75f,.25f,-.9f);
     customer.localPosition=Vector3.Lerp(boardingStart,side,Mathf.SmoothStep(0,1,walk));
     if(serviceTime<2.3f)motion?.Pose("Walk",Mathf.Repeat(serviceTime, .8f)/.8f);
     else
     {
      float t=Mathf.Clamp01((serviceTime-2.3f)/1.3f);customer.localPosition=Vector3.Lerp(side,rider.localPosition+new Vector3(0,.16f,-.9f),Mathf.SmoothStep(0,1,t));
      customer.localRotation=Quaternion.Slerp(Quaternion.Euler(0,CurrentLane==3?-90:90,0),Quaternion.identity,t);motion?.Pose("Board",t);
     }
    }
    else
    {
     float t=Mathf.Clamp01(serviceTime/.9f);Vector3 side=rider.localPosition+new Vector3(CurrentLane==3?.8f:-.8f,.25f,-.9f);
     if(serviceTime<.9f){customer.localPosition=Vector3.Lerp(boardingStart,side,t);motion?.Pose("Board",1-t);}
     else {customer.localRotation=Quaternion.Euler(0,CurrentLane==3?90:-90,0);customer.localPosition=Vector3.Lerp(side,new Vector3(Mathf.Sign(LaneX[CurrentLane])*10.2f,.25f,1),Mathf.Clamp01((serviceTime-.9f)/2.3f));motion?.Pose("Walk",Mathf.Repeat(serviceTime,.8f)/.8f);}
    }
   }
   if(serviceTime<length)return;
   if(pickup)
   {
    if(customer!=null){customer.SetParent(rider,false);customer.localPosition=new Vector3(0,.16f,-.9f);customer.localRotation=Quaternion.identity;customer.GetComponent<PassengerVisualAnimator>()?.Pose("Sit",0);}
    JobPhase=WorkPhase.Delivery;rearDepartureProtection=3;ReplanJob(false);notice="KHÁCH ĐÃ LÊN XE · BẮT ĐẦU CHUYẾN ĐI";noticeTimer=2;
   }
   else
   {
    if(customer!=null){customer.gameObject.SetActive(false);customer.SetParent(transform,false);}customer=null;
    Coins=(int)Math.Min(int.MaxValue,(long)Coins+ActiveJob.reward);PlayerPrefs.SetInt(CoinsKey,Coins);PlayerPrefs.Save();
    notice="HOÀN THÀNH CHUYẾN  +"+FormatCoins(ActiveJob.reward)+" COIN";noticeTimer=3;
    ActiveJob=null;ActiveRoute=null;JobPhase=WorkPhase.None;offersDelay=2;if(jobMarker!=null)jobMarker.gameObject.SetActive(false);
   }
   if(sound)engine?.UnPause();
  }
  void YieldRearForService(TrafficVehicle v)
  {
   if(!jobsEnabled||!ServiceRearProtected||!v.active||v.wallCrashed)return;
   var p=v.model.localPosition;float safe=rider.localPosition.z-v.halfLength-2.6f;
   bool rear=v.previousPosition.z<rider.localPosition.z-.5f&&p.z>v.previousPosition.z;
   if(rear&&Mathf.Abs(p.x-rider.localPosition.x)<v.halfWidth+.85f&&p.z>safe)
   {p.z=safe;v.model.localPosition=p;v.speed=Mathf.Min(v.speed,Speed);v.knockVelocity.z=Mathf.Min(v.knockVelocity.z,Speed);v.motionVelocity.z=Mathf.Min(v.motionVelocity.z,Speed);if(v.brakeLights!=null)v.brakeLights.gameObject.SetActive(true);}
  }
 }
}
