// Layout regression matrix with a mocked Unity loader; real-engine screenshots are verified separately.
const {chromium}=require('playwright');
const fs=require('node:fs');const assert=require('node:assert/strict');
const url=process.argv[2]||'http://127.0.0.1:8765/Assets/WebGLTemplates/PhoVietPWA/';
const results={url,kind:'Shell layout with mocked engine',checks:[],errors:[]};
function check(name,value){assert(value,name);results.checks.push(name);}
const profiles=[
  ['iPhone SE',375,667,[20,0,0,0],[0,0,0,0]],
  ['iPhone notch',390,844,[47,0,34,0],[0,47,21,47]],
  ['iPhone large',430,932,[59,0,34,0],[0,59,21,59]],
  ['iPhone viewport regression',428,926,[47,0,34,0],[0,47,21,47]],
  ['Android phone',412,915,[24,0,24,0],[0,0,24,0]],
  ['iPad',768,1024,[24,0,20,0],[24,0,20,0]],
  ['iPad Pro',1024,1366,[24,0,20,0],[24,0,20,0]],
  ['Android tablet',800,1280,[24,0,24,0],[24,0,24,0]]
];
async function main(){
  const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
  try{
    for(const [name,width,height,portrait,landscape]of profiles){
      const context=await browser.newContext({viewport:{width,height},hasTouch:true,isMobile:true,deviceScaleFactor:1,serviceWorkers:'block',userAgent:name.startsWith('i')?'Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 Version/18.0 Mobile/15E148 Safari/604.1':'Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 Chrome/132.0.0.0 Mobile Safari/537.36'});
      const page=await context.newPage();page.on('pageerror',e=>results.errors.push(e.message));
      await page.addInitScript(({shortViewport})=>{
        Object.defineProperty(navigator,'standalone',{get:()=>true});
        Object.defineProperty(Element.prototype,'requestFullscreen',{value:undefined,configurable:true});
        Object.defineProperty(Element.prototype,'webkitRequestFullscreen',{value:undefined,configurable:true});
        window.__insets=[];
        if(shortViewport)Object.defineProperty(window.visualViewport,'height',{get:()=>innerHeight-47,configurable:true});
      },{shortViewport:name.includes('viewport regression')});
      await page.route('**/Build/*.loader.js',r=>r.fulfill({contentType:'text/javascript',body:'window.createUnityInstance=async()=>({SendMessage:(root,method,value)=>{if(method==="ApplyBrowserInsets")window.__insets.push(JSON.parse(value));}});'}));
      if(url.includes('/Assets/'))await page.route(url,r=>{
        const source=fs.readFileSync('Assets/WebGLTemplates/PhoVietPWA/index.html','utf8');
        const html=source.replace(/window\.PHO_VIET_BUILD=\{[\s\S]*?\n    \};/,'window.PHO_VIET_BUILD={loaderUrl:"Build/preview.loader.js"};');
        return r.fulfill({contentType:'text/html',body:html});
      });
      await page.goto(url,{waitUntil:'networkidle'});
      async function setSafe(values){await page.addStyleTag({content:`:root{--safe-top:${values[0]}px;--safe-right:${values[1]}px;--safe-bottom:${values[2]}px;--safe-left:${values[3]}px}`});await page.evaluate(()=>dispatchEvent(new Event('resize')));}
      await setSafe(portrait);await page.locator('#play').click();
      await page.waitForFunction(()=>!!window.phoVietInstance);
      for(const [orientation,w,h,insets]of [['portrait',width,height,portrait],['landscape',height,width,landscape],['portrait-return',width,height,portrait]]){
        await page.setViewportSize({width:w,height:h});await setSafe(insets);await page.waitForTimeout(650);
        const state=await page.evaluate(()=>{
          const r=document.querySelector('canvas').getBoundingClientRect();const s=getComputedStyle(document.body);const menu=document.querySelector('#game-menu').getBoundingClientRect();
          return {rect:{x:r.x,y:r.y,width:r.width,height:r.height},viewport:{width:innerWidth,height:innerHeight},padding:[s.paddingTop,s.paddingRight,s.paddingBottom,s.paddingLeft],last:window.__insets.at(-1),scroll:document.documentElement.scrollWidth,menu:{x:menu.x,y:menu.y,right:menu.right,bottom:menu.bottom}};
        });
        const prefix=name+' '+orientation;
        check(prefix+' canvas begins at both screen edges',Math.abs(state.rect.x)<1&&Math.abs(state.rect.y)<1);
        check(prefix+' canvas fills viewport width and height',Math.abs(state.rect.width-state.viewport.width)<1&&Math.abs(state.rect.height-state.viewport.height)<1);
        check(prefix+' no body padding or horizontal overflow',state.padding.every(x=>parseFloat(x)===0)&&state.scroll<=w);
        const expected={top:insets[0],right:insets[1],bottom:insets[2],left:insets[3]};
        check(prefix+' safe area passed to HUD only',Object.keys(expected).every(key=>Math.abs(state.last[key]*(key==='left'||key==='right'?state.rect.width:state.rect.height)-expected[key])<1));
        check(prefix+' floating hamburger button is hidden',await page.locator('#game-menu').isHidden());
        check(prefix+' header does not occupy game area',await page.locator('#toolbar').isHidden());
      }
      await context.close();console.log('PASS '+name+' portrait, landscape and rotate-back');
    }
    check('No shell JavaScript errors',results.errors.length===0);
  }finally{await browser.close();}
}
main().then(()=>{results.success=true;console.log(JSON.stringify({success:true,checks:results.checks.length}));}).catch(e=>{results.success=false;results.failure=e.stack;console.error(e);process.exitCode=1;}).finally(()=>fs.writeFileSync('Captures/fullscreen-layout-results.json',JSON.stringify(results,null,2)));
