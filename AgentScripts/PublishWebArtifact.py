"""Store Unity data in Git-sized parts; restore exact runtime bytes for Pages.

pack leaves the original local file available for browser verification. Only its
parts are committed. materialize reconstructs a clean, ordinary Unity website.
"""
from pathlib import Path, PurePosixPath
import argparse, hashlib, json, shutil

MANIFEST='web-data-parts.json'
PART_SIZE=32*1024*1024

def sha(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for chunk in iter(lambda:f.read(1024*1024),b''):h.update(chunk)
    return h.hexdigest()

def safe(root,relative):
    p=PurePosixPath(relative)
    if p.is_absolute() or '..' in p.parts or '\\' in relative:
        raise ValueError('Invalid artifact path: '+relative)
    path=(root/relative).resolve()
    if root.resolve() not in path.parents:raise ValueError('Path escapes artifact')
    return path

def pack(root):
    files=[]
    for path in sorted((root/'Build').glob('*.data.unityweb')):
        rel=path.relative_to(root).as_posix();parts=[]
        with path.open('rb') as src:
            i=0
            while data:=src.read(PART_SIZE):
                part=rel+'.part'+str(i).zfill(3);target=safe(root,part)
                target.write_bytes(data);parts.append(dict(path=part,bytes=len(data),sha256=hashlib.sha256(data).hexdigest()));i+=1
        files.append(dict(path=rel,bytes=path.stat().st_size,sha256=sha(path),parts=parts))
    if not files:raise ValueError('No built Unity data found')
    report=dict(format=1,files=files)
    (root/MANIFEST).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8',newline='\n')
    return report

def materialize(root,output):
    manifest=json.loads((root/MANIFEST).read_text(encoding='utf-8'))
    if manifest.get('format')!=1:raise ValueError('Unknown artifact parts format')
    if output.resolve()==root.resolve() or root.resolve() in output.resolve().parents:
        raise ValueError('Use a separate output directory outside the source tree')
    if output.exists() and any(output.iterdir()):raise ValueError('Output must be new or empty')
    output.mkdir(parents=True,exist_ok=True)
    virtual={f['path'] for f in manifest['files']}
    parts={p['path'] for f in manifest['files'] for p in f['parts']}
    for path in root.rglob('*'):
        if not path.is_file():continue
        rel=path.relative_to(root).as_posix()
        if rel==MANIFEST or rel in virtual or rel in parts:continue
        target=safe(output,rel);target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(path,target)
    for file in manifest['files']:
        target=safe(output,file['path']);target.parent.mkdir(parents=True,exist_ok=True)
        with target.open('wb') as dst:
            for part in file['parts']:
                src=safe(root,part['path'])
                if src.stat().st_size!=part['bytes'] or sha(src)!=part['sha256']:raise ValueError('Corrupt part: '+part['path'])
                with src.open('rb') as stream:shutil.copyfileobj(stream,dst,1024*1024)
        if target.stat().st_size!=file['bytes'] or sha(target)!=file['sha256']:raise ValueError('Reconstructed data mismatch')
    # The same cache identity must cover the reconstructed runtime files.
    info=json.loads((output/'build-info.json').read_text(encoding='utf-8'))
    cache=json.loads((output/'precache.json').read_text(encoding='utf-8'))['files'];digest=hashlib.sha256()
    for rel in cache:
        with safe(output,rel).open('rb') as stream:
            for chunk in iter(lambda:stream.read(1024*1024),b''):digest.update(chunk)
    if digest.hexdigest()[:16]!=info['cache']:raise ValueError('Reconstructed site cache mismatch')
    return dict(success=True,version=info['version'],cache=info['cache'],restored=len(manifest['files']))

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('action',choices=['pack','materialize']);parser.add_argument('--source',type=Path,default=Path('docs'));parser.add_argument('--output',type=Path,default=Path('_site'))
    args=parser.parse_args();print(json.dumps(pack(args.source) if args.action=='pack' else materialize(args.source,args.output),indent=2))
