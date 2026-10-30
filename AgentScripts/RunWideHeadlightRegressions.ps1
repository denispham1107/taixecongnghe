param([switch]$StormOnly)
$ErrorActionPreference='Stop'
$releaseCases=if($StormOnly){@('VerifyThunderstorm')}else{@('VerifyNightJumpLighting','VerifyPerformanceLighting','VerifyThunderstorm')}
foreach($releaseCase in $releaseCases){
 $releaseSource=Get-Content ('AgentScripts/'+$releaseCase+'.cs') -Raw -Encoding utf8
 $releaseSource=$releaseSource.Replace('Captures/','Captures/wide-headlights-regression-')
 # This legacy fixture predates the released, fixed 4.11665 m jump.
 if($releaseCase -eq 'VerifyPerformanceLighting'){$releaseSource=$releaseSource.Replace('peak-ground>4.5f','peak-ground>PhoVietGame.JumpHeightAtSpeed(17)-.12f')}
 $releasePath='.tools/WideHeadlight'+$releaseCase+'.cs'
 [IO.File]::WriteAllText((Join-Path $PWD $releasePath),$releaseSource)
 $releaseOutput='.tools/wide-headlights-'+$releaseCase+'-execution.json'
 unity command run_script --file $releasePath --entry ($releaseCase+'.Main') --timeout 90 --timeout_ms 90000 --json > $releaseOutput
 $releaseWrapped=Get-Content $releaseOutput -Raw|ConvertFrom-Json
 if(-not $releaseWrapped.success -or -not $releaseWrapped.data.result.success -or $releaseWrapped.data.result.result.success -eq $false){throw ('Regression failed: '+$releaseCase+' '+$releaseWrapped.data.result.errorDetails)}
 $releaseResult=$releaseWrapped.data.result.result
 [IO.File]::WriteAllText((Join-Path $PWD ('Captures/wide-headlights-regression-'+$releaseCase+'.json')),($releaseResult|ConvertTo-Json -Depth 30))
 [pscustomobject]@{Test=$releaseCase;success=$releaseResult.success;count=$releaseResult.count}|ConvertTo-Json -Compress
}
