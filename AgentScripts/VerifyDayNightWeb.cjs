// Real WebGL visual check; mobile emulation does not replace physical Safari testing.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict'),sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8765/';
const label=process.argv[3]||'day-night';
const result={url,checks:[],errors:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function luminance(png){const {data,info}=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});let sum=0,n=0;for(let y=Math.floor(info.height*.55);y<info.height*.8;y+=2)for(let x=Math.floor(info.width*.52);x<info.width*.9;x+=2){let p=(y*info.width+x)*info.channels;sum+=data[p]*.2126+data[p+1]*.7152+data[p+2]*.0722;n++;}return sum/n;}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  for(const [name,options] of [['desktop',{viewport:{width:1440,height:900}}],['phone',{...devices['Pixel 7'],deviceScaleFactor:1}]]){
   const context=await browser.newContext(options),page=await context.newPage();
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   const phase=async seconds=>{await page.evaluate(t=>{const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'ResetRide');i.SendMessage(n,'SetCycleTime',t);},seconds);await page.waitForTimeout(350);};
   await phase(0);const day=await page.screenshot({path:`Captures/${label}-web-${name}-welcome.png`,timeout:90000});
   await phase(30);const night=await page.screenshot({path:`Captures/${label}-web-${name}-night.png`,timeout:90000});
   check(name+' actual WASM changes from daylight to visible nighttime',(await luminance(night))<(await luminance(day))*.65&&(await luminance(night))>10);
   await phase(60);const repeat=await page.screenshot({path:`Captures/${label}-web-${name}-morning-repeat.png`,timeout:90000});
   check(name+' full cycle returns to bright morning',(await luminance(repeat))>(await luminance(night))*1.5);
   check(name+' canvas has no overflow',await page.evaluate(()=>document.documentElement.scrollWidth===innerWidth));
   if(name==='phone'){await page.setViewportSize({width:844,height:390});await phase(0);await page.screenshot({path:`Captures/${label}-web-phone-landscape-welcome.png`,timeout:90000});check('Mobile landscape welcome captured with full viewport canvas',await page.evaluate(()=>{const r=document.querySelector('canvas').getBoundingClientRect();return r.width>r.height&&Math.abs(r.width-innerWidth)<1&&Math.abs(r.height-innerHeight)<1;}));}
   await context.close();
  }
  check('No WebGL shader, JavaScript or Unity runtime errors',result.errors.length===0);
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.error=e.stack;process.exitCode=1;console.error(e);}).finally(()=>fs.writeFileSync(`Captures/${label}-web-visual-results.json`,JSON.stringify(result,null,2)));
