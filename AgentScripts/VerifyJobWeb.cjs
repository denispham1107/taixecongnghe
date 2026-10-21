const {chromium,devices}=require('playwright');
const fs=require('node:fs');const assert=require('node:assert/strict');const sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';
const label=process.argv[3]||'jobs-web';
const result={url,checks:[],errors:[]};
function check(name,ok=true){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  for(const [name,options] of [['desktop',{viewport:{width:1440,height:900}}],['phone',{...devices['Pixel 7'],deviceScaleFactor:1}]]){
   const context=await browser.newContext(options);const page=await context.newPage();const touchSession=await context.newCDPSession(page);
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   await page.evaluate(()=>{const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'RestartRide');i.SendMessage(n,'ShowJobOffers');});
   await page.waitForTimeout(300);const offers=await page.screenshot({path:`Captures/${label}-${name}-offers.png`,timeout:90000});
   check(name+' real WASM job selection rendered');
   const box=await page.locator('#unity-canvas').boundingBox();const portrait=box.width<box.height;
   const scale=portrait?box.width/480:Math.min(box.width/1440,box.height/900),uiW=box.width/scale,uiH=box.height/scale;
   const w=portrait?448:790,h=portrait?530:475,x=(uiW-w)/2,y=Math.max(125,(uiH-h)/2);
   const px=box.x+(x+w-86.5)*scale,py=box.y+(y+126+14+(portrait?43:35))*scale;
   if(portrait){const cdp=touchSession;await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:px,y:py,id:0}]});await page.waitForTimeout(250);await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});}else await page.mouse.click(px,py);
   await page.waitForTimeout(250);const gps=await page.screenshot({path:`Captures/${label}-${name}-gps.png`,timeout:90000});
   async function creamCount(png){const {data,info}=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});let count=0;for(let i=0;i<data.length;i+=info.channels)if(Math.abs(data[i]-246)<3&&Math.abs(data[i+1]-249)<3&&Math.abs(data[i+2]-254)<3)count++;return count;}
   check(name+' job accepted through actual canvas button',(await creamCount(gps))<(await creamCount(offers))*.6);
   if(portrait){
    const {data,info}=await sharp(gps).removeAlpha().raw().toBuffer({resolveWithObject:true});let ink=0,total=0;
    for(let yy=Math.floor(info.height*.90);yy<info.height*.98;yy++)for(let xx=Math.floor(info.width*.05);xx<info.width*.95;xx++){
     const at=(yy*info.width+xx)*info.channels;total++;
     if(Math.abs(data[at]-32)<3&&Math.abs(data[at+1]-60)<3&&Math.abs(data[at+2]-67)<3)ink++;
    }
    check('Portrait bottom control panels are removed',ink/total<.05);
    const cdp=touchSession,tx=box.x+(uiW-37)*scale,ty=box.y+35*scale;
    await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:tx,y:ty,id:0}]});await page.waitForTimeout(250);
    await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(250);
    const paused=await page.screenshot({path:`Captures/${label}-phone-paused.png`,timeout:90000});
    check('Compact portrait pause button works through real touch input',(await creamCount(paused))>(await creamCount(gps))*2);
   }else{
    const hs=Math.min(box.width/1280,box.height/420),hx=(box.width/hs-1000)/2;
    await page.mouse.click(box.x+(hx+964)*hs,box.y+46*hs);await page.waitForTimeout(250);
    const paused=await page.screenshot({path:`Captures/${label}-desktop-paused.png`,timeout:90000});
    check('Compact landscape pause button works through actual mouse input',(await creamCount(paused))>(await creamCount(gps))*2);
   }
   check(name+' canvas fits available game surface',await page.evaluate(()=>{const r=document.querySelector('canvas').getBoundingClientRect();return r.width>0&&r.height>0&&r.x>=0&&r.y>=0&&r.right<=innerWidth+1&&r.bottom<=innerHeight+1;}));
   if(portrait){
    await page.setViewportSize({width:844,height:390});await page.waitForTimeout(400);
    await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','TogglePause'));await page.waitForTimeout(250);
    const landscape=await page.screenshot({path:`Captures/${label}-phone-landscape.png`,timeout:90000});check('GPS and job layout rotation captured');
    const lb=await page.locator('#unity-canvas').boundingBox(),ls=Math.min(lb.width/1280,lb.height/420),lx=(lb.width/ls-1000)/2;
    await touchSession.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:lb.x+(lx+964)*ls,y:lb.y+46*ls,id:0}]});await page.waitForTimeout(250);
    await touchSession.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(350);
    const paused=await page.screenshot({path:`Captures/${label}-phone-landscape-paused.png`,timeout:90000});
    check('Compact landscape pause button works through real touch after rotation',(await creamCount(paused))>(await creamCount(landscape))*2);
   }
   await context.close();
  }
  check('No JavaScript or Unity errors in job UI',result.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync(`Captures/${label}-ui-results.json`,JSON.stringify(result,null,2)));
