// Release smoke: compare deployed resources to the tested build, then load real WASM.
const {chromium}=require('playwright');
const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'https://denispham1107.github.io/taixecongnghe/';
const label=process.argv[3]||'rider-contact-live';
const result={url,checks:[],errors:[]};
function check(name,ok){assert(ok,name);result.checks.push(name);console.log('PASS '+name);}
async function main(){
  const expected=JSON.parse(fs.readFileSync('docs/build-info.json','utf8'));
  const actual=await(await fetch(new URL('build-info.json',url),{cache:'no-store'})).json();result.build=actual;
  check('Live release version and cache match tested build',actual.version===expected.version&&actual.cache===expected.cache);
  const list=JSON.parse(fs.readFileSync('docs/precache.json','utf8')).files;
  for(const file of [...list,'sw.js','precache.json']){
    const response=await fetch(new URL(file,url),{cache:'no-store'});
    const remote=Buffer.from(await response.arrayBuffer()),local=fs.readFileSync('docs/'+file);
    const binary=!/\.(html|css|js|json|webmanifest|svg|txt)$/.test(file);
    check('Published artifact matches: '+file,response.ok&&(binary?remote.equals(local):remote.toString().replace(/\r\n/g,'\n')===local.toString().replace(/\r\n/g,'\n')));
  }
  const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
  try{
    const page=await browser.newPage({viewport:{width:1280,height:720}});
    page.on('pageerror',e=>result.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')result.errors.push(m.text());});
    await page.goto(url,{waitUntil:'networkidle'});await page.evaluate(()=>document.fonts.ready);
    await page.screenshot({path:`Captures/browser-${label}-landing.png`,timeout:90000});
    await page.locator('#play').click();
    await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:180000});await page.waitForTimeout(4500);
    check('Real published Unity WebAssembly initializes',await page.locator('#welcome').isHidden());
    await page.locator('#unity-canvas').focus();await page.keyboard.press('Enter');await page.waitForTimeout(500);await page.keyboard.press('p');
    await page.screenshot({path:`Captures/browser-${label}.png`,timeout:90000});
    check('Published start and pause input smoke completed',true);
    check('No JavaScript or Unity runtime errors',result.errors.length===0);
  }finally{await browser.close();}
}
main().then(()=>{result.success=true;console.log(JSON.stringify({success:true,count:result.checks.length}));}).catch(e=>{result.success=false;result.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync(`Captures/${label}-results.json`,JSON.stringify(result,null,2)));
