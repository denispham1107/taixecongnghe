param([ValidateSet('Inspect','EnablePages','Status')][string]$Mode='Inspect')
$ErrorActionPreference='Stop'
$env:GIT_TERMINAL_PROMPT='0'
$env:GCM_INTERACTIVE='Never'
$credentialLines="protocol=https`nhost=github.com`n`n" | git credential fill
$credential=@{}
foreach($line in $credentialLines){if($line -match '^([^=]+)=(.*)$'){$credential[$matches[1]]=$matches[2]}}
if(-not $credential['password']){throw 'GitHub authentication is required through Git Credential Manager.'}
$headers=@{Authorization=('Bearer '+$credential['password']);Accept='application/vnd.github+json';'X-GitHub-Api-Version'='2022-11-28'}
$repo='https://api.github.com/repos/denispham1107/taixecongnghe'
if($Mode -eq 'Inspect') {
  $result=Invoke-RestMethod $repo -Headers $headers
  [pscustomobject]@{Repository=$result.full_name;Branch=$result.default_branch;Private=$result.private;Permissions=$result.permissions} | ConvertTo-Json -Depth 3
}
if($Mode -eq 'EnablePages') {
  try {$pages=Invoke-RestMethod ($repo+'/pages') -Headers $headers}
  catch {if([int]$_.Exception.Response.StatusCode -ne 404){throw}}
  if($pages){$pages=Invoke-RestMethod ($repo+'/pages') -Method Put -Headers $headers -ContentType 'application/json' -Body '{"build_type":"workflow"}'}
  else {$pages=Invoke-RestMethod ($repo+'/pages') -Method Post -Headers $headers -ContentType 'application/json' -Body '{"build_type":"workflow"}'}
  [pscustomobject]@{Url=$pages.html_url;Status=$pages.status;BuildType=$pages.build_type} | ConvertTo-Json
}
if($Mode -eq 'Status') {
  $runs=Invoke-RestMethod ($repo+'/actions/runs?per_page=3') -Headers $headers
  $runs.workflow_runs | Select-Object id,status,conclusion,head_sha,html_url | ConvertTo-Json
  try {$pages=Invoke-RestMethod ($repo+'/pages') -Headers $headers;$pages | Select-Object html_url,status,build_type,https_enforced | ConvertTo-Json}catch {Write-Output ('Pages HTTP '+[int]$_.Exception.Response.StatusCode)}
}
# Credentials exist only in this process and are never written to project files or output.
