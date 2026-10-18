using System;
using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 [Serializable] public sealed class RideAddress
 {
  public Vector2Int node;public int axis;public float offset;public int curb;
  public Vector2 Point=>new Vector2(node.x*80,node.y*80)+(Vector2)CityNavigation.Directions[axis]*offset;
  public Vector2 CurbDirection=>(Vector2)CityNavigation.Directions[(axis+1)%4]*curb;
 }
 public sealed class CityRoute
 {
  public readonly List<Vector2> points=new List<Vector2>();
  public float distance;public int nextTurn;public bool direct;
 }
 public sealed class CityNavigation
 {
  public static readonly Vector2Int[] Directions={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
  readonly Dictionary<string,string> names=new Dictionary<string,string>();readonly string[] catalogue;readonly System.Random random;
  public CityNavigation(string[] streets,int seed){catalogue=streets;random=new System.Random(seed);names["V:0"]="Trần Hưng Đạo";}
  public string Street(Vector2Int node,int heading)
  {
   string key=heading%2==0?"V:"+node.x:"H:"+node.y;
   if(names.TryGetValue(key,out string value))return value;
   var available=new List<string>();foreach(var name in catalogue)if(!names.ContainsValue(name))available.Add(name);
   value=available.Count>0?available[random.Next(available.Count)]:catalogue[names.Count%catalogue.Length]+" nối dài "+(names.Count/catalogue.Length);
   names[key]=value;return value;
  }
  public string Street(RideAddress a)=>Street(a.node,a.axis);
  struct State:IEquatable<State>
  {
   public Vector2Int node;public int heading;
   public State(Vector2Int n,int h){node=n;heading=h;}
   public bool Equals(State s)=>node==s.node&&heading==s.heading;
   public override bool Equals(object o)=>o is State s&&Equals(s);
   public override int GetHashCode()=>node.GetHashCode()*31+heading;
  }
  public static int RelativeTurn(int from,int to){int d=(to-from+4)%4;return d==3?-1:d==1?1:0;}
  public CityRoute Plan(Vector2 position,Vector2Int next,int heading,float remaining,RideAddress target)
  {
   var result=new CityRoute();Vector2 dir=Directions[heading],delta=target.Point-position;
   float ahead=Vector2.Dot(delta,dir),across=Vector2.Dot(delta,(Vector2)Directions[(heading+1)%4]);
   result.points.Add(position);
   if(Mathf.Abs(across)<.15f&&heading%2==target.axis%2&&ahead>=-.05f&&ahead<=remaining+.1f)
   {result.distance=Mathf.Max(0,ahead);result.direct=true;result.points.Add(target.Point);return result;}
   var start=new State(next,heading);var open=new List<State>{start};var cost=new Dictionary<State,float>{{start,Mathf.Max(0,remaining)}};
   var parents=new Dictionary<State,State>();State best=default;int finalHeading=0;float bestCost=float.PositiveInfinity;
   int minX=Math.Min(next.x,target.node.x)-3,maxX=Math.Max(next.x,target.node.x)+4,minY=Math.Min(next.y,target.node.y)-3,maxY=Math.Max(next.y,target.node.y)+4;
   int guard=0;
   while(open.Count>0&&guard++<12000)
   {
    int index=0;for(int i=1;i<open.Count;i++)if(cost[open[i]]<cost[open[index]])index=i;
    State s=open[index];open.RemoveAt(index);float g=cost[s];if(g>=bestCost)continue;
    foreach(int turn in new[]{0,-1,1})
    {
     int h=(s.heading+turn+4)%4;Vector2Int d=Directions[h];
     bool positive=s.node==target.node&&h==target.axis;
     bool negative=s.node==target.node+Directions[target.axis]&&h==(target.axis+2)%4;
     if(positive||negative)
     {
      float value=g+(positive?target.offset:80-target.offset);
      if(value<bestCost){bestCost=value;best=s;finalHeading=h;}
     }
     Vector2Int n=s.node+d;if(n.x<minX||n.x>maxX||n.y<minY||n.y>maxY)continue;
     var t=new State(n,h);float candidate=g+80;
     if(cost.TryGetValue(t,out float old)&&candidate>=old)continue;
     cost[t]=candidate;parents[t]=s;if(!open.Contains(t))open.Add(t);
    }
   }
   if(float.IsInfinity(bestCost))throw new InvalidOperationException("No city route");
   var states=new List<State>{best};while(!best.Equals(start)){best=parents[best];states.Add(best);}states.Reverse();
   foreach(var s in states)result.points.Add(new Vector2(s.node.x*80,s.node.y*80));
   result.points.Add(target.Point);result.distance=bestCost;
   int first=states.Count>1?states[1].heading:finalHeading;result.nextTurn=RelativeTurn(heading,first);return result;
  }
 }
}
