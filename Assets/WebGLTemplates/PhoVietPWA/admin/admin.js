/* 21.378 — trang quản trị https://gotaviet.com/admin.
   Đăng nhập Firebase Auth (Google hoặc email/mật khẩu) → gọi Cloud Function «adminApi» (asia-southeast1); hàm kiểm tra
   admins/{uid} rồi mới trả dữ liệu / sửa / xóa (xem functions/index.js). Trang này không đọc Firestore trực tiếp.
   Hiện: danh sách tài khoản (lúc tạo tới giây, lần chơi gần nhất, số lần vào, tổng thời gian chơi, kỷ lục, số lượt);
   bấm một người → thông tin + sửa (tên, email, mật khẩu mới, khóa, tên trên bảng, kỷ lục) + các lần vào chơi
   (bắt đầu, kết thúc, thời lượng) + điểm từng lượt + xóa tài khoản. Giờ theo múi giờ Việt Nam. */
const SDK = 'https://www.gstatic.com/firebasejs/12.3.0/';
const host = location.hostname;
const CONFIG = {
  apiKey: 'AIzaSyCHs0mMV_mxaaXhxNya50wrWupEb9UxR7g',
  authDomain: ['gotaviet.com', 'www.gotaviet.com', 'gotaviet.web.app', 'gotaviet.firebaseapp.com'].includes(host) ? host : 'gotaviet.firebaseapp.com',
  projectId: 'gotaviet',
  storageBucket: 'gotaviet.firebasestorage.app',
  messagingSenderId: '94356168371',
  appId: '1:94356168371:web:829d8d8dbc80d655059ac6'
};
const $ = id => document.getElementById(id);
const [appMod, authMod, fnMod] = await Promise.all([import(SDK + 'firebase-app.js'), import(SDK + 'firebase-auth.js'), import(SDK + 'firebase-functions.js')]);
const app = appMod.initializeApp(CONFIG);
const auth = authMod.getAuth(app); auth.languageCode = 'vi';
const api = fnMod.httpsCallable(fnMod.getFunctions(app, 'asia-southeast1'), 'adminApi', {timeout: 120000});
const call = async data => (await api(data)).data;

/* ---------- định dạng ---------- */
const TZ = 'Asia/Ho_Chi_Minh';
const dtf = new Intl.DateTimeFormat('vi-VN', {timeZone: TZ, day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23'});
function when(ms) {
  if (!ms) return '—';
  const p = {}; for (const x of dtf.formatToParts(new Date(ms))) p[x.type] = x.value;
  return `${p.day}/${p.month}/${p.year} ${p.hour}:${p.minute}:${p.second}`;
}
function dur(sec) {
  sec = Math.max(0, Math.round(sec || 0));
  const h = Math.floor(sec / 3600), m = Math.floor(sec % 3600 / 60), s = sec % 60;
  if (h) return `${h} giờ ${String(m).padStart(2, '0')} phút ${String(s).padStart(2, '0')} giây`;
  if (m) return `${m} phút ${String(s).padStart(2, '0')} giây`;
  return `${s} giây`;
}
const num = n => new Intl.NumberFormat('vi-VN').format(Math.floor(n || 0));
const metres = n => num(n) + ' m';
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({'&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'}[c]));
const provider = list => list.includes('google.com') ? 'Google' : list.includes('password') ? 'Email' : (list[0] || '—');
function errText(e) {
  const c = e?.code || '';
  if (c.endsWith('permission-denied')) return 'Tài khoản này không phải quản trị viên.';
  if (c.endsWith('unauthenticated')) return 'Cần đăng nhập lại.';
  if (c === 'auth/invalid-credential' || c === 'auth/wrong-password' || c === 'auth/user-not-found' || c === 'auth/invalid-login-credentials') return 'Email hoặc mật khẩu chưa đúng.';
  if (c === 'auth/popup-closed-by-user' || c === 'auth/cancelled-popup-request') return 'Đã đóng cửa sổ Google.';
  if (c === 'auth/too-many-requests') return 'Thử sai nhiều lần, đợi một lát rồi thử lại.';
  if (c.endsWith('email-already-exists') || c === 'functions/already-exists') return 'Email này đã thuộc tài khoản khác.';
  if (e?.message && c.startsWith('functions/')) return e.message;
  if (c.includes('network') || c.endsWith('unavailable')) return 'Cần kết nối mạng.';
  return 'Có lỗi' + (c ? ' (' + c + ')' : '') + (e?.message ? ': ' + e.message : '') + '.';
}
function show(id) { for (const s of ['boot', 'login', 'denied', 'panel']) $(s).hidden = s !== id; }

/* ---------- đăng nhập ---------- */
$('google').addEventListener('click', async () => {
  $('login-msg').textContent = '';
  try { await authMod.signInWithPopup(auth, new authMod.GoogleAuthProvider()); } catch (e) { $('login-msg').textContent = errText(e); }
});
$('login-form').addEventListener('submit', async e => {
  e.preventDefault(); $('login-msg').textContent = '';
  try { await authMod.signInWithEmailAndPassword(auth, $('login-email').value.trim(), $('login-pass').value); } catch (err) { $('login-msg').textContent = errText(err); }
});
const out = () => authMod.signOut(auth);
$('logout').addEventListener('click', out); $('denied-out').addEventListener('click', out);

let users = [], me = null;
authMod.onAuthStateChanged(auth, async u => {
  $('who').hidden = !u; $('who-name').textContent = u ? (u.displayName || u.email || '') : '';
  if (!u) { show('login'); return; }
  show('boot'); $('boot').textContent = 'Đang tải danh sách người chơi…';
  try { await load(); show('panel'); }
  catch (e) {
    if ((e?.code || '').endsWith('permission-denied')) { $('denied-mail').textContent = u.email || u.uid; show('denied'); }
    else { $('boot').textContent = errText(e); }
  }
});

/* ---------- danh sách ---------- */
async function load() {
  const r = await call({action: 'list'}); users = r.users; me = r.me; render();
}
$('reload').addEventListener('click', async () => {
  $('panel-msg').textContent = 'Đang tải…'; $('panel-msg').classList.add('ok');
  try { await load(); $('panel-msg').textContent = ''; } catch (e) { $('panel-msg').classList.remove('ok'); $('panel-msg').textContent = errText(e); }
});
$('search').addEventListener('input', render); $('sort').addEventListener('change', render);

// 21.396: giờ chơi tách 2 loại — luyện tập (chế độ TẬP LUYỆN, plays.practice) và chơi thật (còn lại, đua bảng xếp hạng).
const prac = p => Math.min(p.seconds || 0, p.practice || 0), real = p => Math.max(0, (p.seconds || 0) - prac(p));

function render() {
  const total = users.reduce((a, u) => a + real(u.plays), 0), totalP = users.reduce((a, u) => a + prac(u.plays), 0), visits = users.reduce((a, u) => a + u.plays.count, 0);
  const today = Date.now() - 86400000, active = users.filter(u => u.plays.last > today).length;
  $('stats').innerHTML = [['Tài khoản', num(users.length)], ['Chơi trong 24 giờ', num(active)], ['Tổng lần vào chơi', num(visits)],
    ['Giờ chơi thật (đua xếp hạng)', dur(total)], ['Giờ luyện tập', dur(totalP)], ['Tổng số ván chơi', num(users.reduce((a, u) => a + (u.board?.runs || 0), 0))]]
    .map(([k, v]) => `<div class="stat"><small>${k}</small><b>${v}</b></div>`).join('');
  const q = $('search').value.trim().toLowerCase(), key = $('sort').value;
  const list = users.filter(u => !q || [u.name, u.email, u.board?.name, u.uid].some(x => (x || '').toLowerCase().includes(q)));
  const by = {created: u => -(u.created || 0), lastplay: u => -(u.plays.last || 0), time: u => -real(u.plays), practice: u => -prac(u.plays), best: u => -(u.board?.best || 0)};
  list.sort(key === 'name' ? (a, b) => (a.name || a.email).localeCompare(b.name || b.email, 'vi') : (a, b) => by[key](a) - by[key](b));
  $('users').tBodies[0].innerHTML = list.map(u => `<tr tabindex="0" data-uid="${esc(u.uid)}">
    <td class="pname"><b>${esc(u.name || u.board?.name || '(chưa đặt tên)')}${u.admin ? '<span class="tag admin">QUẢN TRỊ</span>' : ''}${u.disabled ? '<span class="tag off">ĐÃ KHÓA</span>' : ''}</b><small>${esc(u.email || u.uid)} · ${provider(u.providers)}</small></td>
    <td>${when(u.created)}</td><td>${when(u.plays.last)}</td><td class="num">${num(u.plays.count)}</td>
    <td class="num">${dur(real(u.plays))}</td><td class="num">${dur(prac(u.plays))}</td><td class="num">${u.board ? metres(u.board.best) : '—'}</td><td class="num">${num(u.board?.runs || 0)}</td></tr>`).join('')
    || '<tr><td colspan="8" class="muted">Không có người chơi nào.</td></tr>';
}
$('users').addEventListener('click', e => { const tr = e.target.closest('tr[data-uid]'); if (tr) openDetail(tr.dataset.uid); });
$('users').addEventListener('keydown', e => { const tr = e.target.closest('tr[data-uid]'); if (tr && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); openDetail(tr.dataset.uid); } });

/* ---------- chi tiết một người chơi ---------- */
let current = null;
$('d-close').addEventListener('click', () => $('detail').close());
$('detail').addEventListener('click', e => { if (e.target === $('detail')) $('detail').close(); });

async function openDetail(uid) {
  const base = users.find(u => u.uid === uid);
  $('d-title').textContent = base?.name || base?.email || 'Người chơi';
  $('d-body').innerHTML = '<p class="muted">Đang tải…</p>';
  $('detail').showModal(); $('d-title').focus({preventScroll: true});
  try { current = await call({action: 'detail', uid}); current.uid = uid; drawDetail(); }
  catch (e) { $('d-body').innerHTML = `<p class="msg">${esc(errText(e))}</p>`; }
}

function drawDetail() {
  const {user: u, board: b, plays, runs} = current, uid = current.uid, total = plays.reduce((a, p) => a + real(p), 0), totalP = plays.reduce((a, p) => a + prac(p), 0);
  const best = runs.reduce((a, r) => Math.max(a, r.metres), 0);
  const kv = (k, v) => `<div class="kv"><small>${k}</small><span>${v}</span></div>`;
  $('d-title').textContent = u.name || b?.name || u.email || 'Người chơi';
  $('d-body').innerHTML = `
    <div class="grid">
      ${kv('Tạo tài khoản', when(u.created))}${kv('Đăng nhập gần nhất', when(u.lastSignIn))}
      ${kv('Hoạt động gần nhất', when(u.lastActive))}${kv('Đăng nhập bằng', provider(u.providers))}
      ${kv('Giờ chơi thật (đua xếp hạng)', dur(total))}${kv('Giờ luyện tập', dur(totalP))}
      ${kv('Số lần vào chơi', num(plays.length))}
      ${kv('Kỷ lục trên bảng', b ? metres(b.best) : '—')}${kv('Số ván chơi', num(b?.runs || 0))}
      ${kv('Điểm cao nhất đã ghi', runs.length ? metres(best) : '—')}${kv('Mã tài khoản (uid)', esc(uid))}
    </div>

    <div class="sec"><h3>Sửa thông tin</h3>
      <form id="edit" class="form" autocomplete="off">
        <label>Tên hiển thị<input name="dname" maxlength="40" value="${esc(u.name)}"></label>
        <label>Tên trên bảng xếp hạng<input name="dboard" maxlength="24" value="${esc(b?.name || '')}"></label>
        <label>Email<input name="demail" type="email" value="${esc(u.email)}"></label>
        <label>Mật khẩu mới (để trống nếu không đổi)<input name="dpass" type="password" minlength="6" autocomplete="new-password"></label>
        <label>Kỷ lục (mét)<input name="dbest" type="number" min="0" max="2000000" step="1" value="${b ? b.best : ''}"></label>
        <label class="check"><input name="doff" type="checkbox" ${u.disabled ? 'checked' : ''} ${uid === me ? 'disabled' : ''}> Khóa tài khoản (không đăng nhập được, đăng xuất khỏi mọi máy)</label>
        <div class="row"><button class="btn save" type="submit">Lưu thay đổi</button><span id="edit-msg" class="msg" role="status"></span></div>
      </form>
    </div>

    <div class="sec"><h3>Các lần vào chơi (${num(plays.length)} lần · thật ${dur(total)} · luyện tập ${dur(totalP)})</h3>
      <div class="scroll"><table><thead><tr><th>#</th><th>Bắt đầu</th><th>Kết thúc</th><th class="num">Chơi thật</th><th class="num">Luyện tập</th></tr></thead><tbody>
      ${plays.map((p, i) => `<tr><td>${plays.length - i}</td><td>${when(p.start)}</td><td>${when(p.end)}</td><td class="num">${dur(real(p))}</td><td class="num">${dur(prac(p))}</td></tr>`).join('') || '<tr><td colspan="5" class="muted">Chưa có lần vào chơi nào được ghi.</td></tr>'}
      </tbody></table></div>
    </div>

    <div class="sec"><h3>Điểm từng lượt (${num(runs.length)} lượt)</h3>
      <div class="scroll"><table><thead><tr><th>#</th><th>Thời điểm</th><th class="num">Điểm</th></tr></thead><tbody>
      ${runs.map((r, i) => `<tr><td>${runs.length - i}</td><td>${when(r.at)}</td><td class="num">${metres(r.metres)}</td></tr>`).join('') || '<tr><td colspan="3" class="muted">Chưa có lượt nào được ghi.</td></tr>'}
      </tbody></table></div>
    </div>

    ${uid === me ? '' : `<div class="danger-zone"><p>Xóa vĩnh viễn tài khoản đăng nhập, điểm trên bảng xếp hạng và toàn bộ nhật ký chơi của người này. Không hoàn tác được.</p>
      <button id="del" class="btn danger" type="button">Xóa tài khoản</button></div>`}`;

  $('edit').addEventListener('submit', async e => {
    e.preventDefault();
    const E = e.target.elements, m = $('edit-msg'), data = {action: 'update', uid};
    if (E.dname.value.trim() !== (u.name || '')) data.name = E.dname.value;
    if (E.dboard.value.trim() && E.dboard.value.trim() !== (b?.name || '')) data.boardName = E.dboard.value;
    if (E.demail.value.trim() && E.demail.value.trim() !== (u.email || '')) data.email = E.demail.value;
    if (E.dpass.value) data.password = E.dpass.value;
    if (E.dbest.value !== '' && Number(E.dbest.value) !== (b?.best ?? -1)) data.best = Math.floor(Number(E.dbest.value));
    if (uid !== me && E.doff.checked !== u.disabled) data.disabled = E.doff.checked;
    if (Object.keys(data).length === 2) { m.classList.add('ok'); m.textContent = 'Không có gì thay đổi.'; return; }
    m.classList.add('ok'); m.textContent = 'Đang lưu…';
    try {
      await call(data);
      current = {...await call({action: 'detail', uid}), uid}; drawDetail(); $('edit-msg').classList.add('ok'); $('edit-msg').textContent = 'Đã lưu.';
      void load();
    } catch (err) { m.classList.remove('ok'); m.textContent = errText(err); }
  });
  $('del')?.addEventListener('click', () => askDelete(uid, u.name || b?.name || u.email || uid));
}

/* ---------- xóa ---------- */
let deleting = null;
function askDelete(uid, label) {
  deleting = uid; $('c-text').textContent = `Tài khoản «${label}» sẽ bị xóa vĩnh viễn cùng điểm và nhật ký chơi.`;
  $('c-input').value = ''; $('c-ok').disabled = true; $('confirm').showModal(); $('c-input').focus();
}
$('c-input').addEventListener('input', () => { $('c-ok').disabled = $('c-input').value.trim().toUpperCase() !== 'XOA'; });
$('c-cancel').addEventListener('click', () => $('confirm').close());
$('c-ok').addEventListener('click', async () => {
  $('c-ok').disabled = true; $('c-ok').textContent = 'Đang xóa…';
  try {
    await call({action: 'delete', uid: deleting});
    $('confirm').close(); $('detail').close(); users = users.filter(u => u.uid !== deleting); render();
    $('panel-msg').classList.add('ok'); $('panel-msg').textContent = 'Đã xóa tài khoản.';
  } catch (e) { $('c-text').textContent = errText(e); }
  finally { $('c-ok').textContent = 'Xóa vĩnh viễn'; }
});
