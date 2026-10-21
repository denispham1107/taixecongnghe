using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class VerifyDayNight
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;
        var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
        float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
        int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
        Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Func<string,object[],object> call=(n,a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
        Action reset=()=>
        {
            g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();
            foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",100000f);set("nextIntersectionDistance",float.PositiveInfinity);
        };
        try
        {
            Time.timeScale=0;reset();
            check(l!=null&&l.authoredProfile.text.Contains("Blender MCP"),"Runtime loads the lighting profile authored in Blender MCP");
            check(l.streets.Length==70&&l.vehicles.Length==30,"All five streets, the player, all traffic and police have lighting bindings");
            check(l.lightPool.Length==6&&QualitySettings.pixelLightCount<=4,"Real lighting is bounded to six sources with four per-pixel lights");
            check(l.NightLevel==0&&l.LampLevel==0&&l.ActiveRealtimeLights==0,"New run begins in morning with automatic headlights and street lights off");
            check(Mathf.Abs(l.sun.color.r-l.sun.color.g)<.001f&&Mathf.Abs(l.sun.color.g-l.sun.color.b)<.001f,"Morning sun is neutral white instead of warm yellow");
            check(StreetArt.Cream.b>.98f&&StreetArt.Cream.g>.97f,"Road paint and the shared interface white have a neutral/cool tone");
            foreach(var fixture in l.streets)
            {
                var model=fixture.source.parent.Find("Blender • đèn đường LED");var renderer=model.GetComponentInChildren<Renderer>(true);
                check(renderer.bounds.SqrDistance(fixture.source.position)<.01f,"LED optical source stays inside the Blender fixture at "+fixture.source.position);
                check(model.GetComponent<LODGroup>()!=null&&model.GetComponent<LODGroup>().GetLODs().Length==3,"Street fixture has three mobile detail levels at "+fixture.source.position);
            }
            float previous=0;
            for(int second=0;second<=30;second++)
            {
                l.SetCycleTime(second);check(l.NightLevel>=previous-.00001f,"Morning-to-night transition is monotonic at second "+second);previous=l.NightLevel;
            }
            check(l.NightLevel>.9999f&&l.LampLevel==1&&l.sun.intensity<.001f&&l.moon.intensity>.2f,"At 30 seconds night is complete, with moon and automatic lamps on");
            check(l.ActiveRealtimeLights>0&&l.ActiveRealtimeLights<=6,"Night uses a bounded active set, including the player's headlight");
            foreach(var material in l.emissionUsers.SelectMany(r=>r.sharedMaterials).Distinct().Where(m=>m.HasProperty("_EmissionMap")&&m.GetTexture("_EmissionMap")!=null))
                check(material.IsKeywordEnabled("_EMISSION")&&material.GetColor("_EmissionColor").maxColorComponent>1,"Vehicle lenses emit at night: "+material.name);
            for(int second=31;second<=60;second++)
            {l.SetCycleTime(second);check(l.NightLevel<=previous+.00001f,"Night-to-morning transition is monotonic at second "+second);previous=l.NightLevel;}
            check(l.NightLevel<.0001f&&l.LampLevel==0&&l.ActiveRealtimeLights==0,"At 60 seconds the morning returns and lamps switch off");
            foreach(int second in new[]{0,15,30,45})
            {
                l.SetCycleTime(second);float expected=l.NightLevel;l.SetCycleTime(second+60*7);
                check(Mathf.Abs(l.NightLevel-expected)<.0001f,"Cycle repeats continuously after seven full periods at phase "+second);
            }
            l.SetCycleTime(29.999f);float before=l.NightLevel;l.SetCycleTime(30.001f);check(Mathf.Abs(l.NightLevel-before)<.0001f,"No visual step at the nighttime turning point");
            l.SetCycleTime(59.999f);before=l.NightLevel;l.SetCycleTime(.001f);check(Mathf.Abs(l.NightLevel-before)<.0001f,"No visual step when the period wraps back to morning");
            l.SetCycleTime(12);l.Advance(1);check(Mathf.Abs(l.CycleTime-13)<.0001f,"Lighting uses elapsed gameplay seconds, independent of rider speed");
            g.TogglePause();l.Advance(20);check(Mathf.Abs(l.CycleTime-13)<.0001f,"Pause freezes the day/night clock");g.TogglePause();l.Advance(2);check(Mathf.Abs(l.CycleTime-15)<.0001f,"Unpause continues from the existing phase");
            g.jobsEnabled=true;g.ShowJobOffers();l.Advance(10);check(g.JobOffersOpen&&l.CycleTime==15,"Job selection freezes the lighting clock");g.AcceptJob(0);l.Advance(1);check(l.CycleTime==16,"Accepting a job resumes the lighting clock");
            call("BeginService",Array.Empty<object>());l.Advance(1);check(g.ServiceStopped&&l.CycleTime==17,"Passenger service is live gameplay and continues the ambient cycle");
            reset();l.SetCycleTime(30);
            int car=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth>.7f&&v.halfLength<3),bike=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfWidth<.7f);
            var a=g.SpawnTraffic(1,14,car);var b=g.SpawnTraffic(3,22,bike);l.UpdateLamps();
            foreach(var binding in l.vehicles.Where(v=>v.root==a.model||v.root==b.model||v.player))
            {
                check(binding.roadLight.enabled,"Active headlight has road spill: "+binding.root.name);
                check(binding.halos.Any(h=>h.rear)&&binding.halos.Any(h=>!h.rear),"Both headlight and rear lamp are bound: "+binding.root.name);
                check(Mathf.Abs(binding.roadLight.transform.position.y-g.transform.position.y-.035f)<.0001f,"Headlight spill stays on the road surface: "+binding.root.name);
                if(binding.root==b.model){b.model.gameObject.SetActive(false);l.UpdateLamps();check(!binding.roadLight.enabled&&binding.halos.All(h=>!h.glow.enabled),"Pooled inactive traffic leaves no floating lamps");}
            }
            foreach(string file in new[]{"PV_BikeEmission","PV_CarEmission"})
            {
                var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes("Assets/PhoViet/Art/Lighting/"+file+".png"));
                int head=file.Contains("Bike")?20:14,tail=head+1;
                Func<int,Color> pixel=index=>texture.GetPixel((index%8)*16+8,(index/8)*32+16);
                check(pixel(head).r>.8f&&pixel(head).g>.8f&&pixel(head).b>.8f,file+": the front-lamp UV slot is white");
                check(pixel(tail).r>.9f&&pixel(tail).g<.2f&&pixel(tail).b<.2f,file+": the rear-lamp UV slot is red");
                check(pixel(0).maxColorComponent<.01f,file+": paint and clothing do not become emissive");UnityEngine.Object.DestroyImmediate(texture);
            }
            foreach(int lane in new[]{0,1,2,3})
            {
                reset();int guard=0;while(g.NextJunctionDistance>3&&guard++<500)call("TickRide",new object[]{.02f});
                typeof(PhoVietGame).GetProperty("CurrentLane").SetValue(g,lane);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[lane],0,0);g.MoveLane(lane<2?-1:1);
                float delta=(float)typeof(PhoVietGame).GetField("turnLength",F).GetValue(g)+.2f;
                typeof(PhoVietGame).GetProperty("Distance").SetValue(g,g.Distance+delta);call("ScrollRoutes",new object[]{delta});call("BeginOrContinueTurn",new object[]{delta});l.SetCycleTime(30);call("PositionCamera",new object[]{true});
                Vector3 sunView=g.rideCamera.transform.InverseTransformDirection(l.sun.transform.forward),moonView=g.rideCamera.transform.InverseTransformDirection(l.moon.transform.forward);
                call("EndRouteFrame",new object[]{.02f});
                check(Vector3.Angle(sunView,g.rideCamera.transform.InverseTransformDirection(l.sun.transform.forward))<.02f&&Vector3.Angle(moonView,g.rideCamera.transform.InverseTransformDirection(l.moon.transform.forward))<.02f,"Lane "+lane+": sun and moon preserve screen direction across the road rebase");
                check(g.CompletedTurns==1&&l.CycleTime==30&&l.NightLevel>.999f,"Lane "+lane+": turning keeps the current lighting phase");
            }
            reset();l.SetCycleTime(30);g.Crash();float t=l.CycleTime;l.Advance(10);check(g.State==PhoVietGame.RideState.Falling&&l.CycleTime==t,"Crash outcome freezes the current time of day");
            g.ResetRide();check(l.CycleTime==0&&l.NightLevel==0,"Restart restores the bright morning consistently");
            foreach(var size in new[]{new Vector2(480,960),new Vector2(480,720),new Vector2(1440,900),new Vector2(2200,900)})
            {
                bool narrow=size.x<size.y;Rect card=PhoVietGame.WelcomeCardRect(size.x,size.y,narrow),button=PhoVietGame.WelcomeStartRect(card);
                check(card.xMin>=0&&card.yMin>=0&&card.xMax<=size.x&&card.yMax<size.y-50,"Welcome card fits the safe game region at "+size);
                check(card.width*card.height<(narrow?440*350:420*405)*.76f,"Welcome card is at least 24 percent smaller at "+size);
                check(card.Contains(button.min)&&card.Contains(button.max)&&button.height>=44,"Start button stays inside the smaller card at "+size);
            }
            g.ResetRide();var area=(Rect)call("GameGuiArea",null);bool portrait=Screen.width<Screen.height;float ui=portrait?area.width/480:Mathf.Min(area.width/1440,area.height/900);
            Rect start=PhoVietGame.WelcomeStartRect(PhoVietGame.WelcomeCardRect(area.width/ui,area.height/ui,portrait));var centre=start.center;
            string packet=JsonUtility.ToJson(new Tap{x=(area.x+centre.x*ui)/Screen.width,y=(area.y+centre.y*ui)/Screen.height});
            g.ApplyHudTap(packet);check(g.State==PhoVietGame.RideState.Riding,"New welcome button starts the game through the browser touch bridge");
            return new{success=true,count=checks.Count,checks};
        }
        finally
        {
            g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;
            PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
        }
    }
    [Serializable]class Tap{public float x,y;}
}
