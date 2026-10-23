/* Run with NODE_PATH pointing at a Playwright installation.
 * node AgentScripts/VerifyWeb.cjs http://127.0.0.1:8765/docs/ local
 * Device profiles emulate layout and touch; they do not replace physical Safari testing.
 */
const {chromium,devices}=require('playwright');
const fs=require('node:fs');
const path=require('node:path');
const os=require('node:os');
const assert=require('node:assert/strict');
const sharp=require('sharp');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';
const label=process.argv[3]||'local';
const results={url,label,checks:[],errors:[]};
function pass(name,value=true){assert(value,name);results.checks.push(name);console.log('PASS '+name);}
async function shot(page,name){await page.screenshot({path:`Captures/browser-${label}-${name}.png`,timeout:90000});}
async function ready(page){
  await page.locator('#play').click();
  await page.waitForFunction(()=>Boolean(window.phoVietInstance),{},{timeout:180000});
  // createUnityInstance resolves before the Unity splash/first scene has finished.
  await page.waitForTimeout(4500);
  pass('WebAssembly game initialized');
  pass('Loading screen dismissed',await page.locator('#welcome').isHidden());
}
async function inspect(page){
  // Software WebGL rendering can delay the two stable frames required by DOM clicks.
  page.setDefaultTimeout(90000);
  page.on('pageerror',e=>results.errors.push(e.message));
  page.on('console',msg=>{if(msg.type()==='error')results.errors.push(msg.text());});
  await page.goto(url,{waitUntil:'networkidle',timeout:60000});
}
async function main(){
  for(const file of ['index.html','style.css','app.js','manifest.webmanifest','precache.json','sw.js','build-info.json']) {
    const response=await fetch(new URL(file,url));pass('HTTP 200 '+file,response.status===200);
  }
  const manifest=await (await fetch(new URL('manifest.webmanifest',url))).json();
  pass('Installable scoped manifest',['fullscreen','standalone'].includes(manifest.display)&&manifest.start_url==='./'&&manifest.scope==='./');
  for(const icon of manifest.icons){
    const info=await sharp(Buffer.from(await (await fetch(new URL(icon.src,url))).arrayBuffer())).metadata();
    pass('Icon dimensions '+icon.src,`${info.width}x${info.height}`===icon.sizes);
  }
  const precache=await (await fetch(new URL('precache.json',url))).json();
  pass('Offline list contains WASM, data and shell',precache.files.some(x=>x.includes('.wasm'))&&precache.files.some(x=>x.includes('.data'))&&precache.files.includes('index.html'));
  for(const file of precache.files){
    const response=await fetch(new URL(file,url),{method:'HEAD'});assert.equal(response.status,200,file);
  }
  pass('Every offline resource is available');
  const build=await (await fetch(new URL('build-info.json',url))).json();
  results.build=build;pass('Versioned offline cache',build.version==='1.33.0'&&/^[a-f0-9]{16}$/.test(build.cache));
  const launchOptions={channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']};
  const browser=await chromium.launch(launchOptions);
  // PWA installation is disabled in the incognito contexts returned by browser.newContext().
  const profile=fs.mkdtempSync(path.join(os.tmpdir(),'phoviet-web-check-'));
  let desktop;
  try {
    desktop=await chromium.launchPersistentContext(profile,{...launchOptions,viewport:{width:1440,height:900}});
    const page=await desktop.newPage();await inspect(page);
    pass('Landing page has no horizontal overflow',await page.evaluate(()=>document.documentElement.scrollWidth===innerWidth));
    await shot(page,'desktop-welcome');
    await page.locator('#help').click();pass('Installation guide opens',await page.locator('#guide').isVisible());
    await page.locator('#close-help').click();
    const cdp=await desktop.newCDPSession(page);const appManifest=await cdp.send('Page.getAppManifest');
    pass('Browser parses manifest without errors',appManifest.errors.length===0);
    await ready(page);await shot(page,'desktop-ready');
    await page.locator('#unity-canvas').focus();await page.keyboard.press('Enter');await page.waitForTimeout(350);
    await page.keyboard.press('ArrowRight');await page.waitForTimeout(350);await page.keyboard.press('q');await page.waitForTimeout(150);await page.keyboard.press('p');
    await shot(page,'desktop-paused');pass('Keyboard start, steering, kick and pause smoke completed');
    await page.locator('#help').click();await page.locator('#close-help').click();pass('Unity accepts browser pause message without an error');
    await page.waitForFunction(async()=>{const r=await navigator.serviceWorker.getRegistration();return !!(r&&r.active)},{},{timeout:180000});
    await page.waitForFunction(()=>!!navigator.serviceWorker.controller,{},{timeout:30000});
    const cached=await page.evaluate(async()=>{const keys=await caches.keys();return (await caches.open(keys.find(k=>k.startsWith('phoviet:')))).keys().then(r=>r.map(x=>x.url));});
    pass('Entire offline build cached',precache.files.every(file=>cached.includes(new URL(file,url).href)));
    results.installability=await cdp.send('Page.getInstallabilityErrors');
    pass('Chromium reports no installability errors',results.installability.installabilityErrors.length===0);
    await desktop.setOffline(true);await page.reload({waitUntil:'load'});
    pass('Offline home screen loads',await page.locator('#play').isVisible());await ready(page);await shot(page,'offline-ready');
    pass('Complete WebGL game starts with network disabled');await desktop.close();

    // Keep phone CSS dimensions/touch while reducing software-renderer screenshot load.
    const mobile=await browser.newContext({...devices['Pixel 7'],deviceScaleFactor:1});
    const phone=await mobile.newPage();await inspect(phone);
    const touchSession=await mobile.newCDPSession(phone);
    async function touch(x,y,hold=90) {
      await touchSession.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:0}]});
      await phone.waitForTimeout(hold);
      await touchSession.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
    }
    pass('Phone landing fits width',await phone.evaluate(()=>document.documentElement.scrollWidth===innerWidth));
    await shot(phone,'android-welcome');await ready(phone);await shot(phone,'android-ready');
    const box=await phone.locator('#unity-canvas').boundingBox();
    const uiScale=box.width/480,uiHeight=box.height/uiScale;
    const cardY=Math.max(105,Math.min(uiHeight*.15,Math.max(105,uiHeight-258-95)));
    await touch(box.x+box.width/2,box.y+(cardY+258-70+22)*uiScale,160);
    await phone.waitForTimeout(350);
    const sx=box.x+box.width*.40,sy=box.y+box.height*.68;
    await touchSession.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:sx,y:sy,id:0}]});
    await phone.waitForTimeout(90);
    await touchSession.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:sx+box.width*.24,y:sy,id:0}]});
    await phone.waitForTimeout(90);
    await touchSession.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
    await touch(box.x+box.width/2,box.y+box.height*.53);
    await phone.waitForTimeout(90);await touch(box.x+box.width/2,box.y+box.height*.53);
    await phone.waitForTimeout(150);await shot(phone,'android-touch');
    await phone.evaluate(()=>{
      const game='PHỐ VIỆT • Chuyến xe không ngừng';
      window.phoVietInstance.SendMessage(game,'RestartRide');
      window.phoVietInstance.SendMessage(game,'MoveLane',-1);
      window.phoVietInstance.SendMessage(game,'MoveLane',-1);
    });
    await phone.waitForTimeout(550);await shot(phone,'android-left-lane');
    pass('Mobile outer-lane camera scenarios captured for visual inspection');
    pass('Mobile game has no floating hamburger button',await phone.locator('#game-menu').isHidden());
    await phone.evaluate(()=>window.phoVietInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','TogglePause'));
    pass('Touch start, swipe steering, double tap and pause smoke completed');
    await phone.setViewportSize({width:844,height:390});await phone.waitForTimeout(600);await shot(phone,'android-landscape');
    pass('Game canvas fills every edge after rotation',await phone.evaluate(()=>{const r=document.querySelector('canvas').getBoundingClientRect();return Math.abs(r.x)<1&&Math.abs(r.y)<1&&Math.abs(r.width-innerWidth)<1&&Math.abs(r.height-innerHeight)<1&&r.width>r.height;}));
    await mobile.close();
    const ios=await browser.newContext({...devices['iPhone 13'],deviceScaleFactor:1});const iphone=await ios.newPage();await inspect(iphone);
    pass('iOS profile offers home-screen instructions',await iphone.locator('#install').isVisible());
    await iphone.locator('#install').click();pass('iOS installation help available',await iphone.locator('#guide').innerText().then(t=>t.includes('Thêm vào Màn hình chính')));
    await shot(iphone,'ios-install');await ios.close();
    pass('No JavaScript or Unity runtime errors',results.errors.length===0);
  }finally{
    if(desktop)await desktop.close();await browser.close();
    const verified=fs.realpathSync(profile);
    assert(verified.startsWith(fs.realpathSync(os.tmpdir())+path.sep)&&path.basename(verified).startsWith('phoviet-web-check-'));
    fs.rmSync(verified,{recursive:true,force:true});
  }
}
main().then(()=>{results.success=true;console.log(JSON.stringify({success:true,checks:results.checks.length,version:results.build.version}));}).catch(e=>{results.success=false;results.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/browser-${label}.json`,JSON.stringify(results,null,2)));
