"""Merge mobile ETC2 data into the desktop build after verifying shared code."""
import gzip,hashlib,json,re,shutil,sys
from pathlib import Path
mobile=Path(sys.argv[1]);desktop=Path(sys.argv[2])
def one(root,pattern):
 files=list((root/'Build').glob(pattern))
 if len(files)!=1:raise ValueError(f'Expected one {pattern} in {root}: {files}')
 return files[0]
for pattern in ['*.wasm.unityweb']:
 a,b=one(mobile,pattern),one(desktop,pattern)
 if gzip.decompress(a.read_bytes())!=gzip.decompress(b.read_bytes()):raise ValueError(f'Both texture variants must use identical shared code: {pattern}')
fa,fb=one(mobile,'*.framework.js.unityweb'),one(desktop,'*.framework.js.unityweb')
old,new=gzip.decompress(fa.read_bytes()),gzip.decompress(fb.read_bytes())
# The first mobile data build preceded the more conservative growth policy.
# Accept only those two known JS constants; native WASM must match exactly.
normalized=old.replace(b'1+.2/cutDown',b'1+.1/cutDown').replace(b'requestedSize+100663296',b'requestedSize+33554432')
if old!=new and normalized!=new:raise ValueError('Unexpected framework difference between texture variants')
a,b=one(mobile,'*.data.unityweb'),one(desktop,'*.data.unityweb')
if a.name==b.name:raise ValueError('Mobile and desktop data unexpectedly have identical content')
shutil.copy2(a,desktop/'Build'/a.name)
p=desktop/'index.html';html=p.read_text(encoding='utf-8')
tag='mobileDataUrl:"Build/'+a.name+'",'
if 'mobileDataUrl:' in html:html=re.sub(r'mobileDataUrl\s*:\s*"[^"]*"\s*,',tag,html)
else:html=html.replace('window.PHO_VIET_BUILD={','window.PHO_VIET_BUILD={\n      '+tag)
if tag not in html:raise ValueError('Could not add mobile texture selection to the Unity configuration')
p.write_text(html,encoding='utf-8',newline='\n')
print(json.dumps({'mobile':a.name,'mobileBytes':a.stat().st_size,'desktop':b.name,'desktopBytes':b.stat().st_size,'sharedCodeIdentical':True}))
