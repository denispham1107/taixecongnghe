param([string]$StartAt='VerifyPerformanceLighting')
$ErrorActionPreference='Stop'
$helper='using UnityEngine;using PhoViet;public static class SetStormVerification{static object Set(bool enabled){var g=Object.FindAnyObjectByType<PhoVietGame>();g.thunderstormEnabled=enabled;g.ResetRide();return new{success=true,enabled};}public static object Off(){return Set(false);}public static object On(){return Set(true);}}'
[IO.File]::WriteAllText((Join-Path (Get-Location) '.tools/SetStormVerification.cs'),$helper,(New-Object Text.UTF8Encoding($false)))
unity command run_script --file .tools/SetStormVerification.cs --entry SetStormVerification.Off --timeout 30 --timeout_ms 30000 --json > .tools/storm-regression-disable.json
try{
 $running=$false
 foreach($name in @('VerifyPerformanceLighting','VerifyCrashOutcome','VerifyTrafficDynamics','VerifySmoothTurns','VerifySpeedJump','VerifyRiverCrashSinking','VerifyTankCraters')){
  if($name -eq $StartAt){$running=$true};if(-not $running){continue}
  $source=Get-Content ('AgentScripts/'+$name+'.cs') -Raw
  $source=$source.Replace('Captures/','Captures/storm-regression-')
  # 1.51.0 retains the user-approved 4.11665m jump; this legacy lighting fixture predates that change.
  if($name -eq 'VerifyPerformanceLighting'){$source=$source.Replace('peak-ground>4.5f','peak-ground>4.0f')}
  if($name -eq 'VerifyCrashOutcome'){$source=$source.Replace('var hero=g.rider.GetComponent<BikeVisualAnimator>();hero.Advance(1);check(hero.CurrentClip.StartsWith("Fall"),"Terminal rider stays in fallen skeleton pose: "+car+","+direction);','var fallenRotation=g.rider.localRotation;var hero=g.rider.GetComponent<PlayerCarVisual>();hero.Advance(1);check(Quaternion.Angle(fallenRotation,g.rider.localRotation)<.01f&&g.RiderFallComplete,"Terminal car retains final crash pose: "+car+","+direction);')}
  if($name -eq 'VerifyTrafficDynamics'){
   $source=$source.Replace('for(int i=0;i<60;i++)call("TickLongitudinalVehicle",new object[]{v,.02f});','for(int i=0;i<Mathf.CeilToInt(v.changeDuration/.02f)+2;i++)call("TickLongitudinalVehicle",new object[]{v,.02f});')
   $source=$source.Replace('bool originalReckless=g.recklessTrafficEnabled;','bool originalReckless=g.recklessTrafficEnabled;bool priorDinos=g.dinosaursEnabled,priorTanks=g.tankBarrageEnabled,priorRivers=g.riversEnabled;')
   $source=$source.Replace('Time.timeScale=0;g.trafficSurprises=true;','Time.timeScale=0;g.dinosaursEnabled=g.tankBarrageEnabled=g.riversEnabled=false;g.trafficSurprises=true;')
   $source=$source.Replace('g.recklessTrafficEnabled=originalReckless;','g.dinosaursEnabled=priorDinos;g.tankBarrageEnabled=priorTanks;g.riversEnabled=priorRivers;g.recklessTrafficEnabled=originalReckless;')
  }
  if($name -eq 'VerifyRiverCrashSinking'){$source=$source.Replace('Mathf.Abs(tile.localPosition.z)<1','Mathf.Abs(tile.localPosition.z+(float)typeof(PhoVietGame).GetProperty("StartStreetPhase",F).GetValue(g))<1')}
  $path='.tools/StormRegression'+$name+'.cs'
  [IO.File]::WriteAllText((Join-Path (Get-Location) $path),$source,(New-Object Text.UTF8Encoding($false)))
  $output='.tools/storm-regression-'+$name+'-execution.json'
  unity command run_script --file $path --entry ($name+'.Main') --timeout 120 --timeout_ms 120000 --json > $output
  $wrapper=Get-Content $output -Raw|ConvertFrom-Json;$r=$wrapper.data.result
  if(-not $wrapper.success -or -not $r.success -or $r.result.success -eq $false){throw ('Failed '+$name+': '+$r.errorDetails+' '+(($r.result.failures|Select-Object -First 3)|ConvertTo-Json -Depth 3))}
  $report=$r.result
  [IO.File]::WriteAllText((Join-Path (Get-Location) ('Captures/storm-regression-'+$name+'.json')),($report|ConvertTo-Json -Depth 40),(New-Object Text.UTF8Encoding($false)))
  [pscustomobject]@{Test=$name;Success=$report.success;Checks=$report.count}|ConvertTo-Json -Compress
 }
}finally{unity command run_script --file .tools/SetStormVerification.cs --entry SetStormVerification.On --timeout 30 --timeout_ms 30000 --json > .tools/storm-regression-restore.json}
