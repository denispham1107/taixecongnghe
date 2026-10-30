import re,json,subprocess
from pathlib import Path
def docs(text):
 pieces=re.split(r'(?=^--- !u!)',text,flags=re.M);out={}
 for p in pieces:
  m=re.match(r'--- !u!(\d+) &(\d+)',p)
  if m:out[int(m[2])]=(int(m[1]),p)
 return out
def fid(s,key):
 m=re.search(r'^  '+re.escape(key)+r': \{fileID: (\d+)\}',s,re.M)
 return int(m[1]) if m else 0
guid=re.search(r'guid: (\w+)',Path('Assets/PhoViet/Scripts/RiverBoat.cs.meta').read_text())[1]
def boat_scope(d):
 monos={i:p for i,(k,p) in d.items() if k==114 and 'guid: '+guid in p}
 assert len(monos)==140
 allowed=set();roots=set();states=[];by_prefab={};by_go={};transform_by_go={};child_refs=set()
 for j,(kind,text) in d.items():
  prefab=fid(text,'m_PrefabInstance');go=fid(text,'m_GameObject')
  if prefab:by_prefab.setdefault(prefab,set()).add(j)
  if go:by_go.setdefault(go,set()).add(j)
  if kind==4:
   transform_by_go[go]=j
   ca=text.find('  m_Children:');cb=text.find('  m_Father:',ca)
   if ca>=0 and cb>=0:child_refs.update(map(int,re.findall(r'fileID: (\d+)',text[ca:cb])))
 for i,p in monos.items():
  go=fid(p,'m_GameObject');allowed.add(i);allowed.add(go);g=d[go][1]
  states.append(p[p.index('  restPosition:'):].strip())
  prefab=fid(g,'m_PrefabInstance')
  if prefab:
   allowed.add(prefab)
   allowed.update(by_prefab.get(prefab,set())|by_go.get(go,set()))
   candidates={j for j in by_prefab.get(prefab,set()) if d[j][0]==4}
   assert len(candidates)==1,(go,candidates)
   roots.update(candidates)
  else:
   queue=[transform_by_go[go]];roots.add(queue[0]);gos=set()
   while queue:
    tr=queue.pop();allowed.add(tr);s=d[tr][1];gos.add(fid(s,'m_GameObject'))
    a=s.find('  m_Children:');b=s.find('  m_Father:',a)
    if a>=0 and b>=0:queue.extend(map(int,re.findall(r'fileID: (\d+)',s[a:b])))
   allowed.update(gos)
   for child in gos:allowed.update(by_go.get(child,set()))
 return allowed,roots,sorted(states)
old=docs(subprocess.check_output(['git','show','2371c13:Assets/PhoViet/Scenes/PhoViet.unity']).decode());new=docs(Path('Assets/PhoViet/Scenes/PhoViet.unity').read_text())
a,ar,states=boat_scope(old);b,br,fresh=boat_scope(new)
assert states==fresh,'Anchoring, yaw or wave phases changed'
def clean(d,skip,roots):
 out={}
 for i,(k,p) in d.items():
  if i in skip:continue
  p=re.sub(r'\{fileID: (\d+)\}',lambda m:'{fileID: BOAT_ROOT}' if int(m[1]) in roots else m[0],p)
  out[i]=(k,p)
 return out
x=clean(old,a,ar);y=clean(new,b,br)
changed=[i for i in x.keys()|y.keys() if x.get(i)!=y.get(i)]
report=dict(success=not changed,nonBoatDocuments=len(x),changedNonBoatDocumentIds=changed,oldBoatDocuments=len(a),newBoatDocuments=len(b),anchoringYawWavePhasesPreserved=states==fresh)
Path('Captures/boat-signal-scene-scope.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report));assert not changed
