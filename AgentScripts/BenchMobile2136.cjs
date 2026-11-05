// 21.136: real WASM frame-time bench under phone emulation (DPR3, CPU throttle, real desktop GPU through ANGLE).
// Not a phone: use it to compare builds on the same machine. node BenchMobile2136.cjs <url> <label> [throttle]
const {chromium}=require('playwright'),fs=require('node:fs');
const url=process.argv[2]||'http://127.0.0.1:8786/',label=process.argv[3]||'base',throttle=Number(process.argv[4]||4),fixed=process.argv[5]==='fixed';
const GAME='PHỐ VIỆT • Chuyến xe không ngừng',report={url,label,throttle,errors:[],levels:[]};
function stats(v){const s=[...v].sort((a,b)=>a-b),n=s.length,avg=v.reduce((a,b)=>a+b,0)/Math.max(1,n);return{frames:n,avgFps:+(1000/avg).toFixed(1),avgMs:+avg.toFixed(1),p50:+s[n>>1].toFixed(1),p95:+s[Math.floor(n*.95)].toFixed(1),p99:+s[Math.floor(n*.99)].toFixed(1),max:+s[n-1].toFixed(1),over33:v.filter(x=>x>33.4).length,over50:v.filter(x=>x>50).length,over100:v.filter(x=>x>100).length};}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:false,args:['--use-angle=d3d11','--ignore-gpu-blocklist','--enable-gpu-rasterization','--disable-frame-rate-limit=false','--autoplay-policy=no-user-gesture-required','--window-size=900,500','--disable-backgrounding-occluded-windows','--disable-renderer-backgrounding']});
 try{
  const ctx=await browser.newContext({viewport:{width:844,height:390},isMobile:true,hasTouch:true,deviceScaleFactor:3,userAgent:'Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36'});
  const page=await ctx.newPage();page.setDefaultTimeout(240000);
  await page.addInitScript(()=>{window.__dts=[];let last=0;const tick=t=>{if(last)window.__dts.push(t-last);last=t;requestAnimationFrame(tick);};requestAnimationFrame(tick);});
  page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());else if(m.text().includes('[PhoViet]'))(report.logs=report.logs||[]).push(m.text());});
  await page.goto(url,{waitUntil:'load'});await page.locator('#play').click();
  const t0=Date.now();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:240000});report.loadSeconds=(Date.now()-t0)/1000;await page.waitForTimeout(5000);
  const cdp=await ctx.newCDPSession(page);await cdp.send('Emulation.setCPUThrottlingRate',{rate:throttle});
  const send=(m,v)=>page.evaluate(([g,m,v])=>{const i=window.phoVietInstance;v===null?i.SendMessage(g,m):i.SendMessage(g,m,v);},[GAME,m,v??null]);
  report.gl=await page.evaluate(()=>{const c=document.getElementById('unity-canvas'),gl=window.phoVietInstance.Module.ctx||c.getContext('webgl2'),d=gl.getExtension('WEBGL_debug_renderer_info');return{attrs:gl.getContextAttributes(),renderer:d&&gl.getParameter(d.UNMASKED_RENDERER_WEBGL),width:c.width,height:c.height,dpr:window.phoVietInstance.Module.devicePixelRatio,heapMB:window.phoVietInstance.Module.HEAPU8?window.phoVietInstance.Module.HEAPU8.length/1048576:null};});
  if(fixed)await send('SetGraphicsGovernor',0);
  for(const[level,name]of [[0,'Cao'],[1,'TrungBinh'],[2,'Thap']]){
   await send('SetGraphicsQuality',level);if(fixed)await send('SetGraphicsGovernor',0);await page.waitForTimeout(800);const all=[];
   for(let run=0;run<5;run++){
    await send('RestartRide');await page.waitForTimeout(1500);await page.evaluate(()=>{window.__dts.length=0;});await page.waitForTimeout(9000);all.push(...await page.evaluate(()=>window.__dts));
   }
   const size=await page.evaluate(()=>{const c=document.getElementById('unity-canvas'),m=window.phoVietInstance.Module;return{w:c.width,h:c.height,dpr:m.devicePixelRatio,heapMB:m.HEAPU8?m.HEAPU8.length/1048576:null};});
   const r={level:name,...stats(all),canvas:size.w+'x'+size.h,dpr:size.dpr,heapMB:size.heapMB};report.levels.push(r);console.log(JSON.stringify(r));
  }
  await cdp.send('Emulation.setCPUThrottlingRate',{rate:1});await page.screenshot({path:`Captures/perf2136-${label}.png`,timeout:90000});
 }finally{await browser.close();}
}
main().catch(e=>{report.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/perf2136-bench-${label}.json`,JSON.stringify(report,null,2)));
