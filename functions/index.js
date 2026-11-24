/* 21.378 — API trang quản trị https://gotaviet.com/admin (callable «adminApi», vùng asia-southeast1).
   Chỉ tài khoản có tài liệu admins/{uid} trong Firestore được gọi. Admin SDK nên xóa/sửa được cả tài khoản đăng nhập
   (Firebase Authentication), không chỉ dữ liệu bảng xếp hạng.
   Hành động (request.data.action):
     list   → mọi tài khoản: tên, email, cách đăng nhập, lúc tạo, lần đăng nhập cuối, khóa?, kỷ lục, số lượt,
              tổng số lần vào chơi + tổng thời gian chơi + lần chơi gần nhất (từ plays), số điểm đã ghi (từ runs).
     detail → một tài khoản + danh sách lần vào chơi (plays) + điểm từng lượt (runs), mới nhất trước.
     update → sửa tên hiển thị / email / mật khẩu mới / khóa-mở khóa / tên trên bảng / kỷ lục.
     delete → xóa tài khoản đăng nhập + scores/{uid} + mọi plays/runs của người đó.
   Dữ liệu ghi từ máy người chơi: account.js (plays mỗi lần mở game, runs mỗi lượt) — luật ở firestore.rules. */
const {onCall, HttpsError} = require('firebase-functions/v2/https');
const {initializeApp} = require('firebase-admin/app');
const {getAuth} = require('firebase-admin/auth');
const {getFirestore, FieldValue} = require('firebase-admin/firestore');

initializeApp();
const auth = getAuth(), db = getFirestore();
const ms = t => (t && typeof t.toMillis === 'function') ? t.toMillis() : null;

async function requireAdmin(req) {
  const uid = req.auth && req.auth.uid;
  if (!uid) throw new HttpsError('unauthenticated', 'Cần đăng nhập.');
  if (!(await db.doc('admins/' + uid).get()).exists) throw new HttpsError('permission-denied', 'Tài khoản này không phải quản trị viên.');
  return uid;
}

function userInfo(u) {
  return {
    uid: u.uid, email: u.email || '', name: u.displayName || '', disabled: !!u.disabled,
    providers: (u.providerData || []).map(p => p.providerId),
    created: Date.parse(u.metadata.creationTime) || null,
    lastSignIn: Date.parse(u.metadata.lastSignInTime) || null,
    lastActive: Date.parse(u.metadata.lastRefreshTime || '') || null
  };
}

async function allUsers() {
  const out = []; let token;
  do { const r = await auth.listUsers(1000, token); out.push(...r.users); token = r.pageToken; } while (token);
  return out;
}

async function deleteWhere(col, uid) {
  let n = 0;
  for (;;) {
    const snap = await db.collection(col).where('uid', '==', uid).limit(400).get();
    if (snap.empty) return n;
    const b = db.batch(); snap.docs.forEach(d => b.delete(d.ref)); await b.commit(); n += snap.size;
  }
}

exports.adminApi = onCall({region: 'asia-southeast1', memory: '256MiB', timeoutSeconds: 120, maxInstances: 2}, async req => {
  const me = await requireAdmin(req);
  const d = req.data || {}, action = d.action;

  if (action === 'list') {
    const [users, scores, plays, runs, admins] = await Promise.all([
      allUsers(), db.collection('scores').get(), db.collection('plays').select('uid', 'start', 'seconds').get(),
      db.collection('runs').select('uid').get(), db.collection('admins').get()]);
    const sc = {}; scores.forEach(s => { sc[s.id] = s.data(); });
    const pl = {}; plays.forEach(p => { const x = p.data(), a = pl[x.uid] ||= {count: 0, seconds: 0, last: 0};
      a.count++; a.seconds += x.seconds || 0; a.last = Math.max(a.last, ms(x.start) || 0); });
    const rn = {}; runs.forEach(r => { const u = r.get('uid'); rn[u] = (rn[u] || 0) + 1; });
    const adm = new Set(admins.docs.map(a => a.id));
    return {me, users: users.map(u => ({...userInfo(u), admin: adm.has(u.uid),
      board: sc[u.uid] ? {name: sc[u.uid].name, best: sc[u.uid].best, runs: sc[u.uid].runs} : null,
      plays: pl[u.uid] || {count: 0, seconds: 0, last: 0}, runsLogged: rn[u.uid] || 0}))};
  }

  const uid = String(d.uid || '');
  if (!uid) throw new HttpsError('invalid-argument', 'Thiếu uid.');

  if (action === 'detail') {
    const [u, s, plays, runs] = await Promise.all([auth.getUser(uid), db.doc('scores/' + uid).get(),
      db.collection('plays').where('uid', '==', uid).get(), db.collection('runs').where('uid', '==', uid).get()]);
    return {user: userInfo(u), board: s.exists ? {name: s.get('name'), best: s.get('best'), runs: s.get('runs'), updatedAt: ms(s.get('updatedAt'))} : null,
      plays: plays.docs.map(p => ({start: ms(p.get('start')), end: ms(p.get('end')), seconds: p.get('seconds') || 0})).sort((a, b) => b.start - a.start),
      runs: runs.docs.map(r => ({at: ms(r.get('at')), metres: r.get('metres') || 0})).sort((a, b) => b.at - a.at)};
  }

  if (action === 'update') {
    const patch = {};
    if (typeof d.name === 'string') patch.displayName = d.name.trim().slice(0, 40) || null;
    if (typeof d.email === 'string' && d.email.trim()) patch.email = d.email.trim();
    if (typeof d.password === 'string' && d.password) {
      if (d.password.length < 6) throw new HttpsError('invalid-argument', 'Mật khẩu cần ít nhất 6 ký tự.');
      patch.password = d.password;
    }
    if (typeof d.disabled === 'boolean') {
      if (d.disabled && uid === me) throw new HttpsError('failed-precondition', 'Không tự khóa tài khoản quản trị của mình.');
      patch.disabled = d.disabled;
    }
    if (Object.keys(patch).length) {
      try { await auth.updateUser(uid, patch); }
      catch (e) {
        if (e.code === 'auth/email-already-exists') throw new HttpsError('already-exists', 'Email này đã thuộc tài khoản khác.');
        if (e.code === 'auth/invalid-email') throw new HttpsError('invalid-argument', 'Email không hợp lệ.');
        if (e.code === 'auth/invalid-password') throw new HttpsError('invalid-argument', 'Mật khẩu cần ít nhất 6 ký tự.');
        if (e.code === 'auth/user-not-found') throw new HttpsError('not-found', 'Tài khoản không còn tồn tại.');
        throw e;
      }
    }
    if (d.disabled === true) await auth.revokeRefreshTokens(uid);
    const ref = db.doc('scores/' + uid), board = {};
    if (typeof d.boardName === 'string' && d.boardName.trim()) board.name = d.boardName.trim().slice(0, 24);
    if (Number.isInteger(d.best) && d.best >= 0 && d.best <= 2000000) board.best = d.best;
    if (Object.keys(board).length) {
      const cur = await ref.get();
      if (cur.exists) await ref.update({...board, updatedAt: FieldValue.serverTimestamp()});
      else await ref.set({name: board.name || (patch.displayName || 'Người chơi').slice(0, 24), best: board.best || 0, runs: 0, updatedAt: FieldValue.serverTimestamp()});
    }
    return {ok: true};
  }

  if (action === 'delete') {
    if (uid === me) throw new HttpsError('failed-precondition', 'Không tự xóa tài khoản quản trị của mình.');
    try { await auth.deleteUser(uid); } catch (e) { if (e.code !== 'auth/user-not-found') throw e; }
    await db.doc('scores/' + uid).delete();
    const plays = await deleteWhere('plays', uid), runs = await deleteWhere('runs', uid);
    return {ok: true, plays, runs};
  }

  throw new HttpsError('invalid-argument', 'Hành động không hợp lệ.');
});
