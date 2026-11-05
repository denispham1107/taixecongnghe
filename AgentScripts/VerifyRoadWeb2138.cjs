// 21.138: saves the road ahead from the real WASM at each level and orientation, to look at by eye.
// node VerifyRoadWeb2138.cjs <url> <label> [expect=clean|bug]
const {chromium}=require('playwright'),sharp=require('sharp'),fs=require('node:fs');
const url=process.argv[2]||'http://127.0.0.1:8785/',label=process.argv[3]||'road',expect=process.argv[4]||'clean';
const GAME='PHỐ VIỆT • Chuyến xe không ngừng',report={url,label,expect,errors:[],samples:[]};
async function greenPixels(png,box){
 const {data,info}=await sharp(png).extract(box).removeAlpha().raw().toBuffer({resolveWithObject:true});let n=0;
 for(let i=0;i<data.length;i+=info.channels){const r=data[i],g=data[i+1],b=data[i+2];if(g>170&&g>r+10&&g>b+6)n++;}
 return n/(data.length/info.channels);
}
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:false,args:['--use-angle=d3d11','--ignore-gpu-blocklist','--autoplay-policy=no-user-gesture-required','--window-size=900,900','--disable-backgrounding-occluded-windows','--disable-renderer-backgrounding']});
 const UA='Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1';
 let worst=0;
 try{
  for(const[orient,w,h]of [['ngang',844,390],['doc',390,844]]){
   const ctx=await browser.newContext({viewport:{width:w,height:h},isMobile:true,hasTouch:true,deviceScaleFactor:3,userAgent:UA});
   const page=await ctx.newPage();page.setDefaultTimeout(240000);
   page.on('pageerror',e=>report.errors.push(e.message));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   await page.goto(url,{waitUntil:'load'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:240000});await page.waitForTimeout(25000);
   const send=(m,v)=>page.evaluate(([g,m,v])=>{const i=window.phoVietInstance;v===null?i.SendMessage(g,m):i.SendMessage(g,m,v);},[GAME,m,v??null]);
   await send('SetGraphicsGovernor',0);
   for(const[level,name]of [[0,'cao'],[1,'trung'],[2,'thap']]){
    await send('SetGraphicsQuality',level);await send('SetGraphicsGovernor',0);await page.waitForTimeout(500);
    let best=0,bestPng=null;
    for(const wait of [900,1300,1700,2200]){
     await send('RestartRide');await page.waitForTimeout(wait);
     const png=await page.screenshot({timeout:90000});const m=await sharp(png).metadata();
     const box={left:Math.floor(m.width*.34),top:Math.floor(m.height*(orient==='doc'?.28:.14)),width:Math.floor(m.width*.32),height:Math.floor(m.height*.24)};
     const g=await greenPixels(png,box);if(g>=best){best=g;bestPng=png;}
    }
    fs.writeFileSync(`Captures/${label}-${orient}-${name}.png`,bestPng);report.samples.push({orient,level:name,green:+best.toFixed(4)});console.log(orient,name,best.toFixed(4));worst=Math.max(worst,best);
   }
   await ctx.close();
  }
 }finally{await browser.close();}
 // Trees and green shopfronts share this colour range, so the ratio is only a hint: judge the saved images by eye.
 report.worst=worst;report.success=report.errors.length===0;
 fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,1));console.log(JSON.stringify({worst,success:report.success,errors:report.errors.length}));if(!report.success)process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1);});
