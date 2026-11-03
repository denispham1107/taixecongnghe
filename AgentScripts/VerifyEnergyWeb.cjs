// Real release WASM, actual keyboard/touch input and pixels from the energy HUD.
// TickJump is used only to advance the existing physics clock deterministically
// under software rendering. No debug component or replacement binary is used.
const {chromium}=require('playwright'),sharp=require('sharp'),fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8784/',label=process.argv[3]||'release154-energy';
const report={url,checks:[],errors:[],samples:[]};
function check(name,ok){assert(ok,name);report.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  for(const mobile of (process.argv.includes('--desktop-only')?[false]:[false,true])){
   const profile=mobile?'mobile':'desktop',width=mobile?412:960,height=mobile?839:600;
   const context=await browser.newContext({viewport:{width,height},isMobile:mobile,hasTouch:mobile,deviceScaleFactor:1});
   const page=await context.newPage();page.setDefaultTimeout(90000);
   await page.addInitScript(()=>{window.__releaseFrames=0;const raf=window.requestAnimationFrame;window.requestAnimationFrame=function(cb){return raf.call(this,t=>{cb(t);window.__releaseFrames++;});};});
   page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   const send=async(method,value)=>page.evaluate(([method,value])=>{const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';value===null?i.SendMessage(n,method):i.SendMessage(n,method,value);},[method,value??null]);
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();
   await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   check(profile+': new WebAssembly initializes',await page.locator('#welcome').isHidden());
   const cdp=mobile?await context.newCDPSession(page):null;
   async function frames(n){const at=await page.evaluate(()=>window.__releaseFrames);await page.waitForFunction(at=>window.__releaseFrames>=at,at+n,{timeout:90000});}
   async function jump(){
    if(!mobile){await page.locator('#unity-canvas').focus();await page.keyboard.down('Space');await frames(3);await page.keyboard.up('Space');await frames(2);return;}
    const x=width*.5,y=height*.7,at=Date.now()/1000;
    await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}],timestamp:at});
    await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x,y:y-65,id:0}],timestamp:at+.05});
    await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[],timestamp:at+.08});
    await frames(2);
   }
   async function sample(name){
    await page.waitForTimeout(180);
    const png=await page.screenshot({path:`Captures/${label}-${profile}-${name}.png`,timeout:90000});
    const raw=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});
    const box=await page.locator('#unity-canvas').boundingBox();
    const scale=mobile?box.width/480:Math.min(box.width/1280,box.height/420),left=mobile?225:(box.width/scale-640)/2+214,top=mobile?89.5:67.5,span=mobile?233:114;
    let lit=0,total=0;
    for(let x=Math.ceil(box.x+left*scale);x<Math.floor(box.x+(left+span)*scale);x++){
     const y=Math.floor(box.y+top*scale),i=(y*raw.info.width+x)*raw.info.channels,[r,g,b]=raw.data.subarray(i,i+3);
     total++;if(g>130&&b>180&&b-r>65&&g-r>50)lit++;
    }
    const fill=lit/total;report.samples.push({profile,name,fill});return fill;
   }
   await send('RestartRide');const full=await sample('full');
   check(profile+': full150 energy bar renders',full>.94);
   await jump();await send('TickJump',.45);await jump();
   const hover=await sample('hover');check(profile+': jump and hover spend energy',hover<full-.015&&hover>.5);
   await send('TickJump',1);const drained=await sample('drain');
   check(profile+': holding altitude consumes energy in real WASM',drained<hover-.07&&drained>0);
   if(mobile){
    const x=width*.5,y=height*.62,at=Date.now()/1000;
    for(let tap=0;tap<2;tap++){
     await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}],timestamp:at+tap*.12});
     await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[],timestamp:at+tap*.12+.03});
    }
   }else{await page.keyboard.down('q');await frames(2);await page.keyboard.up('q');}
   await send('TickJump',mobile?.35:.1);await sample('roll');
   await jump();await send('TickJump',.2);await sample('descend');
   check(profile+': roll and manual descent input/render smoke completed',report.errors.length===0);
   await send('RestartRide');const reset=await sample('reset');
   check(profile+': retry restores full energy bar',reset>.94);
   await context.close();
  }
  check('No JavaScript or Unity runtime errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,2)));
