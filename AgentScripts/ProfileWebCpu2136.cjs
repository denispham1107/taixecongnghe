// 21.136: where the WASM main thread goes on a real build: GL call counts per frame and a CPU profile grouped by function.
// node ProfileWebCpu2136.cjs <url> <label>
const {chromium}=require('playwright'),fs=require('node:fs');
const url=process.argv[2]||'http://127.0.0.1:8786/',label=process.argv[3]||'base156';const GAME='PHỐ VIỆT • Chuyến xe không ngừng';
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:false,args:['--use-angle=d3d11','--ignore-gpu-blocklist','--autoplay-policy=no-user-gesture-required','--window-size=900,500','--disable-backgrounding-occluded-windows','--disable-renderer-backgrounding']});
 const ctx=await browser.newContext({viewport:{width:844,height:390},isMobile:true,hasTouch:true,deviceScaleFactor:3,userAgent:'Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36'});
 const page=await ctx.newPage();page.setDefaultTimeout(240000);
 await page.addInitScript(()=>{
  window.__gl={};window.__glFrames=0;
  const P=WebGL2RenderingContext.prototype;
  for(const k of Object.getOwnPropertyNames(P)){const d=Object.getOwnPropertyDescriptor(P,k);if(!d||typeof d.value!=='function')continue;const f=d.value;P[k]=function(...a){window.__gl[k]=(window.__gl[k]||0)+1;return f.apply(this,a);};}
  const raf=window.requestAnimationFrame;let lastT=-1;window.requestAnimationFrame=cb=>raf(t=>{if(t!==lastT){lastT=t;window.__glFrames++;}cb(t);});
 });
 await page.goto(url,{waitUntil:'load'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:240000});await page.waitForTimeout(5000);
 const send=(m,v)=>page.evaluate(([g,m,v])=>{const i=window.phoVietInstance;v===null?i.SendMessage(g,m):i.SendMessage(g,m,v);},[GAME,m,v??null]);
 const cdp=await ctx.newCDPSession(page);await cdp.send('Profiler.enable');await cdp.send('Profiler.setSamplingInterval',{interval:500});
 const report={label,levels:[]};
 await send('SetGraphicsGovernor',0);
 for(const[level,name]of [[2,'Thap'],[1,'TrungBinh'],[0,'Cao']]){
  await send('SetGraphicsQuality',level);await send('SetGraphicsGovernor',0);await send('RestartRide');await page.waitForTimeout(1500);
  await page.evaluate(()=>{window.__gl={};window.__glFrames=0;});await cdp.send('Profiler.start');await page.waitForTimeout(7000);
  const {profile}=await cdp.send('Profiler.stop');
  const counts=await page.evaluate(()=>({frames:window.__glFrames,gl:window.__gl}));
  const per=Object.entries(counts.gl).map(([k,v])=>[k,+(v/Math.max(1,counts.frames)).toFixed(1)]).sort((a,b)=>b[1]-a[1]).slice(0,14);
  const dt=profile.timeDeltas,hit=new Map();profile.samples.forEach((id,i)=>hit.set(id,(hit.get(id)||0)+(dt[i]||0)));
  const by=new Map();let total=0;for(const n of profile.nodes){const t=hit.get(n.id)||0;total+=t;const cf=n.callFrame;let key=cf.functionName||'(anonymous)';if(cf.url&&cf.url.startsWith('wasm'))key='wasm:'+key;by.set(key,(by.get(key)||0)+t);}
  const top=[...by.entries()].sort((a,b)=>b[1]-a[1]).slice(0,12).map(([k,v])=>[k,+(100*v/total).toFixed(1)]);
  const wasm=[...by.entries()].filter(([k])=>k.startsWith('wasm:')).reduce((s,[,v])=>s+v,0);
  const r={name,frames:counts.frames,fps:+(counts.frames/7).toFixed(1),glPerFrame:per,top,wasmPct:+(100*wasm/total).toFixed(1),gpuProcessNote:'idle counts GPU wait'};report.levels.push(r);console.log(JSON.stringify(r));
 }
 fs.writeFileSync(`Captures/perf2136-webcpu-${label}.json`,JSON.stringify(report,null,1));await browser.close();
})().catch(e=>{console.error(e);process.exit(1);});
