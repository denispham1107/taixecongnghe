/* 21.356: tài khoản người chơi + bảng xếp hạng chung (Firebase dự án «gotaviet»).
   • Đăng ký/đăng nhập: email + mật khẩu hoặc Google (Firebase Authentication).
   • Bảng xếp hạng: Firestore scores/{uid} = {name, best (kỷ lục mét xa nhất), runs, updatedAt}; luật ở firestore.rules
     (ai cũng đọc, chỉ chủ tài khoản ghi, kỷ lục không giảm). Hạng = số người có kỷ lục cao hơn + 1.
   • Game báo hết lượt qua jslib GotaRunEnded → window.gotaRunEnded(mét lượt này, kỷ lục trên máy).
     Khách (chưa đăng nhập) vẫn chơi bình thường; kỷ lục lượt khách giữ ở localStorage và tự gửi khi đăng nhập.
   SDK tải từ gstatic khi có mạng; mất mạng thì trang/game vẫn chạy, chỉ phần tài khoản báo cần mạng. */
const SDK='https://www.gstatic.com/firebasejs/12.3.0/';
const host=location.hostname;
const CONFIG={
  apiKey:'AIzaSyCHs0mMV_mxaaXhxNya50wrWupEb9UxR7g',
  // Cùng tên miền với trang để đăng nhập Google chạy được cả khi Safari chặn cookie bên thứ ba.
  // 21.367: tên miền chính gotaviet.com (Firebase Hosting phục vụ /__/auth/handler trên tên miền tùy chỉnh; sw.js bỏ qua /__/).
  authDomain:['gotaviet.com','www.gotaviet.com','gotaviet.web.app','gotaviet.firebaseapp.com'].includes(host)?host:'gotaviet.firebaseapp.com',
  projectId:'gotaviet',
  storageBucket:'gotaviet.firebasestorage.app',
  messagingSenderId:'94356168371',
  appId:'1:94356168371:web:829d8d8dbc80d655059ac6'
};
const PENDING_KEY='gota:pendingBest';
const $=id=>document.getElementById(id);
const fmt=n=>new Intl.NumberFormat('vi-VN').format(Math.max(0,Math.floor(n||0)))+' m';
const standalone=matchMedia('(display-mode: standalone), (display-mode: fullscreen)').matches||navigator.standalone;
let fb=null,fbPromise=null,user=null,myScore=null,signupMode=false,signingUp=false;

function loadSdk(){
  return fbPromise||=(async()=>{
    const [app,auth,store]=await Promise.all([import(SDK+'firebase-app.js'),import(SDK+'firebase-auth.js'),import(SDK+'firebase-firestore.js')]);
    const appInst=app.initializeApp(CONFIG);
    const a=auth.getAuth(appInst);a.languageCode='vi';
    fb={auth:a,A:auth,db:store.getFirestore(appInst),F:store};
    auth.getRedirectResult(a).catch(e=>showMsg(errText(e)));
    auth.onAuthStateChanged(a,u=>{user=u;myScore=null;renderAccount();if(u)void playStart();else playStop();// Lúc vừa tạo tài khoản chưa có tên hiển thị → chờ updateProfile rồi mới gửi điểm (trong submit).
      if(u&&!signingUp)void flushPending().finally(refreshBoard);else if(!u)refreshBoard();});
    return fb;
  })().catch(e=>{fbPromise=null;throw e;});
}

/* ---------- giao diện tài khoản ---------- */
function displayName(u){return ((u?.displayName||'').trim()||(u?.email||'Người chơi').split('@')[0]).slice(0,24);}
function showMsg(text,ok){const m=$('acct-msg');if(!m)return;m.textContent=text||'';m.classList.toggle('ok',Boolean(ok));}
function errText(e){
  const c=e?.code||'';
  if(c==='auth/invalid-email')return 'Email không hợp lệ.';
  if(c==='auth/missing-password'||c==='auth/weak-password')return 'Mật khẩu cần ít nhất 6 ký tự.';
  if(c==='auth/email-already-in-use')return 'Email này đã có tài khoản — hãy chọn Đăng nhập.';
  if(c==='auth/invalid-credential'||c==='auth/wrong-password'||c==='auth/user-not-found'||c==='auth/invalid-login-credentials')return 'Email hoặc mật khẩu chưa đúng.';
  if(c==='auth/too-many-requests')return 'Thử sai nhiều lần, đợi một lát rồi thử lại.';
  if(c==='auth/network-request-failed'||c==='unavailable')return 'Cần kết nối mạng để dùng tài khoản.';
  if(c==='auth/popup-closed-by-user'||c==='auth/cancelled-popup-request')return 'Đã đóng cửa sổ Google.';
  if(c==='auth/account-exists-with-different-credential')return 'Email này đã đăng ký bằng mật khẩu — hãy đăng nhập bằng email.';
  if(c==='permission-denied')return 'Máy chủ từ chối lưu điểm.';
  return 'Có lỗi'+(c?' ('+c+')':'')+'. Thử lại sau.';
}
function setMode(signup){
  signupMode=signup;
  $('tab-login').classList.toggle('on',!signup);$('tab-signup').classList.toggle('on',signup);
  $('tab-login').setAttribute('aria-selected',String(!signup));$('tab-signup').setAttribute('aria-selected',String(signup));
  $('row-name').hidden=!signup;$('acct-forgot').hidden=signup;
  $('acct-pass').autocomplete=signup?'new-password':'current-password';
  $('acct-submit').textContent=signup?'TẠO TÀI KHOẢN':'ĐĂNG NHẬP';
  showMsg('');
}
function renderAccount(){
  const btn=$('account-open');
  if(btn){btn.textContent=user?displayName(user):'Đăng nhập';btn.classList.toggle('signed',Boolean(user));}
  if(!$('account'))return;
  $('acct-out').hidden=Boolean(user);$('acct-in').hidden=!user;
  if(user){
    $('acct-who').textContent=displayName(user);$('acct-mail').textContent=user.email||'';
    $('rename').value=displayName(user);
    $('acct-best').textContent=myScore?`Kỷ lục trên bảng: ${fmt(myScore.best)}${myScore.rank?' · hạng #'+myScore.rank:''}`:'Chưa có điểm trên bảng — chơi một lượt để ghi điểm.';
  }
}
// Người dùng 23/11/2026: đăng nhập/đăng ký CHỈ ở trang chủ web — khi game đã mở (canvas hiện) thì không mở hộp tài khoản.
function gameOpen(){const c=$('unity-canvas');return Boolean(c&&!c.hidden);}
async function openAccount(){
  if(gameOpen())return;
  showMsg('');renderAccount();
  if(!$('account').open)$('account').showModal();
  try{await loadSdk();renderAccount();}catch(e){showMsg('Cần kết nối mạng để đăng nhập.');}
}

/* ---------- điểm ---------- */
function readPending(){try{return Math.max(0,parseInt(localStorage.getItem(PENDING_KEY)||'0',10)||0);}catch(_){return 0;}}
function writePending(v){try{if(v>0)localStorage.setItem(PENDING_KEY,String(v));else localStorage.removeItem(PENDING_KEY);}catch(_){}}
async function rankOf(best){
  const {db,F}=fb;
  const higher=await F.getCountFromServer(F.query(F.collection(db,'scores'),F.where('best','>',best)));
  return higher.data().count+1;
}
async function saveScore(metres,countRun){
  const {db,F}=fb;const ref=F.doc(db,'scores',user.uid);
  const snap=await F.getDoc(ref);const old=snap.exists()?snap.data():null;
  const best=Math.max(old?.best||0,Math.floor(metres));
  const data={name:displayName(user),best,runs:(old?.runs||0)+(countRun?1:0),updatedAt:F.serverTimestamp()};
  await F.setDoc(ref,data);
  const rank=await rankOf(best);
  myScore={best,rank,runs:data.runs};
  return {best,rank,newBest:best>(old?.best||0)};
}
async function flushPending(){
  const p=readPending();if(!p||!user)return;
  try{await saveScore(p,false);writePending(0);}catch(e){console.warn('pending score',e);}
}
// Gọi từ Unity (jslib GotaRunEnded) mỗi khi hết lượt.
// 21.375: đang chơi (Unity báo gotaRunStarted) thì không hiện thông báo xếp hạng; vào lượt mới tắt thông báo còn đang hiện
// (kể cả kết quả gửi điểm về muộn sau khi người chơi đã bấm chơi lại).
let inRun=false;
window.gotaRunStarted=()=>{inRun=true;clearTimeout(toastTimer);const t=$('run-toast');if(t)t.hidden=true;};
window.gotaRunEnded=async(metres,localBest)=>{
  inRun=false;
  metres=Math.max(0,Math.floor(Number(metres)||0));
  if(!user){
    writePending(Math.max(readPending(),metres));
    toast(`Lượt này ${fmt(metres)}. Đăng nhập ở trang chủ để ghi kỷ lục lên bảng xếp hạng chung.`);
    void loadSdk().catch(()=>{});
    return;
  }
  void logRun(metres);
  try{
    const r=await Promise.race([saveScore(metres,true),new Promise((_,no)=>setTimeout(()=>no({code:'unavailable'}),12000))]);
    toast(`${r.newBest?'KỶ LỤC MỚI! ':''}Lượt này ${fmt(metres)} · Kỷ lục ${fmt(r.best)} · Hạng #${r.rank}`);
    refreshBoard();
  }catch(e){
    writePending(Math.max(readPending(),metres));
    toast(`Lượt này ${fmt(metres)}. Chưa gửi được điểm (${errText(e)}) — sẽ tự gửi lại.`);
  }
};

/* ---------- 21.378: nhật ký chơi cho trang quản trị (/admin) ----------
   Chỉ tài khoản đã đăng nhập. plays/{id} = một lần vào chơi: tạo khi game mở (canvas hiện) và trang đang hiển thị, cập nhật
   seconds/end mỗi 30 s; trang bị ẩn (chuyển app, khóa máy, đóng tab) thì chốt lần đó, hiện lại thì mở lần mới.
   runs/{id} = điểm (mét) từng lượt. Luật ở firestore.rules; trang quản trị đọc qua Cloud Function adminApi. */
let play=null;
async function playStart(){
  if(play||!user||!gameOpen()||document.hidden)return;
  const p=play={ref:null,from:performance.now(),timer:0};
  try{
    const {db,F}=await loadSdk();
    if(!user||play!==p){if(play===p)play=null;return;}
    p.ref=await F.addDoc(F.collection(db,'plays'),{uid:user.uid,start:F.serverTimestamp(),end:F.serverTimestamp(),seconds:0});
    if(play!==p){await playWrite(p);return;}   // đã bị chốt trong lúc chờ tạo
    p.timer=setInterval(()=>void playWrite(p),30000);
  }catch(e){console.warn('play log',e);if(play===p)play=null;}
}
async function playWrite(p){
  if(!p?.ref||!fb)return;
  try{await fb.F.updateDoc(p.ref,{end:fb.F.serverTimestamp(),seconds:Math.max(0,Math.floor((performance.now()-p.from)/1000))});}
  catch(e){console.warn('play log',e);}
}
function playStop(){const p=play;if(!p)return;play=null;clearInterval(p.timer);void playWrite(p);}
document.addEventListener('visibilitychange',()=>{if(document.hidden)playStop();else void playStart();});
window.addEventListener('pagehide',playStop);
async function logRun(metres){
  if(!user)return;
  try{const {db,F}=await loadSdk();await F.addDoc(F.collection(db,'runs'),{uid:user.uid,metres:Math.floor(metres),at:F.serverTimestamp()});}
  catch(e){console.warn('run log',e);}
}

/* ---------- thông báo sau mỗi lượt (nằm trên màn hình game) ---------- */
let toastTimer=0;
function toast(text){
  const t=$('run-toast');if(!t||inRun)return;
  $('run-toast-text').textContent=text;
  t.hidden=false;clearTimeout(toastTimer);toastTimer=setTimeout(()=>{t.hidden=true;},9000);
}

/* ---------- bảng xếp hạng ---------- */
function rows(list,docs){
  list.textContent='';
  if(!docs.length){const li=document.createElement('li');li.className='muted';li.textContent='Chưa có ai trên bảng — hãy là người đầu tiên!';list.appendChild(li);return;}
  let rank=0,prev=-1;
  docs.forEach((d,i)=>{
    const s=d.data();if(s.best!==prev){rank=i+1;prev=s.best;}
    const li=document.createElement('li');if(user&&d.id===user.uid)li.className='me';
    const no=document.createElement('span');no.className='no';no.textContent=rank;
    const nm=document.createElement('span');nm.className='nm';nm.textContent=s.name||'Người chơi';
    const m=document.createElement('span');m.className='m';m.textContent=fmt(s.best);
    li.append(no,nm,m);list.appendChild(li);
  });
}
async function topScores(n){
  const {db,F}=fb;
  return (await F.getDocs(F.query(F.collection(db,'scores'),F.orderBy('best','desc'),F.limit(n)))).docs;
}
async function myLine(){
  if(!user)return gameOpen()?'Đăng nhập ở trang chủ để ghi kỷ lục lên bảng chung và xem hạng của bạn.':'Đăng nhập để ghi kỷ lục lên bảng chung và xem hạng của bạn.';
  if(!myScore){
    const snap=await fb.F.getDoc(fb.F.doc(fb.db,'scores',user.uid));
    if(!snap.exists())return `${displayName(user)}: chưa có điểm — chơi một lượt để lên bảng.`;
    const best=snap.data().best||0;myScore={best,rank:await rankOf(best),runs:snap.data().runs||0};
  }
  const total=(await fb.F.getCountFromServer(fb.F.collection(fb.db,'scores'))).data().count;
  return `Bạn (${displayName(user)}): hạng #${myScore.rank} / ${total} người · kỷ lục ${fmt(myScore.best)}`;
}
let boardBusy=false;
async function refreshBoard(){
  if(boardBusy||!$('rank-top'))return;boardBusy=true;
  try{
    await loadSdk();
    rows($('rank-top'),await topScores(10));
    $('rank-me').textContent=await myLine();renderAccount();
    if($('ranks').open)await fillDialog();
  }catch(e){
    $('rank-me').textContent=navigator.onLine===false?'Cần kết nối mạng để xem bảng xếp hạng.':'Chưa tải được bảng xếp hạng ('+errText(e)+').';
  }finally{boardBusy=false;}
}
async function fillDialog(){
  rows($('ranks-list'),await topScores(100));
  $('ranks-me').textContent=await myLine();
}
async function openRanks(){
  if(!$('ranks').open)$('ranks').showModal();
  $('ranks-me').textContent='Đang tải…';
  try{await loadSdk();await fillDialog();}catch(e){$('ranks-me').textContent='Cần kết nối mạng để xem bảng xếp hạng.';}
}

/* ---------- gắn sự kiện ---------- */
function closeDialog(d){d.close();const c=$('unity-canvas');if(c&&!c.hidden)c.focus({preventScroll:true});}
$('account-open')?.addEventListener('click',openAccount);
$('rank-all')?.addEventListener('click',openRanks);
// 21.357: ô «Bảng xếp hạng» trên màn chào trong game (jslib GotaOpenRanks) mở hộp Top 100 — chỉ xem, không có đăng nhập.
window.gotaOpenRanks=()=>{void openRanks();};
$('toast-ranks')?.addEventListener('click',()=>{$('run-toast').hidden=true;openRanks();});
$('toast-close')?.addEventListener('click',()=>{$('run-toast').hidden=true;});
$('close-account').addEventListener('click',()=>closeDialog($('account')));
$('close-ranks').addEventListener('click',()=>closeDialog($('ranks')));
for(const d of [$('account'),$('ranks')])d.addEventListener('click',e=>{if(e.target===d)closeDialog(d);});
$('tab-login').addEventListener('click',()=>setMode(false));
$('tab-signup').addEventListener('click',()=>setMode(true));
$('acct-form').addEventListener('submit',async e=>{
  e.preventDefault();
  const email=$('acct-email').value.trim(),pass=$('acct-pass').value,name=$('acct-name').value.trim();
  if(signupMode&&!name){showMsg('Nhập tên hiển thị trên bảng xếp hạng.');$('acct-name').focus();return;}
  $('acct-submit').disabled=true;showMsg('Đang xử lý…',true);
  try{
    const {auth,A}=await loadSdk();
    if(signupMode){
      signingUp=true;
      try{
        const cred=await A.createUserWithEmailAndPassword(auth,email,pass);
        await A.updateProfile(cred.user,{displayName:name.slice(0,24)});
        user=auth.currentUser;await flushPending();
      }finally{signingUp=false;}
    }else await A.signInWithEmailAndPassword(auth,email,pass);
    $('acct-pass').value='';showMsg('Đã đăng nhập!',true);renderAccount();refreshBoard();
  }catch(err){showMsg(errText(err));}
  finally{$('acct-submit').disabled=false;}
});
$('acct-forgot').addEventListener('click',async()=>{
  const email=$('acct-email').value.trim();
  if(!email){showMsg('Nhập email rồi bấm «Quên mật khẩu?» lần nữa.');$('acct-email').focus();return;}
  try{const {auth,A}=await loadSdk();await A.sendPasswordResetEmail(auth,email);showMsg('Nếu email đã đăng ký, thư đặt lại mật khẩu đã được gửi (xem cả mục Spam).',true);}
  catch(err){showMsg(errText(err));}
});
$('acct-google').addEventListener('click',async()=>{
  showMsg('');
  try{
    const {auth,A}=await loadSdk();const provider=new A.GoogleAuthProvider();
    provider.setCustomParameters({prompt:'select_account'});
    // Ứng dụng đã cài trên iPhone/Android không mở được cửa sổ bật lên → chuyển trang sang Google rồi quay lại.
    if(standalone){await A.signInWithRedirect(auth,provider);return;}
    try{await A.signInWithPopup(auth,provider);}
    catch(err){if(err?.code==='auth/popup-blocked'||err?.code==='auth/operation-not-supported-in-this-environment')await A.signInWithRedirect(auth,provider);else throw err;}
    showMsg('Đã đăng nhập!',true);
  }catch(err){showMsg(errText(err));}
});
$('rename-form').addEventListener('submit',async e=>{
  e.preventDefault();
  const name=$('rename').value.trim().slice(0,24);if(!name||!user)return;
  try{
    const {A,F,db}=await loadSdk();await A.updateProfile(user,{displayName:name});
    const ref=F.doc(db,'scores',user.uid);const snap=await F.getDoc(ref);
    if(snap.exists())await F.setDoc(ref,{...snap.data(),name,updatedAt:F.serverTimestamp()});
    showMsg('Đã đổi tên.',true);renderAccount();refreshBoard();
  }catch(err){showMsg(errText(err));}
});
$('acct-logout').addEventListener('click',async()=>{
  try{const {auth,A}=await loadSdk();await A.signOut(auth);showMsg('Đã đăng xuất.',true);}catch(err){showMsg(errText(err));}
});
setMode(false);renderAccount();
// Game đã mở → ẩn nút tài khoản trên thanh trên (menu ☰ máy tính chỉ còn «Tải game») và đóng hộp tài khoản nếu đang mở.
{const c=$('unity-canvas'),sync=()=>{const on=gameOpen();if($('account-open'))$('account-open').hidden=on;if(on&&$('account').open)$('account').close();if(on)void playStart();else playStop();};
 if(c){new MutationObserver(sync).observe(c,{attributes:true,attributeFilter:['hidden']});sync();}}
// Tải SDK + bảng xếp hạng khi trang rảnh (không chen vào lúc tải game).
const idle=window.requestIdleCallback||(f=>setTimeout(f,1200));
idle(()=>{if(navigator.onLine!==false)refreshBoard();});
window.addEventListener('online',()=>{refreshBoard();});
