"""Check Pages reassembly without an original data file, including failure cases."""
from pathlib import Path
import hashlib,importlib.util,json,tempfile

spec=importlib.util.spec_from_file_location('publisher',Path(__file__).with_name('PublishWebArtifact.py'))
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);m.PART_SIZE=64
checks=[]
def check(label,value):
    assert value,label
    checks.append(label)
def fails(fn):
    try:fn()
    except (ValueError,FileNotFoundError):return True
    return False
with tempfile.TemporaryDirectory(prefix='pages-parts-test-',dir='.tools') as tmp:
    root=Path(tmp)
    assert root.resolve().is_relative_to(Path('.tools').resolve()) and root.name.startswith('pages-parts-test-')
    src=root/'docs';(src/'Build').mkdir(parents=True)
    data=b'UnityWebData1.0'+bytes(range(256))*3;path=src/'Build/test.data.unityweb';path.write_bytes(data)
    (src/'index.html').write_text('test');files=['Build/test.data.unityweb','index.html'];h=hashlib.sha256()
    for f in files:h.update((src/f).read_bytes())
    (src/'build-info.json').write_text(json.dumps(dict(version='test',cache=h.hexdigest()[:16])))
    (src/'precache.json').write_text(json.dumps(dict(files=files)))
    manifest=m.pack(src);parts=manifest['files'][0]['parts']
    check('All stored parts obey the configured size',len(parts)>1 and all(p['bytes']<=64 for p in parts))
    path.unlink();out=root/'site';r=m.materialize(src,out)
    check('Rebuild succeeds from parts without local original',r['success'] and (out/'Build/test.data.unityweb').read_bytes()==data)
    check('Runtime site contains no publication helper files',not (out/m.MANIFEST).exists() and not list(out.rglob('*.part*')))
    check('Existing nonempty output is protected',fails(lambda:m.materialize(src,out)))
    check('Paths outside the artifact are rejected',fails(lambda:m.safe(src,'../escape')))
    part=src/parts[0]['path'];original=part.read_bytes();part.write_bytes(bytes([original[0]^1])+original[1:])
    check('Corruption with unchanged size is rejected',fails(lambda:m.materialize(src,root/'corrupt')))
    part.write_bytes(original);part.unlink()
    check('Missing part is rejected',fails(lambda:m.materialize(src,root/'missing')))
report=dict(success=True,count=len(checks),checks=checks)
Path('Captures/release140-parts-checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
