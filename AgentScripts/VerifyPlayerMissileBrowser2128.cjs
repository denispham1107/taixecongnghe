// DOM-to-Unity routing with a mocked engine. Real weapon physics/HUD pixels are
// verified in Unity separately; this is not a test of a newly built WASM file.
const {chromium}=require('playwright'),fs=require('node:fs'),assert=require('node:assert/strict');
const report={kind:'Mock engine input bridge',checks:[],errors:[]};
const check=(name,b)=>{assert(b,name);report.checks.push(name);};
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  for(const [name,w,h,dpr,touch]of [['phone',390,844,3,true],['short-landscape',568,320,2,true],['tablet',1024,768,2,true],['desktop',1440,900,1,false]]){
   const context=await browser.newContext({viewport:{width:w,height:h},deviceScaleFactor:dpr,hasTouch:touch,isMobile:touch,serviceWorkers:'block'});const page=await context.newPage();page.on('pageerror',e=>report.errors.push(e.message));
   await page.addInitScript(()=>{window.__messages=[];Element.prototype.requestFullscreen=undefined;});
   await page.route('**/Build/*.loader.js',r=>r.fulfill({contentType:'text/javascript',body:'window.createUnityInstance=async(canvas,config,progress)=>{progress(1);return {Module:{devicePixelRatio:config.devicePixelRatio},SendMessage:(root,method,value)=>{window.__messages.push({root,method,value});if(method==="ApplyBrowserInsets")window.__latestInsets=value;}};};'}));
   await page.goto(process.argv[2]||'http://127.0.0.1:8787/',{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance);await page.waitForTimeout(600);
   for(let orientation=0;orientation<2;orientation++){
    if(orientation)await page.setViewportSize({width:h,height:w});await page.evaluate(()=>syncViewport());await page.waitForTimeout(200);
    const state=await page.evaluate(()=>({last:window.__latestInsets,ratio:window.phoVietInstance.Module.devicePixelRatio,r:(()=>{const b=document.querySelector('canvas').getBoundingClientRect();return {x:b.x,y:b.y,width:b.width,height:b.height};})()}));
    const insets=JSON.parse(state.last);
    check(name+orientation+' finite normalized insets', ['left','top','right','bottom'].every(k=>Number.isFinite(insets[k])&&insets[k]>=0&&insets[k]<=.4));
    check(name+orientation+' actual configured render ratio delivered',insets.pixelRatio===state.ratio&&state.ratio>0&&state.ratio<=(touch?3:2));
    check(name+orientation+' graphics pixel budget preserved',state.r.width*state.r.height*state.ratio**2<=(touch?3200000:8300000)+1);
    await page.evaluate(()=>window.__messages=[]);const x=state.r.x+state.r.width*.88,y=state.r.y+state.r.height*.85;
    if(touch)await page.touchscreen.tap(x,y);else await page.mouse.click(x,y);await page.waitForTimeout(100);
    const messages=await page.evaluate(()=>window.__messages),taps=messages.filter(m=>m.method==='ApplyHudTap');check(name+orientation+' one HUD tap per pointer release',taps.length===1);
    const p=JSON.parse(taps[0].value);check(name+orientation+' normalized pointer coordinates',Math.abs(p.x-.88)<.005&&Math.abs(p.y-.85)<.005);
    check(name+orientation+' one begin and one terminal gesture',messages.filter(m=>m.method==='ApplyBrowserGesture').length===2);
   }
   await context.close();
  }
  check('No browser runtime errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>{fs.writeFileSync('Captures/player2128-browser-bridge.json',JSON.stringify(report,null,2));console.log(JSON.stringify({success:report.success,count:report.checks.length}));});
