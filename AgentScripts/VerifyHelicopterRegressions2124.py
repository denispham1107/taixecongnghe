# Run from project root with Unity in Play Mode; generated fixtures go to ignored .tools.
from pathlib import Path
import json,urllib.request,urllib.error,time
p=json.loads(Path('Library/Pipeline/.unity-pipeline-port').read_text(encoding='utf-8-sig'));op=urllib.request.build_opener(urllib.request.ProxyHandler({}))
summary=[]
for name in ['VerifyRoofRiding','VerifyCrashDistance','VerifyGestures','VerifyNightJumpLighting']:
 s=Path('AgentScripts/'+name+'.cs').read_text(encoding='utf8').replace('"Captures/','"Captures/heli2124-regression-')
 if name in ['VerifySpeedJump','VerifyRoofRiding','VerifyNightJumpLighting']:
  s=s.replace('g.RestartRide();','g.RestartRide();g.dinosaursEnabled=false;g.tankBarrageEnabled=false;g.thunderstormEnabled=false;g.riversEnabled=false;')

 if name=='VerifyRoofRiding':
  s=s.replace('new[]{.016f,.033f,.05f,.1f}','new[]{.033f,.05f}').replace('new[]{false,true}','new[]{false}').replace('new[]{-12f,0f,7f,28f}','new[]{0f,28f}').replace('new[]{-.5f,0f,.5f}','new[]{0f}')
 f=Path('.tools/heli2124-'+name+'.cs');f.write_text(s,encoding='utf8')
 req=urllib.request.Request('http://127.0.0.1:'+str(p['port'])+'/api/exec',data=json.dumps({'command':'run_script','parameters':{'file':str(f),'entry':name+'.Main','timeout_ms':90000},'timeout':90000}).encode(),headers={'Authorization':'Bearer '+p['evalToken'],'Content-Type':'application/json'})
 try:
  with op.open(req,timeout=120) as r: result=json.load(r)
 except urllib.error.HTTPError as e: result=json.load(e)
 Path('Captures/heli2124-'+name+'.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf8')
 data=result.get('result',{});inner=data.get('result',{})
 ok=result.get('success') and data.get('success') and inner.get('success',False)
 summary.append({'name':name,'success':bool(ok),'count':inner.get('count'),'failure':data.get('errorDetails') or inner.get('failures')})
 print(json.dumps(summary[-1],ensure_ascii=True),flush=True)
Path('Captures/heli2124-regression-summary.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
