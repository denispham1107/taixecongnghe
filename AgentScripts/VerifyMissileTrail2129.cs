using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using PhoViet;
public static class VerifyMissileTrail2129
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  float scale=Time.timeScale;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);GameObject root=null;
  try
  {
   Time.timeScale=0;g.RestartRide();root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Helicopter/PV_HelicopterMissile"),g.transform);var visual=root.GetComponent<MissileFlightVisual>();root.transform.localPosition=new Vector3(0,5,20);root.transform.localRotation=Quaternion.identity;
   check(visual.fireTrail!=null&&visual.smoke!=null&&visual.flame!=null,"All three plume layers assigned");
   check(root.GetComponentsInChildren<Light>(true).Length==0&&visual.fireTrail.shadowCastingMode==ShadowCastingMode.Off&&!visual.fireTrail.receiveShadows,"No new lights/shadows");
   check(visual.smoke.main.maxParticles==32&&visual.smoke.main.startLifetime.constant==.6f,"Smoke is bounded at32 and0.6seconds");check(visual.smoke.main.startSize.constantMin>=.24f&&visual.smoke.main.startSize.constantMax>=.34f,"Smoke sprites larger than previous tiny trail");
   foreach(float dt in new[]{1f/120,1f/60,1f/30,.05f})foreach(float speed in new[]{50f,70f,140f})
   {
    visual.End();root.transform.localPosition=new Vector3(0,5,20);root.transform.localRotation=Quaternion.identity;visual.Begin();
    check(visual.FireTrailPointCount==2&&visual.fireTrail.enabled,"Fire visible on the launch frame "+dt+"/"+speed);
    for(int i=0;i<60;i++)
    {root.transform.Rotate(0,dt*40,0);root.transform.position+=root.transform.forward*speed*dt;visual.Advance(dt);}
    int n=visual.fireTrail.positionCount;var points=new Vector3[n];visual.fireTrail.GetPositions(points);float length=0;for(int i=1;i<n;i++)length+=Vector3.Distance(points[i-1],points[i]);
    check(n>1&&n<=32&&length>1&&length<=MissileFlightVisual.FireTrailLength+.001f,"Visible bounded trail at each FPS/speed "+dt+"/"+speed);
    check(Vector3.Distance(points[0],root.transform.TransformPoint(new Vector3(0,0,-.65f)))<.001f,"Fire starts at actual rear nozzle "+dt+"/"+speed);
    check(Vector3.Dot(points[n-1]-points[0],root.transform.forward)<-.5f,"Fire stays behind travelling missile "+dt+"/"+speed);
   }
   visual.smoke.Simulate(.25f,false,false);var particles=new ParticleSystem.Particle[32];int count=visual.smoke.GetParticles(particles);check(count>0&&count<=32,"A live visible smoke plume is emitted");var before=particles.Take(count).Select(p=>p.position).ToArray();
   var positions=new Vector3[visual.fireTrail.positionCount];visual.fireTrail.GetPositions(positions);visual.Scroll(7);var after=new Vector3[positions.Length];visual.fireTrail.GetPositions(after);visual.smoke.GetParticles(particles);
   check(after.Select((p,i)=>Vector3.Distance(p,positions[i]-g.transform.TransformVector(Vector3.forward*7))<.001f).All(b=>b),"All fire history scrolls with the road");check(before.Select((p,i)=>Vector3.Distance(particles[i].position,p-Vector3.forward*7)<.001f).All(b=>b),"All32 smoke slots use the correct shared scroll");
   var block=new MaterialPropertyBlock();visual.fireTrail.GetPropertyBlock(block);float phase=block.GetFloat("_Phase");
   g.TogglePause();var flameScale=visual.flame.localScale;visual.Advance(2);var paused=new Vector3[after.Length];visual.fireTrail.GetPositions(paused);visual.fireTrail.GetPropertyBlock(block);check(paused.SequenceEqual(after)&&visual.flame.localScale==flameScale&&visual.smoke.isPaused,"Pause freezes line, authored core and smoke");check(block.GetFloat("_Phase")==phase,"Pause also freezes flame shader turbulence");g.TogglePause();visual.Advance(.02f);check(visual.smoke.isPlaying,"Resume restarts smoke emission");
   visual.RebaseTrail();check(visual.fireTrail.positionCount==0&&!visual.fireTrail.enabled&&visual.smoke.particleCount==0,"Turn rebase clears old-street fire and smoke");root.transform.Rotate(0,90,0);visual.Advance(.02f);check(visual.fireTrail.enabled&&visual.FireTrailPointCount==2,"New-street plume starts behind new heading");
   visual.End();check(visual.fireTrail.positionCount==0&&!visual.fireTrail.enabled&&!visual.flame.gameObject.activeSelf&&visual.smoke.particleCount==0,"Impact/retire clears all plume layers");
   for(int i=0;i<30;i++){visual.Begin();visual.End();}check(root.GetComponentsInChildren<LineRenderer>(true).Length==1,"Thirty pool reuses create no new renderers");
   typeof(MissileFlightVisual).GetField("fireProperties",F).SetValue(visual,null);visual.Begin();check(visual.fireTrail.enabled,"First launch safely reinitializes renderer properties after reload");visual.End();
   foreach(string array in new[]{"playerMissiles","helicopterMissiles"})
   {
    var pool=(Array)typeof(PhoVietGame).GetField(array,F).GetValue(g);int assigned=0;
    foreach(var missile in pool){var v=(MissileFlightVisual)missile.GetType().GetField("visual").GetValue(missile);if(v.fireTrail!=null&&v.smoke.main.maxParticles==32)assigned++;}
    check(assigned==pool.Length,"Shared upgrade reaches every pooled "+array);
   }
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/trail2129-fx-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);g.ResetRide();typeof(PhoVietGame).GetProperty("BestDistance",F).SetValue(g,best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);Time.timeScale=scale;}
 }
}
