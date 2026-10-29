'use strict';
const PREFIX='phoviet:'+self.registration.scope+':';
const CACHE=PREFIX+'72d4794dd7d8a0a4';
const ROOT=new URL('./',self.location.href);
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
async function offlineStatus(client){
  // Late callbacks from a retired worker must not recreate its deleted cache.
  if(!await caches.has(CACHE))return;
  const {files,dataVariants=[]}=await manifest(),cache=await caches.open(CACHE);
  const stored=new Set((await cache.keys()).map(request=>request.url));
  const alternatives=new Set(dataVariants);
  const ready=files.filter(file=>!alternatives.has(file)).every(file=>stored.has(new URL(file,ROOT).href))&&
    (!dataVariants.length||dataVariants.some(file=>stored.has(new URL(file,ROOT).href)));
  const message={type:'PHOVIET_OFFLINE_STATUS',ready};
  if(client)client.postMessage(message);
  else for(const view of await self.clients.matchAll())view.postMessage(message);
}
self.addEventListener('install',event=>{
  // Cache the small shell first, one response at a time. Fetching a second copy
  // of the whole Unity build during startup can kill mobile WebKit.
  // Runtime files are cached from their actual game requests below.
  event.waitUntil((async()=>{
    const {files}=await manifest();
    const cache=await caches.open(CACHE);
    try {for(const file of files.filter(file=>!file.startsWith('Build/')))await cache.add(new Request(new URL(file,ROOT),{cache:'reload'}));}
    catch(error){await caches.delete(CACHE);throw error;}
  })());
});
self.addEventListener('activate',event=>{
  event.waitUntil((async()=>{
    await Promise.all((await caches.keys()).filter(key=>key.startsWith(PREFIX)&&key!==CACHE).map(key=>caches.delete(key)));
    await self.clients.claim();
    await offlineStatus();
  })());
});
self.addEventListener('message',event=>{
  if(event.data?.type==='PHOVIET_CHECK_OFFLINE')event.waitUntil(offlineStatus(event.source).catch(()=>{}));
});
self.addEventListener('fetch',event=>{
  const url=new URL(event.request.url);
  if(event.request.method!=='GET'||url.origin!==ROOT.origin||!url.pathname.startsWith(ROOT.pathname))return;
  if(['sw.js','precache.json','build-info.json'].includes(url.pathname.substring(ROOT.pathname.length)))return;
  // Register the lifetime promise synchronously, before awaiting network I/O.
  let finishWrite;
  event.waitUntil(new Promise(resolve=>{finishWrite=resolve;}));
  event.respondWith((async()=>{
    if(!await caches.has(CACHE)){finishWrite();return fetch(event.request);}
    const cache=await caches.open(CACHE);
    if(event.request.mode==='navigate'){finishWrite();return (await cache.match(new URL('index.html',ROOT)))||fetch(event.request);}
    const hit=await cache.match(event.request);if(hit){finishWrite();return hit;}
    const response=await fetch(event.request);
    const relative=url.pathname.substring(ROOT.pathname.length);
    if(!response.ok||!(await manifest()).files.includes(relative)){finishWrite();return response;}
    // Stream to the cache without materializing an ArrayBuffer in JavaScript.
    const write=cache.put(event.request,response.clone()).then(()=>offlineStatus()).catch(()=>{});
    write.finally(finishWrite);
    return response;
  })().catch(error=>{finishWrite();throw error;}));
});
