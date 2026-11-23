'use strict';
// 21.356: GờTa Việt chuyển sang https://gotaviet.web.app/. Service worker này thay bản cũ trên GitHub Pages:
// xóa toàn bộ bộ nhớ đệm offline của bản cũ, tự gỡ đăng ký và đưa các cửa sổ đang mở sang địa chỉ mới.
const TARGET='https://gotaviet.com/'; // 21.367: tên miền chính mới
self.addEventListener('install',()=>self.skipWaiting());
self.addEventListener('activate',event=>{
  event.waitUntil((async()=>{
    await Promise.all((await caches.keys()).map(key=>caches.delete(key)));
    await self.clients.claim();
    for(const client of await self.clients.matchAll({type:'window'}))try{await client.navigate(TARGET);}catch(_){}
    await self.registration.unregister();
  })());
});
self.addEventListener('fetch',event=>{
  if(event.request.mode==='navigate')event.respondWith(Response.redirect(TARGET,302));
});
