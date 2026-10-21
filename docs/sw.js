'use strict';
const PREFIX='phoviet:'+self.registration.scope+':';
const CACHE=PREFIX+'d0d0e5b1e9e91d91';
const ROOT=new URL('./',self.location.href);
self.addEventListener('install',event=>{
  // Install only succeeds when the entire build is cached. Keep old versions alive until all old tabs close.
  event.waitUntil((async()=>{
    const response=await fetch(new URL('precache.json',ROOT),{cache:'no-store'});
    if(!response.ok)throw new Error('Offline manifest unavailable');
    const {files}=await response.json();
    const cache=await caches.open(CACHE);
    try {await cache.addAll(files.map(file=>new Request(new URL(file,ROOT),{cache:'reload'})));}
    catch(error){await caches.delete(CACHE);throw error;}
  })());
});
self.addEventListener('activate',event=>{
  event.waitUntil((async()=>{
    await Promise.all((await caches.keys()).filter(key=>key.startsWith(PREFIX)&&key!==CACHE).map(key=>caches.delete(key)));
    await self.clients.claim();
  })());
});
self.addEventListener('fetch',event=>{
  const url=new URL(event.request.url);
  if(event.request.method!=='GET'||url.origin!==ROOT.origin||!url.pathname.startsWith(ROOT.pathname))return;
  if(['sw.js','precache.json','build-info.json'].includes(url.pathname.substring(ROOT.pathname.length)))return;
  event.respondWith((async()=>{
    const cache=await caches.open(CACHE);
    if(event.request.mode==='navigate')return (await cache.match(new URL('index.html',ROOT)))||fetch(event.request);
    return (await cache.match(event.request))||fetch(event.request);
  })());
});
