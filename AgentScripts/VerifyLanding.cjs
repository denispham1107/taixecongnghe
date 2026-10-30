// Layout/accessibility edge cases on the real web shell; gameplay is checked separately.
const {chromium,devices}=require('playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8783/';
const capturePrefix=process.argv[3]||'landing';
const report={url,checks:[],errors:[],layouts:[]};
const check=(name,ok)=>{assert(ok,name);report.checks.push(name);};
async function inspect(page,label,safe){
 await page.locator('.night-preview img').scrollIntoViewIfNeeded();
 await page.waitForFunction(()=>document.querySelector('.night-preview img').complete&&document.querySelector('.night-preview img').naturalWidth>=1280);
 await page.locator('#welcome').evaluate(e=>{e.scrollTop=0;});
 const data=await page.evaluate(()=>{
  const rect=s=>{const e=document.querySelector(s),r=e.getBoundingClientRect();return {x:r.x,y:r.y,right:r.right,bottom:r.bottom,width:r.width,height:r.height};};
  const pairs=['.intro','.street-preview','.street-gallery','.night-preview','#welcome footer','.primary','#load-status','.controls','.touch-guide'];
  return {width:innerWidth,height:innerHeight,bodyOverflow:document.documentElement.scrollWidth>innerWidth+1,welcomeOverflow:document.querySelector('#welcome').scrollWidth>document.querySelector('#welcome').clientWidth+1,
   boxes:Object.fromEntries(pairs.map(s=>[s,rect(s)])),targets:[...document.querySelectorAll('#toolbar a,#toolbar button,.play-panel button')].filter(e=>e.getClientRects().length).map(e=>({id:e.id||'brand',...rect(e.id?'#'+e.id:'.wordmark')})),
   texts:[...document.querySelectorAll('.intro h1,.description,#load-status,.touch-guide,.preview-label,footer,nav')].every(e=>e.scrollWidth<=e.clientWidth+1),
   image:{src:document.querySelector('.street-preview img').currentSrc,width:document.querySelector('.street-preview img').naturalWidth},night:{src:document.querySelector('.night-preview img').currentSrc,width:document.querySelector('.night-preview img').naturalWidth}};
 });
 const overlap=(a,b)=>Math.min(a.right,b.right)-Math.max(a.x,b.x)>1&&Math.min(a.bottom,b.bottom)-Math.max(a.y,b.y)>1;
 check(label+': no horizontal page/hero overflow',!data.bodyOverflow&&!data.welcomeOverflow&&data.texts);
 const b=data.boxes;check(label+': photo gallery and content never overlap',!overlap(b['.intro'],b['.street-gallery']));
 check(label+': night photo is directly below dawn without overlap',b['.night-preview'].y>=b['.street-preview'].bottom+15&&Math.abs(b['.night-preview'].x-b['.street-preview'].x)<1);
 check(label+': footer follows both content columns and both photos',b['#welcome footer'].y>=Math.max(b['.intro'].bottom,b['.street-gallery'].bottom)-1);
 check(label+': status and instructions follow the action',b['#load-status'].y>=b['.primary'].bottom-1&&b['.touch-guide'].y>=b['#load-status'].bottom-1);
 check(label+': buttons never overlap',data.targets.every((a,i)=>data.targets.slice(i+1).every(b=>!overlap(a,b))));
 check(label+': header stays outside cutouts',data.targets.filter(t=>t.id!=='play'&&t.id!=='retry').every(t=>t.x>=safe.left-1&&t.right<=data.width-safe.right+1&&t.y>=safe.top-1));
 check(label+': new game photo loads at full resolution',data.image.src.includes('poster-morning')&&data.image.width>=1280);
 check(label+': real night game photo loads at full resolution',data.night.src.includes('poster-night.jpg')&&data.night.width>=1280);
 await page.locator('#play').scrollIntoViewIfNeeded();
 check(label+': main action is reachable',await page.locator('#play').isVisible());
 report.layouts.push({label,...data});
}
async function main(){
 const browser=await chromium.launch({channel:'chrome',headless:true});
 try{
  const context=await browser.newContext({...devices['iPhone 13'],deviceScaleFactor:1});
  const page=await context.newPage();page.on('pageerror',e=>report.errors.push(e.message));
  for(const [label,width,height,top,right,bottom,left] of [
   ['small-phone',320,568,20,0,0,0],['phone',390,844,47,0,34,0],['large-phone',430,932,59,0,34,0],
   ['phone-landscape',844,390,0,47,21,47],['small-landscape',568,320,0,0,0,0],['large-landscape',932,430,0,59,21,59],
   ['tablet-portrait',768,1024,24,0,20,0],['tablet-landscape',1024,768,24,0,20,0],['desktop-short',1280,600,0,0,0,0],
   ['desktop',1440,900,0,0,0,0],['desktop-wide',1920,1080,0,0,0,0],['zoom-equivalent',640,450,0,0,0,0]]){
   await page.setViewportSize({width,height});await page.goto(url,{waitUntil:'networkidle'});await page.evaluate(()=>document.fonts.ready);
   await page.addStyleTag({content:`:root{--safe-top:${top}px;--safe-right:${right}px;--safe-bottom:${bottom}px;--safe-left:${left}px}`});
   await page.evaluate(()=>dispatchEvent(new Event('resize')));await page.waitForTimeout(150);
   const safe={top,right,bottom,left};await inspect(page,label,safe);
   await page.evaluate(()=>document.querySelector('#welcome').scrollTop=0);
   if(['phone','phone-landscape','desktop','small-phone'].includes(label))await page.screenshot({path:`Captures/${capturePrefix}-${label}.png`});
   await page.locator('#help').click();
   const modal=await page.locator('#guide').boundingBox();
   check(label+': help fits safe viewport',modal.x>=left&&modal.y>=top&&modal.x+modal.width<=width-right+1&&modal.y+modal.height<=height-bottom+1);
   await page.locator('#close-help').click();check(label+': help returns keyboard focus',await page.locator('#help').evaluate(e=>e===document.activeElement));
   await page.evaluate(()=>{
    document.getElementById('play').disabled=true;document.getElementById('play').textContent='ĐANG MỞ PHỐ…';
    document.getElementById('progress').hidden=false;document.getElementById('progress').value=.53;
    document.getElementById('load-status').textContent='Đang tải game, lần đầu có thể mất một chút thời gian. Nếu kết nối chậm, vui lòng giữ trang này mở.';
   });await inspect(page,label+' loading',safe);
   await page.evaluate(()=>{document.getElementById('retry').hidden=false;document.getElementById('offline-status').textContent='Có bản cập nhật mới · đóng và mở lại để cập nhật game trên máy này';});
   await inspect(page,label+' retry/long-status',safe);
   console.log('PASS '+label+' normal/loading/retry/safe-area/help');
  }
  await context.close();
  const desktop=await browser.newContext({viewport:{width:1440,height:900}}),p=await desktop.newPage();
  await p.goto(url,{waitUntil:'networkidle'});await p.evaluate(()=>document.fonts.ready);await p.screenshot({path:`Captures/${capturePrefix}-desktop.png`});
  await p.locator('#help').click();await p.keyboard.press('Escape');check('Desktop Escape dismisses help',!await p.locator('#guide').isVisible());
  check('Current copy removes retired motorcycle, kick and lives instructions',!await p.locator('#welcome,#guide').allInnerTexts().then(t=>/Áo đỏ|Đạp xe|mất 1 mạng|đón\/trả khách/.test(t.join(' '))));
  await desktop.close();check('No JavaScript errors',report.errors.length===0);report.success=true;
 }finally{await browser.close();}
}
main().catch(e=>{report.success=false;report.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>{fs.writeFileSync(`Captures/${capturePrefix}-layout-checks.json`,JSON.stringify(report,null,2));console.log(JSON.stringify({success:report.success,count:report.checks.length}));});
