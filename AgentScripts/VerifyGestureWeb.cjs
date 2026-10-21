// DOM delivery timing is asserted before finger-up; native VerifyGestures checks
// actual game actions separately. Emulation does not measure physical iOS GPU FPS.
const {chromium}=require('playwright');const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';const label=process.argv[3]||'gesture';const mock=process.argv.includes('--mock');
const result={url,mock,checks:[],errors:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  const context=await browser.newContext({viewport:{width:412,height:839},hasTouch:true,isMobile:true,deviceScaleFactor:1,serviceWorkers:'block'});
  const page=await context.newPage();page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
  if(mock){
   await page.route('**/Build/*.loader.js',r=>r.fulfill({contentType:'text/javascript',body:'window.createUnityInstance=async()=>({SendMessage(){}});'}));
   await page.route('**/app.js',r=>r.fulfill({contentType:'text/javascript',body:fs.readFileSync('Assets/WebGLTemplates/PhoVietPWA/app.js','utf8')}));
  }
  await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});if(!mock)await page.waitForTimeout(4500);
  await page.evaluate(()=>{window.__gestures=[];const instance=window.phoVietInstance,send=instance.SendMessage.bind(instance);instance.SendMessage=(root,method,data)=>{if(method==='ApplyBrowserGesture')window.__gestures.push(JSON.parse(data));return send(root,method,data);};});
  const cdp=await context.newCDPSession(page);
  const touch=(type,points,timestamp)=>cdp.send('Input.dispatchTouchEvent',{type,touchPoints:points,...(timestamp===undefined?{}:{timestamp})});
  for(const [orientation,width,height]of [['portrait',412,839],['landscape',839,412]]){
   await page.setViewportSize({width,height});await page.waitForTimeout(650);
   const box=await page.locator('#unity-canvas').boundingBox();const x=box.x+box.width*.5,y=box.y+box.height*.53;
   if(!mock)await page.evaluate(()=>{const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'RestartRide');i.SendMessage(n,'ShowJobOffers');i.SendMessage(n,'AcceptJob',0);});
   for(const [name,dx,dy,action]of [['left',-36,0,-1],['right',36,0,1],['up',0,-54,2],['down',0,54,3]]){
    await page.evaluate(()=>window.__gestures=[]);
    await touch('touchStart',[{x,y,id:0}]);await touch('touchMove',[{x:x+dx,y:y+dy,id:0}]);
    let calls=await page.evaluate(()=>window.__gestures);
    check(`${orientation} ${name}: short swipe dispatched before finger-up`,calls.filter(p=>p.phase===1&&p.action===action).length===1);
    await touch('touchMove',[{x:x+dx*1.3,y:y+dy*1.3,id:0}]);await touch('touchEnd',[]);
    calls=await page.evaluate(()=>window.__gestures);
    check(`${orientation} ${name}: hold and release never duplicate action`,calls.filter(p=>p.phase===1).length===1&&!calls.some(p=>p.phase===2));
   }
   await page.evaluate(()=>window.__gestures=[]);
   // Software rendering can delay CDP round trips beyond the deliberate 240 ms
   // tap limit. Set actual input timestamps to model a short physical tap.
   const tapAt=Date.now()/1000;
   await touch('touchStart',[{x,y,id:0}],tapAt);await touch('touchMove',[{x:x+4,y:y+3,id:0}],tapAt+.02);await touch('touchEnd',[],tapAt+.04);
   let calls=await page.evaluate(()=>window.__gestures);check(`${orientation}: small finger jitter remains a tap`,calls.some(p=>p.phase===2)&&!calls.some(p=>p.phase===1));
   await page.evaluate(()=>window.__gestures=[]);
   await touch('touchStart',[{x,y,id:0}]);await touch('touchCancel',[]);
   calls=await page.evaluate(()=>window.__gestures);check(`${orientation}: cancellation clears pointer without action`,calls.some(p=>p.phase===3)&&!calls.some(p=>p.phase===1||p.phase===2));
   await page.evaluate(()=>window.__gestures=[]);
   await touch('touchStart',[{x,y,id:0}]);await page.evaluate(()=>dispatchEvent(new Event('orientationchange')));await touch('touchMove',[{x:x+60,y,id:0}]);await touch('touchEnd',[]);
   calls=await page.evaluate(()=>window.__gestures);check(`${orientation}: rotation cancels held gesture`,!calls.some(p=>p.phase===1||p.phase===2));
   // A new touch must work immediately after the canceled one.
   await page.evaluate(()=>window.__gestures=[]);await touch('touchStart',[{x,y,id:0}]);await touch('touchMove',[{x:x+36,y,id:0}]);await touch('touchEnd',[]);
   check(`${orientation}: fresh swipe after rotation is accepted`,(await page.evaluate(()=>window.__gestures)).some(p=>p.phase===1&&p.action===1));
  }
  check('No JavaScript or Unity runtime errors',result.errors.length===0);await context.close();
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-web-gestures-results.json`,JSON.stringify(result,null,2)));
