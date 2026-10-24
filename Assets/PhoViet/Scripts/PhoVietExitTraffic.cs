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
        readonly TrafficVehicle[] rearExitReserved=new TrafficVehicle[2];
        readonly float[] rearExitWarning=new float[2];
        bool RearExitReserved(TrafficVehicle v)=>rearExitReserved[0]==v||rearExitReserved[1]==v;
        public bool OrdinaryRearWarningVisible(int lane)=>lane>=2&&lane<=3&&turnExitTrafficEnabled&&rearExitReserved[lane-2]!=null&&!IsTurning&&!ServiceStopped;
        public float OrdinaryRearWarningRemaining(int lane)=>lane>=2&&lane<=3&&rearExitReserved[lane-2]!=null?rearExitWarning[lane-2]:-1;
        void ClearRearExitWarnings(){for(int i=0;i<2;i++){rearExitReserved[i]=null;rearExitWarning[i]=0;}}
        void ResetExitTraffic(){ExitRearSpawns=0;ExitOncomingSpawns=0;exitRearRemaining=0;exitOncomingRemaining=0;exitWaveTimer=0;exitWaveAge=0;ClearRearExitWarnings();}
        void QueueExitTraffic()
        {
            if(!turnExitTrafficEnabled)return;
            ClearRearExitWarnings();
            exitRearRemaining=TrafficCount(rng.Next(3,6));exitOncomingRemaining=TrafficCount(rng.Next(3,6));exitWaveAge=0;exitWaveTimer=.25f;
            if(TryExitTraffic(true))exitRearRemaining--;
            if(TryExitTraffic(false))exitOncomingRemaining--;
        }
        void TickExitTraffic(float dt)
        {
            if(!turnExitTrafficEnabled||dt<=0||(exitRearRemaining==0&&exitOncomingRemaining==0))return;
            // Allow the original wave size to finish despite the two-second lane warnings.
            exitWaveAge+=dt;exitWaveTimer-=dt;if(exitWaveAge>12){exitRearRemaining=0;exitOncomingRemaining=0;return;}
            if(exitWaveTimer>0)return;
            bool rear=exitOncomingRemaining==0||(exitRearRemaining>0&&rng.Next(2)==0);
            if(TryExitTraffic(rear)){if(rear)exitRearRemaining--;else exitOncomingRemaining--;}
            exitWaveTimer=.20f+(float)rng.NextDouble()*.30f;
        }
        bool TryExitTraffic(bool rear)
        {
            if(rear)
            {
                int first=rng.Next(2);
                for(int i=0;i<2;i++)
                {
                    int slot=(first+i)%2;if(rearExitReserved[slot]!=null)continue;
                    int selected=FindExitCandidate();if(selected<0)return false;
                    rearExitReserved[slot]=vehicles[selected];rearExitWarning[slot]=2;return true;
                }
                return false;
            }
            return SpawnExitTraffic(false,-1,null);
        }
        int FindExitCandidate()
        {
            int choice=-1,count=0;
            for(int i=0;i<vehicles.Count;i++)if(!vehicles[i].active&&!vehicles[i].eventOnly&&!RearExitReserved(vehicles[i])&&rng.Next(++count)==0)choice=i;
            return choice;
        }
        void TickOrdinaryRearWarnings(float dt)
        {
            if(!turnExitTrafficEnabled){ClearRearExitWarnings();return;}
            if(dt<=0)return;
            for(int slot=0;slot<2;slot++)
            {
                if(rearExitReserved[slot]==null)continue;
                if(IsTurning||ServiceStopped){rearExitWarning[slot]=2;continue;}
                rearExitWarning[slot]=Mathf.Max(0,rearExitWarning[slot]-dt);
                if(rearExitWarning[slot]<=0&&SpawnExitTraffic(true,slot+2,rearExitReserved[slot]))rearExitReserved[slot]=null;
            }
        }
        bool SpawnExitTraffic(bool rear,int fixedLane,TrafficVehicle reserved)
        {
            int choice=rear?vehicles.IndexOf(reserved):FindExitCandidate();
            if(choice<0)return false;var candidate=vehicles[choice];
            if(candidate.active)return false;
            candidate.model.GetComponent<MeshyCarVariants>()?.PrepareForSpawn(rng,candidate,!rear);
            for(int attempt=0;attempt<8;attempt++)
            {
                int lane=rear?fixedLane:rng.Next(0,2);
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
                if(rear)rearExitReserved[lane-2]=null;
                var v=SpawnTraffic(lane,z,choice);
                if(v==null){if(rear)rearExitReserved[lane-2]=candidate;return false;}
                if(rear)
                {
                    v.enteringFromBehind=true;v.rearEntryBoost=10+(float)rng.NextDouble()*5;
                    v.speed=v.baseTrafficSpeed=TrafficPaceSpeed+v.rearEntryBoost;ExitRearSpawns++;
                }
                else ExitOncomingSpawns++;
                return true;
            }
            return false;
        }
    }
}
