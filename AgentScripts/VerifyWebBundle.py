from pathlib import Path
import json,gzip,hashlib,sys
root=Path(sys.argv[1] if len(sys.argv)>1 else 'docs');info=json.loads((root/'build-info.json').read_text(encoding='utf-8'));offline=json.loads((root/'precache.json').read_text(encoding='utf-8'));files=offline['files'];checks=[]
def check(label,value):
 assert value,label
 checks.append(label)
check('Version is 1.50.0',info['version']=='1.50.0')
check('Offline files are unique',len(files)==len(set(files)))
digest=hashlib.sha256()
for f in files:
 p=root/f
 check('Nonempty resource: '+f,p.is_file() and p.stat().st_size>0)
 digest.update(p.read_bytes())
 if '.wasm.' in f:check('Gzip WASM has real WebAssembly header',gzip.decompress(p.read_bytes()).startswith(b'\0asm'))
 if '.data.' in f:check('Gzip data has real UnityWebData header',gzip.decompress(p.read_bytes()).startswith(b'UnityWebData1.0'))
check('Content SHA matches cache ID',digest.hexdigest()[:16]==info['cache'])
check('Worker uses the same cache ID',info['cache'] in (root/'sw.js').read_text(encoding='utf-8'))
runtime=[p for p in (root/'Build').iterdir() if p.is_file() and '.part' not in p.name]
variants=offline.get('dataVariants',[])
check('Both mobile and desktop texture data are published',len(variants)==2 and len(set(variants))==2 and all(v in files and v.endswith('.data.unityweb') for v in variants))
check('Shared loader/framework/WASM and two data variants',len(runtime)==5)
check('Only final runtime assets exist',set(p.name for p in runtime)=={Path(f).name for f in files if f.startswith('Build/')})
html=(root/'index.html').read_text(encoding='utf-8')
check('Device-specific texture selection is configured','mobileDataUrl:' in html and all(Path(f).name in html for f in variants))
if (root/'web-data-parts.json').is_file():
 manifest=json.loads((root/'web-data-parts.json').read_text(encoding='utf-8'));virtual=set()
 for f in manifest['files']:
  virtual.add(f['path']);digest=hashlib.sha256();total=0
  for part in f['parts']:
   data=(root/part['path']).read_bytes();total+=len(data);digest.update(data)
   check('Git-sized part with correct hash: '+part['path'],len(data)==part['bytes'] and len(data)<100_000_000 and hashlib.sha256(data).hexdigest()==part['sha256'])
  check('Parts reconstruct original runtime data: '+f['path'],total==f['bytes'] and digest.hexdigest()==f['sha256'] and hashlib.sha256((root/f['path']).read_bytes()).hexdigest()==f['sha256'])
 check('Other Git resources remain under 100 MB',all((root/f).stat().st_size<100_000_000 for f in files if f not in virtual))
else:
 check('Data larger than Git limit is materialized runtime output',all((root/f).stat().st_size<100_000_000 or '.data.unityweb' in f for f in files))
total=sum((root/f).stat().st_size for f in files)
common=total-sum((root/f).stat().st_size for f in variants)
report={'success':True,'count':len(checks),'checks':checks,'version':info['version'],'cache':info['cache'],'bytes':total,'perDeviceBytes':{f:common+(root/f).stat().st_size for f in variants}}
Path('Captures/release-1.50-bundle-checks.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='checks'}))
