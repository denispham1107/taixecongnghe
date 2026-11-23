'use strict';
const PREFIX='phoviet:'+self.registration.scope+':';
const CACHE=PREFIX+'__PHOVIET_CACHE__';
// 21.360: dữ liệu game (Build/*, tên tệp có mã băm nội dung nên không bao giờ đổi nội dung) nằm trong một kho RIÊNG, không theo
// mã cache của vỏ trang. Trước đây mọi tệp chung một kho theo mã băm của cả bản → chỉ sửa vỏ trang (21.348/21.349/21.356/21.359)
// là máy xóa sạch và tải lại ~110 MB dữ liệu game dù bản build không đổi. Kho riêng chỉ bỏ các tệp không còn trong precache.json.
const BUILD=PREFIX+'build';
// 21.368: tên miền chính gotaviet.com. Worker này khi chạy trên gotaviet.web.app / gotaviet.firebaseapp.com (máy đã cài app từ
// địa chỉ cũ hoặc còn giữ trang cũ) không lưu gì nữa: xóa kho cũ, đưa các cửa sổ đang mở và mọi lần mở trang sang gotaviet.com.
const HOME='https://gotaviet.com';
const MOVED=['gotaviet.web.app','gotaviet.firebaseapp.com'].includes(self.location.hostname);
const movedUrl=url=>HOME+url.pathname+url.search+url.hash;
const ROOT=new URL('./',self.location.href);
// 21.278: Safari/WebKit (mọi trình duyệt trên iPhone/iPad) giới hạn bộ nhớ chặt — không sao chép gói dữ liệu Unity
// (70–90 MB) vào bộ nhớ đệm trong lúc game đang tải; các tệp nhỏ vẫn lưu offline như cũ.
// 21.360: thay vào đó, khi game đã mở xong app.js nhắn PHOVIET_CACHE_DATA → worker tự lưu gói dữ liệu (cache.add, trình duyệt
// ghi thẳng xuống đĩa, không qua bộ nhớ JavaScript). Lần sau vào game không phải tải lại.
const WEBKIT=/AppleWebKit/.test(self.navigator.userAgent)&&!/Chrome|Chromium|Edg|Android|Firefox/.test(self.navigator.userAgent);
const isBuild=relative=>relative.startsWith('Build/');
let manifestPromise;
function manifest(){
  return manifestPromise ||= (async()=>{
    const cache=await caches.open(CACHE),url=new URL('precache.json',ROOT);
    const stored=await cache.match(url);if(stored)return stored.json();
    const response=await fetch(url,{cache:'no-store'});
    if(!response.ok)throw new Error('Offline manifest unavailable');
    await cache.put(url,response.clone()).catch(()=>{});
    return response.json();
  })().catch(error=>{manifestPromise=null;throw error;});
}
async function storedUrls(){
  const urls=new Set();
  for(const name of [CACHE,BUILD])for(const request of await (await caches.open(name)).keys())urls.add(request.url);
  return urls;
}
async function offlineStatus(client){
  // Late callbacks from a retired worker must not recreate its deleted cache.
  if(!await caches.has(CACHE))return;
  const {files,dataVariants=[]}=await manifest(),stored=await storedUrls();
  const alternatives=new Set(dataVariants);
  const ready=files.filter(file=>!alternatives.has(file)).every(file=>stored.has(new URL(file,ROOT).href))&&
    (!dataVariants.length||dataVariants.some(file=>stored.has(new URL(file,ROOT).href)));
  const message={type:'PHOVIET_OFFLINE_STATUS',ready,online:!ready};
  if(client)client.postMessage(message);
  else for(const view of await self.clients.matchAll())view.postMessage(message);
}
self.addEventListener('install',event=>{
  // 21.278: bản mới thay ngay bản cũ (không chờ đóng hết tab) để máy không kẹt ở bản đã lưu.
  self.skipWaiting();
  if(MOVED)return;
  // Cache the small shell first, one response at a time. Fetching a second copy
  // of the whole Unity build during startup can kill mobile WebKit.
  // Runtime files are cached from their actual game requests below.
  event.waitUntil((async()=>{
    const {files}=await manifest();
    const cache=await caches.open(CACHE);
    try {for(const file of files.filter(file=>!isBuild(file)))await cache.add(new Request(new URL(file,ROOT),{cache:'reload'}));}
    catch(error){await caches.delete(CACHE);throw error;}
  })());
});
self.addEventListener('activate',event=>{
  if(MOVED){
    event.waitUntil((async()=>{
      await Promise.all((await caches.keys()).filter(key=>key.startsWith('phoviet:')).map(key=>caches.delete(key)));
      await self.clients.claim();
      for(const client of await self.clients.matchAll({type:'window'}))try{await client.navigate(movedUrl(new URL(client.url)));}catch(_){}
    })());
    return;
  }
  event.waitUntil((async()=>{
    const {files}=await manifest(),keep=new Set(files.filter(isBuild).map(file=>new URL(file,ROOT).href));
    const build=await caches.open(BUILD);
    // Chuyển dữ liệu game còn dùng từ các kho cũ (bản trước 21.360 lưu chung kho theo mã cache) sang kho riêng rồi mới xóa kho cũ.
    for(const key of (await caches.keys()).filter(key=>key.startsWith(PREFIX)&&key!==CACHE&&key!==BUILD)){
      const old=await caches.open(key);
      for(const request of await old.keys()){
        if(!keep.has(request.url)||await build.match(request))continue;
        const response=await old.match(request);if(response)await build.put(request,response).catch(()=>{});
      }
      await caches.delete(key);
    }
    // Bỏ dữ liệu của bản build cũ không còn dùng.
    for(const request of await build.keys())if(!keep.has(request.url))await build.delete(request);
    await self.clients.claim();
    await offlineStatus();
  })());
});
async function cacheData(url){
  const target=new URL(url,ROOT),relative=target.pathname.substring(ROOT.pathname.length);
  if(target.origin!==ROOT.origin||!isBuild(relative)||!(await manifest()).files.includes(relative))return;
  const build=await caches.open(BUILD);
  if(await build.match(target.href))return;
  // Trình duyệt tự ghi phản hồi xuống đĩa; có sẵn trong bộ nhớ đệm HTTP (Build/* immutable) thì không tải lại.
  await build.add(new Request(target.href)).catch(()=>{});
}
self.addEventListener('message',event=>{
  if(MOVED)return;
  if(event.data?.type==='PHOVIET_CHECK_OFFLINE')event.waitUntil(offlineStatus(event.source).catch(()=>{}));
  if(event.data?.type==='PHOVIET_CACHE_DATA')event.waitUntil(cacheData(event.data.url).then(()=>offlineStatus()).catch(()=>{}));
});
self.addEventListener('fetch',event=>{
  const url=new URL(event.request.url);
  if(MOVED){
    if(event.request.mode==='navigate'&&url.origin===ROOT.origin&&!url.pathname.startsWith('/__/'))event.respondWith(Response.redirect(movedUrl(url),302));
    return;
  }
  if(event.request.method!=='GET'||url.origin!==ROOT.origin||!url.pathname.startsWith(ROOT.pathname))return;
  if(['sw.js','precache.json','build-info.json'].includes(url.pathname.substring(ROOT.pathname.length)))return;
  // 21.356: trang đăng nhập Google của Firebase (/__/auth/handler, /__/firebase/init.json) phải đi thẳng ra mạng — không trả index.html.
  if(url.pathname.startsWith('/__/'))return;
  // Register the lifetime promise synchronously, before awaiting network I/O.
  let finishWrite;
  event.waitUntil(new Promise(resolve=>{finishWrite=resolve;}));
  event.respondWith((async()=>{
    if(!await caches.has(CACHE)){finishWrite();return fetch(event.request);}
    const relative=url.pathname.substring(ROOT.pathname.length);
    const cache=await caches.open(isBuild(relative)?BUILD:CACHE);
    if(event.request.mode==='navigate'){finishWrite();return (await (await caches.open(CACHE)).match(new URL('index.html',ROOT)))||fetch(event.request);}
    const hit=await cache.match(event.request);if(hit){finishWrite();return hit;}
    const response=await fetch(event.request);
    if(!response.ok||!(await manifest()).files.includes(relative)){finishWrite();return response;}
    if(WEBKIT&&relative.endsWith('.data.unityweb')){finishWrite();return response;}   // lưu sau khi game mở xong (PHOVIET_CACHE_DATA)
    // Stream to the cache without materializing an ArrayBuffer in JavaScript.
    const write=cache.put(event.request,response.clone()).then(()=>offlineStatus()).catch(()=>{});
    write.finally(finishWrite);
    return response;
  })().catch(error=>{finishWrite();throw error;}));
});
