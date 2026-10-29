// Present actual Blender renders in an approval board; no AI image generation/editing.
const { chromium }=require('playwright');
const fs=require('fs');const path=require('path');const {pathToFileURL}=require('url');
(async()=>{
 const root=__dirname;const browser=await chromium.launch({channel:'chrome',headless:true});
 const page=await browser.newPage();const checks=[];
 const url=pathToFileURL(path.join(root,'preview.html')).href;
 for(const size of [{width:1440,height:900},{width:390,height:844},{width:844,height:390}]){
  await page.setViewportSize(size);await page.goto(url);await page.evaluate(()=>Promise.all(Array.from(document.images,i=>i.decode())));
  const result=await page.evaluate(()=>({overflow:document.documentElement.scrollWidth>innerWidth,images:[...document.images].map(i=>({src:i.getAttribute('src'),loaded:i.complete&&i.naturalWidth>0})),cards:document.querySelectorAll('figure').length}));
  checks.push({viewport:size,...result});
 }
 await page.setViewportSize({width:1800,height:1200});await page.goto(url+'?board=1');
 await page.evaluate(()=>Promise.all([...document.images].map(i=>i.decode())));
 await page.screenshot({path:path.join(root,'city-approval-board.png'),fullPage:true});
 const report={passed:checks.every(c=>!c.overflow&&c.images.every(i=>i.loaded)&&c.cards===6),viewports:checks};
 fs.writeFileSync(path.join(root,'viewer-checks.json'),JSON.stringify(report,null,2));
 console.log(JSON.stringify({passed:report.passed,viewports:checks.length,images:checks[0].images.length,board:'city-approval-board.png'}));
 await browser.close();if(!report.passed)process.exitCode=1;
})().catch(e=>{console.error(e);process.exitCode=1});
