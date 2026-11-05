// 21.140: real WASM: touch the flight button, read its ring colour and the energy bar from the rendered page.
// node VerifyFlightWeb2140.cjs <url> <label>
const {chromium}=require('playwright'),sharp=require('sharp'),fs=require('node:fs');
const url=process.argv[2]||'http://127.0.0.1:8785/',label=process.argv[3]||'flight-web';
const GAME='PHỐ VIỆT • Chuyến xe không ngừng',report={url,label,checks:[],errors:[],samples:[]};
function check(name,ok){if(!ok)throw new Error('FAIL '+name);report.checks.push(name);console.log('PASS '+name);}
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:false,args:['--use-angle=d3d11','--ignore-gpu-blocklist','--autoplay-policy=no-user-gesture-required','--window-size=900,900','--disable-backgrounding-occluded-windows','--disable-renderer-backgrounding']});
 const UA='Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1';
 try{
  for(const[orient,w,h]of [['ngang',844,390],['doc',390,844]]){
   const ctx=await browser.newContext({viewport:{width:w,height:h},isMobile:true,hasTouch:true,deviceScaleFactor:3,userAgent:UA,serviceWorkers:'block'});
   const page=await ctx.newPage();page.setDefaultTimeout(240000);
   page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   await page.goto(url,{waitUntil:'load'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:240000});await page.waitForTimeout(25000);
   const send=(m,v)=>page.evaluate(([g,m,v])=>{const i=window.phoVietInstance;v===null?i.SendMessage(g,m):i.SendMessage(g,m,v);},[GAME,m,v??null]);
   await send('SetGraphicsGovernor',0);
   const cdp=await ctx.newCDPSession(page);
   async function button(){
    // Same layout as PlayerMissileButtonLayout + FlightButtonLayout, with the page's own safe-area free viewport.
    const b=await page.locator('#unity-canvas').boundingBox(),narrow=b.width<b.height,s=narrow?b.width/480:Math.min(b.width/1280,b.height/420),size=Math.max(76*s,56);
    return{x:b.x+18*s,y:b.y+b.height-size-24*s-size-14*s,size};
   }
   async function tap(){const r=await button(),x=r.x+r.size/2,y=r.y+r.size/2;await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}]});await page.waitForTimeout(40);await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});}
   async function ring(name){
    const png=await page.screenshot({path:`Captures/${label}-${orient}-${name}.png`,timeout:90000}),r=await button(),scale=3,cx=(r.x+r.size/2)*scale,cy=(r.y+r.size/2)*scale,rad=r.size/2*scale;
    const meta=await sharp(png).metadata(),f=meta.width/(w*scale);
    const {data,info}=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});let gold=0,blue=0,total=0;
    for(let a=0;a<360;a+=2)for(const k of [.96,.975,.99]){const x=Math.round((cx+Math.cos(a*Math.PI/180)*rad*k)*f),y=Math.round((cy+Math.sin(a*Math.PI/180)*rad*k)*f);if(x<0||y<0||x>=info.width||y>=info.height)continue;const i=(y*info.width+x)*info.channels,R=data[i],G=data[i+1],B=data[i+2];total++;if(R>190&&G>150&&B<150)gold++;if(B>170&&R<130&&G>110)blue++;}
    const sample={orient,name,gold:+(gold/total).toFixed(3),blue:+(blue/total).toFixed(3)};report.samples.push(sample);console.log(JSON.stringify(sample));return sample;
   }
   // Traffic is live: a crash hides the HUD buttons, so a run that ends early is retried (at most three times).
   let done=false;
   for(let attempt=0;attempt<3&&!done;attempt++){
    await send('RestartRide');await page.waitForTimeout(700);
    const ready=await ring('san-sang');
    await tap();await page.waitForTimeout(900);
    const fly=await ring('dang-bay');
    // The ring switches as soon as the press lands; the car then descends on its own.
    await tap();await page.waitForTimeout(250);
    const land=await ring('da-dap');
    if(ready.blue+ready.gold<.1||fly.blue+fly.gold<.1||land.blue+land.gold<.1){report.samples.push({orient,attempt,retried:'button hidden, ride ended'});continue;}
    check(orient+': ready button has the blue ring',ready.blue>ready.gold&&ready.blue>.15);
    check(orient+': one touch lifts off and the ring turns gold',fly.gold>fly.blue&&fly.gold>.15);
    check(orient+': second touch lands and the ring is blue again',land.blue>land.gold&&land.blue>.15);done=true;
   }
   check(orient+': a full press-fly-press-land cycle completed',done);
   await ctx.close();
  }
  check('No JavaScript or Unity runtime errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
})().catch(e=>{report.success=false;report.failure=String(e.stack||e);console.error(e.message);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,1)));
