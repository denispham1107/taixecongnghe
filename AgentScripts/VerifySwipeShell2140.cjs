// 21.140: real multi-touch through the web shell, checking which gesture reaches Unity. Compare shells on one build:
// node VerifySwipeShell2140.cjs <site url> <label> [path to app.js to test instead of the site's]
const {chromium}=require('playwright'),fs=require('node:fs');
const url=process.argv[2]||'http://127.0.0.1:8785/',label=process.argv[3]||'swipe',shell=process.argv[4];
const report={url,label,shell:shell||'site',errors:[],cases:[]};
const near=(f,to,n)=>Array.from({length:n},(_,i)=>f+(to-f)*(i+1)/n);
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:false,args:['--use-angle=d3d11','--ignore-gpu-blocklist','--autoplay-policy=no-user-gesture-required','--window-size=900,900','--disable-backgrounding-occluded-windows','--disable-renderer-backgrounding']});
 const UA='Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1';
 try{
  for(const[orient,w,h]of [['ngang',844,390],['doc',390,844]]){
   const ctx=await browser.newContext({viewport:{width:w,height:h},isMobile:true,hasTouch:true,deviceScaleFactor:3,userAgent:UA,serviceWorkers:'block'});
   if(shell)await ctx.route('**/app.js',r=>r.fulfill({contentType:'text/javascript',body:fs.readFileSync(shell,'utf8')}));
   const page=await ctx.newPage();page.setDefaultTimeout(240000);
   page.on('pageerror',e=>report.errors.push(e.message));
   await page.goto(url,{waitUntil:'load'});await page.locator('#play').click();await page.waitForFunction(()=>!!window.phoVietInstance,{},{timeout:240000});await page.waitForTimeout(20000);
   await page.evaluate(()=>{const i=window.phoVietInstance,send=i.SendMessage.bind(i);window.__gestures=[];i.SendMessage=(g,m,v)=>{if(m==='ApplyBrowserGesture')window.__gestures.push(JSON.parse(v));return send(g,m,v);};send('PHỐ VIỆT • Chuyến xe không ngừng','RestartRide');});
   const cdp=await ctx.newCDPSession(page);
   const box=await page.locator('#unity-canvas').boundingBox();const short=Math.min(box.width,box.height);
   async function swipe(x,y,dx,dy,steps=6,hold=null){
    const points=(a,b)=>hold?[{x:hold[0],y:hold[1],id:0},{x:a,y:b,id:1}]:[{x:a,y:b,id:0}];
    if(hold)await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:hold[0],y:hold[1],id:0}]});
    await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:points(x,y)});
    const xs=near(x,x+dx,steps),ys=near(y,y+dy,steps);
    for(let i=0;i<steps;i++){await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(xs[i],ys[i])});await page.waitForTimeout(12);}
    await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(60);
   }
   const b=box,cx=b.x+b.width*.62,cy=b.y+b.height*.6,len=short*.2;
   const rad=d=>d*Math.PI/180;
   const cases=[
    ['len thang',2,()=>swipe(cx,cy,0,-len)],
    ['len cheo 25 do',2,()=>swipe(cx,cy,-Math.sin(rad(25))*len,-Math.cos(rad(25))*len)],
    ['len cheo 40 do',2,()=>swipe(cx,cy,-Math.sin(rad(40))*len,-Math.cos(rad(40))*len)],
    ['len cheo 45 do',2,()=>swipe(cx,cy,-Math.sin(rad(45))*len,-Math.cos(rad(45))*len)],
    ['len ngan 10%',2,()=>swipe(cx,cy,0,-short*.1)],
    ['flick nhanh 6%',2,()=>swipe(cx,cy,0,-short*.06,2)],
    ['dat ngon thu 2 dang giu ngon khac',2,()=>swipe(cx,cy,0,-len,6,[b.x+b.width*.1,b.y+b.height*.55])],
    ['xuong',3,()=>swipe(cx,cy,0,len)],
    ['trai',-1,()=>swipe(cx,cy,-len,0)],
    ['phai',1,()=>swipe(cx,cy,len,0)],
    ['phai hoi len',1,()=>swipe(cx,cy,len,-len*.25)],
   ];
   for(const[name,want,run]of cases){
    await page.evaluate(()=>{window.__gestures.length=0;});await run();await page.waitForTimeout(60);
    const seen=await page.evaluate(()=>window.__gestures.filter(g=>g.phase===1).map(g=>g.action));
    const ok=seen.length===1&&seen[0]===want;report.cases.push({orient,name,want,seen,ok});console.log((ok?'PASS ':'FAIL ')+orient+' '+name+' -> '+JSON.stringify(seen));
   }
   await ctx.close();
  }
 }finally{await browser.close();}
 report.passed=report.cases.filter(c=>c.ok).length;report.total=report.cases.length;report.success=report.passed===report.total&&report.errors.length===0;
 fs.writeFileSync(`Captures/${label}-checks.json`,JSON.stringify(report,null,1));console.log(JSON.stringify({passed:report.passed,total:report.total,errors:report.errors.length}));
})().catch(e=>{console.error(e);process.exit(1);});
