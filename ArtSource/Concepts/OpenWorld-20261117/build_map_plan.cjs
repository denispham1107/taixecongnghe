// Sinh sơ đồ bản đồ mở Phố Việt (21.238) từ layout.json → map-plan.svg.
// Chạy: node ArtSource/Concepts/OpenWorld-20261117/build_map_plan.cjs
const fs = require('fs');
const path = require('path');
const dir = __dirname;
const L = JSON.parse(fs.readFileSync(path.join(dir, 'layout.json'), 'utf8'));

const S = 0.7, PAD = 40;
const W = (L.bounds.maxX - L.bounds.minX) * S + PAD * 2;
const H = (L.bounds.maxZ - L.bounds.minZ) * S + PAD * 2 + 150;
const X = x => PAD + (x - L.bounds.minX) * S;
const Y = z => PAD + (L.bounds.maxZ - z) * S; // z bắc hướng lên trên

const label = {
  R1: 'Đại lộ Lê Lợi', R2: 'Tôn Đức Thắng', R3: 'Bạch Đằng', R4: 'Hai Bà Trưng',
  L1: 'Lý Thường Kiệt', L2: 'Trần Hưng Đạo', L3: 'Quang Trung', L4: 'Lý Nam Đế',
  L5: 'Đinh Bộ Lĩnh', L6: 'Nguyễn Trãi', L7: 'Pasteur', L8: 'Đồng Khởi',
  E1: 'Ven sông Thủ Thiêm', E2: 'Mai Chí Thọ', N1: 'Ven sông bờ bắc',
  B1: 'Cầu vòm Ánh Dương', B2: 'Cầu vòm phía bắc', Q1: 'Bùng binh',
  M1: 'Chợ Bến Thành', M2: 'Tháp Tài Chính', M3: 'Cụm cao ốc', M4: 'Căn hộ Thủ Thiêm', M5: 'Bến tàu',
  D1: 'Phố chợ Bến Thành', D2: 'Phố ven sông', D3: 'Khu tài chính', D4: 'Phố cổ trung tâm', D5: 'Bờ đông Thủ Thiêm', D6: 'Bờ bắc'
};
const tint = { D1: '#EAD7B5', D3: '#D5DCE4', D4: '#E6DCC2', D5: '#D4E2D0', D6: '#DCE7D2' };

let o = [];
o.push(`<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" font-family="Segoe UI, Arial, sans-serif">`);
o.push(`<rect width="${W}" height="${H}" fill="#F4EFE4"/>`);
o.push(`<rect x="${X(L.bounds.minX)}" y="${Y(L.bounds.maxZ)}" width="${(L.bounds.maxX - L.bounds.minX) * S}" height="${(L.bounds.maxZ - L.bounds.minZ) * S}" fill="#E9E2D0" stroke="#8A8270" stroke-width="2"/>`);

for (const d of L.districts) {
  if (!d.area) continue;
  const [[x0, z0], [x1, z1]] = d.area;
  o.push(`<rect x="${X(x0)}" y="${Y(z1)}" width="${(x1 - x0) * S}" height="${(z1 - z0) * S}" fill="${tint[d.id] || '#E5DDC8'}" opacity=".85"/>`);
}

// Sông
o.push(`<polygon points="${L.river.outline.map(([x, z]) => `${X(x)},${Y(z)}`).join(' ')}" fill="#6FA9C9" stroke="#3F7A99" stroke-width="2"/>`);
o.push(`<text x="${X(320)}" y="${Y(-300)}" fill="#fff" font-size="16" font-weight="700" transform="rotate(-90 ${X(320)} ${Y(-300)})" text-anchor="middle">SÔNG SÀI GÒN</text>`);
o.push(`<text x="${X(-300)}" y="${Y(320)}" fill="#fff" font-size="16" font-weight="700" text-anchor="middle">SÔNG SÀI GÒN</text>`);

// Đường: vỉa hè rồi lòng đường
const bridgeRoads = Object.fromEntries(L.bridges.map(b => [b.road, b]));
for (const pass of ['walk', 'road', 'line']) {
  for (const r of L.roads) {
    const p = L.roadProfiles[r.profile];
    const w = pass === 'walk' ? p.carriageway + p.sidewalk * 2 : p.carriageway;
    const col = pass === 'walk' ? '#C9BFAA' : pass === 'road' ? '#4B5052' : (r.profile === 'main' ? '#F2C230' : '#E8D9A0');
    const sw = pass === 'line' ? (r.profile === 'main' ? 1.4 : 0.8) : w * S;
    o.push(`<line x1="${X(r.from[0])}" y1="${Y(r.from[1])}" x2="${X(r.to[0])}" y2="${Y(r.to[1])}" stroke="${col}" stroke-width="${sw}" stroke-linecap="square"${pass === 'line' && r.profile !== 'main' ? ' stroke-dasharray="4 4"' : ''}/>`);
  }
}

// Cầu
for (const b of L.bridges) {
  const [x0, z0] = b.from, [x1, z1] = b.to;
  const horiz = z0 === z1;
  const half = 12;
  const rx = Math.min(X(x0), X(x1)) - (horiz ? 0 : half * S), ry = Math.min(Y(z0), Y(z1)) - (horiz ? half * S : 0);
  const rw = horiz ? Math.abs(x1 - x0) * S : half * 2 * S, rh = horiz ? half * 2 * S : Math.abs(z1 - z0) * S;
  o.push(`<rect x="${rx}" y="${ry}" width="${rw}" height="${rh}" fill="#4B5052" stroke="#fff" stroke-width="3"/>`);
  // vòm (nhìn từ trên: hai đường cong trắng hai bên mép cầu)
  if (horiz) for (const s of [-1, 1]) o.push(`<path d="M${X(x0)},${Y(z0 + s * half)} Q${X((x0 + x1) / 2)},${Y(z0 + s * (half + 10))} ${X(x1)},${Y(z0 + s * half)}" fill="none" stroke="#fff" stroke-width="3"/>`);
  else o.push(`<path d="M${X(x0)},${Y(z0)} Q${X(x0 + 9)},${Y((z0 + z1) / 2)} ${X(x1)},${Y(z1)}" fill="none" stroke="#fff" stroke-width="3"/>`);
  const lx = X((x0 + x1) / 2), ly = Y((z0 + z1) / 2);
  o.push(`<text x="${lx + (horiz ? 0 : 16)}" y="${ly + (horiz ? -16 : 4)}" font-size="12" font-weight="700" fill="#123" text-anchor="${horiz ? 'middle' : 'start'}" stroke="#fff" stroke-width="3" paint-order="stroke">${label[b.id]}</text>`);
}

// Bùng binh
for (const q of L.roundabouts) {
  o.push(`<circle cx="${X(q.center[0])}" cy="${Y(q.center[1])}" r="${q.radius * S}" fill="#4B5052"/>`);
  o.push(`<circle cx="${X(q.center[0])}" cy="${Y(q.center[1])}" r="${(q.radius - 9) * S}" fill="#E4C04A" stroke="#7A9B5A" stroke-width="2"/>`);
}

// Công trình nổi bật
const m = Object.fromEntries(L.landmarks.map(x => [x.id, x]));
{ const c = m.M1; const [w, h] = c.size;
  o.push(`<rect x="${X(c.center[0] - w / 2)}" y="${Y(c.center[1] + h / 2)}" width="${w * S}" height="${h * S}" fill="#F3E3B8" stroke="#A8402E" stroke-width="3"/>`);
  o.push(`<rect x="${X(c.center[0]) - 5}" y="${Y(c.center[1] - h / 2) - 8}" width="10" height="10" fill="#A8402E"/>`); }
{ const c = m.M2; o.push(`<circle cx="${X(c.center[0])}" cy="${Y(c.center[1])}" r="9" fill="#5B7A93" stroke="#fff" stroke-width="2"/>`); }
{ const c = m.M5; o.push(`<rect x="${X(c.center[0]) - 6}" y="${Y(c.center[1]) - 14}" width="12" height="28" fill="#fff" stroke="#C0392B" stroke-width="2"/>`); }
// Cao ốc tượng trưng
const towers = [[30, -400], [60, -250], [140, -330], [40, -100], [90, 40], [170, 60], [30, 160], [480, -480], [600, -420], [480, -300], [610, -250], [490, -60], [610, 0], [500, 150], [620, 160], [600, 320]];
for (const [x, z] of towers) o.push(`<rect x="${X(x) - 7}" y="${Y(z) - 7}" width="14" height="14" fill="#8EA1B2" stroke="#56687A"/>`);

// Nhãn khu và công trình
const tag = (x, z, t, size = 12, col = '#3B3325') => o.push(`<text x="${X(x)}" y="${Y(z)}" font-size="${size}" font-weight="700" fill="${col}" text-anchor="middle" stroke="#fff" stroke-width="3" paint-order="stroke">${t}</text>`);
tag(-360, -380, label.D1, 14); tag(105, -520, label.D3, 13); tag(-300, 60, label.D4, 14); tag(560, -110, label.D5, 13); tag(60, 560, label.D6, 14);
tag(m.M1.center[0], m.M1.center[1] + 4, label.M1, 13, '#8A2E1E');
tag(m.M2.center[0], m.M2.center[1] - 20, label.M2, 12, '#24425A');
tag(m.M5.center[0] - 40, m.M5.center[1] + 4, label.M5, 11, '#24425A');
for (const r of L.roads.filter(r => label[r.id])) {
  const horiz = r.from[1] === r.to[1];
  const mx = horiz ? (r.from[0] * 0.35 + r.to[0] * 0.65) : r.from[0], mz = horiz ? r.from[1] : (r.from[1] * 0.3 + r.to[1] * 0.7);
  const rot = horiz ? '' : ` transform="rotate(-90 ${X(mx)} ${Y(mz)})"`;
  o.push(`<text x="${X(mx)}" y="${Y(mz) + 3.5}" font-size="10" fill="#fff" text-anchor="middle"${rot}>${label[r.id]}</text>`);
}

// Ba góc nhìn ảnh tham chiếu + điểm xuất phát
const cams = [
  { n: '1', x: 120, z: 211.5, dir: [1, 0], c: '#D9480F' },
  { n: '2', x: -375, z: -151.75, dir: [1, 0], c: '#D9480F' },
  { n: '3', x: 218.5, z: -400, dir: [0, 1], c: '#D9480F' }
];
for (const k of cams) {
  const len = 90, sp = 30;
  const tip = [k.x + k.dir[0] * len, k.z + k.dir[1] * len];
  const a = [tip[0] - k.dir[1] * sp, tip[1] + k.dir[0] * sp], b = [tip[0] + k.dir[1] * sp, tip[1] - k.dir[0] * sp];
  o.push(`<polygon points="${X(k.x)},${Y(k.z)} ${X(a[0])},${Y(a[1])} ${X(b[0])},${Y(b[1])}" fill="${k.c}" opacity=".28"/>`);
  o.push(`<circle cx="${X(k.x)}" cy="${Y(k.z)}" r="10" fill="${k.c}" stroke="#fff" stroke-width="2"/>`);
  o.push(`<text x="${X(k.x)}" y="${Y(k.z) + 4}" font-size="12" font-weight="700" fill="#fff" text-anchor="middle">${k.n}</text>`);
}

// Thước tỉ lệ + chú thích
const ly0 = PAD + (L.bounds.maxZ - L.bounds.minZ) * S + 30;
o.push(`<line x1="${PAD}" y1="${ly0}" x2="${PAD + 200 * S}" y2="${ly0}" stroke="#222" stroke-width="3"/><text x="${PAD}" y="${ly0 + 18}" font-size="12">200 m</text>`);
const legend = [
  ['#4B5052', 'Đường chính 4 làn (14 m) + vỉa hè 6 m — đường nhánh 2 làn'],
  ['#6FA9C9', 'Sông hình chữ L, lan can sắt + lối đi bộ ven sông'],
  ['#F3E3B8', 'Chợ Bến Thành, bùng binh trước chợ'],
  ['#8EA1B2', 'Cao ốc (Tháp Tài Chính dáng búp sen nhìn thấy từ cả ba ảnh)'],
  ['#D9480F', 'Số 1/2/3 = vị trí và hướng camera của 3 ảnh bạn gửi; số 3 cũng là điểm xuất phát']
];
legend.forEach(([c, t], i) => {
  o.push(`<rect x="${PAD + 220}" y="${ly0 - 10 + i * 20}" width="14" height="14" fill="${c}"/>`);
  o.push(`<text x="${PAD + 242}" y="${ly0 + 2 + i * 20}" font-size="13" fill="#222">${t}</text>`);
});
o.push(`<text x="${PAD}" y="${PAD - 14}" font-size="18" font-weight="700" fill="#222">Phố Việt — sơ đồ bản đồ mở v1 (1,3 × 1,3 km) — chờ duyệt</text>`);
o.push('</svg>');
fs.writeFileSync(path.join(dir, 'map-plan.svg'), o.join('\n'));
console.log('map-plan.svg', Math.round(W), 'x', Math.round(H));
