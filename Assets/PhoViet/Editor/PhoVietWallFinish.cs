using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PhoViet.Editor
{
    // UV x=.75 inside a palette cell marks plaster without changing palette colour or adding a draw call.
    // Kept in the model importer so a new Blender export or a clean checkout retains the finish.
    public sealed class PhoVietWallFinish : AssetPostprocessor
    {
        public override uint GetVersion()=>2123401;
        const string RiverFolder="Assets/PhoViet/Art/River/";
        public static bool IsBuilding(string path)=>
            path.StartsWith("Assets/PhoViet/Art/Streets/PV_StreetShop",StringComparison.Ordinal)&&path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)||
            path==RiverFolder+"PV_RiverApartments.fbx"||
            path==RiverFolder+"PV_RiverPetshopExtra.fbx";
        void OnPostprocessModel(GameObject root)
        {
            if(!IsBuilding(assetPath))return;
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))
                if(f.sharedMesh!=null)MarkPlaster(f.sharedMesh,f.GetComponent<Renderer>()?.sharedMaterials,
                    assetPath.StartsWith(RiverFolder,StringComparison.Ordinal));
        }
        public static int MarkPlaster(Mesh mesh,Material[] materials,bool river=false)
        {
            var uv=mesh.uv;var vertices=mesh.vertices;
            if(uv.Length!=vertices.Length)return 0;
            var marked=new bool[uv.Length];int faces=0;
            for(int s=0;s<mesh.subMeshCount;s++)
            {
                string material=materials!=null&&s<materials.Length&&materials[s]!=null?materials[s].name:"";
                // These exports have fixed slots Atlas / Glass / optional Room. With material import disabled
                // Unity supplies Default-Material during this callback; scene material names are unavailable.
                if(s==1||material.Contains("Glass"))continue;
                bool room=s==2||material.Contains("Room");
                var triangles=mesh.GetTriangles(s);
                for(int i=0;i<triangles.Length;i+=3)
                {
                    int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
                    int colour=Mathf.FloorToInt(uv[a].x*8)+8*Mathf.FloorToInt(uv[a].y*8);
                    bool paint=colour>=0&&colour<=9||colour==21||colour==22||room&&colour==19;
                    if(!paint)continue;
                    Vector3 cross=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);float area=cross.magnitude*.5f;
                    // FBX author-space up is Z. Roofs/floors, fine lettering, foliage and narrow metal frames are excluded.
                    if(area<.25f||Mathf.Abs(cross.z)>cross.magnitude*.65f)continue;
                    float y=(vertices[a].z+vertices[b].z+vertices[c].z)/3;
                    // Flat painted boards sit in front of the shell at their own fixed elevations.
                    // Keep the board substrate in its original finish, even when it shares the wall swatch.
                    if(s==0&&IsShopBoard(vertices[a],vertices[b],vertices[c],cross,river))continue;
                    if((colour==8||colour==9)&&area<1.2f&&y<3.2f)continue;
                    marked[a]=marked[b]=marked[c]=true;faces++;
                }
            }
            for(int i=0;i<uv.Length;i++)uv[i].x=(Mathf.Floor(uv[i].x*8)+(marked[i] ? .75f : .5f))/8;
            mesh.uv=uv;return faces;
        }
        static bool IsShopBoard(Vector3 a,Vector3 b,Vector3 c,Vector3 normal,bool river)
        {
            if(Mathf.Abs(normal.y)<normal.magnitude*.99f)return false;
            float low=Mathf.Min(a.z,Mathf.Min(b.z,c.z)),high=Mathf.Max(a.z,Mathf.Max(b.z,c.z));
            float depth=(a.y+b.y+c.y)/3;
            if(!river)return low>=3.32f&&high<=4.88f&&depth>=3.89f&&depth<=4.13f;
            if(low<2.92f||high>3.94f)return false;
            // Four front wall planes in author space (road and canal on each bank).
            return IsPlaqueFace(depth,-1.5f,-1,.215f)||IsPlaqueFace(depth,-4.5f,1,.138f)||
                   IsPlaqueFace(depth,-58.5f,1,.215f)||IsPlaqueFace(depth,-55.5f,-1,.138f);
        }
        static bool IsPlaqueFace(float depth,float wall,float facing,float projection)
        {
            float offset=(wall-depth)*facing;
            return Mathf.Abs(offset)<.001f||Mathf.Abs(offset-projection)<.002f||
                   Mathf.Abs(offset-(projection+.0185f))<.002f;
        }
    }
}
