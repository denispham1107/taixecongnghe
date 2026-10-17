/* The shell contains no gameplay timers: Unity owns the run and touch input. */
'use strict';
const $ = id => document.getElementById(id);
const canvas = $('unity-canvas');
let unityInstance, installPrompt, starting = false;
const standalone = matchMedia('(display-mode: standalone)').matches || navigator.standalone;
const ios = /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);

$('help').addEventListener('click', () => {
  if (unityInstance) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
  $('guide').showModal();
});
$('close-help').addEventListener('click', () => {$('guide').close();canvas.focus();});
$('guide').addEventListener('click', e => {if(e.target === $('guide')) $('guide').close();});
$('retry').addEventListener('click', () => location.reload());
$('fullscreen').addEventListener('click', async () => {
  try {
    if(document.fullscreenElement) await document.exitFullscreen();
    else if(document.documentElement.requestFullscreen) await document.documentElement.requestFullscreen();
    else if(unityInstance) unityInstance.SetFullscreen(1);
  } catch { /* iOS can use the standalone home-screen application. */ }
});
window.addEventListener('beforeinstallprompt', e => {
  e.preventDefault();installPrompt=e;
  if(!standalone) $('install').hidden=false;
});
window.addEventListener('appinstalled', () => {$('install').hidden=true;installPrompt=null;});
if(ios && !standalone) {$('install').hidden=false;$('install').textContent='Thêm vào màn hình';}
$('install').addEventListener('click', async () => {
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
      $('welcome').hidden=true;canvas.focus();
      if(document.hidden) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
    }catch(error){canvas.hidden=true;fail(error);}
  };
  document.body.appendChild(script);
});
document.addEventListener('visibilitychange',()=>{
  if(document.hidden && unityInstance) unityInstance.SendMessage('PHỐ VIỆT • Chuyến xe không ngừng','PauseForBrowser');
});
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
