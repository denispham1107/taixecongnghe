/* The shell contains no gameplay timers: Unity owns the run and touch input. */
'use strict';
const $ = id => document.getElementById(id);
const canvas = $('unity-canvas');
let unityInstance, installPrompt, starting = false;
const standalone = matchMedia('(display-mode: standalone), (display-mode: fullscreen)').matches || navigator.standalone;
const ios = /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
const iphoneApp=standalone && /iPhone|iPod/.test(navigator.userAgent);
const touchDevice=ios || navigator.maxTouchPoints>0 || matchMedia('(any-pointer: coarse)').matches;
let immersive=false, menuOpen=false, lastInsets='';
let serviceWorkerReady;
function checkOffline(){navigator.serviceWorker?.controller?.postMessage({type:'PHOVIET_CHECK_OFFLINE'});}
async function controlGameRequests(){
  // Let the small-shell worker claim this page before downloading Unity, so
  // offline storage consumes the game's requests instead of downloading twice.
  if(!serviceWorkerReady)return;
  await Promise.race([serviceWorkerReady.catch(()=>{}),new Promise(resolve=>setTimeout(resolve,4000))]);
  if(navigator.serviceWorker.controller)return;
  await new Promise(resolve=>{
    const done=()=>{clearTimeout(timer);navigator.serviceWorker.removeEventListener('controllerchange',done);resolve();};
    const timer=setTimeout(done,1500);navigator.serviceWorker.addEventListener('controllerchange',done);
  });
}
// The graphics level sets the render density (21.136); touch devices start modestly until Unity reports it.
let pixelCap=touchDevice?1.6:0;
window.phoVietTouchDevice=touchDevice;
function gamePixelRatio(width,height){
  const nativeRatio=Number.isFinite(window.devicePixelRatio)&&window.devicePixelRatio>0?window.devicePixelRatio:1;
  const cap=pixelCap>0?pixelCap:2;
  // Bound large tablet framebuffers.
  const budget=touchDevice?2600000:8300000;
  return Math.min(nativeRatio,cap,Math.sqrt(budget/Math.max(1,width*height)));
}
window.phoVietSetPixelCap=cap=>{pixelCap=Number.isFinite(cap)&&cap>=0?cap:0;if(typeof syncViewport==='function')syncViewport();};
function pauseGame(){if(unityInstance)unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');}
function setMenu(open){
  menuOpen=Boolean(open);$('toolbar').hidden=immersive&&!menuOpen;
  $('game-menu').setAttribute('aria-expanded',String(menuOpen));
  $('game-menu').textContent=menuOpen?'×':'☰';
}
function syncViewport(){
  const view=window.visualViewport;
  const root=document.documentElement;
  const unzoomed=view?.scale===1;
  const width=Math.round(unzoomed?view.width:innerWidth);
  const css=getComputedStyle($('safe-area-probe'));
  const top=parseFloat(css.paddingTop)||0,bottom=parseFloat(css.paddingBottom)||0;
  const measuredHeight=Math.round(ios&&standalone?Math.max(innerHeight,root.clientHeight,unzoomed?view.height:0):(unzoomed?view.height:innerHeight));
  let height=measuredHeight;
  // A full-width installed iPhone app can initially lose one status-bar-sized
  // strip in portrait. Restrict the correction to that geometry.
  const missing=screen.height-height;
  if(iphoneApp&&(!view||unzoomed)&&width<height&&screen.width<screen.height&&
     Math.abs(screen.width-width)<=2&&missing>0&&missing<=Math.max(64,top+2,bottom+2))height=screen.height;
  document.documentElement.style.setProperty('--viewport-width',width+'px');
  document.documentElement.style.setProperty('--viewport-height',height+'px');
  if(!unityInstance)return;
  const bounds=canvas.getBoundingClientRect();
  if(!bounds.width||!bounds.height)return;
  // Unity reads this Module property each frame; recompute after rotate/fullscreen.
  unityInstance.Module.devicePixelRatio=gamePixelRatio(bounds.width,bounds.height);
  const clamp=v=>Math.max(0,Math.min(.4,v));
  const insets=JSON.stringify({
    left:clamp(((parseFloat(css.paddingLeft)||0)-bounds.left)/bounds.width),
    top:clamp((top-bounds.top)/bounds.height),
    right:clamp(((parseFloat(css.paddingRight)||0)-(width-bounds.right))/bounds.width),
    bottom:clamp((Math.max(bottom,height-measuredHeight)-(Math.max(height,bounds.bottom)-bounds.bottom))/bounds.height),
    pixelRatio:unityInstance.Module.devicePixelRatio
  });
  if(insets!==lastInsets){unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','ApplyBrowserInsets',insets);lastInsets=insets;}
}
function scheduleViewport(){
  requestAnimationFrame(()=>requestAnimationFrame(syncViewport));
  // Safari updates viewport and cutout values in separate orientation/layout passes.
  setTimeout(syncViewport,150);setTimeout(syncViewport,500);
}
function setImmersive(enabled){
  immersive=Boolean(enabled);document.body.classList.toggle('immersive',immersive);
  document.documentElement.classList.toggle('game-immersive',immersive);
  document.querySelector('meta[name="theme-color"]').content=immersive?'#202020':'#eef5fb';
  $('game-menu').hidden=!immersive||touchDevice||!unityInstance;setMenu(false);syncViewport();scheduleViewport();
}
async function requestNativeFullscreen(){
  if(document.fullscreenElement || (ios && standalone) || matchMedia('(display-mode: fullscreen)').matches)return true;
  const request=document.documentElement.requestFullscreen || document.documentElement.webkitRequestFullscreen;
  if(!request)return false;
  try {await request.call(document.documentElement,{navigationUI:'hide'});return true;}catch{return false;}
}
$('game-menu').addEventListener('click',()=>{pauseGame();setMenu(!menuOpen);});
window.addEventListener('resize',scheduleViewport);
window.addEventListener('orientationchange',scheduleViewport);
window.visualViewport?.addEventListener('resize',scheduleViewport);
document.addEventListener('fullscreenchange',()=>{if(!canvas.hidden)setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));else scheduleViewport();});
new ResizeObserver(scheduleViewport).observe(canvas);
syncViewport();

$('help').addEventListener('click', () => {
  pauseGame();setMenu(false);
  $('guide').showModal();
});
$('close-help').addEventListener('click', () => {$('guide').close();if(unityInstance)canvas.focus();else $('help').focus();});
$('guide').addEventListener('click', e => {if(e.target === $('guide')) $('guide').close();});
$('retry').addEventListener('click', () => location.reload());
$('fullscreen').addEventListener('click', async () => {
  setMenu(false);
  if(document.fullscreenElement){await document.exitFullscreen();return;}
  if(unityInstance)setImmersive(true);
  const supported=await requestNativeFullscreen();
  if(!supported && ios && !standalone){pauseGame();$('guide').showModal();}
});
window.addEventListener('beforeinstallprompt', e => {
  e.preventDefault();installPrompt=e;
  if(!standalone) $('install').hidden=false;
});
window.addEventListener('appinstalled', () => {$('install').hidden=true;installPrompt=null;});
if(ios && !standalone) {$('install').hidden=false;$('install').textContent='Thêm vào màn hình';}
$('install').addEventListener('click', async () => {
  pauseGame();setMenu(false);
  if(!installPrompt) {$('guide').showModal();return;}
  await installPrompt.prompt();await installPrompt.userChoice;installPrompt=null;$('install').hidden=true;
});

function fail(error) {
  console.error(error);$('load-status').textContent='Chưa tải được game. Kiểm tra kết nối và thử tải lại. Nếu máy thiếu bộ nhớ, hãy đóng bớt ứng dụng.';
  $('retry').hidden=false;$('progress').hidden=true;
}
$('play').addEventListener('click', async () => {
  if(starting)return;
  const probe=document.createElement('canvas'),gl=probe.getContext('webgl2');
  const supported=Boolean(window.WebAssembly&&gl);
  const mobileTextures=Boolean(gl?.getExtension('WEBGL_compressed_texture_etc'));
  gl?.getExtension('WEBGL_lose_context')?.loseContext();
  if(!supported) {
    $('load-status').textContent='Thiết bị cần trình duyệt hỗ trợ WebGL 2 và WebAssembly. Hãy mở bằng Chrome, Edge hoặc Safari mới nhất.';return;
  }
  // Invoke while the tap still grants user activation, before loading WebAssembly.
  if(touchDevice)void requestNativeFullscreen();
  starting=true;$('play').disabled=true;$('play').textContent='ĐANG MỞ PHỐ…';$('progress').hidden=false;
  $('load-status').textContent='Đang tải game, lần đầu có thể mất một chút thời gian…';
  await controlGameRequests();
  const {loaderUrl,mobileDataUrl,...build}=window.PHO_VIET_BUILD;
  if(mobileTextures&&mobileDataUrl)build.dataUrl=mobileDataUrl;
  const config={...build,streamingAssetsUrl:'StreamingAssets',
    devicePixelRatio:gamePixelRatio(innerWidth,innerHeight),
    showBanner:(message,type) => {if(type==='error')fail(message);else console.warn(message);}
  };
  const script=document.createElement('script');script.src=loaderUrl;
  script.onerror=()=>fail('Could not load Unity loader');
  script.onload=async()=> {
    try {
      canvas.hidden=false;
      setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));
      config.devicePixelRatio=gamePixelRatio(canvas.clientWidth,canvas.clientHeight);
      unityInstance=await createUnityInstance(canvas,config,p=>{
        $('progress').value=p;$('load-status').textContent=`Đang tải game · ${Math.round(p*100)}%`;
      });
      window.phoVietInstance=unityInstance;
      checkOffline();
      setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));
      $('welcome').hidden=true;canvas.focus();
      if(document.hidden) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
    }catch(error){canvas.hidden=true;setImmersive(false);fail(error);}
  };
  document.body.appendChild(script);
});
document.addEventListener('visibilitychange',()=>{
  if(document.hidden && unityInstance) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
  if(!document.hidden)scheduleViewport();
});
window.addEventListener('pageshow',scheduleViewport);
canvas.addEventListener('contextmenu',e=>e.preventDefault());
// DOM events retain very quick gestures even when one Unity frame spans the entire
// touch. Use the short CSS edge, so rotating or changing render DPR never doubles
// the required travel. One pointer owns one action, triggered before finger-up.
let gamePointer=null;
const gameRoot='PHỐ VIỆT • Chuyến xe không ngừng';
let footMode=false;
const footPointers=new Map();
window.phoVietSetFootMode=enabled=>{
  if(footMode===enabled)return;
  footMode=enabled;
  cancelGamePointer();
  footPointers.clear();
};
function sendFootPointer(p,phase,e){
  const x=Math.max(0,Math.min(1,(e.clientX-p.rect.x)/p.rect.width));
  const y=Math.max(0,Math.min(1,(e.clientY-p.rect.y)/p.rect.height));
  unityInstance?.SendMessage(gameRoot,'ApplyFootPointer',JSON.stringify({phase,id:p.id,x,y}));
  if(phase===2&&Math.hypot(e.clientX-p.x,e.clientY-p.y)<Math.min(p.rect.width,p.rect.height)*.035)
    unityInstance?.SendMessage(gameRoot,'ApplyHudTap',JSON.stringify({x,y}));
}
function cancelFootPointers(){
  for(const p of footPointers.values())sendFootPointer(p,3,{clientX:p.x,clientY:p.y});
  footPointers.clear();
}
function sendGesture(p,phase,e,action=0){
  if(!unityInstance)return;
  unityInstance.SendMessage(gameRoot,'ApplyBrowserGesture',JSON.stringify({phase,id:p.id,action,
    x:Math.max(0,Math.min(1,(e.clientX-p.rect.x)/p.rect.width)),
    y:Math.max(0,Math.min(1,(e.clientY-p.rect.y)/p.rect.height)),at:e.timeStamp/1000}));
}
function cancelGamePointer(){
  if(!gamePointer)return;
  const p=gamePointer;gamePointer=null;
  sendGesture(p,3,{clientX:p.x,clientY:p.y,timeStamp:performance.now()});
}
// Keep in step with IsJumpSwipe/IsBoostSwipe/SwipeAction in Unity. No angle may match nothing: a thumb on a
// sideways phone arcs diagonally, and the old 1.3 ratio dropped 37-52 degree swipes without any response (21.140).
function swipeAction(dx,dy,short,scale=1){
  if(dy<0&&-dy>short*.085*scale&&-dy>=Math.abs(dx)*.85)return 2;
  if(dy>0&&dy>short*.12*scale&&dy>=Math.abs(dx)*.85)return 3;
  if(Math.abs(dx)>short*.075*scale&&Math.abs(dy)<Math.abs(dx)*.85)return dx>0?1:-1;
  return 0;
}
function pointerMoved(e){
  const foot=footPointers.get(e.pointerId);
  if(foot){sendFootPointer(foot,1,e);return;}
  const p=gamePointer;
  if(!p||p.id!==e.pointerId||p.consumed)return;
  // A toolbar or render-scale change mid-swipe keeps the gesture; only a rotation ends it.
  const r=canvas.getBoundingClientRect();
  if((r.width>r.height)!==(p.rect.width>p.rect.height)){cancelGamePointer();return;}
  const action=swipeAction(e.clientX-p.x,e.clientY-p.y,Math.min(r.width,r.height));
  if(action){p.consumed=true;sendGesture(p,1,e,action);}
}
canvas.addEventListener('pointerdown',e=>{
  if(e.button!==0||!unityInstance)return;
  const rect=canvas.getBoundingClientRect();if(!rect.width||!rect.height)return;
  if(footMode){
    const foot={id:e.pointerId,x:e.clientX,y:e.clientY,rect};footPointers.set(foot.id,foot);
    canvas.setPointerCapture?.(foot.id);sendFootPointer(foot,0,e);return;
  }
  // Newest finger wins: a thumb resting on a sideways phone must not block the one that swipes.
  if(gamePointer)cancelGamePointer();
  gamePointer={id:e.pointerId,x:e.clientX,y:e.clientY,at:e.timeStamp,rect,consumed:false,touch:e.pointerType!=='mouse'};
  canvas.setPointerCapture?.(e.pointerId);sendGesture(gamePointer,0,e);
},{passive:true});
canvas.addEventListener('pointermove',pointerMoved,{passive:true});
canvas.addEventListener('pointerup',e=>{
  const foot=footPointers.get(e.pointerId);
  if(foot){footPointers.delete(foot.id);sendFootPointer(foot,2,e);return;}
  if(!gamePointer||gamePointer.id!==e.pointerId)return;
  pointerMoved(e);const p=gamePointer;if(!p)return;gamePointer=null;
  const short=Math.min(p.rect.width,p.rect.height);
  // A quick flick that lifts before the normal distance still counts.
  if(!p.consumed&&e.timeStamp-p.at<220){
    const flick=swipeAction(e.clientX-p.x,e.clientY-p.y,short,.6);
    if(flick){p.consumed=true;sendGesture(p,1,e,flick);sendGesture(p,3,e);return;}
  }
  const tap=!p.consumed&&Math.hypot(e.clientX-p.x,e.clientY-p.y)<short*.035;
  if(tap){
    unityInstance.SendMessage(gameRoot,'ApplyHudTap',JSON.stringify({x:(e.clientX-p.rect.x)/p.rect.width,y:(e.clientY-p.rect.y)/p.rect.height}));
    if(p.touch&&e.timeStamp-p.at<240)sendGesture(p,2,e);else sendGesture(p,4,e,1);
  }else sendGesture(p,4,e);
},{passive:true});
// Safari can start scrolling a sideways page and cancel the pointer before the swipe distance is reached.
canvas.addEventListener('touchmove',e=>e.preventDefault(),{passive:false});
function cancelPointer(e){
  const foot=footPointers.get(e.pointerId);
  if(foot){footPointers.delete(foot.id);sendFootPointer(foot,3,e);}
  if(gamePointer?.id===e.pointerId)cancelGamePointer();
}
canvas.addEventListener('pointercancel',cancelPointer,{passive:true});
canvas.addEventListener('lostpointercapture',cancelPointer,{passive:true});
function cancelAllPointers(){cancelGamePointer();cancelFootPointers();}
window.addEventListener('orientationchange',cancelAllPointers);
window.addEventListener('blur',cancelAllPointers);
document.addEventListener('visibilitychange',()=>{if(document.hidden)cancelAllPointers();});
window.addEventListener('keydown',e=>{
  if(unityInstance && document.activeElement===canvas && ['Space','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.code))e.preventDefault();
});

// ?perf=1 shows frame rate and render size on a real phone, for reporting back (21.136).
if(new URLSearchParams(location.search).has('perf')){
  const box=document.createElement('div');box.id='perf-readout';
  box.style.cssText='position:fixed;left:6px;top:6px;z-index:99;padding:3px 7px;font:12px monospace;color:#fff;background:#000a;pointer-events:none;white-space:pre';
  document.body.appendChild(box);
  let last=performance.now(),frames=0,worst=0,previous=last;
  const tick=now=>{frames++;worst=Math.max(worst,now-previous);previous=now;
    if(now-last>=1000){box.textContent=`${Math.round(frames*1000/(now-last))} FPS  max ${Math.round(worst)} ms\n${canvas.width}x${canvas.height}  dpr ${unityInstance?.Module.devicePixelRatio?.toFixed(2)||'-'}`;frames=0;worst=0;last=now;}
    requestAnimationFrame(tick);};
  requestAnimationFrame(tick);
}
if('serviceWorker' in navigator) {
  navigator.serviceWorker.addEventListener('message',event=>{
    if(event.data?.type==='PHOVIET_OFFLINE_STATUS')$('offline-status').textContent=event.data.ready?
      'Đã lưu để chơi offline trên máy này':'Game sẽ được lưu offline sau khi tải xong';
  });
  navigator.serviceWorker.addEventListener('controllerchange',checkOffline);
  (async()=>{
    try {
      serviceWorkerReady=navigator.serviceWorker.register('./sw.js',{updateViaCache:'none'}).then(()=>navigator.serviceWorker.ready);
      const reg=await serviceWorkerReady;
      if(reg.active)checkOffline();
      reg.addEventListener('updatefound',()=>{
        const worker=reg.installing;
        worker.addEventListener('statechange',()=>{
          if(worker.state==='activated')checkOffline();
          else if(worker.state==='installed' && reg.active)$('offline-status').textContent='Có bản mới · đóng và mở lại để cập nhật';
        });
      });
    }catch(error){console.warn('Offline cache unavailable',error);}
  })();
}
