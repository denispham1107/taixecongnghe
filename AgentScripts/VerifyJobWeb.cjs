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
   const context=await browser.newContext(options);const page=await context.newPage();
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   await page.evaluate(()=>{const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'RestartRide');i.SendMessage(n,'ShowJobOffers');});
   await page.waitForTimeout(300);const offers=await page.screenshot({path:`Captures/${label}-${name}-offers.png`,timeout:90000});
   check(name+' real WASM job selection rendered');
   const box=await page.locator('#unity-canvas').boundingBox();const portrait=box.width<box.height;
   const scale=portrait?box.width/480:Math.min(box.width/1440,box.height/900),uiW=box.width/scale,uiH=box.height/scale;
   const w=portrait?448:790,h=portrait?530:475,x=(uiW-w)/2,y=Math.max(125,(uiH-h)/2);
   const px=box.x+(x+w-86.5)*scale,py=box.y+(y+126+14+(portrait?43:35))*scale;
   if(portrait){const cdp=await context.newCDPSession(page);await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:px,y:py,id:0}]});await page.waitForTimeout(250);await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});}else await page.mouse.click(px,py);
   await page.waitForTimeout(250);const gps=await page.screenshot({path:`Captures/${label}-${name}-gps.png`,timeout:90000});
   async function creamCount(png){const {data,info}=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});let count=0;for(let i=0;i<data.length;i+=info.channels)if(Math.abs(data[i]-255)<3&&Math.abs(data[i+1]-241)<3&&Math.abs(data[i+2]-207)<3)count++;return count;}
   check(name+' job accepted through actual canvas button',(await creamCount(gps))<(await creamCount(offers))*.6);
   await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','TogglePause'));
   check(name+' canvas fits available game surface',await page.evaluate(()=>{const r=document.querySelector('canvas').getBoundingClientRect();return r.width>0&&r.height>0&&r.x>=0&&r.y>=0&&r.right<=innerWidth+1&&r.bottom<=innerHeight+1;}));
   if(portrait){await page.setViewportSize({width:844,height:390});await page.waitForTimeout(400);await page.screenshot({path:`Captures/${label}-phone-landscape.png`,timeout:90000});check('GPS and job layout rotation captured');}
   await context.close();
  }
  check('No JavaScript or Unity errors in job UI',result.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync(`Captures/${label}-ui-results.json`,JSON.stringify(result,null,2)));
