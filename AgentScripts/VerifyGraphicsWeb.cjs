// Real shipped WebAssembly, pointer input and visible radio state. No debug API.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict'),sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8771/';
const label=process.argv[3]||'release145-graphics';
const report={url,checks:[],errors:[]};
function check(name,ok){assert(ok,name);report.checks.push(name);console.log('PASS '+name);}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--autoplay-policy=no-user-gesture-required']});
 try{
  for(const profile of ['desktop','phone']){
   const context=await browser.newContext(profile==='desktop'?{viewport:{width:1440,height:900}}:{...devices['iPhone 13'],deviceScaleFactor:1});
   const page=await context.newPage();page.setDefaultTimeout(90000);
   page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   const boot=async()=>{await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(5000);};
   await boot();
   async function layout(){
    await page.addStyleTag({content:':root{--safe-top:0px;--safe-right:0px;--safe-bottom:0px;--safe-left:0px}'});
    await page.evaluate(()=>dispatchEvent(new Event('resize')));await page.waitForTimeout(400);
    const b=await page.locator('#unity-canvas').boundingBox(),narrow=b.width<b.height,s=narrow?b.width/480:Math.min(b.width/1440,b.height/900),w=b.width/s,h=b.height/s;
    const card={x:narrow?(w-432)/2:48,y:Math.max(105,Math.min(h*(narrow?.15:.27),Math.max(105,h-(narrow?258:276)-95))),width:narrow?432:360};
    const modal={x:(w-Math.min(narrow?432:560,w-32))/2,y:(h-440)/2,width:Math.min(narrow?432:560,w-32)};
    return {b,s,card,modal};
   }
   let l=await layout();
   async function tap(x,y){const px=l.b.x+x*l.s,py=l.b.y+y*l.s;if(profile==='phone')await page.touchscreen.tap(px,py);else await page.mouse.click(px,py);await page.waitForTimeout(350);}
   const gear=()=>tap(l.card.x+l.card.width-38,l.card.y+30);
   const choice=i=>tap(l.modal.x+100,l.modal.y+118+76*i);
   const done=()=>tap(l.modal.x+100,l.modal.y+396);
   async function selected(expected,name){
    const png=await page.screenshot({timeout:90000});const raw=await sharp(png).removeAlpha().raw().toBuffer({resolveWithObject:true});
    const picked=[];for(let i=0;i<3;i++){
     const x=Math.floor(l.b.x+(l.modal.x+49)*l.s),y=Math.floor(l.b.y+(l.modal.y+118+76*i)*l.s),at=(y*raw.info.width+x)*raw.info.channels;
     const [r,g,b]=raw.data.subarray(at,at+3);if(r<120&&g>r+20&&b>r+20)picked.push(i);
    }
    check(profile+': '+name,picked.length===1&&picked[0]===expected);
   }
   await gear();await selected(0,'gear opens modal with default High');
   await page.locator('#unity-canvas').focus();await page.keyboard.press('Space');await page.waitForTimeout(350);await selected(0,'Space cannot start through modal');
   await page.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','StartRide'));await selected(0,'Start API cannot bypass modal');
   for(const i of [1,2,0,1]){await choice(i);await selected(i,'visible choice '+i);}
   await done();await gear();await selected(1,'Done returns to Ready and gear reopens');
   await page.waitForTimeout(1500);await boot();l=await layout();await gear();await selected(1,'Medium survives page reload');
   if(profile==='phone'){
    await page.setViewportSize({width:844,height:390});l=await layout();await selected(1,'modal and saved level survive landscape rotation');
    await choice(2);await selected(2,'landscape touch chooses Low');
    await page.screenshot({path:`Captures/${label}-phone-landscape.png`,timeout:90000});
    await page.setViewportSize({width:390,height:844});l=await layout();await selected(2,'portrait rotation retains Low');
   }
   await page.screenshot({path:`Captures/${label}-${profile}.png`,timeout:90000});
   await choice(0);await done();await gear();await selected(0,'High restored without starting game');
   await page.locator('#unity-canvas').focus();await page.keyboard.press('Enter');await page.waitForTimeout(350);await gear();await selected(0,'Enter closes without triggering Start');
   await done();await context.close();
  }
  check('No JavaScript, WebGL or Unity runtime errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,2)));
