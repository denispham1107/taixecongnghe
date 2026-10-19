using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public static class WaypointMesh
 {
  static readonly Vector2[] Outline={new Vector2(-.28f,.85f),new Vector2(.28f,.85f),new Vector2(.28f,-.05f),new Vector2(.68f,-.05f),new Vector2(0,-.85f),new Vector2(-.68f,-.05f),new Vector2(-.28f,-.05f)};
  static readonly int[] Front={0,1,2,0,2,6,6,3,4,6,4,5};
  static Vector3 Point(int i,float scale,float z)=>new Vector3(Outline[i].x*scale,Outline[i].y*scale,z);
  public static Mesh FlatArrow()
  {
   var v=new Vector3[Outline.Length];for(int i=0;i<v.Length;i++)v[i]=Point(i,1,0);
   var mesh=new Mesh{name="Waypoint glow silhouette"};mesh.vertices=v;mesh.triangles=Front;mesh.RecalculateBounds();return mesh;
  }
  public static Mesh BeveledArrow()
  {
   var vertices=new List<Vector3>();var triangles=new List<int>();
   System.Action<Vector3,Vector3,Vector3> tri=(a,b,c)=>{int at=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(at);triangles.Add(at+1);triangles.Add(at+2);};
   System.Action<Vector3,Vector3,Vector3,Vector3> quad=(a,b,c,d)=>{tri(a,b,c);tri(a,c,d);};
   for(int i=0;i<Front.Length;i+=3)
   {
    tri(Point(Front[i],.86f,-.24f),Point(Front[i+1],.86f,-.24f),Point(Front[i+2],.86f,-.24f));
    tri(Point(Front[i+2],1,.22f),Point(Front[i+1],1,.22f),Point(Front[i],1,.22f));
   }
   for(int i=0;i<Outline.Length;i++)
   {
    int j=(i+1)%Outline.Length;
    quad(Point(i,.86f,-.24f),Point(i,1,-.12f),Point(j,1,-.12f),Point(j,.86f,-.24f));
    quad(Point(i,1,-.12f),Point(i,1,.22f),Point(j,1,.22f),Point(j,1,-.12f));
   }
   var mesh=new Mesh{name="Beveled solid waypoint arrow"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  public static Mesh GroundRing()
  {
   const int count=48;var v=new Vector3[count*2];var t=new int[count*6];
   for(int i=0;i<count;i++)
   {
    float a=i*Mathf.PI*2/count;v[i*2]=new Vector3(Mathf.Cos(a)*.51f,0,Mathf.Sin(a)*.51f);v[i*2+1]=new Vector3(Mathf.Cos(a)*.66f,0,Mathf.Sin(a)*.66f);
    int n=(i+1)%count;t[i*6]=i*2;t[i*6+1]=n*2;t[i*6+2]=n*2+1;t[i*6+3]=i*2;t[i*6+4]=n*2+1;t[i*6+5]=i*2+1;
   }
   var mesh=new Mesh{name="Waypoint ground halo"};mesh.vertices=v;mesh.triangles=t;mesh.RecalculateBounds();return mesh;
  }
 }
}
