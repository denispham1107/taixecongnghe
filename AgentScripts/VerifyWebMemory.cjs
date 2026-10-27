// Measure the shipped WebAssembly heap at full phone DPR, real cache/network
// behavior, repeated games, rotation and background/resume. This is browser
// automation on Windows, not a claim of testing a physical iPhone.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/';
const label=process.argv[3]||'memory144',seconds=Number(process.argv[4]||120);
const report={url,label,checks:[],profiles:[],errors:[]};
function check(name,ok){assert(ok,name);report.checks.push(name);console.log('PASS '+name);}
async function main(){
 let browser;
 try{
  for(const name of ['desktop','iPhone 13 Pro Max'].filter(n=>!process.argv[5]||(process.argv[5]==='phone'?n!=='desktop':n==='desktop'))){
   browser=await chromium.launch({channel:'chrome',headless:true,args:name==='desktop'?[]:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
   const context=await browser.newContext(name==='desktop'?{viewport:{width:1280,height:720}}:{...devices['iPhone 13 Pro Max']});
   const page=await context.newPage();page.setDefaultTimeout(90000);
   const profile={name,samples:[],requests:{},network:{},crashes:0};report.profiles.push(profile);
   page.on('pageerror',e=>report.errors.push({name,error:e.message}));
   page.on('crash',()=>{profile.crashes++;});
   page.on('console',m=>{if(m.type()==='error')report.errors.push({name,error:m.text()});});
   // This context event also observes the worker's own network requests.
   context.on('request',r=>{if(r.url().includes('/Build/'))profile.requests[r.url()]=(profile.requests[r.url()]||0)+1;});
   context.on('response',r=>{if(r.url().includes('/Build/')&&!r.fromServiceWorker())profile.network[r.url()]=(profile.network[r.url()]||0)+1;});
   await page.goto(url,{waitUntil:'networkidle'});
   await page.waitForTimeout(1500);
   check(name+': landing does not preload a second complete Unity build',Object.keys(profile.requests).length===0);
   await page.locator('#play').click();
   await page.waitForFunction(()=>Boolean(window.phoVietInstance),{},{timeout:180000});
   const sample=async phase=>{
    const value=await page.evaluate(()=>{
     const m=window.phoVietInstance.Module;
     const views=Object.values(m).filter(v=>ArrayBuffer.isView(v));
     return {heapBytes:Math.max(...views.map(v=>v.buffer.byteLength)),metrics:window.phoVietInstance.GetMetricsInfo?.(),dpr:m.devicePixelRatio,canvas:{width:document.getElementById('unity-canvas').width,height:document.getElementById('unity-canvas').height},jsBytes:performance.memory?.usedJSHeapSize||null};
    });profile.samples.push({phase,...value});
   };
   await page.waitForTimeout(6000);await sample('ready');
   await page.waitForFunction(()=>document.getElementById('offline-status').textContent.includes('Đã lưu để chơi offline'),{},{timeout:90000});
   check(name+': complete cache confirmed after the game downloads',true);
   check(name+': each runtime resource has only one network response',Object.values(profile.network).every(n=>n===1));
   check(name+': only one texture data variant is downloaded',Object.keys(profile.network).filter(p=>p.includes('.data.unityweb')).length===1);
   const end=Date.now()+seconds*1000;let cycle=0;
   while(Date.now()<end){
    await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','RestartRide'));
    await page.locator('#unity-canvas').focus();
    await page.keyboard.press('Space');await page.keyboard.press('w');
    await page.waitForTimeout(3000);
    await page.keyboard.press(cycle%2?'ArrowLeft':'ArrowRight');await page.keyboard.press('q');
    await page.waitForTimeout(17000);await sample('cycle-'+(++cycle));
   }
   if(name!=='desktop'){
    await page.setViewportSize({width:926,height:428});await page.waitForTimeout(1200);await sample('landscape');
    await page.setViewportSize({width:428,height:926});await page.waitForTimeout(1200);await sample('portrait-again');
    check(name+': phone keeps its full-density framebuffer',profile.samples[0].dpr>2.5&&profile.samples[0].canvas.width>1000);
   }
   await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser'));
   await page.waitForTimeout(1000);await sample('paused');
   const heaps=profile.samples.map(s=>s.heapBytes);
   profile.peakHeap=Math.max(...heaps);
   check(name+': WebAssembly heap stays below 450 MiB',profile.peakHeap<450*1048576);
   check(name+': repeated games do not continually grow the heap',heaps.at(-1)<=heaps[1]+32*1048576);
   check(name+': no renderer crash',profile.crashes===0);
   await page.screenshot({path:`Captures/${label}-${name==='desktop'?'desktop':'iphone'}.png`,timeout:90000});
   // Kill the worker so its in-memory manifest is gone, then reopen offline.
   await context.setOffline(true);
   const cdp=await context.newCDPSession(page);await cdp.send('ServiceWorker.enable');await cdp.send('ServiceWorker.stopAllWorkers');
   await page.reload({waitUntil:'load'});
   await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});
   await page.waitForTimeout(4000);await sample('offline-reload');
   check(name+': cached game launches without network',true);
   await context.close();await browser.close();browser=null;
  }
  check('No WebAssembly, WebGL or JavaScript runtime errors',report.errors.length===0);
  report.success=true;
 }finally{if(browser)await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,2)));
