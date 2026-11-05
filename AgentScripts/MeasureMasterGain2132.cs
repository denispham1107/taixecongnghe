using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using PhoViet;
public static class MeasureMasterGain2132
{
 static AudioSource probe;static AudioClip clip;static AudioSource[] paused;static float[] buffer=new float[4096];static int phase;static double next,deadline,baseline;static bool running;static float originalScale;static List<double> energies=new List<double>();static double[] baselineSamples;
 public static object Main()
 {
  if(!Application.isPlaying||running)throw new Exception("Fresh Play fixture required");
  paused=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include).Where(s=>s.isPlaying).ToArray();foreach(var s in paused)s.Pause();
  originalScale=Time.timeScale;Time.timeScale=0;var o=new GameObject("Master gain2132 measurement");probe=o.AddComponent<AudioSource>();probe.playOnAwake=false;probe.spatialBlend=0;probe.loop=true;probe.volume=.2f;
  int rate=AudioSettings.outputSampleRate;float[] wave=new float[rate];for(int i=0;i<wave.Length;i++)wave[i]=.1f*Mathf.Sin(i*2*Mathf.PI*400/rate);clip=AudioClip.Create("Gain reference400Hz",rate,1,rate,false);clip.SetData(wave,0);probe.clip=clip;
  phase=0;energies.Clear();running=true;probe.Play();next=EditorApplication.timeSinceStartup+.5;deadline=next+15;EditorApplication.update+=Update;return new{started=true,measurement="AudioListener output RMS before/after master routing"};
 }
 static void Update()
 {
  if(!running||EditorApplication.timeSinceStartup<next)return;
  try
  {
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("No stable audible listener data from Editor");
   AudioListener.GetOutputData(buffer,0);double energy=0;foreach(float s in buffer)energy+=s*s;energy/=buffer.Length;next=EditorApplication.timeSinceStartup+.025;
   // Editor/driver startup may return empty DSP buffers. Wait for signal and
   // take the median across twelve audible buffers, preserving the raw samples.
   if(energy<.000000000001)return;energies.Add(Math.Sqrt(energy));if(energies.Count<12)return;
   double rms=energies.OrderBy(x=>x).ElementAt(energies.Count/2);
   if(phase==0){baseline=rms;baselineSamples=energies.ToArray();PhoVietAudio.Route(probe);phase=1;energies.Clear();next=EditorApplication.timeSinceStartup+.5;deadline=next+15;return;}
   double ratio=rms/baseline;double expected=PhoVietAudio.MasterGain;bool pass=Math.Abs(ratio-expected)<.03;File.WriteAllText("Captures/audio2132-output-measurement.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=pass,baselineRms=baseline,boostedRms=rms,baselineSamples,boostedSamples=energies.ToArray(),measuredGain=ratio,expectedGain=expected,tolerance=.03,physicalDevice=false},Newtonsoft.Json.Formatting.Indented));Finish();
  }
  catch(Exception e){File.WriteAllText("Captures/audio2132-output-measurement.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=false,error=e.ToString()}));Finish();}
 }
 static void Finish(){running=false;EditorApplication.update-=Update;if(probe!=null){probe.Stop();UnityEngine.Object.DestroyImmediate(probe.gameObject);}if(clip!=null)UnityEngine.Object.DestroyImmediate(clip);foreach(var s in paused)if(s!=null)s.UnPause();Time.timeScale=originalScale;}
}
