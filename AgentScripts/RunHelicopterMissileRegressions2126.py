import json,urllib.request,urllib.error
from pathlib import Path
# Run from project root with the existing Unity instance in Play Mode.
Path('.tools').mkdir(exist_ok=True)
for name in ['VerifyThunderstorm','VerifyCrashDistance']:
 source=Path('AgentScripts/'+name+'.cs').read_text(encoding='utf8').replace('"Captures/','"Captures/missile2126-regression-')
 Path('.tools/missile2126-'+name+'.cs').write_text(source,encoding='utf8')
p=json.loads(Path('Library/Pipeline/.unity-pipeline-port').read_text(encoding='utf-8-sig'));op=urllib.request.build_opener(urllib.request.ProxyHandler({}));summary=[]
for name,file in [('VerifyHelicopterMissiles2126','AgentScripts/VerifyHelicopterMissiles2126.cs'),('VerifyHelicopterFleet2126','AgentScripts/VerifyHelicopterFleet2126.cs'),('VerifyHelicopterJump2126','AgentScripts/VerifyHelicopterJump2126.cs'),('VerifyHelicopterRoutes2126','AgentScripts/VerifyHelicopterRoutes2126.cs'),('VerifyThunderstorm','.tools/missile2126-VerifyThunderstorm.cs'),('VerifyCrashDistance','.tools/missile2126-VerifyCrashDistance.cs')]:
 req=urllib.request.Request('http://127.0.0.1:'+str(p['port'])+'/api/exec',data=json.dumps({'command':'run_script','parameters':{'file':file,'entry':name+'.Main','timeout_ms':90000},'timeout':90000}).encode(),headers={'Authorization':'Bearer '+p['evalToken'],'Content-Type':'application/json'})
 try:
  with op.open(req,timeout=120) as r: result=json.load(r)
 except urllib.error.HTTPError as e: result=json.load(e)
 Path('.tools/missile2126-'+name+'-api.json').write_text(json.dumps(result,indent=2),encoding='utf8')
 data=result.get('result',{});inner=data.get('result',{});ok=result.get('success') and data.get('success') and inner.get('success',False)
 summary.append({'name':name,'success':bool(ok),'count':inner.get('count'),'failure':data.get('errorDetails') or inner.get('failures')});print(json.dumps(summary[-1],ensure_ascii=True),flush=True)
 Path('Captures/missile2126-regression-summary.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
 if not ok:break
