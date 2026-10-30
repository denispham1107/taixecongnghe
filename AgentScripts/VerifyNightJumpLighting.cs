using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using PhoViet;
public static class VerifyNightJumpLighting
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var l=g.daylight;
        float scale=Time.timeScale;bool jobs=g.jobsEnabled,surprises=g.trafficSurprises,reckless=g.recklessTrafficEnabled,exits=g.turnExitTrafficEnabled;
        int coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0),best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0);
        var checks=new List<string>();Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception("FAIL: "+s);checks.Add(s);};
        Action<string,object> set=(n,v)=>typeof(PhoVietGame).GetField(n,F).SetValue(g,v);
        Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
        Action reset=()=>{g.jobsEnabled=false;g.trafficSurprises=false;g.recklessTrafficEnabled=false;g.turnExitTrafficEnabled=false;g.RestartRide();foreach(var v in g.vehicles){v.active=false;v.model.gameObject.SetActive(false);}set("spawnTimer",10000f);set("nextIntersectionDistance",float.PositiveInfinity);l.SetCycleTime(30);};
        try
        {
            Time.timeScale=0;reset();
            check(!ShaderUtil.ShaderHasError(Shader.Find("PhoViet/Optical Light")),"Projection shader compiles without errors");
            check(typeof(DayNightLighting).GetCustomAttribute<DefaultExecutionOrder>().order>0,"Lighting LateUpdate runs after the bike and car pose updates");
            // Many nearby vehicles previously displaced all street lights from the common pool.
            for(int i=0;i<8;i++){int index=g.vehicles.FindIndex(v=>!v.active&&!v.eventOnly);g.SpawnTraffic(i%4,8+i*3,index);}
            l.UpdateLamps();
            var nearest=l.streets.Where(s=>s.source.gameObject.activeInHierarchy).OrderBy(s=>(s.source.position-g.rider.position).sqrMagnitude).Take(2).ToArray();
            for(int i=0;i<2;i++)
            {
                var light=l.lightPool[4+i];
                check(light.enabled&&Vector3.Distance(light.transform.position,nearest[i].source.position)<.001f,"Crowded traffic retains nearest street spotlight "+i);
                check(light.intensity>=3.59f&&light.range>=20&&light.renderMode==LightRenderMode.ForcePixel,"Street spotlight "+i+" is brighter and remains a pixel light");
            }
            var block=new MaterialPropertyBlock();nearest[0].pool.GetPropertyBlock(block);
            check(block.GetColor("_Tint").a>=.43f,"Blender street-light pool has stronger visible illumination");
            check(l.ActiveRealtimeLights==6&&QualitySettings.pixelLightCount<=4,"Light count stays within existing mobile budget under traffic load");
            // Current player is a Supercar; its fixed 4.12 m jump has a small suspension pitch.
            reset();set("timeRiding",100f);var hero=g.rider.GetComponent<PlayerCarVisual>();var binding=l.vehicles.Single(v=>v.player);
            hero.Advance(.01f);l.UpdateLamps();float groundHead=binding.head.position.y,maxHead=groundHead,minAim=1,maxAim=-1,maxFootprint=0;
            check(g.TryJump(),"Nighttime jump starts normally");
            for(int step=0;step<180;step++)
            {
                tick(.01f);hero.Advance(.01f);l.UpdateLamps();
                var head=binding.head;var light=l.lightPool[0];
                if(step%10==0)
                {
                    check(Vector3.Distance(light.transform.position,head.position)<.001f,"Jump frame "+step+": real light follows the animated lamp position");
                    check(Vector3.Angle(light.transform.forward,(head.forward-head.up*.085f).normalized)<.03f,"Jump frame "+step+": spotlight follows wheelie/landing pitch");
                    binding.roadLight.GetPropertyBlock(block);var matrix=block.GetMatrix("_WorldToHead");
                    check(block.GetFloat("_ProjectHeadlight")==1&&matrix.MultiplyPoint3x4(head.position).magnitude<.001f&&Vector3.Distance(matrix.MultiplyPoint3x4(head.position+head.forward),Vector3.forward)<.001f,"Jump frame "+step+": cookie projector uses the same moving lamp frame");
                    check(Mathf.Abs(binding.roadLight.transform.position.y-g.transform.position.y-.035f)<.001f,"Jump frame "+step+": receiver remains on the physical road instead of hovering with the bike");
                }
                maxHead=Mathf.Max(maxHead,head.position.y);minAim=Mathf.Min(minAim,light.transform.forward.y);maxAim=Mathf.Max(maxAim,light.transform.forward.y);
                Vector3 ray=head.TransformDirection(new Vector3(0,-1.1f,13));
                if(ray.y<-.001f){float d=(g.transform.position.y+.035f-head.position.y)/ray.y;maxFootprint=Mathf.Max(maxFootprint,(ray*d).magnitude);}
            }
            check(maxHead-groundHead>PhoVietGame.JumpHeightAtSpeed(17)-.12f,"Lamp rises with the full fixed jump rather than staying at road height");
            check(maxAim-minAim>.03f,"Beam pitch follows car takeoff and landing suspension");
            check(maxFootprint>40,"Aerial/wheelie beam projects beyond the near-road patch and is range-limited");
            check(!g.RiderAirborne&&g.RiderJumpHeight==0&&g.State==PhoVietGame.RideState.Riding,"Landing completes with the original safe jump behavior");
            // A truck roof changes source altitude even when the rider is no longer airborne.
            reset();int truckIndex=g.vehicles.FindIndex(v=>!v.eventOnly&&v.halfLength>3);var truck=g.SpawnTraffic(2,0,truckIndex);
            set("roofVehicle",truck);set("jumpHeight",3.2f);set("jumpPreviousHeight",3.2f);g.rider.localPosition=new Vector3(PhoVietGame.LaneX[2],3.2f,0);
            hero.Advance(.01f);l.UpdateLamps();
            check(g.RiderOnRoof&&binding.head.position.y>3.5f&&l.lightPool[0].transform.position.y>3.5f,"Riding on a truck keeps the actual car headlamp above the roof");
            check(binding.roadLight.transform.localScale.x==64,"Raised roof source has enough receiver extent for a farther footprint");
            l.SetCycleTime(0);check(l.ActiveRealtimeLights==0&&!binding.roadLight.enabled,"Daylight still turns off headlight and street lighting");
            return new{success=true,count=checks.Count,maximumLampHeight=maxHead,minimumBeamY=minAim,maximumBeamY=maxAim,checks};
        }
        finally
        {
            g.jobsEnabled=jobs;g.trafficSurprises=surprises;g.recklessTrafficEnabled=reckless;g.turnExitTrafficEnabled=exits;
            PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.Save();g.ResetRide();Time.timeScale=scale;
        }
    }
}
