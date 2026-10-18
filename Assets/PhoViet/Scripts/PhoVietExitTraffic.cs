using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        public bool turnExitTrafficEnabled=true;
        public int ExitRearSpawns {get;private set;}
        public int ExitOncomingSpawns {get;private set;}
        int exitRearRemaining,exitOncomingRemaining;
        float exitWaveTimer,exitWaveAge;
        void ResetExitTraffic(){ExitRearSpawns=0;ExitOncomingSpawns=0;exitRearRemaining=0;exitOncomingRemaining=0;exitWaveTimer=0;exitWaveAge=0;}
        void QueueExitTraffic()
        {
            if(!turnExitTrafficEnabled)return;
            exitRearRemaining=rng.Next(3,6);exitOncomingRemaining=rng.Next(3,6);exitWaveAge=0;exitWaveTimer=.25f;
            if(TryExitTraffic(true))exitRearRemaining--;
            if(TryExitTraffic(false))exitOncomingRemaining--;
        }
        void TickExitTraffic(float dt)
        {
            if(!turnExitTrafficEnabled||dt<=0||(exitRearRemaining==0&&exitOncomingRemaining==0))return;
            exitWaveAge+=dt;exitWaveTimer-=dt;if(exitWaveAge>7){exitRearRemaining=0;exitOncomingRemaining=0;return;}
            if(exitWaveTimer>0)return;
            bool rear=exitOncomingRemaining==0||(exitRearRemaining>0&&rng.Next(2)==0);
            if(TryExitTraffic(rear)){if(rear)exitRearRemaining--;else exitOncomingRemaining--;}
            exitWaveTimer=.20f+(float)rng.NextDouble()*.30f;
        }
        bool TryExitTraffic(bool rear)
        {
            int choice=-1,count=0;
            for(int i=0;i<vehicles.Count;i++)if(!vehicles[i].active&&!vehicles[i].eventOnly&&rng.Next(++count)==0)choice=i;
            if(choice<0)return false;var candidate=vehicles[choice];
            for(int attempt=0;attempt<8;attempt++)
            {
                int lane=rear?rng.Next(2,4):rng.Next(0,2);
                float z=rear?Mathf.Min(-22,rideCamera.transform.localPosition.z-candidate.halfLength-6)-(float)rng.NextDouble()*12-attempt*5:
                    125+(float)rng.NextDouble()*35+attempt*5;
                Vector3 position=new Vector3(LaneX[lane],0,z);bool clear=true;
                foreach(var other in vehicles)
                {
                    if(!other.active||other.model.localPosition.y>2.4f)continue;
                    Vector2 half=VehicleFootprint(other);Vector3 delta=position-other.model.localPosition;
                    if(Mathf.Abs(delta.x)<candidate.halfWidth+half.x+.45f&&Mathf.Abs(delta.z)<candidate.halfLength+half.y+2){clear=false;break;}
                }
                if(clear&&IsChased&&policeBike!=null&&policeBike.gameObject.activeSelf)
                {
                    Vector3 delta=position-policeBike.localPosition;
                    if(Mathf.Abs(delta.x)<candidate.halfWidth+.8f&&Mathf.Abs(delta.z)<candidate.halfLength+2.7f)clear=false;
                }
                if(!clear)continue;
                var v=SpawnTraffic(lane,z,choice);if(v==null)return false;
                if(rear)
                {
                    v.enteringFromBehind=true;v.rearEntryBoost=10+(float)rng.NextDouble()*5;
                    v.speed=v.baseTrafficSpeed=Mathf.Max(cruiseSpeed,17)+v.rearEntryBoost;ExitRearSpawns++;
                }
                else ExitOncomingSpawns++;
                return true;
            }
            return false;
        }
    }
}
