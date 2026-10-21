using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  readonly List<Vector3> crashPolicePath=new List<Vector3>();
  int crashPoliceWaypoint;
  float crashPoliceReplan;
  Vector3 crashPoliceGoal;
  public int CrashPoliceReplans {get;private set;}
  void ResetCrashPoliceNavigation()
  {crashPolicePath.Clear();crashPoliceWaypoint=0;crashPoliceReplan=0;CrashPoliceReplans=0;}
  bool CrashPoliceRoad(Vector3 p)
  {
   // A crash can freeze the rider halfway round a junction. Both arms remain
   // driveable in that coordinate frame; buildings beyond those arms do not.
   return Mathf.Abs(p.x)<=8.4f||(IsTurning&&Mathf.Abs(p.z-NextJunctionDistance)<=8.4f);
  }
  bool CrashPoliceSegmentClear(Vector3 from,Vector3 to,float dt)
  {
   float length=Vector3.Distance(from,to);int samples=Mathf.Max(1,Mathf.CeilToInt(length));
   for(int i=1;i<=samples;i++)if(!CrashPoliceRoad(Vector3.Lerp(from,to,(float)i/samples)))return false;
   Vector3 delta=to-from;float yaw=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;
   return RoutePolicePathClear(from,to,yaw,dt);
  }
  bool CrashPoliceWaypoint(Vector3 start,float dt,out Vector3 target)
  {
   Vector3 scroll=Vector3.forward*Speed*dt;
   for(int i=crashPoliceWaypoint;i<crashPolicePath.Count;i++)crashPolicePath[i]-=scroll;
   crashPoliceGoal-=scroll;crashPoliceReplan=Mathf.Max(0,crashPoliceReplan-dt);
   Vector3 goal=rider.localPosition;goal.y=0;
   if(Flat(goal-crashPoliceGoal).sqrMagnitude>2.25f){crashPolicePath.Clear();crashPoliceReplan=0;}
   while(crashPoliceWaypoint<crashPolicePath.Count&&Vector3.Distance(start,crashPolicePath[crashPoliceWaypoint])<.08f)crashPoliceWaypoint++;
   if(crashPoliceWaypoint<crashPolicePath.Count)
   {
    target=crashPolicePath[crashPoliceWaypoint];
    if(CrashPoliceSegmentClear(start,target,dt))return true;
    crashPolicePath.Clear();crashPoliceWaypoint=0;
   }
   // Use a direct approach whenever the whole segment is clear, not just the
   // next 3 metres. A blocked local fan can otherwise alternate forever.
   Vector3 delta=goal-start;
   Vector3 localGoal=start+Vector3.ClampMagnitude(delta,60);
   if(CrashPoliceSegmentClear(start,localGoal,dt)){target=localGoal;return true;}
   if(crashPoliceReplan<=0)
   {
    crashPoliceGoal=goal;crashPoliceReplan=.4f;CrashPoliceReplans++;
    PlanCrashPolicePath(start,localGoal,delta.magnitude<=60?2.05f:1.2f);
   }
   target=crashPoliceWaypoint<crashPolicePath.Count?crashPolicePath[crashPoliceWaypoint]:start;
   return crashPoliceWaypoint<crashPolicePath.Count;
  }
  void PlanCrashPolicePath(Vector3 start,Vector3 goal,float captureRadius)
  {
   crashPolicePath.Clear();crashPoliceWaypoint=0;
   const float cell=1.2f;
   float minX=Mathf.Min(start.x,goal.x)-10,maxX=Mathf.Max(start.x,goal.x)+10;
   if(!IsTurning){minX=Mathf.Max(-8.4f,minX);maxX=Mathf.Min(8.4f,maxX);}
   int left=Mathf.FloorToInt((minX-start.x)/cell),right=Mathf.CeilToInt((maxX-start.x)/cell);
   int bottom=Mathf.FloorToInt((Mathf.Min(start.z,goal.z)-14-start.z)/cell),top=Mathf.CeilToInt((Mathf.Max(start.z,goal.z)+14-start.z)/cell);
   int width=right-left+1,height=top-bottom+1,count=width*height;
   if(count<=0||count>10000||left>0||right<0)return;
   var cost=new float[count];var score=new float[count];var parent=new int[count];var closed=new bool[count];
   for(int i=0;i<count;i++){cost[i]=float.PositiveInfinity;parent[i]=-1;}
   int first=(-bottom)*width-left;cost[first]=0;score[first]=Vector3.Distance(start,goal);
   var open=new List<int>{first};int found=-1;Vector3 finalApproach=goal;bool useFinalApproach=false;
   while(open.Count>0)
   {
    int best=0;for(int i=1;i<open.Count;i++)if(score[open[i]]<score[open[best]])best=i;
    int current=open[best];open.RemoveAt(best);if(closed[current])continue;closed[current]=true;
    int x=current%width+left,z=current/width+bottom;Vector3 p=start+new Vector3(x*cell,0,z*cell);
    if(Vector3.Distance(p,goal)<=captureRadius){found=current;break;}
    // The fallen rider may lie beside/under the vehicle that hit them. Connect
    // to a reachable arrest position around the rider, not the occupied centre.
    // Exact endpoints also avoid losing a narrow clearance to grid rounding.
    if(Vector3.Distance(p,goal)<captureRadius+cell*3)
    {
     for(int ring=0;ring<16;ring++)
     {
      float angle=ring*Mathf.PI/8;Vector3 q=goal+new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*captureRadius;
      if(!CrashPoliceSegmentClear(p,q,.15f))continue;
      found=current;finalApproach=q;useFinalApproach=true;break;
     }
     if(found>=0)break;
    }
    for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)
    {
     if(dx==0&&dz==0)continue;int nx=x+dx,nz=z+dz;if(nx<left||nx>right||nz<bottom||nz>top)continue;
     int next=(nz-bottom)*width+nx-left;if(closed[next])continue;
     Vector3 q=start+new Vector3(nx*cell,0,nz*cell);float nextCost=cost[current]+Vector3.Distance(p,q);
     if(nextCost>=cost[next]||!CrashPoliceSegmentClear(p,q,.15f))continue;
     bool fresh=float.IsPositiveInfinity(cost[next]);cost[next]=nextCost;score[next]=nextCost+Mathf.Max(0,Vector3.Distance(q,goal)-captureRadius);parent[next]=current;
     if(fresh)open.Add(next);
    }
   }
   if(found<0)return;
   if(useFinalApproach)crashPolicePath.Add(finalApproach);
   for(int at=found;at!=first&&at>=0;at=parent[at])crashPolicePath.Add(start+new Vector3((at%width+left)*cell,0,(at/width+bottom)*cell));
   crashPolicePath.Reverse();
  }
 }
}
