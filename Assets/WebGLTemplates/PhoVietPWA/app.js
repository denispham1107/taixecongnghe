/* The shell contains no gameplay timers: Unity owns the run and touch input.
   1.74.0 (21.264): bản đồ mở — Unity tự nhận chạm (joystick, NHẢY, TÊN LỬA); khung trang chỉ báo tạm dừng,
   hướng màn hình và vùng tai thỏ cho đối tượng "VongChoi" (OpenWorldGame). */
'use strict';
const gameRoot='VongChoi';
const $ = id => document.getElementById(id);
const canvas = $('unity-canvas');
const rotateGame = $('rotate-game');
let unityInstance, installPrompt, starting = false;
let gameRequested=false, gamePortrait=false, lastGameOrientation;
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
// 21.291: mật độ điểm ảnh do mức «Cấu hình đồ họa» trong game quyết định (OpenWorldGame.Graphics.cs → jslib
// PhoVietSetPixelPlan). Lưu localStorage để lần mở sau tải đúng mật độ ngay từ đầu. Chưa chọn lần nào: điện thoại 1,6
// (mức Trung bình tự động bắt đầu 55 % mật độ gốc), máy tính 2.
const pixelPlanKey='phoviet.ow.pixelplan.v1';
let pixelCap=touchDevice?1.6:0,pixelBudget=0;
try{const saved=JSON.parse(localStorage.getItem(pixelPlanKey)||'null');if(saved&&saved.cap>0){pixelCap=saved.cap;pixelBudget=saved.budget>0?saved.budget:0;}}catch(_){}
// 21.282: ?diag=dpr1 (chẩn đoán) — mật độ điểm ảnh 1.
const diagDpr1=/[?&]diag=[^&]*dpr1/.test(location.search);
if(diagDpr1)pixelCap=1;
window.phoVietTouchDevice=touchDevice;
function nativePixelRatio(){return Number.isFinite(window.devicePixelRatio)&&window.devicePixelRatio>0?window.devicePixelRatio:1;}
function gamePixelRatio(width,height){
  const cap=pixelCap>0?pixelCap:2;
  // Bound large tablet framebuffers.
  const budget=pixelBudget>0?pixelBudget:(touchDevice?3800000:8300000);
  return Math.max(.5,Math.min(nativePixelRatio(),cap,Math.sqrt(budget/Math.max(1,width*height))));
}
window.phoVietSetPixelCap=cap=>{if(!diagDpr1)pixelCap=Number.isFinite(cap)&&cap>=0?cap:0;if(typeof syncViewport==='function')syncViewport();};
window.phoVietSetPixelPlan=(cap,budget)=>{
  if(!diagDpr1)pixelCap=Number.isFinite(cap)&&cap>0?cap:0;
  pixelBudget=Number.isFinite(budget)&&budget>0?budget:0;
  try{localStorage.setItem(pixelPlanKey,JSON.stringify({cap:pixelCap,budget:pixelBudget}));}catch(_){}
  if(typeof syncViewport==='function')syncViewport();
};
function pauseGame(){if(unityInstance)unityInstance.SendMessage(gameRoot,'PauseForBrowser');}
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
  syncGameOrientation(width,height);
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
    pixelRatio:unityInstance.Module.devicePixelRatio,
    nativeRatio:nativePixelRatio()
  });
  if(insets!==lastInsets){unityInstance.SendMessage(gameRoot,'ApplyBrowserInsets',insets);lastInsets=insets;}
}
function syncGameOrientation(width,height){
  const blocked=gameRequested&&width<height;
  if(blocked!==gamePortrait){
    gamePortrait=blocked;
    cancelAllPointers();
    rotateGame.hidden=!blocked;
    $('welcome').inert=$('toolbar').inert=canvas.inert=$('game-menu').inert=blocked;
    if(blocked)$('rotate-back').focus({preventScroll:true});
    else if(unityInstance)canvas.focus({preventScroll:true});
  }
  if(unityInstance&&lastGameOrientation!==blocked){
    unityInstance.SendMessage(gameRoot,'ApplyBrowserOrientation',blocked?'1':'0');
    lastGameOrientation=blocked;
  }
  if(gameRequested&&!blocked&&!starting)void loadGame();
}
function scheduleViewport(){
  requestAnimationFrame(()=>requestAnimationFrame(syncViewport));
  // Safari updates viewport and cutout values in separate orientation/layout passes.
  setTimeout(syncViewport,150);setTimeout(syncViewport,500);
}
function setImmersive(enabled){
  immersive=Boolean(enabled);document.body.classList.toggle('immersive',immersive);
  document.documentElement.classList.toggle('game-immersive',immersive);
  document.querySelector('meta[name="theme-color"]').content=immersive?'#202020':'#0a1622';   // 21.331: trang chủ nền tối
  // 21.347: menu ☰ (máy tính) chỉ còn «Tải game» nên ẩn khi nút này ẩn. 21.348: chú thích này trước nằm cuối dòng lệnh và
  // vô hiệu luôn setMenu/syncViewport phía sau → thanh trên rỗng che màn hình game (1.94.0). Không đặt // giữa dòng lệnh.
  $('game-menu').hidden=!immersive||touchDevice||!unityInstance||$('install').hidden;
  setMenu(false);syncViewport();scheduleViewport();
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

// 21.347: bỏ nút Cách chơi trên thanh trên; ô bánh răng cạnh LÊN XE THÔI vẫn mở Cách chơi.
$('help-gear').addEventListener('click',()=>{pauseGame();setMenu(false);$('guide').showModal();});
$('close-help').addEventListener('click', () => {$('guide').close();if(unityInstance)canvas.focus();else $('help-gear').focus();});
$('guide').addEventListener('click', e => {if(e.target === $('guide')) $('guide').close();});
// 21.347: nút «Tải game» mở hộp chỉ có phần Cài game & chơi offline.
$('close-install').addEventListener('click', () => {$('install-guide').close();if(unityInstance)canvas.focus();else $('install').focus();});
$('install-guide').addEventListener('click', e => {if(e.target === $('install-guide')) $('install-guide').close();});
$('retry').addEventListener('click', () => location.reload());
window.addEventListener('beforeinstallprompt', e => {
  e.preventDefault();installPrompt=e;
  if(!standalone) $('install').hidden=false;
});
window.addEventListener('appinstalled', () => {$('install').hidden=true;installPrompt=null;});
if(!standalone) $('install').hidden=false;   // 21.347: «Tải game» luôn hiện khi chưa cài (iPhone, Android, máy tính)
// 21.358: hộp «Tải game» có 3 thẻ hướng dẫn; mở sẵn thẻ đúng máy (iPhone Chrome = CriOS, iPhone khác = Safari).
function installTab(name){
  for(const b of document.querySelectorAll('.install-tabs [data-tab]')){const on=b.dataset.tab===name;b.classList.toggle('on',on);b.setAttribute('aria-selected',String(on));}
  for(const p of document.querySelectorAll('.install-pane'))p.hidden=p.dataset.pane!==name;
}
for(const b of document.querySelectorAll('.install-tabs [data-tab]'))b.addEventListener('click',()=>installTab(b.dataset.tab));
function openInstallGuide(){
  installTab(ios?(/CriOS/.test(navigator.userAgent)?'ios-chrome':'ios-safari'):'other');
  $('install-guide').showModal();$('install-guide').scrollTop=0;
  $('install-title').focus({preventScroll:true});   // 21.359: không để tiêu điểm (viền xanh) nằm trên nút × khi vừa mở
}
$('install').addEventListener('click', async () => {
  pauseGame();setMenu(false);
  if(!installPrompt) {openInstallGuide();return;}
  await installPrompt.prompt();await installPrompt.userChoice;installPrompt=null;$('install').hidden=true;
});

function fail(error) {
  console.error(error);$('load-status').textContent='Chưa tải được game. Kiểm tra kết nối và thử tải lại. Nếu máy thiếu bộ nhớ, hãy đóng bớt ứng dụng.';
  $('retry').hidden=false;$('progress').hidden=true;
}
$('rotate-back').addEventListener('click',()=>{
  if(starting){location.reload();return;}
  gameRequested=false;syncViewport();$('play').focus({preventScroll:true});
});
$('play').addEventListener('click',()=>{
  gameRequested=true;
  if(innerWidth>=innerHeight&&touchDevice)void requestNativeFullscreen();
  syncViewport();
});
async function loadGame(){
  if(starting)return;
  const probe=document.createElement('canvas'),gl=probe.getContext('webgl2');
  const supported=Boolean(window.WebAssembly&&gl);
  const mobileTextures=Boolean(gl?.getExtension('WEBGL_compressed_texture_etc'));
  gl?.getExtension('WEBGL_lose_context')?.loseContext();
  if(!supported) {
    gameRequested=false;
    $('load-status').textContent='Thiết bị cần trình duyệt hỗ trợ WebGL 2 và WebAssembly. Hãy mở bằng Chrome, Edge hoặc Safari mới nhất.';return;
  }
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
      startRunRecorder();
      checkOffline();
      setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));
      $('welcome').hidden=true;if(!gamePortrait)canvas.focus();
      if(document.hidden) unityInstance.SendMessage(gameRoot,'PauseForBrowser');
    }catch(error){gameRequested=false;canvas.hidden=true;setImmersive(false);fail(error);}
  };
  document.body.appendChild(script);
}
document.addEventListener('visibilitychange',()=>{
  if(document.hidden && unityInstance) unityInstance.SendMessage(gameRoot,'PauseForBrowser');
  if(!document.hidden)scheduleViewport();
});
window.addEventListener('pageshow',scheduleViewport);
canvas.addEventListener('contextmenu',e=>e.preventDefault());
// Unity nhận chạm trực tiếp (Input.touches). Khung trang chỉ chặn cuộn/thu phóng và menu chuột phải.
function cancelAllPointers(){}
window.phoVietSetFootMode=()=>{};
canvas.addEventListener('touchmove',e=>e.preventDefault(),{passive:false});
window.addEventListener('keydown',e=>{
  if(unityInstance && document.activeElement===canvas && ['Space','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.code))e.preventDefault();
});

// 21.278: khung «Bản cập nhật» — phiên bản của trang đang chạy (cấu hình build) và bản trên máy chủ (build-info.json,
// không qua bộ nhớ đệm). Lệch nhau = máy còn giữ bản cũ → nút tải bản mới (xóa bộ nhớ đệm offline rồi tải lại).
(()=>{
  const here=window.PHO_VIET_BUILD?.productVersion||'';
  if($('release-version'))$('release-version').textContent=here||'—';
  fetch('build-info.json?v='+Date.now(),{cache:'no-store'}).then(r=>r.ok?r.json():null).then(info=>{
    if(!info?.version||!here||info.version===here)return;
    const box=$('release-check');if(!box)return;
    box.hidden=false;box.textContent='Máy đang mở bản '+here+', đã có bản '+info.version+'. ';
    const btn=document.createElement('button');btn.textContent='Tải bản mới';box.appendChild(btn);
    $('release-notes').open=true;
    btn.addEventListener('click',async()=>{
      try{const regs=await navigator.serviceWorker?.getRegistrations?.()||[];await Promise.all(regs.map(r=>r.unregister()));}catch(e){}
      try{const keys=await caches.keys();await Promise.all(keys.filter(k=>k.startsWith('phoviet:')).map(k=>caches.delete(k)));}catch(e){}
      location.reload();
    });
  }).catch(()=>{});
})();

// 21.281: «hộp đen» — khi đang chơi, mỗi 2 giây ghi thời gian chơi, FPS, heap Unity, khung hình vào localStorage.
// Safari đóng trang vì sự cố thì không chạy pagehide → lần mở sau trang chủ hiện số liệu lần chơi bị đóng đột ngột.
const RUN_KEY='phoviet:lastRun';
function runStore(value){try{if(value)localStorage.setItem(RUN_KEY,JSON.stringify(value));else localStorage.removeItem(RUN_KEY);}catch(e){}}
(()=>{
  let prev=null;try{prev=JSON.parse(localStorage.getItem(RUN_KEY)||'null');}catch(e){}
  runStore(null);
  if(!prev||prev.ended||Date.now()-prev.last>30*60*1000)return;
  const box=$('release-check');if(!box)return;
  box.hidden=false;$('release-notes').open=true;
  box.textContent=`Lần chơi trước bị đóng đột ngột sau ${prev.secs} giây (bản ${prev.v}, FPS ${prev.fps}, heap ${prev.heap} MB, khung ${prev.size}${prev.diag?', diag '+prev.diag:''}, ${prev.ua}). Nếu lỗi còn, hãy chụp màn hình dòng này gửi lại.`;
})();
function startRunRecorder(){
  const start=Date.now();let frames=0,fps=0,last=performance.now();
  const tick=now=>{frames++;if(now-last>=1000){fps=Math.round(frames*1000/(now-last));frames=0;last=now;}requestAnimationFrame(tick);};
  requestAnimationFrame(tick);
  const write=ended=>{const heap=unityInstance?.Module?.HEAPU32?.buffer?.byteLength;
    runStore({v:window.PHO_VIET_BUILD?.productVersion||'',secs:Math.round((Date.now()-start)/1000),last:Date.now(),fps,
      heap:heap?Math.round(heap/1048576):0,size:canvas.width+'x'+canvas.height+'@'+(unityInstance?.Module?.devicePixelRatio||0).toFixed(2),
      diag:(location.search.match(/diag=([^&]*)/)||[])[1]||'',ua:(navigator.userAgent.match(/OS [\d_]+|Android [\d.]+|Chrome\/\d+|Version\/[\d.]+/g)||[]).join(' '),ended});};
  setInterval(()=>{if(!document.hidden)write(false);},2000);write(false);
  window.addEventListener('pagehide',()=>write(true));
  document.addEventListener('visibilitychange',()=>write(document.hidden));
}

// ?perf=1 shows frame rate and render size on a real phone, for reporting back (21.136).
if(new URLSearchParams(location.search).has('perf')){
  const box=document.createElement('div');box.id='perf-readout';
  box.style.cssText='position:fixed;left:6px;top:6px;z-index:99;padding:3px 7px;font:12px monospace;color:#fff;background:#000a;pointer-events:none;white-space:pre';
  document.body.appendChild(box);
  let last=performance.now(),frames=0,worst=0,previous=last;
  const tick=now=>{frames++;worst=Math.max(worst,now-previous);previous=now;
    if(now-last>=1000){const heap=unityInstance?.Module?.HEAPU32?.buffer?.byteLength;box.textContent=`${Math.round(frames*1000/(now-last))} FPS  max ${Math.round(worst)} ms\n${canvas.width}x${canvas.height}  dpr ${unityInstance?.Module.devicePixelRatio?.toFixed(2)||'-'}  heap ${heap?Math.round(heap/1048576)+' MB':'-'}`;frames=0;worst=0;last=now;}
    requestAnimationFrame(tick);};
  requestAnimationFrame(tick);
}
if('serviceWorker' in navigator) {
  // 21.347: chân trang ghi «Tác giả: PHẠM MINH QUÂN» cố định, bỏ dòng trạng thái offline (trước ghi «iPhone/iPad: cần kết nối mạng…»).
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
        });
      });
    }catch(error){console.warn('Offline cache unavailable',error);}
  })();
}
