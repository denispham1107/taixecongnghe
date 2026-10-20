using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyJunctionRefinements
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Func<string,object[],object> call=(n,args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
        Action<float> step=dt=>call("TickRide",new object[]{dt});
        float scale=Time.timeScale;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);bool exit=g.turnExitTrafficEnabled,reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises;
        Action clean=()=>{g.turnExitTrafficEnabled=false;g.RestartRide();g.recklessTrafficEnabled=false;g.trafficSurprises=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);};
        Action<int,float> at=(lane,z)=>{int guard=0;while(g.NextJunctionDistance>z&&guard++<800)step(.01f);if(guard>=800)throw new Exception("Approach timeout");typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);};
        Action finish=()=>{int guard=0;while(g.IsTurning&&guard++<600){step(.01f);set("nextIntersectionDistance",float.PositiveInfinity);}if(guard>=600)throw new Exception("Turn timeout");};
        try
        {
            Time.timeScale=0;clean();
            foreach(var row in g.roadRows)
            {
                var mesh=row.tiles[0].Find("Vạch mũi tên giao lộ • 4 hướng").GetComponent<MeshFilter>().sharedMesh;
                Vector3[] points=mesh.vertices;int[] triangles=mesh.triangles;
                for(int arm=0;arm<4;arm++)
                {
                    Quaternion inverse=Quaternion.Inverse(Quaternion.Euler(0,arm*90,0));float min=float.MaxValue,max=float.MinValue;
                    for(int i=arm*48;i<(arm+1)*48;i++){var p=inverse*(points[i]-new Vector3(0,.035f,70));min=Mathf.Min(min,-p.z);max=Mathf.Max(max,-p.z);}
                    float crosswalkOutside=arm%2==0?10.05f:13.05f;
                    check(min>crosswalkOutside+1.5f,row.root.name+" arm "+arm+": every arrow is beyond its crosswalk with a visible clear gap");
                }
            }
            foreach(int side in new[]{-1,1})foreach(float near in new[]{6f,2f,0f,-3f,-6.6f})
            {
                clean();at(side<0?0:3,near);string name=g.NextStreet;float centre=g.NextJunctionDistance;float before=g.Distance;
                check(g.QueuedTurn==0&&!g.IsTurning,"Late "+side+" at "+near+": no prior selection required");g.MoveLane(side);
                check(g.IsTurning&&g.CurrentLane==(side<0?0:3),"Late "+side+" at "+near+": matching curb input turns immediately inside the red-box region");
                bool onRoad=true,finite=true;int guard=0;
                while(g.IsTurning&&guard++<600)
                {
                    step(.01f);set("nextIntersectionDistance",float.PositiveInfinity);
                    if(!g.IsTurning)break;
                    float travel=g.Distance-before;Vector3 p=g.rider.localPosition+Vector3.forward*travel;
                    onRoad&=Mathf.Abs(p.x)<6.9f||Mathf.Abs(p.z-centre)<6.9f;
                    finite&=!float.IsNaN(g.TurnYaw)&&!float.IsNaN(p.x)&&!float.IsNaN(p.z);
                }
                check(onRoad&&finite&&guard<600,"Late "+side+" at "+near+": curved path stays on the intersecting road surfaces without a positional jump");
                check(g.CompletedTurns==1&&g.CurrentStreet==name&&g.Distance>before&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[side<0?0:3])<.001f,"Late "+side+" at "+near+": new street, ongoing distance and exact previously confirmed exit lane");
                check(g.blocks.All(b=>Mathf.Abs(b.localPosition.x)<.001f)&&g.NextJunctionDistance>40,"Late "+side+" at "+near+": new street geometry aligns with the player and continues to another junction");
            }
            clean();at(2,0);g.MoveLane(1);check(!g.IsTurning,"Middle lane inside the junction still changes lane rather than turning from the wrong curb");
            clean();at(3,0);g.MoveLane(-1);check(!g.IsTurning&&g.CurrentLane==2,"Right curb does not turn left across the junction");
            clean();at(0,0);g.MoveLane(1);check(!g.IsTurning&&g.CurrentLane==1,"Left curb does not turn right across the junction");
            clean();at(3,-8);g.MoveLane(1);check(!g.IsTurning,"Turn input beyond the far edge of the intersection is not accepted");
            clean();at(3,0);g.MoveLane(1);step(.2f);typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,true);
            var hit=g.SpawnTraffic(2,50,0);hit.model.localPosition=g.rider.localPosition;hit.previousPosition=hit.model.localPosition;
            call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,0f});check(g.State==PhoVietGame.RideState.Falling,"Late turning never grants ghosting or pursuit collision immunity");
            var patterns=new HashSet<string>();
            for(int run=0;run<5;run++)
            {
                clean();set("timeRiding",10f);at(run%2==0?0:3,2);g.turnExitTrafficEnabled=true;g.MoveLane(run%2==0?-1:1);finish();
                var initial=g.vehicles.Where(v=>v.active&&!v.crossing).ToArray();
                check(g.ExitRearSpawns==0&&g.ExitOncomingSpawns==1&&initial.Length==1,"Turn "+run+": rear traffic warns before entry while oncoming traffic enters normally");
                call("TickOrdinaryRearWarnings",new object[]{2.01f});
                var rear=g.vehicles.Single(v=>v.active&&v.enteringFromBehind);var coming=initial.Single(v=>v.speed<0);
                check(rear.model.localPosition.z<=-22&&rear.lane>=2&&rear.speed>Mathf.Max(g.Speed,17),"Turn "+run+": same-direction vehicles start behind camera and catch up from the bottom of the screen");
                check(coming.model.localPosition.z>=125&&coming.lane<2,"Turn "+run+": oncoming vehicles enter from the far end of the new street");
                for(int i=0;i<60;i++){call("TickExitTraffic",new object[]{.2f});call("TickOrdinaryRearWarnings",new object[]{.2f});}
                check(g.ExitRearSpawns>=3&&g.ExitRearSpawns<=5&&g.ExitOncomingSpawns>=3&&g.ExitOncomingSpawns<=5,"Turn "+run+": multiple randomized vehicles in both directions join the exit waves");
                var all=g.vehicles.Where(v=>v.active&&!v.crossing).ToArray();bool separated=true;
                for(int a=0;a<all.Length;a++)for(int b=a+1;b<all.Length;b++)
                {
                    Vector3 d=all[a].model.localPosition-all[b].model.localPosition;
                    if(Mathf.Abs(d.x)<all[a].halfWidth+all[b].halfWidth&&Mathf.Abs(d.z)<all[a].halfLength+all[b].halfLength)separated=false;
                }
                check(separated,"Turn "+run+": exit traffic never spawns overlapping another active vehicle"+(separated?"":" · "+string.Join(" | ",all.Select(v=>v.model.name+" "+v.model.localPosition+" bounds "+v.halfWidth+","+v.halfLength+" yaw "+v.model.localEulerAngles.y))));
                patterns.Add(string.Join("|",all.Select(v=>v.model.name+":"+v.lane+":"+v.model.localPosition.z.ToString("F2"))));
            }
            check(patterns.Count==5,"Restarting changes exit vehicle types, lanes and spacing instead of replaying one fixed pattern");
            clean();g.turnExitTrafficEnabled=true;var crowded=g.vehicles.Where(v=>!v.eventOnly).ToArray();
            foreach(var v in crowded){v.active=true;v.model.gameObject.SetActive(true);v.model.localPosition=new Vector3(0,0,50);}
            call("QueueExitTraffic",null);check(g.ExitRearSpawns==0&&g.ExitOncomingSpawns==0&&crowded.All(v=>v.active),"A full pool defers exit spawns without stealing or teleporting existing traffic");
            clean();g.turnExitTrafficEnabled=true;call("QueueExitTraffic",null);call("TickOrdinaryRearWarnings",new object[]{2.01f});var entrant=g.vehicles.Single(v=>v.active&&v.enteringFromBehind);
            float z0=entrant.model.localPosition.z;for(int i=0;i<8;i++)call("TickLongitudinalVehicle",new object[]{entrant,.05f});
            check(entrant.model.localPosition.z>z0,"Rear entrants genuinely advance toward the rider");
            entrant.model.localPosition=new Vector3(PhoVietGame.LaneX[2],0,5);call("TickLongitudinalVehicle",new object[]{entrant,.02f});
            check(!entrant.enteringFromBehind&&entrant.baseTrafficSpeed>=7&&entrant.baseTrafficSpeed<=14,"After overtaking, rear entrants return to ordinary traffic behavior");
            g.ResetRide();check(g.ExitRearSpawns==0&&g.ExitOncomingSpawns==0&&!g.IsTurning,"Restart clears late-turn and exit-wave state");
            return new{success=true,count=checks.Count,checks};
        }
        finally{g.turnExitTrafficEnabled=exit;g.recklessTrafficEnabled=reckless;g.trafficSurprises=surprises;g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=scale;}
    }
}
