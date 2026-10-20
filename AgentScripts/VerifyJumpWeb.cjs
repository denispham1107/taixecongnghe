const {chromium}=require('playwright');
const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';
const label=process.argv[3]||'jump';const result={url,checks:[],errors:[]};
function pass(name,ok=true){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
 try{
  for(const mobile of [false,true]){
   const context=await browser.newContext({viewport:mobile?{width:412,height:839}:{width:1440,height:900},isMobile:mobile,hasTouch:mobile,deviceScaleFactor:1});
   const page=await context.newPage();page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
   await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
   pass((mobile?'Mobile':'Desktop')+' real WASM loaded');
   await page.evaluate(()=>{let i=window.phoVietInstance,n='PHỐ VIỆT • Chuyến xe không ngừng';i.SendMessage(n,'RestartRide');i.SendMessage(n,'ShowJobOffers');i.SendMessage(n,'AcceptJob',0);});
   await page.waitForTimeout(250);
   if(mobile){
    const box=await page.locator('#unity-canvas').boundingBox();const x=box.x+box.width*.5,y=box.y+box.height*.78;
    const cdp=await context.newCDPSession(page);
    await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}]});await page.waitForTimeout(80);
    await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x,y:y-box.height*.24,id:0}]});await page.waitForTimeout(80);
    await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});pass('Actual upward touch swipe delivered to WebGL');
   }else{await page.locator('#unity-canvas').focus();await page.keyboard.press('Space');pass('Actual SpaceBar key delivered to WebGL');}
   await page.waitForTimeout(650);await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','TogglePause'));
   await page.screenshot({path:`Captures/${label}-web-${mobile?'mobile':'desktop'}-jump.png`,timeout:90000});
   pass((mobile?'Mobile':'Desktop')+' jump/input smoke has no runtime errors',result.errors.length===0);await context.close();
  }
 }finally{await browser.close();fs.writeFileSync(`Captures/${label}-web-jump-results.json`,JSON.stringify(result,null,2));}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
