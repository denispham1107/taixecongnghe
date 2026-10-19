/* The shell contains no gameplay timers: Unity owns the run and touch input. */
'use strict';
const $ = id => document.getElementById(id);
const canvas = $('unity-canvas');
let unityInstance, installPrompt, starting = false;
const standalone = matchMedia('(display-mode: standalone), (display-mode: fullscreen)').matches || navigator.standalone;
const ios = /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
const touchDevice=ios || navigator.maxTouchPoints>0 || matchMedia('(any-pointer: coarse)').matches;
let immersive=false, menuOpen=false, lastInsets='';
function pauseGame(){if(unityInstance)unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');}
function setMenu(open){
  menuOpen=Boolean(open);$('toolbar').hidden=immersive&&!menuOpen;
  $('game-menu').setAttribute('aria-expanded',String(menuOpen));
  $('game-menu').textContent=menuOpen?'×':'☰';
}
function syncViewport(){
  const view=window.visualViewport;
  const width=Math.round(view && view.scale===1?view.width:innerWidth);
  // Installed WebKit apps can report visualViewport.height minus the safe area.
  // Keep the available layout viewport; never force physical screen dimensions (iPad split view).
  const height=Math.round(ios && standalone?Math.max(innerHeight,view?.height||0):(view && view.scale===1?view.height:innerHeight));
  document.documentElement.style.setProperty('--viewport-width',width+'px');
  document.documentElement.style.setProperty('--viewport-height',height+'px');
  if(!unityInstance)return;
  const bounds=canvas.getBoundingClientRect(),css=getComputedStyle($('safe-area-probe'));
  if(!bounds.width||!bounds.height)return;
  const clamp=v=>Math.max(0,Math.min(.4,v));
  const insets=JSON.stringify({
    left:clamp(((parseFloat(css.paddingLeft)||0)-bounds.left)/bounds.width),
    top:clamp(((parseFloat(css.paddingTop)||0)-bounds.top)/bounds.height),
    right:clamp(((parseFloat(css.paddingRight)||0)-(width-bounds.right))/bounds.width),
    bottom:clamp(((parseFloat(css.paddingBottom)||0)-(height-bounds.bottom))/bounds.height)
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
  $('game-menu').hidden=!immersive||touchDevice;setMenu(false);syncViewport();scheduleViewport();
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
document.addEventListener('fullscreenchange',()=>{if(unityInstance)setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));else scheduleViewport();});
new ResizeObserver(scheduleViewport).observe(canvas);
syncViewport();

$('help').addEventListener('click', () => {
  pauseGame();setMenu(false);
  $('guide').showModal();
});
$('close-help').addEventListener('click', () => {$('guide').close();canvas.focus();});
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
  if(!window.WebAssembly || !document.createElement('canvas').getContext('webgl2')) {
    $('load-status').textContent='Thiết bị cần trình duyệt hỗ trợ WebGL 2 và WebAssembly. Hãy mở bằng Chrome, Edge hoặc Safari mới nhất.';return;
  }
  // Invoke while the tap still grants user activation, before loading WebAssembly.
  if(touchDevice)void requestNativeFullscreen();
  starting=true;$('play').disabled=true;$('play').textContent='ĐANG MỞ PHỐ…';$('progress').hidden=false;
  $('load-status').textContent='Đang tải game, lần đầu có thể mất một chút thời gian…';
  const {loaderUrl,...build}=window.PHO_VIET_BUILD;
  const config={...build,streamingAssetsUrl:'StreamingAssets',
    devicePixelRatio:Math.min(window.devicePixelRatio || 1,matchMedia('(pointer: coarse)').matches?1.5:2),
    showBanner:(message,type) => {if(type==='error')fail(message);else console.warn(message);}
  };
  const script=document.createElement('script');script.src=loaderUrl;
  script.onerror=()=>fail('Could not load Unity loader');
  script.onload=async()=> {
    try {
      canvas.hidden=false;
      unityInstance=await createUnityInstance(canvas,config,p=>{
        $('progress').value=p;$('load-status').textContent=`Đang tải game · ${Math.round(p*100)}%`;
      });
      window.phoVietInstance=unityInstance;
      setImmersive(touchDevice||standalone||Boolean(document.fullscreenElement));
      $('welcome').hidden=true;canvas.focus();
      if(document.hidden) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
    }catch(error){canvas.hidden=true;fail(error);}
  };
  document.body.appendChild(script);
});
document.addEventListener('visibilitychange',()=>{
  if(document.hidden && unityInstance) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
  if(!document.hidden)scheduleViewport();
});
window.addEventListener('pageshow',scheduleViewport);
canvas.addEventListener('contextmenu',e=>e.preventDefault());
window.addEventListener('keydown',e=>{
  if(unityInstance && document.activeElement===canvas && ['Space','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.code))e.preventDefault();
});

if('serviceWorker' in navigator) {
  window.addEventListener('load',async()=>{
    try {
      const reg=await navigator.serviceWorker.register('./sw.js',{updateViaCache:'none'});
      const ready=()=>{$('offline-status').textContent='Đã lưu để chơi offline trên máy này';};
      if(reg.active)ready();
      reg.addEventListener('updatefound',()=>{
        const worker=reg.installing;
        worker.addEventListener('statechange',()=>{
          if(worker.state==='activated')ready();
          else if(worker.state==='installed' && reg.active)$('offline-status').textContent='Có bản mới · đóng và mở lại để cập nhật';
        });
      });
    }catch(error){console.warn('Offline cache unavailable',error);}
  });
}
