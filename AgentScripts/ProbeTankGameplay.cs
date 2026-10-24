using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using PhoViet;
// Đo xe tăng trong lượt chơi bình thường (Play Mode): bao lâu thì gặp, xe tăng có hiển thị thật không.
public static class ProbeTankGameplay
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();if(!Application.isPlaying)throw new Exception("Play Mode required");
  float scale=Time.timeScale;int best=PlayerPrefs.GetInt("PhoViet.BestMetres.v1",0),coins=PlayerPrefs.GetInt("PhoViet.Coins.v1",0);
  Action<float> tick=dt=>typeof(PhoVietGame).GetMethod("TickRide",F).Invoke(g,new object[]{dt});
  var seen=new HashSet<PhoVietGame.TrafficVehicle>();int spawns=0,carSlotSpawns=0,tankSpawns=0,restarts=0;float t=0,firstTank=-1;var log=new List<string>();
  var carSlots=new HashSet<PhoVietGame.TrafficVehicle>(g.vehicles.Where(v=>v.model.GetComponent<MeshyCarVariants>()?.variants.Any(x=>x.armored)==true));
  int visibleTankFrames=0;
  try
  {
   Time.timeScale=0;g.RestartRide();
   for(int i=0;i<30*600;i++)
   {
    tick(1/30f);t+=1/30f;
    foreach(var v in g.vehicles)
    {
     if(!v.active){seen.Remove(v);continue;}
     if(seen.Add(v)){spawns++;if(carSlots.Contains(v))carSlotSpawns++;if(v.armored){tankSpawns++;if(firstTank<0)firstTank=t;if(log.Count<12)log.Add($"t={t:0.0} tank lane z={v.model.localPosition.z:0} speed={v.speed:0.0}");}}
     if(v.armored)
     {
      var m=v.model.GetComponent<MeshyCarVariants>();var tank=m.variants.First(x=>x.armored);
      if(tank.skins.Any(r=>r!=null&&r.enabled&&r.gameObject.activeInHierarchy))visibleTankFrames++;
     }
    }
    var state=typeof(PhoVietGame).GetProperty("State").GetValue(g).ToString();
    if(state!="Riding"){restarts++;g.RestartRide();seen.Clear();}
   }
   return new{success=true,seconds=t,spawns,carSlotSpawns,tankSpawns,firstTank,restarts,visibleTankFrames,log=log.ToArray()};
  }
  finally{Time.timeScale=scale;PlayerPrefs.SetInt("PhoViet.BestMetres.v1",best);PlayerPrefs.SetInt("PhoViet.Coins.v1",coins);PlayerPrefs.Save();g.ResetRide();}
 }
}
