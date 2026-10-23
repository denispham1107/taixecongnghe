// Run against the actual Unity WebGL build at physical phone density, not DPR=1.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/';
const label=process.argv[3]||'mobile-clarity';
const before=process.argv.includes('--before');
const result={url,before,checks:[],frames:[],errors:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  const ctx=await browser.newContext({...devices['iPhone 13'],deviceScaleFactor:3});
  const page=await ctx.newPage();page.setDefaultTimeout(180000);
  page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
  await page.addInitScript(()=>Object.defineProperty(navigator,'standalone',{get:()=>true}));
  await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();
  await page.waitForFunction(()=>!!window.phoVietInstance);await page.waitForTimeout(5000);
  await page.evaluate(()=>{const g='PHỐ VIỆT • Chuyến xe không ngừng';window.phoVietInstance.SendMessage(g,'RestartRide');window.phoVietInstance.SendMessage(g,'PauseForBrowser');});
  for(const [name,width,height]of (before?[['portrait',390,844]]:[['portrait',390,844],['landscape',844,390],['tablet',1024,768],['portrait-again',390,844]])){
   await page.setViewportSize({width,height});
   const ratio=before?1.5:Math.min(3,Math.sqrt(3200000/(width*height)));
   await page.waitForFunction(({width,height,ratio})=>{const c=document.querySelector('canvas'),r=c.getBoundingClientRect();return Math.abs(r.width-width)<1&&Math.abs(r.height-height)<1&&Math.abs(c.width-width*ratio)<3&&Math.abs(c.height-height*ratio)<3;},{width,height,ratio},{timeout:120000});
   const frame=await page.evaluate(()=>{const c=document.querySelector('canvas'),r=c.getBoundingClientRect();return{width:c.width,height:c.height,cssWidth:r.width,cssHeight:r.height,nativeDpr:devicePixelRatio,moduleDpr:window.phoVietInstance.Module.devicePixelRatio};});
   frame.name=name;result.frames.push(frame);
   check(name+': actual WebGL framebuffer matches physical density / budget',Math.abs(frame.width-width*ratio)<3&&Math.abs(frame.height-height*ratio)<3);
   check(name+': Unity Module density agrees after layout',Math.abs(frame.moduleDpr-ratio)<.005);
   if(!before)check(name+': bounded framebuffer',frame.width*frame.height<3210000);
   if(name==='portrait'||name==='landscape'){
    // Capture the actual unobscured rider/road, not a pause card hiding the subject.
    await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','TogglePause'));
    await page.waitForTimeout(400);
    await page.screenshot({path:`Captures/${label}-${name}.png`,timeout:180000});
    await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser'));
   }
  }
  check('No horizontal overflow',await page.evaluate(()=>document.documentElement.scrollWidth===innerWidth));
  check('No runtime errors at real phone density',result.errors.length===0);
  await ctx.close();
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length,frames:result.frames}));}).catch(e=>{result.success=false;result.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-results.json`,JSON.stringify(result,null,2)));
