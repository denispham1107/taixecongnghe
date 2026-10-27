// Real WebGL rendering and edge coverage, including the insets from notched mobile devices.
const {chromium,devices}=require('playwright');const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/docs/';const label=process.argv[3]||'local';
const result={url,checks:[],errors:[]};
function check(name,value){assert(value,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
  const info=await(await fetch(new URL('build-info.json',url))).json();result.build=info;
  check('Published version is 1.45.0',info.version==='1.45.0');
  for(const file of ['index.html','style.css','app.js','manifest.webmanifest','sw.js','precache.json']){
    const response=await fetch(new URL(file,url));const remote=(await response.text()).replace(/\r\n/g,'\n');
    const local=fs.readFileSync('docs/'+file,'utf8').replace(/\r\n/g,'\n');
    check('Published file matches tested artifact: '+file,response.ok&&remote===local);
  }
  const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--autoplay-policy=no-user-gesture-required']});
  try{
    const context=await browser.newContext({...devices['iPhone 13'],deviceScaleFactor:1});const page=await context.newPage();
    page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
    await page.addInitScript(()=>Object.defineProperty(navigator,'standalone',{get:()=>true}));
    await page.goto(url,{waitUntil:'networkidle'});await page.locator('#play').click();
    await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
    for(const [name,width,height,top,right,bottom,left]of [
      ['iphone-portrait',390,844,47,0,34,0],['iphone-landscape',844,390,0,47,21,47],
      ['ipad-portrait',768,1024,24,0,20,0],['ipad-landscape',1024,768,24,0,20,0]
    ]){
      await page.setViewportSize({width,height});
      await page.addStyleTag({content:`:root{--safe-top:${top}px;--safe-right:${right}px;--safe-bottom:${bottom}px;--safe-left:${left}px}`});
      await page.evaluate(()=>dispatchEvent(new Event('resize')));await page.waitForTimeout(750);
      const box=await page.locator('#unity-canvas').boundingBox();
      check(name+' scene fills all viewport edges',Math.abs(box.x)<1&&Math.abs(box.y)<1&&Math.abs(box.width-width)<1&&Math.abs(box.height-height)<1);
      check(name+' has no permanent header',await page.locator('#toolbar').isHidden());
      await page.screenshot({path:`Captures/browser-fullscreen-${label}-${name}.png`,timeout:90000});
      check(name+' floating hamburger button is hidden',await page.locator('#game-menu').isHidden());
    }
    check('No JavaScript or Unity runtime errors',result.errors.length===0);
    await context.close();
  }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/fullscreen-game-${label}-results.json`,JSON.stringify(result,null,2)));
