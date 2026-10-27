const {chromium}=require('playwright');
const http=require('node:http'),fs=require('node:fs'),assert=require('node:assert/strict');
const source=fs.readFileSync('Assets/WebGLTemplates/PhoVietPWA/sw.js','utf8');
const report={checks:[],errors:[]};let version=1,failWasm=true;const hits={};
const html=`<!doctype html><script>
window.statuses=[];navigator.serviceWorker.addEventListener('message',e=>statuses.push(e.data));
navigator.serviceWorker.register('./sw.js').then(()=>navigator.serviceWorker.ready).then(()=>{window.ready=true});
</script><p>Cache integration fixture</p>`;
const server=http.createServer((req,res)=>{
 const file=req.url.split('?')[0];hits[file]=(hits[file]||0)+1;
 res.setHeader('Cache-Control','no-store');
 if(file==='/sw.js'){res.setHeader('Content-Type','application/javascript');res.end(source.replace('__PHOVIET_CACHE__','fixture-'+version));}
 else if(file==='/precache.json'){res.setHeader('Content-Type','application/json');res.end(JSON.stringify({files:['index.html',`Build/game${version}.data.unityweb`,`Build/game${version}.wasm.unityweb`,`Build/alternative${version}.data.unityweb`],dataVariants:[`Build/game${version}.data.unityweb`,`Build/alternative${version}.data.unityweb`]}));}
 else if(file.endsWith('.wasm.unityweb')&&failWasm){res.writeHead(503);res.end('fixture temporary failure');}
 else if(file.startsWith('/Build/')){res.setHeader('Content-Type','application/octet-stream');res.end(Buffer.alloc(1024*1024,version));}
 else if(file==='/'||file==='/index.html'){res.setHeader('Content-Type','text/html');res.end(html);}
 else{res.writeHead(404);res.end();}
});
function check(name,ok){assert(ok,name);report.checks.push(name);console.log('PASS '+name);}
async function poll(page,predicate){
 const end=Date.now()+30000;
 while(Date.now()<end){if(await page.evaluate(predicate))return;await page.waitForTimeout(100);}
 throw new Error('Timed out waiting for service worker state');
}
async function main(){
 await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));const url=`http://127.0.0.1:${server.address().port}/`;
 const browser=await chromium.launch({channel:'chrome',headless:true});
 try{
  const context=await browser.newContext(),page=await context.newPage();page.on('pageerror',e=>report.errors.push(e.message));
  await page.goto(url);await page.waitForFunction(()=>window.ready&&navigator.serviceWorker.controller);
  await page.evaluate(()=>navigator.serviceWorker.controller.postMessage({type:'PHOVIET_CHECK_OFFLINE'}));
  await page.waitForFunction(()=>statuses.length>0);
  check('Landing caches the shell without downloading runtime data',!hits['/Build/game1.data.unityweb']&&!hits['/Build/game1.wasm.unityweb']);
  check('An incomplete cache is never reported ready',await page.evaluate(()=>statuses.every(s=>!s.ready)));
  check('Runtime download succeeds while another file is temporarily unavailable',await page.evaluate(async()=>{
   const data=await fetch('Build/game1.data.unityweb'),wasm=await fetch('Build/game1.wasm.unityweb');return data.status===200&&wasm.status===503;
  }));
  await page.waitForTimeout(300);check('A failed download does not mark the cache complete',await page.evaluate(()=>statuses.every(s=>!s.ready)));
  failWasm=false;
  await page.evaluate(async()=>{const r=await fetch('Build/game1.wasm.unityweb');await r.arrayBuffer();});
  await page.waitForFunction(()=>statuses.some(s=>s.ready));
  check('Cache becomes ready only after every runtime response is stored',true);
  check('No duplicate download of the large data resource',hits['/Build/game1.data.unityweb']===1);
  check('The unused texture variant is not downloaded',!hits['/Build/alternative1.data.unityweb']);
  const beforeOffline=JSON.stringify(hits);
  await context.setOffline(true);const cdp=await context.newCDPSession(page);await cdp.send('ServiceWorker.enable');await cdp.send('ServiceWorker.stopAllWorkers');
  await page.reload({waitUntil:'load'});
  check('Offline shell survives a service worker restart',await page.locator('p').textContent()==='Cache integration fixture');
  check('Offline runtime bytes survive a service worker restart',await page.evaluate(async()=>{
   const a=new Uint8Array(await (await fetch('Build/game1.data.unityweb')).arrayBuffer());return a.length===1024*1024&&a[0]===1&&a.at(-1)===1;
  }));
  await page.evaluate(()=>navigator.serviceWorker.controller.postMessage({type:'PHOVIET_CHECK_OFFLINE'}));
  await page.waitForFunction(()=>statuses.some(s=>s.ready));
  check('Offline readiness uses the saved manifest after worker restart',true);
  check('Offline operation does not issue network requests',JSON.stringify(hits)===beforeOffline);
  await context.setOffline(false);version=2;
  const oldWorker=context.serviceWorkers()[0];
  const oldClosed=new Promise(resolve=>oldWorker.once('close',resolve));
  await page.evaluate(async()=>{const r=await navigator.serviceWorker.getRegistration();await r.update();});
  await poll(page,async()=>Boolean((await navigator.serviceWorker.getRegistration()).waiting));
  check('Updating leaves the current tab on its matching old runtime',await page.evaluate(async()=>new Uint8Array(await (await fetch('Build/game1.data.unityweb')).arrayBuffer())[0]===1));
  const keys=await page.evaluate(()=>caches.keys());check('Both versions coexist until the old tab closes',keys.length===2);
  await page.close();
  // Reopening before the old worker has retired can attach a fresh tab to it,
  // legitimately keeping the update waiting. Let retirement finish first.
  let retirementTimer;
  await Promise.race([oldClosed,new Promise((_,reject)=>{retirementTimer=setTimeout(()=>reject(new Error('Old worker did not retire')),30000);})]);clearTimeout(retirementTimer);
  const next=await context.newPage();await next.goto(url);await next.waitForFunction(()=>window.ready&&navigator.serviceWorker.controller);
  await poll(next,async()=>{const keys=await caches.keys();return keys.length===1&&keys[0].endsWith('fixture-2');});
  report.keysAfterUpdate=await next.evaluate(()=>caches.keys());
  check('Old cache is reclaimed after the old tab closes',report.keysAfterUpdate[0].endsWith('fixture-2'));
  check('No JavaScript runtime errors',report.errors.length===0);report.success=true;await context.close();
 }finally{await browser.close();server.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;console.error(e);process.exitCode=1;server.close();}).finally(()=>fs.writeFileSync('Captures/web-cache-memory-checks.json',JSON.stringify(report,null,2)));
