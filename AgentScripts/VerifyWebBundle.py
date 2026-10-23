from pathlib import Path
import json,gzip,hashlib
root=Path('docs');info=json.loads((root/'build-info.json').read_text());files=json.loads((root/'precache.json').read_text())['files'];checks=[]
def check(label,value):
 assert value,label
 checks.append(label)
check('Version is 1.32.0',info['version']=='1.32.0')
check('Offline files are unique',len(files)==len(set(files)))
digest=hashlib.sha256()
for f in files:
 p=root/f
 check('Nonempty resource: '+f,p.is_file() and p.stat().st_size>0)
 digest.update(p.read_bytes())
 if '.wasm.' in f:check('Gzip WASM has real WebAssembly header',gzip.decompress(p.read_bytes()).startswith(b'\0asm'))
 if '.data.' in f:check('Gzip data has real UnityWebData header',gzip.decompress(p.read_bytes()).startswith(b'UnityWebData1.0'))
check('Content SHA matches cache ID',digest.hexdigest()[:16]==info['cache'])
check('Worker uses the same cache ID',info['cache'] in (root/'sw.js').read_text())
check('Exactly four loader/framework/WASM/data files',len([p for p in (root/'Build').iterdir() if p.is_file()])==4)
check('Only the final four binary build assets exist',set(p.name for p in (root/'Build').iterdir() if p.is_file())=={Path(f).name for f in files if f.startswith('Build/')})
check('No GitHub file exceeds 100 MB',all((root/f).stat().st_size<100_000_000 for f in files))
report={'success':True,'count':len(checks),'checks':checks,'version':info['version'],'cache':info['cache'],'bytes':sum((root/f).stat().st_size for f in files)}
Path('Captures/riders-release-bundle-checks.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='checks'}))
