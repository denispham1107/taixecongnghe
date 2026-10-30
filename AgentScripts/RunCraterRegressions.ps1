$ErrorActionPreference='Stop'
$names=@('VerifySpeedJump','VerifyLaneResponse','VerifyTankBarrage','VerifySmoothTurns')
foreach($name in $names){
 $source=Get-Content ('AgentScripts/'+$name+'.cs') -Raw
 $source=$source.Replace('Captures/release150-','Captures/crater-regression-')
 $path='.tools/'+$name+'Crater.cs'
 [System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$source,(New-Object System.Text.UTF8Encoding($false)))
 $out='.tools/crater-'+$name+'-execution.json'
 unity command run_script --file $path --entry ($name+'.Main') --timeout 120 --timeout_ms 120000 --json > $out
 $wrapper=Get-Content $out -Raw|ConvertFrom-Json
 $r=$wrapper.data.result
 if(-not $wrapper.success -or -not $r.success -or $r.result.success -eq $false){throw ('Failed '+$name+': '+$r.errorDetails)}
 $report=$r.result
 [System.IO.File]::WriteAllText((Join-Path (Get-Location) ('Captures/crater-regression-'+$name+'.json')),($report|ConvertTo-Json -Depth 40),(New-Object System.Text.UTF8Encoding($false)))
 [pscustomobject]@{Test=$name;Success=$report.success;Checks=$report.count}|ConvertTo-Json -Compress
}
