# 21.345 — 4 icon xe vàng theo phong cách mẫu D «Không chiến đêm» + E «Đường đua năng lượng» (21.165):
# hình phẳng, viền đậm, nét trang trí mờ, không phát sáng. Ghép _defs.txt (gradient + xe vàng nhìn từ trên) vào từng SVG.
# Chạy: python -X utf8 build.py  → k1..k4-*.svg ; rồi  sh ../render.sh (từ thư mục cha với đường dẫn kieu-de/...)
import os
HERE = os.path.dirname(os.path.abspath(__file__))
DEFS = open(os.path.join(HERE, '_defs.txt'), encoding='utf8').read()

def svg(title, bg, body):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>Phố Việt · {title}</title>'
            f'<defs>{DEFS}{bg}</defs>\n{body}</svg>\n')

def car(x, y, a=0, s=1.0):
    return f'<use href="#carTop" transform="translate({x} {y}) rotate({a}) scale({s}) translate(-256 -305)"/>'

# xe vàng nhìn từ sau (vẽ lại từ mẫu D, đổi màu vàng + nắp máy đen)
CAR_REAR = '''<g transform="translate(-12 29) scale(1.035)"><g stroke-linejoin="round">
  <path d="M155 333q-7 0-10 13l-6 39q0 13 15 14l27 1 7-33h136l7 33 27-1q15-1 15-14l-6-39q-3-13-10-13" fill="#0a0c14"/>
  <path d="m166 316-18-10-11 5 3 11 24 8m182-14 18-10 11 5-3 11-24 8" fill="#ffcf22" stroke="#4a2a00" stroke-width="5"/>
  <path d="m177 300 21-38q12-21 35-23h46q23 2 35 23l21 38 21 20 11 36-5 23-32 15H182l-32-15-5-23 11-36Z" fill="url(#body)" stroke="#4a2a00" stroke-width="7"/>
  <path d="M211 260q18-10 45-9 28-1 45 9l22 35-28 21h-78l-28-21Z" fill="#17161c" stroke="#4a2a00" stroke-width="5"/>
  <path d="M247 252h18l3 64h-24Z" fill="#ffc81e"/>
  <path d="m214 283 30 0m24 0h30m-79 11h27m24 0h27m-66 12h20m24 0h20" stroke="#4a4652" stroke-width="4"/>
  <path d="m170 315 24-25 9 30-24 24-23-5Zm172 0-24-25-9 30 24 24 23-5Z" fill="#e39a00"/>
  <path d="m203 317 21 9h64l21-9 15 27-15 12H203l-15-12Z" fill="#c98200"/>
  <path d="M146 347l29 8 34 2 12-8h70l12 8 34-2 29-8-4 31-32 16H182l-32-16Z" fill="url(#rear)" stroke="#0c0b10" stroke-width="4"/>
  <path d="m157 354 22 10 24-2m152-8-22 10-24-2" fill="none" stroke="#ff566b" stroke-width="8" stroke-linecap="round"/>
  <path d="m158 352 22 10m174-10-22 10" fill="none" stroke="#ffd1bb" stroke-width="3" stroke-linecap="round"/>
  <path d="m194 382 21-10h82l21 10m-90-14h56" fill="none" stroke="#4a4652" stroke-width="3"/>
  <path d="M187 384h15m108 0h15" stroke="#bed4e8" stroke-width="4" stroke-linecap="round"/>
  <path d="m180 296 23-32 18-13m111 45-23-32-18-13" fill="none" stroke="#fff6c0" stroke-width="4" stroke-linecap="round"/>
</g></g>'''

MISSILE_UP = '''<g stroke-linejoin="round">
 <path d="M-6 16 0 58 6 16Z" fill="url(#jetv)"/>
 <path d="M-10 26v34m20-34v26" stroke="#f6a65f" stroke-width="3" stroke-linecap="round" opacity=".6"/>
 <path d="M-8 18h16l8 10v6l-10-4h-12l-10 4v-6Z" fill="#6cbfff" stroke="#16254b" stroke-width="3"/>
 <path d="M-8-14v32h16v-32q0-16-8-24-8 8-8 24Z" fill="#f7f2e9" stroke="#16254b" stroke-width="3"/>
 <path d="M-7-14q1-14 7-22 6 8 7 22Z" fill="#f75851"/>
</g>'''

TANK_DOWN = '''<g stroke-linejoin="round">
 <rect x="-50" y="-62" width="24" height="124" rx="6" fill="#1f2619" stroke="#0e120b" stroke-width="3"/>
 <rect x="26" y="-62" width="24" height="124" rx="6" fill="#1f2619" stroke="#0e120b" stroke-width="3"/>
 <path d="M-46-50h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16M30-50h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16m-16 14h16" stroke="#46503a" stroke-width="3"/>
 <rect x="-32" y="-56" width="64" height="112" rx="10" fill="url(#olive)" stroke="#232b14" stroke-width="5"/>
 <rect x="-5" y="18" width="10" height="70" rx="3" fill="#56622f" stroke="#232b14" stroke-width="4"/>
 <circle r="27" cy="4" fill="#7c8a45" stroke="#232b14" stroke-width="5"/>
 <circle r="8" cx="-8" cy="-4" fill="#56622f" stroke="#232b14" stroke-width="3"/>
 <path d="M10-22 16-30" stroke="#232b14" stroke-width="4" stroke-linecap="round"/>
 <circle r="7" cx="0" cy="-44" fill="#da251d" stroke="#232b14" stroke-width="2"/>
</g>'''

POLICE = '''<g stroke-linejoin="round">
 <path d="M213 257h18v36h-18zm68 0h18v36h-18zm-68 76h18v36h-18zm68 0h18v36h-18z" fill="#07142c"/>
 <path d="m223 261 8-20 15-8h20l15 8 8 20 4 97-9 20h-56l-9-20Z" fill="url(#police)" stroke="#1b2a48" stroke-width="5"/>
 <path d="m233 269 13-8h20l13 8-4 26h-38Z" fill="url(#glass)" stroke="#cfe0f5" stroke-width="3"/>
 <path d="M224 300h64v10h-64z" fill="#2c66d8"/>
 <rect x="236" y="298" width="20" height="12" rx="3" fill="#ff3b4e" stroke="#1b2a48" stroke-width="2"/>
 <rect x="256" y="298" width="20" height="12" rx="3" fill="#3b8cff" stroke="#1b2a48" stroke-width="2"/>
 <path d="M238 318h36v24h-36z" fill="url(#glass)" stroke="#cfe0f5" stroke-width="3"/>
 <path d="m225 257 15-9m47 9-15-9" stroke="#edfcff" stroke-width="4" stroke-linecap="round"/>
 <path d="m225 358 13 7m49-7-13 7" stroke="#ff626f" stroke-width="5" stroke-linecap="round"/>
</g>'''

ROAD = lambda d, w=78: (f'<path d="{d}" fill="none" stroke="#0b1e32" stroke-width="{w+27}"/>'
                        f'<path d="{d}" fill="none" stroke="#57d8cc" stroke-width="{w+11}"/>'
                        f'<path d="{d}" fill="none" stroke="#1b384c" stroke-width="{w}"/>'
                        f'<path d="{d}" fill="none" stroke="#e7d581" stroke-width="4" stroke-dasharray="18 20"/>')

def palm(x, y, r=26, a=0):
    leaves = ''.join(f'<path d="M0 0 Q{r*.5:.1f} {-r*.32:.1f} {r:.1f} 0 Q{r*.5:.1f} {r*.32:.1f} 0 0Z" transform="rotate({a+i*60})"/>' for i in range(6))
    return f'<g transform="translate({x} {y})" fill="#2f8f63" stroke="#14513a" stroke-width="3" stroke-linejoin="round">{leaves}<circle r="6" fill="#7a5a2a" stroke="#3d2c12"/></g>'

icons = {}

# 1 — Đêm giông né sét (nhìn từ sau, như D)
icons['k1-dem-giong-ne-set'] = svg('Đêm giông né sét',
 '<linearGradient id="bg" x2="30%" y2="100%"><stop offset="0" stop-color="#0e1030"/><stop offset="0.54" stop-color="#2f2e70"/><stop offset="1" stop-color="#121433"/></linearGradient>',
 f'''<rect width="512" height="512" fill="url(#bg)"/>
 <path d="M0 0h512v74q-30 22-64 10-26 26-66 12-30 24-70 6-34 22-74 4-34 20-70 2-34 18-66 0-30 12-62-6Z" fill="#1d1f4e"/>
 <path d="M0 74q32 18 62 6 32 18 66 0 36 18 70-2 40 18 74-4 40 18 70-6 40 14 66-12 34 12 64-10" fill="none" stroke="#5b5fb4" stroke-width="5" opacity=".7"/>
 <path d="M452 70 424 128h22l-28 56h20l-32 52 64-72h-24l28-48h-22l18-38Z" fill="#f2f0ff" stroke="#9c8cff" stroke-width="4" stroke-linejoin="round"/>
 <path d="M406 238l-14-4m20 14-4 14m22-20 14 4" stroke="#ffd36b" stroke-width="4" stroke-linecap="round"/>
 <path d="M0 121h42v64h25v-33h31v97h34v-43h28v82H0Zm512 16h-47v27h-30v70h-37v-21h-32v77h146Z" fill="#141a3e"/>
 <path d="M17 156h12m-12 27h12m51 7h8m-8 20h8m-8 20h8m388-54h21m-21 23h21m-51 23h8m-8 20h8" stroke="#ffbf6b" stroke-width="4" opacity=".55"/>
 <path d="M42 512 238 271h44l193 241Z" fill="#141f3b"/>
 <path d="M51 512 238 271m230 241L282 271" stroke="#345889" stroke-width="6"/>
 <path d="M259 278v16m0 15v27m0 23v63m0 32v58" stroke="#ffcf3a" stroke-width="5"/>
 
 <path d="M60 100l-10 26m120-6-10 26m260-20-10 26m-30 80-10 26M90 260l-10 26m380 40-10 26M40 380l-10 26" stroke="#9fb0ff" stroke-width="3" stroke-linecap="round" opacity=".45"/>
 <ellipse cx="253" cy="410" rx="110" ry="21" fill="#090e1d"/>
 {CAR_REAR}
 <path d="m169 413-31 31m198-31 31 31" stroke="#ffcf3a" stroke-width="4" opacity=".5" stroke-linecap="round"/>''')

# 2 — Tên lửa hạ xe tăng (nhìn từ trên, như E)
RD2 = 'M226 560C236 440 286 330 286 220S270 60 276 -40'
icons['k2-ten-lua-ha-xe-tang'] = svg('Tên lửa hạ xe tăng',
 '<linearGradient id="bg" x2="100%" y2="100%"><stop offset="0" stop-color="#173544"/><stop offset="0.52" stop-color="#0f5b60"/><stop offset="1" stop-color="#0a2636"/></linearGradient>',
 f'''<rect width="512" height="512" fill="url(#bg)"/>
 <path d="M0 0h150v170H0Zm0 230h120v150H0Zm392 0h120v120H392Zm20-230h100v190H412Z" fill="#1d4c57" stroke="#2f7880" stroke-width="3"/>
 <path d="M20 30h40v40H20zm60 0h40v40H80zm-60 70h40v40H20zm412-60h40v40h-40zm0 70h40v40h-40zM16 260h36v36H16zm0 60h36v36H16zm396-60h36v36h-36z" fill="#13394a"/>
 <path d="M70 0v120m52-120v85m300 395V394m49 118V395M0 454h180" stroke="#63b8b5" opacity=".25" stroke-width="2"/>
 {ROAD(RD2, 96)}
 {palm(150, 440)}{palm(380, 420)}{palm(110, 210, 22)}{palm(420, 210, 22)}
 <g transform="translate(278 92) rotate(4)">{TANK_DOWN}</g>
 <path d="M248 150l-14-10m60 12 14-12m-42-14v-16" stroke="#ffd36b" stroke-width="5" stroke-linecap="round"/>
 <g transform="translate(276 214) rotate(-4)">{MISSILE_UP}</g>
 {car(248, 392, -8, 1.02)}''')

# 3 — Đại lộ ven sông (nhìn từ trên, như E, có sông + dừa + huy hiệu sao)
RD3 = 'M-15 430C110 430 160 390 200 330c26-40 34-76 76-102 34-22 70-18 96-50 24-30 24-80 14-190'
icons['k3-dai-lo-ven-song'] = svg('Đại lộ ven sông',
 '<linearGradient id="bg" x2="100%" y2="100%"><stop offset="0" stop-color="#143946"/><stop offset="0.52" stop-color="#096669"/><stop offset="1" stop-color="#092839"/></linearGradient><linearGradient id="water" x2="100%" y2="100%"><stop offset="0" stop-color="#2d7fb8"/><stop offset="1" stop-color="#174c86"/></linearGradient>',
 f'''<rect width="512" height="512" fill="url(#bg)"/>
 <path d="M0 0h300c-6 60-20 96-60 120-44 26-70 40-96 84-24 42-60 70-144 76Z" fill="url(#water)" stroke="#8fd8ff" stroke-width="4"/>
 <path d="M40 60q14-8 28 0t28 0m40 30q14-8 28 0t28 0M40 160q14-8 28 0t28 0m90-110q14-8 28 0t28 0" fill="none" stroke="#a9e6ff" stroke-width="3" opacity=".6" stroke-linecap="round"/>
 <g transform="translate(118 116) rotate(-30)" stroke-linejoin="round"><path d="M-30-10h44l18 10-18 10h-44Z" fill="#c8422e" stroke="#4a160e" stroke-width="3"/><path d="M-20-6h22v12h-22Z" fill="#f0e2c4" stroke="#4a160e" stroke-width="2"/><path d="M-46 0h-16m14 8h-22" stroke="#a9e6ff" stroke-width="3" stroke-linecap="round" opacity=".7"/></g>
 <path d="M0 95h0M300 0h212v170q-40 30-90 30" fill="none"/>
 <path d="M70 512V400m52 112v-85m300 85V394m49 118V395M412 0v120m52-120v85" stroke="#63b8b5" opacity=".25" stroke-width="2"/>
 {ROAD(RD3)}
 {palm(60, 320, 22)}{palm(236, 430)}{palm(330, 380)}{palm(440, 300)}{palm(470, 210, 22)}
 {car(140, 392, 64, 1.0)}
 <circle cx="352" cy="102" r="52" fill="#3a1414" stroke="#ff8f7a" stroke-width="3"/>
 <circle cx="352" cy="102" r="43" fill="#da251d" stroke="#ffd9a0" stroke-width="3"/>
 <path d="M352 70l9.4 29h30.5l-24.7 17.9 9.4 29-24.6-17.9-24.6 17.9 9.4-29-24.7-17.9h30.5Z" fill="#ffd400" stroke="#8a4a00" stroke-width="3" stroke-linejoin="round"/>
 <path d="M296 44v18m-9-9h18m100 96v18m-9-9h18" stroke="#ffe7b0" stroke-width="3" stroke-linecap="round"/>''')

# 4 — CSGT truy đuổi (nhìn từ trên)
RD4 = 'M90 560L420 -48'
icons['k4-csgt-truy-duoi'] = svg('CSGT truy đuổi',
 '<linearGradient id="bg" x2="100%" y2="100%"><stop offset="0" stop-color="#11183a"/><stop offset="0.52" stop-color="#22336a"/><stop offset="1" stop-color="#0d1430"/></linearGradient>',
 f'''<rect width="512" height="512" fill="url(#bg)"/>
 <path d="M0 0h200L60 260H0Zm512 512H310l146-268h56Z" fill="#1a2550" stroke="#33478a" stroke-width="3"/>
 <path d="M20 30h50v40H20zm70 0h50v40H90zM20 100h50v40H20zm0 70h30v40H20zm420 110h50v40h-50zm0 70h50v40h-50zm-70 70h50v40h-50z" fill="#121a3c"/>
 <path d="M30 40h10m30 0h10M30 110h10m400 180h10m-10 70h10" stroke="#ffbf6b" stroke-width="5" opacity=".6"/>
 <path d="M300 0v60m40-60v30m-340 420h80m340-450v120m40-120v80" stroke="#6f8ad0" opacity=".25" stroke-width="2"/>
 <path d="{RD4}" stroke="#0b1430" stroke-width="196"/>
 <path d="{RD4}" stroke="#4f6fc8" stroke-width="180"/>
 <path d="{RD4}" stroke="#1a2444" stroke-width="168"/>
 <path d="{RD4}" stroke="#e7d581" stroke-width="5" stroke-dasharray="22 20"/>
 <path d="M42 520 372 -88m-200 650L502 -46" stroke="#33477e" stroke-width="3" stroke-dasharray="14 18"/>
 <circle cx="166" cy="372" r="74" fill="#ff3b4e" opacity=".16"/><circle cx="196" cy="398" r="74" fill="#3b8cff" opacity=".16"/>
 <g transform="translate(178 398) rotate(31) scale(.95) translate(-256 -305)">{POLICE}</g>
 <path d="M150 300l-18-8m14 22-20 2m96-28 4-18" stroke="#ff5a6a" stroke-width="4" stroke-linecap="round"/>
 <path d="M268 320l-26 44m40-36-26 44m40-36-26 44" stroke="#c9d6ff" stroke-width="3" opacity=".4" stroke-linecap="round"/>
 {car(322, 176, 31, 1.08)}''')

for k, v in icons.items():
    open(os.path.join(HERE, k + '.svg'), 'w', encoding='utf8').write(v)
print(list(icons))
