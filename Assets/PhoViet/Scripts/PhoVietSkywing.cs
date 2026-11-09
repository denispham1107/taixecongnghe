using System.Collections.Generic;
using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        static readonly float[] SkywingTierRatios={1.3f,1.7f,2.1f};
        const int SkywingsPerTier=2;
        const float SkywingSpeed=20,SkywingSpacing=12;
        readonly List<HelicopterActor> skywings=new List<HelicopterActor>(SkywingTierRatios.Length*SkywingsPerTier);
        GameObject skywingPrefab;
        System.Random skywingRng;
        static readonly Bounds[] SkywingCollisionParts={
            new Bounds(new Vector3(0,-.03f,.04f),new Vector3(1.08f,1.02f,3.88f)),
            new Bounds(new Vector3(0,.84f,.62f),new Vector3(5.94f,.23f,.98f)),
            new Bounds(new Vector3(0,-.25f,.45f),new Vector3(5.9f,.22f,1.21f)),
            new Bounds(new Vector3(0,.34f,-1.57f),new Vector3(2.17f,.45f,.61f)),
            new Bounds(new Vector3(-.025f,0,1.78f),new Vector3(1.9f,1.9f,.19f))
        };

        public static int SkywingTierCount(float heightAboveSupport)
        {
            int count=0;
            foreach(float ratio in SkywingTierRatios)
                if(heightAboveSupport>=FlightHeightAboveSupport*ratio)count++;
            return count;
        }

        void ResetSkywings()
        {
            skywingRng=new System.Random(RunSeed^0x56ba837);
            if(Application.isPlaying&&skywingPrefab==null)skywingPrefab=Resources.Load<GameObject>("Skywing/PV_Skywing");
            foreach(var plane in skywings)
            {
                plane.active=plane.wrecked=false;
                plane.root.gameObject.SetActive(false);
            }
        }

        void SpawnSkywings()
        {
            if(!RiderFlying||skywingPrefab==null)return;
            int tiers=SkywingTierCount(jumpHeight-jumpBaseHeight);
            for(int tier=0;tier<tiers;tier++)
            {
                int active=0,firstLane=-1;
                foreach(var plane in skywings)
                    if(plane.active&&plane.tier==tier){active++;firstLane=plane.spawnLane;}
                while(active<SkywingsPerTier)
                {
                    int lane=firstLane<0?skywingRng.Next(4):(firstLane+2)%4;
                    if(!LaunchSkywing(tier,lane))break;
                    firstLane=lane;
                    active++;
                }
            }
        }

        bool LaunchSkywing(int tier,int lane)
        {
            HelicopterActor plane=null;
            foreach(var candidate in skywings)
                if(!candidate.active){plane=candidate;break;}
            if(plane==null)
            {
                if(!Application.isPlaying||skywings.Count>=SkywingTierRatios.Length*SkywingsPerTier)return false;
                var root=Instantiate(skywingPrefab,transform).transform;
                root.name="Crimson Skywing "+(skywings.Count+1);
                plane=new HelicopterActor{root=root,skywing=root.GetComponent<SkywingVisual>()};
                skywings.Add(plane);
            }
            float z=Mathf.Max(70,(Speed+SkywingSpeed)*1.65f);
            foreach(var other in skywings)
                if(other.active&&!other.wrecked&&other.velocity.z< -1)
                    z=Mathf.Max(z,other.root.localPosition.z+SkywingSpacing);
            plane.tier=tier;
            plane.spawnLane=lane;
            float y=jumpBaseHeight+FlightHeightAboveSupport*SkywingTierRatios[tier]+PlayerHeight*.5f;
            LaunchHelicopter(plane,new Vector3(LaneX[lane],y,z));
            return true;
        }

        static bool SkywingTouchesSegment(Vector3 from,Vector3 to,Vector3 expansion,out float at)
        {
            at=2;
            Vector3 delta=to-from;
            float distance=delta.magnitude;
            foreach(var part in SkywingCollisionParts)
            {
                var box=part;
                box.Expand(expansion*2);
                if(box.Contains(from)){at=0;return true;}
                if(distance>.0001f&&box.IntersectRay(new Ray(from,delta/distance),out float hit)&&hit<=distance)
                    at=Mathf.Min(at,hit/distance);
            }
            return at<=1;
        }

        bool SkywingTouchesRider(Vector3 from,Vector3 to,Quaternion heading,Vector3 previousRider)
        {
            Quaternion inverse=Quaternion.Inverse(heading);
            Quaternion relative=inverse*rider.localRotation;
            Vector3 x=relative*Vector3.right*PlayerHalfWidth;
            Vector3 y=relative*Vector3.up*(PlayerHeight*.5f);
            Vector3 z=relative*Vector3.forward*PlayerHalfLength;
            Vector3 half=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),
                Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z));
            Vector3 lift=Vector3.up*(PlayerHeight*.5f);
            return SkywingTouchesSegment(inverse*(previousRider+lift-from),inverse*(rider.localPosition+lift-to),half,out _);
        }
    }
}
