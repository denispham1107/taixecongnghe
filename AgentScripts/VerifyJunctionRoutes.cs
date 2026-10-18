using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using PhoViet;
public static class VerifyJunctionRoutes
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL: "+n);checks.Add(n);};
        Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action<float> step=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
        Action<string,object[]> call=(n,args)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,args);
        float scale=Time.timeScale;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);bool reckless=g.recklessTrafficEnabled,surprises=g.trafficSurprises,exitTraffic=g.turnExitTrafficEnabled;
        Action clean=()=>{g.turnExitTrafficEnabled=false;g.RestartRide();g.trafficSurprises=false;g.recklessTrafficEnabled=false;foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);};
        Action<int> lane=n=>{typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,n);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[n],0,0);};
        Action approach=()=>{int guard=0;while(g.NextJunctionDistance>24&&guard++<300)step(.02f);if(guard>=300)throw new Exception("Approach timeout");};
        Action begin=()=>{int guard=0;while(!g.IsTurning&&guard++<150)step(.02f);if(guard>=150)throw new Exception("Begin timeout");};
        Action finish=()=>{int guard=0;while(g.IsTurning&&guard++<200){step(.02f);set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);}if(guard>=200)throw new Exception("Finish timeout");};
        try
        {
            Time.timeScale=0;clean();
            check(g.roadRows.Count==5&&g.roadRows.All(r=>r.tiles.Count==7),"Five pooled seven-block road rows support incoming, outgoing and retained junction scenery");
            foreach(var row in g.roadRows)
            {
                check(row.tiles.All(t=>t.Find("Vạch mũi tên giao lộ • 4 hướng")!=null),row.root.name+": every intersection has four-arm painted arrows");
                var paint=row.tiles[0].Find("Vạch mũi tên giao lộ • 4 hướng");var mesh=paint.GetComponent<MeshFilter>().sharedMesh;
                check(mesh.vertexCount==192&&mesh.triangles.Length==192,row.root.name+": sixteen filled arrow markings face all four approaches");
                check(paint.GetComponent<Renderer>().sharedMaterial.color==StreetArt.Cream,row.root.name+": arrow paint matches crosswalk paint exactly");
                check(row.root.GetComponentsInChildren<TextMesh>(true).Any(t=>t.text=="QUÁN PHỞ")&&row.root.GetComponentsInChildren<TextMesh>(true).Any(t=>t.text.Contains("TẠP HÓA")),row.root.name+": Vietnamese food shops and groceries continue on the new roads");
            }
            check(g.GetComponentsInChildren<BikeVisualAnimator>(true).Length==33&&g.GetComponentsInChildren<CarVisualAnimator>(true).Length==11,"New scenery shares static parked art and preserves all existing active-vehicle controllers");
            check(g.worldTextMaterial!=null&&g.worldTextMaterial.shader.name=="PhoViet/World Street Text","Street labels use depth-tested world text instead of drawing through adjacent buildings");
            check(g.scenery.GetComponentsInChildren<TextMesh>(true).All(t=>t.GetComponent<Renderer>().sharedMaterial==g.worldTextMaterial),"Every branch and main-road sign uses the same depth-tested font atlas");
            g.MoveLane(1);check(g.CurrentLane==3&&g.QueuedTurn==0,"Outside arrow zone right input remains lane movement");
            approach();lane(2);g.MoveLane(-1);check(g.CurrentLane==1&&g.QueuedTurn==0,"Middle lanes cannot choose a junction turn");
            lane(3);g.MoveLane(1);check(g.QueuedTurn==1&&g.CurrentLane==3,"One right input in outer lane queues the upcoming right turn");
            g.MoveLane(1);check(g.QueuedTurn==1&&g.CurrentLane==3,"Repeated input in the chosen direction retains the single queued turn");
            g.MoveLane(-1);check(g.QueuedTurn==0&&g.CurrentLane==2,"Moving away cancels the queued turn");
            foreach(int side in new[]{-1,1})
            {
                clean();approach();int selected=side<0?0:3;lane(selected);string before=g.CurrentStreet,next=g.NextStreet;float distance=g.Distance;
                g.MoveLane(side);check(g.QueuedTurn==side&&!g.IsTurning,"Direction "+side+": input selects without prematurely turning before junction");begin();
                check(g.IsTurning&&g.CurrentLane==selected,"Direction "+side+": automatic curve begins in the selected outer lane");
                for(int i=0;i<15;i++)step(.02f);
                check(Mathf.Sign(g.TurnYaw)==side&&Mathf.Abs(g.TurnYaw)>5&&Mathf.Abs(g.rider.localPosition.z)>.01f,"Direction "+side+": bike follows a real horizontal arc, including longitudinal movement");
                check(g.Speed<=10.001f,"Direction "+side+": corner speed fits the junction radius");
                g.TogglePause();float yaw=g.TurnYaw,dist=g.Distance;Vector3 pose=g.rider.localPosition,cam=g.rideCamera.transform.localPosition;call("Update",null);
                check(g.TurnYaw==yaw&&g.Distance==dist&&g.rider.localPosition==pose&&g.rideCamera.transform.localPosition==cam,"Direction "+side+": pause freezes turn progress and turn camera");g.TogglePause();
                g.MoveLane(-side);check(g.IsTurning&&g.CurrentLane==selected,"Direction "+side+": steering input cannot break the active automatic curve");
                finish();check(g.CompletedTurns==1&&!g.IsTurning,"Direction "+side+": exactly one completed junction turn");
                check(g.CurrentLane==selected&&Mathf.Abs(g.rider.localPosition.x-PhoVietGame.LaneX[selected])<.001f&&Mathf.Abs(g.rider.localPosition.z)<.001f,"Direction "+side+": exit retains the exact outer lane requested by the user");
                check(g.CurrentStreet==next&&g.CurrentStreet!=before&&g.Distance>distance,"Direction "+side+": street name changes while total journey distance continues");
                check(g.NextJunctionDistance>45&&g.NextJunctionDistance<80&&g.blocks.All(b=>Mathf.Abs(b.localPosition.x)<.01f&&Quaternion.Angle(b.localRotation,Quaternion.identity)<.01f),"Direction "+side+": new road remains aligned and contains a next usable junction");
                check(Mathf.Abs(Mathf.DeltaAngle(g.rider.localEulerAngles.y,0))<.01f,"Direction "+side+": normal rear-camera heading restored by rigid rebase");
                float end=g.Distance;step(.05f);check(g.Distance>end&&g.State==PhoVietGame.RideState.Riding,"Direction "+side+": riding continues normally on the new street");
            }
            clean();var names=new HashSet<string>{g.CurrentStreet};
            for(int t=0;t<12;t++)
            {
                approach();int side=t%2==0?-1:1;lane(side<0?0:3);g.MoveLane(side);begin();finish();
                check(names.Add(g.CurrentStreet),"Chained turn "+t+": genuinely new Vietnamese street name");
                check(g.roadRows.Count==5&&g.blocks.Count==7,"Chained turn "+t+": road pool stays bounded instead of allocating an endless city");
                foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("nextIntersectionDistance",float.PositiveInfinity);
            }
            clean();approach();lane(3);g.MoveLane(1);begin();step(.2f);
            var target=g.SpawnTraffic(2,20,0);target.model.localPosition=g.rider.localPosition;target.previousPosition=target.model.localPosition;
            typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,true);Vector3 crash=g.rider.localPosition;
            call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,0f});
            check(g.State==PhoVietGame.RideState.Falling,"During a turn and police pursuit, traffic contact still immediately loses");
            call("Update",null);check(Mathf.Abs(g.rider.localPosition.z-crash.z)<.5f,"Falling during a corner stays at the actual crash location instead of snapping to the straight road");
            clean();approach();lane(0);call("StartChase",null);g.MoveLane(-1);begin();finish();
            check(g.IsChased&&g.State==PhoVietGame.RideState.Riding&&g.CompletedTurns==1,"Police pursuit survives the corner without resetting the run or granting immunity");
            check(g.PoliceDistance>0&&g.PoliceSpeed>=0&&g.policeBike.localPosition.sqrMagnitude<10000,"Police follow the stored curved route with finite physical position");
            clean();approach();lane(3);call("StartChase",null);g.MoveLane(1);begin();step(.15f);
            var blocker=g.SpawnTraffic(2,20,0);blocker.model.localPosition=g.policeBike.localPosition;blocker.previousPosition=blocker.model.localPosition;
            int falls=g.PoliceFalls;call("TickRoutePolice",new object[]{.02f});
            check(g.PoliceFallen&&g.PoliceFalls==falls+1&&Mathf.Abs(g.PoliceFallRemaining-1.2f)<.001f,"Police on a curved route collide physically and fall for exactly 1.2 seconds");
            blocker.active=false;blocker.model.gameObject.SetActive(false);call("TickRoutePolice",new object[]{.6f});call("TickRoutePolice",new object[]{.6f});
            check(!g.PoliceFallen&&g.IsChased,"Curved-route police recover without cancelling the pursuit");
            foreach(int side in new[]{-1,1})foreach(int kind in new[]{0,1,3,26})
            {
                clean();approach();lane(side<0?0:3);g.MoveLane(side);begin();step(.18f);
                var v=g.SpawnTraffic(2,20,kind);v.model.localPosition=g.rider.localPosition;v.previousPosition=v.model.localPosition;typeof(PhoVietGame).GetProperty("IsChased").SetValue(g,true);
                call("ResolveRiderContacts",new object[]{g.rider.localPosition.x,0f});
                check(g.State==PhoVietGame.RideState.Falling,"Curve "+side+" pursuit contact with pool type "+kind+" immediately loses");
            }
            g.ResetRide();check(g.CompletedTurns==0&&!g.IsTurning&&g.QueuedTurn==0&&g.CurrentStreet=="Trần Hưng Đạo"&&g.roadRows[0].root.gameObject.activeSelf,"Restart restores original road, name, controls and branch pool");
            return new{success=true,count=checks.Count,checks};
        }
        finally{g.turnExitTrafficEnabled=exitTraffic;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.ResetRide();PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();Time.timeScale=scale;}
    }
}
