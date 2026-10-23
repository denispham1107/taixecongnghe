// Real WASM crash -> police capture -> jail. Mobile profiles emulate layout, not physical Safari.
const {chromium,devices}=require('playwright');const fs=require('node:fs'),assert=require('node:assert/strict'),sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8765/';const result={url,checks:[],errors:[],captures:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function jailPanel(png){
 const {data,info}=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});const w=info.width,h=info.height,portrait=w<h,s=portrait?w/480:Math.min(w/1440,h/900),uw=w/s,uh=h/s;
 const panel={x:portrait?20:uw-420-42,y:portrait?uh-296-22:(uh-360)/2,w:portrait?440:420};
 const button={x:(panel.x+32)*s,y:(panel.y+(portrait?207:234))*s,w:(panel.w-64)*s,h:42*s};let red=0,n=0;
 for(let y=Math.ceil(button.y);y<Math.floor(button.y+button.h);y++)for(let x=Math.ceil(button.x);x<Math.floor(button.x+button.w);x++){const i=(y*w+x)*info.channels;if(data[i]>180&&data[i+1]<100&&data[i+2]<110)red++;n++;}
 return {visible:n>0&&red/n>.6,redRatio:red/n,width:w,height:h};
}
async function main(){
 const build=await(await fetch(new URL('build-info.json',url))).json();result.build=build;check('Tested WebGL release is 1.33.0',build.version==='1.33.0');
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  for(const [label,options] of [['desktop',{viewport:{width:1024,height:640}}],['phone-portrait',{...devices['Pixel 7'],deviceScaleFactor:1}],['phone-landscape',{...devices['Pixel 7'],deviceScaleFactor:1,viewport:{width:844,height:390}}]]){
   const context=await browser.newContext(options),page=await context.newPage();page.setDefaultTimeout(90000);
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(3000);
   check(label+' Unity WASM initialized',await page.locator('#welcome').isHidden());
   await page.evaluate(()=>{const game='PHỐ VIỆT • Chuyến xe không ngừng';for(const action of ['RestartRide','StartChase','Crash'])window.phoVietInstance.SendMessage(game,action);});
   let found=false,measurement,png;const began=Date.now();
   for(let attempt=0;attempt<9;attempt++){
    await page.waitForTimeout(3500);png=await page.locator('#unity-canvas').screenshot({timeout:90000});measurement=await jailPanel(png);if(measurement.visible){found=true;break;}
   }
   const path=`Captures/crash-police-web-${label}-jail.png`;fs.writeFileSync(path,png);result.captures.push({label,path,elapsedMs:Date.now()-began,...measurement});
   check(label+' pursuit crash physically completes capture and displays the jail replay panel',found);
   await context.close();
  }
  check('No JavaScript or Unity runtime errors',result.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync('Captures/crash-police-web-results.json',JSON.stringify(result,null,2)));
