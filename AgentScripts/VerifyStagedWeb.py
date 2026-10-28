"""Verify the Git index will publish the exact locally tested WebGL artifact."""
import hashlib,json,subprocess
from pathlib import Path

def blob(path):
    return subprocess.check_output(['git','cat-file','blob',':docs/'+path])

checks=[]
def check(name,ok):
    if not ok:raise ValueError(name)
    checks.append(name)

parts=json.loads(blob('web-data-parts.json'))['files']
virtual={f['path']:f for f in parts}
manifest=json.loads(blob('precache.json'))
info=json.loads(blob('build-info.json'))
digest=hashlib.sha256()
for name in manifest['files']:
    if name in virtual:
        record=virtual[name];combined=hashlib.sha256();size=0
        for part in record['parts']:
            data=blob(part['path']);combined.update(data);digest.update(data);size+=len(data)
            check('Index part matches SHA: '+part['path'],len(data)==part['bytes'] and hashlib.sha256(data).hexdigest()==part['sha256'])
        check('Index parts reconstruct tested data: '+name,size==record['bytes'] and combined.hexdigest()==record['sha256'] and hashlib.sha256(Path('docs',name).read_bytes()).hexdigest()==combined.hexdigest())
    else:
        data=blob(name);digest.update(data)
        check('Index matches tested file: '+name,data==Path('docs',name).read_bytes())
check('Index content matches release cache',digest.hexdigest()[:16]==info['cache'])
for name in ['sw.js','precache.json','build-info.json','web-data-parts.json']:
    check('Index metadata matches tested file: '+name,blob(name)==Path('docs',name).read_bytes())
check('Two alternative texture data files',len(manifest['dataVariants'])==2)
report=dict(success=True,count=len(checks),version=info['version'],cache=info['cache'],checks=checks)
Path('Captures/release146-staged-checks.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='checks'}))
