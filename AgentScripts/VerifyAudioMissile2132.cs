using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.Audio;using PhoViet;
public static class VerifyAudioMissile2132
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var checks=new List<string>();Action<bool,string> check=(b,s)=>{if(!b)throw new Exception(s);checks.Add(s);};
  object Get(string n)=>typeof(PhoVietGame).GetField(n,F).GetValue(g);
  void Set(string n,object v){var p=typeof(PhoVietGame).GetProperty(n,F);if(p!=null)p.SetValue(g,v);else typeof(PhoVietGame).GetField(n,F).SetValue(g,v);}
  object Call(string n,params object[] a)=>typeof(PhoVietGame).GetMethod(n,F).Invoke(g,a);
  var flags=new[]{g.dinosaursEnabled,g.thunderstormEnabled,g.tankBarrageEnabled,g.riversEnabled,g.recklessTrafficEnabled,g.trafficSurprises,g.turnExitTrafficEnabled};float scale=Time.timeScale;bool sound=g.sound;int best=g.BestDistance,coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);GameObject voice=null;
  try
  {
   Time.timeScale=0;g.RestartRide();g.sound=true;g.dinosaursEnabled=g.thunderstormEnabled=g.tankBarrageEnabled=g.riversEnabled=g.recklessTrafficEnabled=g.trafficSurprises=g.turnExitTrafficEnabled=false;
   var mixer=Resources.Load<AudioMixer>(PhoVietAudio.MixerResource);check(mixer!=null&&mixer.GetFloat(PhoVietAudio.VolumeParameter,out float db)&&Mathf.Abs(db-20*Mathf.Log10(1.3f))<.0001f,"Master bus delivers exactly1.3 linear gain / +2.2788668dB");
   var sources=g.GetComponentsInChildren<AudioSource>(true);check(sources.Length>=9,"Engine, effects, police, rain and five rotors instantiated");
   foreach(var source in sources)check(source.outputAudioMixerGroup!=null&&source.outputAudioMixerGroup.audioMixer==mixer,"Existing game source uses the master bus: "+source.name+" / "+(source.clip!=null?source.clip.name:"one-shots"));
   check(Mathf.Abs(((AudioSource)Get("engine")).volume-.08f*1.7f)<.0001f,"Previous engine cruise/boost balance retained before the master gain");check(Mathf.Abs(((AudioSource)Get("fx")).volume-.3f)<.0001f,"All crash/cannon/lightning/missile one-shots share original FX mix");check(Mathf.Abs(((AudioSource)Get("siren")).volume-.12f)<.0001f,"Police siren keeps original relative volume");
   voice=new GameObject("Inactive navigation routing probe");var nav=voice.AddComponent<AudioSource>();nav.volume=.9f;PhoVietAudio.Route(nav);check(nav.outputAudioMixerGroup.audioMixer==mixer&&nav.volume==.9f,"Navigation can gain30percent without clamping0.9source to1.0");check(!g.jobsEnabled&&!PhoVietGame.RideJobsReleased,"Routing voice never unlocks jobs");
   var volume=nav.volume;for(int i=0;i<20;i++)PhoVietAudio.Route(nav);check(nav.volume==volume&&nav.outputAudioMixerGroup.audioMixer==mixer,"Repeated routing never compounds the30percent gain");
   foreach(float speed in new[]{0f,17f,30f,67.5f})foreach(float dt in new[]{1f/120,1f/60,1f/30,.05f})
   {
    g.RestartRide();g.TryJump();Call("TickJump",.4f);var p=g.rider.localPosition;p.y=g.RiderJumpHeight;g.rider.localPosition=p;g.TryJump();Set("Speed",speed);Call("SpawnHelicopter");
    var h=((Array)Get("helicopters")).GetValue(0);g.HelicopterRoot.localPosition=new Vector3(g.rider.localPosition.x,g.rider.localPosition.y+g.PlayerHeight*.5f,55);
    check((bool)Call("FireHelicopterMissile",h),"Enemy launches while hovering / speed="+speed+" dt="+dt);
    var m=((Array)Get("helicopterMissiles")).GetValue(0);var t=m.GetType();var velocity=(Vector3)t.GetField("velocity").GetValue(m);var root=(Transform)t.GetField("root").GetValue(m);var before=root.localPosition;
    check(Mathf.Abs(velocity.magnitude-40)<.0001f,"Enemy missile is20percent slower:40m/s / speed="+speed+" dt="+dt);
    Call("ScrollHelicopterMissiles",speed*dt);Call("TickHelicopterMissiles",dt,g.rider.localPosition,false);
    check(Vector3.Distance(root.localPosition,before+velocity*dt-Vector3.forward*(speed*dt))<.0001f,"40m/s world travel and player scrolling stay separate");
   }
   g.RestartRide();g.TryJump();Call("TickJump",.4f);var pos=g.rider.localPosition;pos.y=g.RiderJumpHeight;g.rider.localPosition=pos;g.TryJump();check(g.TryFirePlayerMissile(),"Player still fires without target");
   var pm=((Array)Get("playerMissiles")).GetValue(0);float playerSpeed=(float)pm.GetType().GetField("speed").GetValue(pm);check(Mathf.Abs(playerSpeed-Mathf.Max(70,g.Speed+50))<.001f,"Player missile speed unchanged");
   var result=new{success=true,count=checks.Count,checks};File.WriteAllText("Captures/audio2132-gameplay-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{result.success,result.count};
  }
  finally
  {
   if(voice!=null)UnityEngine.Object.DestroyImmediate(voice);g.dinosaursEnabled=flags[0];g.thunderstormEnabled=flags[1];g.tankBarrageEnabled=flags[2];g.riversEnabled=flags[3];g.recklessTrafficEnabled=flags[4];g.trafficSurprises=flags[5];g.turnExitTrafficEnabled=flags[6];g.sound=sound;g.ResetRide();Set("BestDistance",best);PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);Time.timeScale=scale;
  }
 }
}
