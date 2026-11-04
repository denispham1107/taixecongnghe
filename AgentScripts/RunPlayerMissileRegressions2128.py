# Run from the project root with the existing Unity in Play Mode.
import json,urllib.request,urllib.error
from pathlib import Path
Path('.tools').mkdir(exist_ok=True)
for name in ['VerifyHelicopterMissiles2126','VerifyHelicopterFleet2126','VerifyHelicopterJump2126','VerifyHelicopterRoutes2126','VerifyContactFire2127','VerifyCrashDistance']:
 source=Path('AgentScripts/'+name+'.cs').read_text(encoding='utf8').replace('"Captures/','"Captures/player2128-regression-')
 Path('.tools/player2128-'+name+'.cs').write_text(source,encoding='utf8')
p=json.loads(Path('Library/Pipeline/.unity-pipeline-port').read_text(encoding='utf-8-sig'));op=urllib.request.build_opener(urllib.request.ProxyHandler({}));summary=[]
suites=[('VerifyPlayerMissiles2128','AgentScripts/VerifyPlayerMissiles2128.cs')]+[(n,'.tools/player2128-'+n+'.cs') for n in ['VerifyHelicopterMissiles2126','VerifyHelicopterFleet2126','VerifyHelicopterJump2126','VerifyHelicopterRoutes2126','VerifyContactFire2127','VerifyCrashDistance']]
for name,file in suites:
 req=urllib.request.Request('http://127.0.0.1:'+str(p['port'])+'/api/exec',data=json.dumps({'command':'run_script','parameters':{'file':file,'entry':name+'.Main','timeout_ms':90000},'timeout':90000}).encode(),headers={'Authorization':'Bearer '+p['evalToken'],'Content-Type':'application/json'})
 try:
  with op.open(req,timeout=120) as r:result=json.load(r)
 except urllib.error.HTTPError as e:result=json.load(e)
 Path('.tools/player2128-'+name+'-api.json').write_text(json.dumps(result,indent=2),encoding='utf8')
 data=result.get('result',{});inner=data.get('result',{});ok=result.get('success') and data.get('success') and inner.get('success',False)
 item={'name':name,'success':bool(ok),'count':inner.get('count'),'failure':data.get('errorDetails') or inner.get('failures')};summary.append(item);print(json.dumps(item,ensure_ascii=True),flush=True)
 Path('Captures/player2128-regression-summary.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
 if not ok:break
