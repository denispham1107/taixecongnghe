// Real-build captures: the resulting HUD/GameOver/Jailed screenshots are reviewed
// visually, in addition to VerifyCrashOutcome's deterministic native assertions.
const {chromium}=require('playwright');const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';const label=process.argv[3]||'crash-outcome';const result={url,checks:[],errors:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{
  for(const mobile of [false,true]){
   const page=await browser.newPage({viewport:mobile?{width:412,height:839}:{width:1280,height:800},isMobile:mobile,hasTouch:mobile,deviceScaleFactor:1});
   page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   check((mobile?'Mobile':'Desktop')+' real engine loaded',await page.locator('#welcome').isHidden());
   const name=mobile?'portrait':'wide';
   for(const phase of ['hud','gameover','jailed']){
    await page.evaluate(phase=>{
     const i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'RestartRide');
     if(phase==='hud'){i.SendMessage(n,'ShowJobOffers');i.SendMessage(n,'AcceptJob',0);return;}
     if(phase==='jailed')i.SendMessage(n,'StartChase');i.SendMessage(n,'Crash');
     for(let step=0;step<300;step++)i.SendMessage(n,'TickRiderCrash',.05);
     if(phase==='jailed')i.SendMessage(n,'TickArrest',1.2);
    },phase);
    await page.waitForTimeout(400);await page.screenshot({path:`Captures/${label}-web-${name}-${phase}.png`,timeout:90000});
    check(`${name} ${phase} capture generated for visual state review`,true);
   }
   check(name+' has no runtime errors',result.errors.length===0);await page.close();
  }
 }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-web-capture-results.json`,JSON.stringify(result,null,2)));
