using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
    public static class JunctionStreetArt
    {
        public static Transform AddArrows(Transform block)
        {
            var old=block.Find("Vạch mũi tên giao lộ • 4 hướng");if(old!=null)return old;
            var root=StreetArt.Group("Vạch mũi tên giao lộ • 4 hướng",block);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh=CreateArrowMesh();root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=StreetArt.Mat(StreetArt.Cream);return root;
        }
        public static Mesh CreateArrowMesh()
        {
            var vertices=new List<Vector3>();var indices=new List<int>();
            for(int arm=0;arm<4;arm++)
            {
                Quaternion rotation=Quaternion.Euler(0,arm*90,0);
                for(int lane=0;lane<4;lane++)
                {
                    if(lane==0||lane==3)
                    {
                        float s=lane==0?-1:1;
                        // Separate rectangles and a triangular arrowhead avoid concave triangulation.
                        Rect(vertices,indices,rotation,lane,-.14f,-1.1f,.14f,.42f);
                        Rect(vertices,indices,rotation,lane,Mathf.Min(0,s*.85f),.12f,Mathf.Max(0,s*.85f),.42f);
                        Triangle(vertices,indices,rotation,lane,new Vector2(s*1.42f,.27f),new Vector2(s*.75f,.83f),new Vector2(s*.75f,-.29f));
                    }
                    else
                    {
                        Rect(vertices,indices,rotation,lane,-.14f,-1.1f,.14f,.55f);
                        Triangle(vertices,indices,rotation,lane,new Vector2(0,1.22f),new Vector2(-.56f,.48f),new Vector2(.56f,.48f));
                    }
                }
            }
            var mesh=new Mesh{name="PV_JunctionArrows16"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }
        static Vector3 Point(Quaternion q,int lane,Vector2 p)
        {
            // Side crosswalks are 12 m from centre (the main-road ones are 9 m).
            // Leave a clear gap past the outside edge of each crosswalk.
            bool side=Mathf.Abs((q*Vector3.forward).x)>.5f;
            return new Vector3(0,.035f,VietnamStreet.IntersectionCenter)+q*new Vector3(PhoVietGame.LaneX[lane]+p.x,0,-(side?16f:13.5f)+p.y);
        }
        static void Triangle(List<Vector3> v,List<int> f,Quaternion q,int lane,Vector2 a,Vector2 b,Vector2 c)
        {
            int n=v.Count;v.Add(Point(q,lane,a));v.Add(Point(q,lane,b));v.Add(Point(q,lane,c));
            if(Vector3.Cross(v[n+1]-v[n],v[n+2]-v[n]).y>0)f.AddRange(new[]{n,n+1,n+2});else f.AddRange(new[]{n,n+2,n+1});
        }
        static void Rect(List<Vector3> v,List<int> f,Quaternion q,int lane,float x0,float z0,float x1,float z1)
        {Triangle(v,f,q,lane,new Vector2(x0,z0),new Vector2(x1,z0),new Vector2(x1,z1));Triangle(v,f,q,lane,new Vector2(x0,z0),new Vector2(x1,z1),new Vector2(x0,z1));}
    }
}
