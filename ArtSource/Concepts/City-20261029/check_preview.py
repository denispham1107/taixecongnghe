"""File-level approval-preview checks; no Unity mutation, build or web release."""
from pathlib import Path
import json,hashlib,struct,ast
OUT=Path(__file__).resolve().parent
ROOT=OUT.parents[2]
CHECKS=[]
def check(name,passed,detail=None):
    CHECKS.append({'name':name,'passed':bool(passed),'detail':detail})
meta=json.loads((OUT/'study.json').read_text(encoding='utf8'))
approval=json.loads((OUT/'approval.json').read_text(encoding='utf8'))
check('Approval remains pending',approval['approved'] is False and approval['installed_in_unity'] is False)
check('Nineteen building families',len(meta['building_families'])==19)
check('Thirteen standard shops',sum(x['collection'].startswith('PV86_House_') and not x['collection'].endswith('_Corner') for x in meta['building_families'])==13)
check('Two corner houses',sum(x['collection'].endswith('_Corner') for x in meta['building_families'])==2)
check('Four river apartment blocks',sum('River_Apartment' in x['collection'] for x in meta['building_families'])==4)
check('Trees retained',meta['trees']['geometry_unchanged'] and len(meta['trees']['sources'])==3)
for d in meta['building_families']:
    check('Valid dimensions '+d['collection'],0<d['width']<=16.72 and 0<d['depth']<=7.8 and d['height']>0)
for render in meta['renders']:
    p=OUT/(render['name']+'.png')
    if not p.exists():check('Image '+render['name'],False,'Missing');continue
    data=p.read_bytes();size=list(struct.unpack('>II',data[16:24]))
    check('Image '+render['name'],data[:8]==b'\x89PNG\r\n\x1a\n' and size==render['size'] and len(data)>50000,{'size':size,'bytes':len(data)})
for name in ['build_city_study.py','render_city_study.py']:
    ast.parse((OUT/name).read_text(encoding='utf8'));check('Python syntax '+name,True)
old=json.loads((OUT/'protected-game-fingerprint.json').read_text(encoding='utf8'))
changed=[f for f,h in old.items() if not (ROOT/f).exists() or hashlib.sha256((ROOT/f).read_bytes()).hexdigest()!=h]
current={str(p.relative_to(ROOT)).replace('\\','/') for d in ['Assets','ProjectSettings','Packages','docs'] for p in (ROOT/d).rglob('*') if p.is_file()}
added=sorted(current-set(old))
check('Protected game files unchanged',not changed and not added,{'count':len(old),'changed':changed,'added':added})
report={'passed':sum(c['passed'] for c in CHECKS),'failed':sum(not c['passed'] for c in CHECKS),'checks':CHECKS}
(OUT/'preview-checks.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
(OUT/'game-unchanged-checks.json').write_text(json.dumps({'count':len(old),'changed':changed,'added':added},indent=2),encoding='utf8')
print(json.dumps({'passed':report['passed'],'failed':report['failed'],'changedGameFiles':len(changed),'addedGameFiles':len(added)}))
raise SystemExit(1 if report['failed'] else 0)
