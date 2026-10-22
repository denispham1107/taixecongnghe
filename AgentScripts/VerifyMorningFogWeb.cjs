// Production WASM/shader checks. Browser mobile emulation is not a physical iOS test.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict'),sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8765/';
const result={url,checks:[],errors:[],measurements:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function pixels(png){return sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});}
function stats(a,b,rect){
 const {width:w,height:h,channels:c}=a.info;let delta=0,lightA=0,lightB=0,n=0,magenta=0;
 for(let y=Math.floor(rect[1]*h);y<Math.floor((rect[1]+rect[3])*h);y+=2)for(let x=Math.floor(rect[0]*w);x<Math.floor((rect[0]+rect[2])*w);x+=2){const i=(y*w+x)*c;for(let k=0;k<3;k++)delta+=Math.abs(a.data[i+k]-b.data[i+k])/3;lightA+=a.data[i]*.2126+a.data[i+1]*.7152+a.data[i+2]*.0722;lightB+=b.data[i]*.2126+b.data[i+1]*.7152+b.data[i+2]*.0722;if(a.data[i]>230&&a.data[i+1]<35&&a.data[i+2]>230)magenta++;n++;}
 return {difference:delta/n,first:lightA/n,second:lightB/n,magenta:magenta/n};
}
async function main(){
 const build=await(await fetch(new URL('build-info.json',url))).json();check('Tested build is version 1.30.6',build.version==='1.30.6');result.build=build;
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  for(const [device,options] of [['desktop',{viewport:{width:1440,height:900}}],['phone',{...devices['Pixel 7'],deviceScaleFactor:1}]]){
   const context=await browser.newContext(options),page=await context.newPage();page.setDefaultTimeout(90000);
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(2500);
   const layouts=device==='phone'?['portrait','landscape']:['wide'];
   for(const layout of layouts){
    if(layout==='landscape')await page.setViewportSize({width:844,height:390});
    const label=device+'-'+layout;
    const capture=async time=>{await page.evaluate(t=>{const n='PHỐ VIỆT • Chuyến xe không ngừng';window.phoVietInstance.SendMessage(n,'ResetRide');window.phoVietInstance.SendMessage(n,'SetCycleTime',t);},time);await page.waitForTimeout(500);return pixels(await page.screenshot({path:`Captures/morning-fog-web-${label}-${time}s.png`,timeout:90000}));};
    const initial=await capture(0),end=await capture(6);
    const far=layout==='portrait'?[.31,.48,.36,.15]:[.44,.25,.25,.25],near=layout==='portrait'?[.1,.80,.8,.15]:[.52,.75,.36,.15];
    const difference=stats(initial,end,far);result.measurements.push({label,...difference});
    check(label+' real WebGL image changes as dawn mist disperses',difference.difference>3&&difference.first>difference.second+2);
    check(label+' fog shader does not render an error color',difference.magenta<.001);
    check(label+' canvas fits its layout without overflow',await page.evaluate(()=>{const r=document.querySelector('canvas').getBoundingClientRect();return Math.abs(r.width-innerWidth)<1&&r.height>0&&r.top>=0&&r.bottom<=innerHeight+1&&document.documentElement.scrollWidth===innerWidth;}));
    if(device==='desktop'){
     const repeated=await capture(60),isolatedFar=stats(initial,repeated,far),isolatedNear=stats(initial,repeated,near);
     result.measurements.push({label:'same-sunlight-depth-comparison',far:isolatedFar,near:isolatedNear});
     check('Same morning illumination at 0/60 seconds confirms actual fog rather than a sun-color change',isolatedFar.difference>3&&isolatedFar.first>isolatedFar.second+2);
     check('WebGL fog has more depth in the distant street than in the foreground',isolatedFar.difference>isolatedNear.difference*1.1);
    }
   }
   await context.close();
  }
  check('No WebGL shader, JavaScript or Unity runtime errors',result.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync('Captures/morning-fog-web-results.json',JSON.stringify(result,null,2)));
