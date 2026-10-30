// Observe actual WebGL draws in the shipped build; no debug component or modified binary.
const {chromium}=require('playwright');const fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8774/';const label=process.argv[3]||'release152-storm';
const report={url,checks:[],errors:[],attempts:[]};const check=(n,b)=>{assert(b,n);report.checks.push(n);};
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  const page=await browser.newPage({viewport:{width:480,height:300},deviceScaleFactor:1});
  page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
  await page.addInitScript(()=>{
   const spy=window.stormDraws={wet:0,modes:[],draws:0,names:[]};
   for(const api of ['WebGLRenderingContext','WebGL2RenderingContext']){
    const proto=window[api]?.prototype;if(!proto)continue;
    const names=new WeakMap(),programs=new WeakMap(),weatherShaders=new WeakSet();let current;
    const wrap=(key,fn)=>{const original=proto[key];if(original)proto[key]=function(...args){return fn.call(this,original,args);};};
    wrap('shaderSource',function(original,args){if(args[1].includes('_Mode')&&args[1].includes('_Progress'))weatherShaders.add(args[0]);return original.apply(this,args);});
    wrap('attachShader',function(original,args){if(weatherShaders.has(args[1]))programs.set(args[0],{weather:true,mode:-1});return original.apply(this,args);});
    wrap('useProgram',function(original,args){current=args[0];return original.apply(this,args);});
    wrap('getUniformLocation',function(original,args){const location=original.apply(this,args);if(location)names.set(location,args[1]);if(/Storm|_Mode|_Progress/.test(args[1])&&!spy.names.includes(args[1]))spy.names.push(args[1]);return location;});
    for(const key of ['uniform1f','uniform1i','uniform1fv'])wrap(key,function(original,args){const n=names.get(args[0])||'',v=key==='uniform1fv'?args[1][args[2]||0]:args[1];if(n.includes('PVStormWetness')||n==='_Storm')spy.wet=Math.max(spy.wet,v);if(n==='_Mode'&&current){const data=programs.get(current);if(data)data.mode=Math.round(v);}return original.apply(this,args);});
    for(const key of ['drawArrays','drawElements','drawArraysInstanced','drawElementsInstanced'])wrap(key,function(original,args){const data=current&&programs.get(current);if(data?.weather&&spy.wet>.1){spy.draws++;if(!spy.modes.includes(data.mode))spy.modes.push(data.mode);}return original.apply(this,args);});
   }
  });
  await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
  check('Real release WASM initializes',await page.locator('#welcome').isHidden());
  // Rendering probe only: advance the existing clock setter to the storm phase.
  // Full 30-second timing/damage/pause rules are covered by the Unity regression suite.
  // Avoid waiting for random traffic to end a ride before 15 seconds of simulation.
  for(let attempt=0;attempt<2;attempt++){
   await page.evaluate(()=>{const game='PHỐ VIỆT • Chuyến xe không ngừng';window.phoVietInstance.SendMessage(game,'RestartRide');window.phoVietInstance.SendMessage(game,'set_StormTime',18.5);window.stormDraws.wet=0;window.stormDraws.modes=[];window.stormDraws.draws=0;});
   const start=Date.now();let snapshot;
   while(Date.now()-start<105000){
    await page.evaluate(()=>{const game='PHỐ VIỆT • Chuyến xe không ngừng';for(const method of ['HideDinosaurs','ClearShells','TryJump'])window.phoVietInstance.SendMessage(game,method);});
    await page.waitForTimeout(250);snapshot=await page.evaluate(()=>({...window.stormDraws}));
    if(report.errors.length)throw new Error(report.errors.join('\n'));
    if(snapshot.wet>.95&&[0,1,2].every(m=>snapshot.modes.includes(m)))break;
   }
   report.attempts.push({seconds:(Date.now()-start)/1000,...snapshot});
   if(snapshot?.wet>.95&&[0,1,2].every(m=>snapshot.modes.includes(m)))break;
  }
  await page.screenshot({path:`Captures/${label}.png`,timeout:90000});const last=report.attempts.at(-1);check('Heavy rain renders in actual WASM at the storm phase',last.wet>.95);
  check('GPU draws rain with the weather shader',last.modes.includes(0));check('GPU draws the pre-strike warning',last.modes.includes(2));check('GPU draws lightning core/glow/branches',last.modes.includes(1));
  await page.screenshot({path:`Captures/${label}.png`,timeout:90000});check('No JavaScript or Unity runtime errors',report.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{report.success=true;console.log(JSON.stringify({success:true,count:report.checks.length}));}).catch(e=>{report.success=false;report.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,2)));
