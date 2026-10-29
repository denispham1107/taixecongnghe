using System.Collections.Generic;
using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        // Mirrors the Blender signal housing around its authored depth centre.
        const float SignalFaceCentreZ=-.022f;
        readonly Dictionary<float,Mesh> signalFaceMeshes=new Dictionary<float,Mesh>();
        readonly List<KeyValuePair<MeshFilter,Mesh>> signalFaceBindings=new List<KeyValuePair<MeshFilter,Mesh>>();

        void InitializeTrafficSignalFaces()
        {
            if(!Application.isPlaying||signalFaceBindings.Count>0)return;
            foreach(var lamp in trafficLights)
            {
                if(lamp==null)continue;
                var filter=lamp.GetComponent<MeshFilter>();
                if(filter==null||filter.sharedMesh==null)continue;
                var t=lamp.transform;
                if(Mathf.Abs(t.localScale.z)<.0001f)continue;
                float offset=Mathf.Round((2*SignalFaceCentreZ-2*t.localPosition.z)/t.localScale.z*100000f)/100000f;
                if(!signalFaceMeshes.TryGetValue(offset,out var mesh))
                {
                    mesh=CreateSignalFaceMesh(offset);
                    signalFaceMeshes.Add(offset,mesh);
                }
                signalFaceBindings.Add(new KeyValuePair<MeshFilter,Mesh>(filter,filter.sharedMesh));
                filter.sharedMesh=mesh;
            }
        }

        // Both domed lenses share the original renderer and material: colour and
        // emission always match, with no extra draws, components or frame polling.
        static Mesh CreateSignalFaceMesh(float rearOffset)
        {
            const int sides=24,rings=4;
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var indices=new List<int>();
            for(int face=0;face<2;face++)
            {
                int start=vertices.Count;float direction=face==0?-1:1,centre=face==0?0:rearOffset;
                vertices.Add(new Vector3(0,0,centre+direction*.5f));normals.Add(Vector3.forward*direction);
                for(int ring=1;ring<=rings;ring++)
                {
                    float angle=ring*Mathf.PI*.5f/rings,radius=Mathf.Sin(angle)*.5f,depth=Mathf.Cos(angle)*.5f;
                    for(int side=0;side<sides;side++)
                    {
                        float a=side*2*Mathf.PI/sides;
                        var normal=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,direction*depth)*2;
                        vertices.Add(new Vector3(normal.x*.5f,normal.y*.5f,centre+normal.z*.5f));normals.Add(normal);
                    }
                }
                void Triangle(int a,int b,int c)
                {
                    if(Vector3.Dot(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]),normals[a])<0){int temp=b;b=c;c=temp;}
                    indices.Add(a);indices.Add(b);indices.Add(c);
                }
                for(int side=0;side<sides;side++)Triangle(start,start+1+side,start+1+(side+1)%sides);
                for(int ring=0;ring<rings-1;ring++)for(int side=0;side<sides;side++)
                {
                    int a=start+1+ring*sides+side,b=start+1+ring*sides+(side+1)%sides;
                    Triangle(a,a+sides,b+sides);Triangle(a,b+sides,b);
                }
            }
            var mesh=new Mesh{name="PV_SignalLenses_BothFaces"};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
            return mesh;
        }

        void ShutdownTrafficSignalFaces()
        {
            foreach(var binding in signalFaceBindings)if(binding.Key!=null)binding.Key.sharedMesh=binding.Value;
            signalFaceBindings.Clear();
            foreach(var mesh in signalFaceMeshes.Values)if(mesh!=null)Destroy(mesh);
            signalFaceMeshes.Clear();
        }
    }
}
