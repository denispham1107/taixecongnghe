// Exercise the actual release WASM and inspect the rendered ammo count.
const {chromium}=require('playwright'),sharp=require('sharp'),fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8785/',label=process.argv[3]||'release1571-missiles';
const report={url,checks:[],errors:[],samples:[]};
function check(name,ok){assert(ok,name);report.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  for(const mobile of [false,true]){
   const profile=mobile?'mobile':'desktop',width=mobile?412:960,height=mobile?839:600;
   const ctx=await browser.newContext({viewport:{width,height},isMobile:mobile,hasTouch:mobile,deviceScaleFactor:1}),page=await ctx.newPage();page.setDefaultTimeout(90000);
   await page.addInitScript(()=>{
    window.__releaseFrames=0;const raf=requestAnimationFrame;window.requestAnimationFrame=cb=>raf(t=>{cb(t);window.__releaseFrames++;});
    window.__masterGainNodes=[];
    for(const T of [window.AudioContext,window.webkitAudioContext].filter((x,i,a)=>x&&a.indexOf(x)===i)){
     const original=T.prototype.createGain;T.prototype.createGain=function(){const node=original.call(this);window.__masterGainNodes.push(node);return node;};
    }
   });
   page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   const send=(method,value)=>page.evaluate(([m,v])=>{const game='PHỐ VIỆT • Chuyến xe không ngừng',i=window.phoVietInstance;v===null?i.SendMessage(game,m):i.SendMessage(game,m,v);},[method,value??null]);
   async function frames(n){const at=await page.evaluate(()=>window.__releaseFrames);await page.waitForFunction(at=>window.__releaseFrames>=at,at+n,{timeout:90000});}
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(60000);await frames(3);
   check(profile+': release WASM initializes',await page.locator('#welcome').isHidden());
   const touch=mobile?await ctx.newCDPSession(page):null;
   async function jump(){
    if(!mobile){await page.locator('#unity-canvas').focus();await page.keyboard.down('Space');await frames(3);await page.keyboard.up('Space');await frames(1);return;}
    const x=width*.5,y=height*.65,at=Date.now()/1000;
    await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}],timestamp:at});
    await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x,y:y-65,id:0}],timestamp:at+.05});
    await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[],timestamp:at+.08});await frames(2);
   }
   async function button(){
    const b=await page.locator('#unity-canvas').boundingBox(),s=mobile?b.width/480:Math.min(b.width/1280,b.height/420),size=Math.max(76*s,56);
    return{x:b.x+18*s,y:b.y+b.height-size-24*s,size};
   }
   async function sample(name){
    await frames(2);const png=await page.screenshot({path:`Captures/${label}-${profile}-${name}.png`,timeout:90000});const r=await button();
    const crop=await sharp(png).extract({left:Math.round(r.x+r.size*.18),top:Math.round(r.y+r.size*.63),width:Math.floor(r.size*.64),height:Math.floor(r.size*.25)}).removeAlpha().raw().toBuffer({resolveWithObject:true});
    const mask=[];let count=0;for(let i=0;i<crop.data.length;i+=crop.info.channels){const [r,g,b]=crop.data.subarray(i,i+3);const on=r>180&&g>180&&b>145;mask.push(+on);count+=on;}
    report.samples.push({profile,name,whitePixels:count});return {mask,count};
   }
   await send('RestartRide');const full=await sample('full');check(profile+': round ammo3/3 button renders',full.count>8);
   await jump();await jump();await send('TickJump',.35);
   if(mobile){const b=await button(),at=Date.now()/1000;await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:b.x+b.size*.5,y:b.y+b.size*.5,id:0}],timestamp:at});await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[],timestamp:at+.05});await frames(3);}
   else{await page.keyboard.down('f');await frames(3);await page.keyboard.up('f');}
   const shot=await sample('fired');const changed=shot.mask.filter((v,i)=>v!==full.mask[i]).length;
   check(profile+': actual F/touch shot changes rendered ammo digits',shot.count>8&&changed>=2);
   const gains=await page.evaluate(()=>window.__masterGainNodes.map(n=>n.gain.value));report.samples.push({profile,webAudioGains:gains});
   // Unity Web Audio nhân gain mixer vào node của từng nguồn, không tạo node master =1,3: động cơ 0,136 phải thành 0,136×1,56 (21.133, 21.134).
   check(profile+': Web Audio master gain1.56 reaches the engine source (0.136×1.56)',gains.some(v=>Math.abs(v-.136*1.56)<.001));
   await send('RestartRide');const reset=await sample('retry');
   // Nút hơi trong suốt: vạch đường trắng phía sau làm lệch vài điểm ảnh, nên so với độ thay đổi do chính phát bắn tạo ra (21.133).
   const backToFull=reset.mask.filter((v,i)=>v!==full.mask[i]).length;report.samples.push({profile,shotChanged:changed,retryDiffersFromFull:backToFull});
   check(profile+': retry restores original ammo digits ('+backToFull+' vs shot '+changed+')',reset.count>8&&backToFull*2<=changed);
   await ctx.close();
  }
  check('No JavaScript or Unity runtime errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,2)));
