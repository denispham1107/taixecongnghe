"""21.369 — Âm thanh xe người chơi rơi xuống sông/biển (GờTa Việt / Phố Việt, bản đồ mở).

Người dùng: «Dựng âm thanh khi xe người chơi rơi xuống sông xuống biển (âm thanh xuyên suốt quá trình xe rơi chạm mặt nước
và đến khi xe chìm hoàn toàn)».

Tổng hợp bằng numpy theo mô hình vật lý âm thanh chất lỏng (van den Doel 2005, «Physically based models for liquid sounds»):
mỗi bọt khí trong nước là một bộ cộng hưởng Minnaert f ≈ 3,26 / r (Hz, r = bán kính m), tắt dần d = 0,13 f + 0,0072 f^1,5,
cao độ hơi vút lên khi bọt nổi (σ). Tiếng nước = tiếng vỗ (nhiễu băng rộng) + hàng trăm bọt nhiều cỡ + giọt rơi lại.

Xuất 3 tệp WAV 44,1 kHz 16 bit stereo vào Assets/PhoViet/Resources/NuocVang/ (21.371: bỏ tiếng gió khi đang rơi — người
dùng: «chỉ cần âm thanh bắt đầu lúc chạm nước đến khi chìm hẳn»):
  PV_AmNuocVang.wav — cú «ÙM» chạm nước: va thân xe, vỗ nước, hốc khí sập «bụp», mây bọt, nước bắn rơi lộp độp lại (3 s)
  PV_AmXeChim.wav   — xe đang chìm (nóc còn trên mặt nước): nước ào vào khoang, bọt khí ùng ục thoát ra, kẽo kẹt (2,4 s)
  PV_AmXeNgap.wav   — nóc xe vừa ngập: «ực» lớn (túi khí cuối thoát ra) + ầm trầm, mọi tiếng bóp nghẹt như nghe dưới
                      nước, bọt lẻ tẻ nổi lên rồi lặng khi xe nằm đáy (2,6 s). Game phát tệp này đúng lúc nóc xe ngập
                      (đo Unity AgentScripts/SinkAudio21369.cs: 0,6 s sau khi chạm nước khi rơi 8 m, ~1,1 s khi trôi chậm).
Stereo thật (21.370, người dùng: «cho âm thanh là stereo nhé không phải mono»): mỗi tiếng điểm (bọt, giọt, vỗ, «ực») được
đặt trong trường stereo bằng chênh mức + chênh thời gian tới hai tai (ITD ≤ 0,65 ms); tiếng nền (nước ào, sóng) là nguồn riêng
từng kênh và di chuyển trái↔phải; phản xạ sớm lệch hai bên (bờ kè trái gần, phải xa) + vang riêng mỗi kênh; nới rộng M/S
phần trên 250 Hz. Mục tiêu đo: tương quan T/P ≤ 0,45, tỉ lệ side/mid ≥ 0,6 cho mọi tệp (in ra khi chạy).
Chạy: python ArtSource/Audio/ow_water_fall_audio.py   (--png xuất phổ kiểm tra, --mix ghép bản nghe thử trọn cú rơi
      vào ArtSource/Audio/preview/nuoc-roi-tron-bo.wav)
"""
import os, sys, wave
import numpy as np

SR = 44100
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'PhoViet', 'Resources', 'NuocVang')
PREV = os.path.join(os.path.dirname(__file__), 'preview')



def rng(seed):
    return np.random.default_rng(seed)


def band(x, lo, hi, tilt=0.0):
    """lọc băng bằng FFT (mép mềm); tilt < 0 nghiêng về trầm (−1 ≈ nhiễu hồng)"""
    n = len(x); X = np.fft.rfft(x); f = np.fft.rfftfreq(n, 1 / SR)
    g = np.ones_like(f)
    if lo > 0: g *= 1 / (1 + (lo / np.maximum(f, 1e-3)) ** 4)
    if hi > 0: g *= 1 / (1 + (f / hi) ** 4)
    if tilt: g *= (np.maximum(f, 20) / 1000.0) ** (tilt / 2)
    return np.fft.irfft(X * g, n)


def lowpass_var(x, cut0, cut1, t0, t1):
    """bóp nghẹt dần: trộn bản gốc ↔ bản lọc trầm theo thời gian (nghe như chìm xuống nước)"""
    lo = band(x, 0, cut1)
    t = np.arange(len(x)) / SR
    m = np.clip((t - t0) / max(t1 - t0, 1e-3), 0, 1)
    m = m * m * (3 - 2 * m)
    if cut0 > 0:
        x = band(x, 0, cut0)
    return x * (1 - m) + lo * m


def smooth_noise(n, rate, r):
    """nhiễu chậm (dao động ngẫu nhiên ~rate Hz) quanh 1"""
    k = max(2, int(n * rate / SR) + 2)
    pts = r.standard_normal(k)
    return np.interp(np.linspace(0, k - 1, n), np.arange(k), pts)


def env_ad(t, a, d):
    return np.where(t < a, t / max(a, 1e-6), np.exp(-(t - a) / d))


MAXITD = 0.00065                                              # chênh thời gian tối đa tới hai tai (s)


def put(buf, i0, s, pan):
    """đặt một âm điểm vào trường stereo: chênh mức (luật công suất) + chênh thời gian tới hai tai"""
    p = float(np.clip(pan, 0, 1)); g = (np.cos(p * np.pi / 2), np.sin(p * np.pi / 2))
    d = int(round(abs(p - 0.5) * 2 * MAXITD * SR))
    for ch, ii in ((0, i0 + (d if p > 0.5 else 0)), (1, i0 + (d if p < 0.5 else 0))):
        m = min(len(s), buf.shape[1] - ii)
        if m > 0: buf[ch, ii:ii + m] += g[ch] * s[:m]


def add_moving(buf, s, pan):
    """nguồn liên tục di chuyển trái↔phải theo đường pan (mảng 0..1 cùng độ dài)"""
    p = np.clip(pan, 0, 1); buf[0] += s * np.cos(p * np.pi / 2); buf[1] += s * np.sin(p * np.pi / 2)


def wide_pan(r, a=0.55):
    """vị trí ngẫu nhiên nghiêng ra hai biên (beta < 1) → tiếng lộp độp/bọt rải rộng cả hai tai"""
    return r.beta(a, a)


def reflections(buf, left=(0.017, 0.31), right=(0.029, 0.26), hi=4500):
    """phản xạ sớm lệch hai bên: bờ kè/nhà bên trái gần (trễ ngắn), bên phải xa (trễ dài); dội chéo kênh"""
    out = buf.copy()
    for ch, (dl, g), src in ((0, left, 1), (1, right, 0)):
        k = int(dl * SR); y = band(buf[src], 150, hi)
        out[ch, k:] += g * y[:-k]
        k2 = int(dl * 2.3 * SR); out[ch, k2:] += 0.45 * g * band(buf[ch], 200, hi * 0.7)[:-k2]
    return out


def widen(buf, amount=1.35, lo=250):
    """nới rộng M/S: tăng phần khác nhau giữa hai kênh ở dải trên lo Hz (dải trầm giữ nguyên cho chắc)"""
    M = (buf[0] + buf[1]) / 2; S = (buf[0] - buf[1]) / 2
    S = S + (amount - 1) * band(S, lo, 0)
    return np.vstack([M + S, M - S])


def bubble(buf, t0, f0, amp, pan, sigma=0.1, decay=1.0, r=None):
    """một bọt khí Minnaert vào bộ đệm stereo"""
    d = (0.13 * f0 + 0.0072 * f0 ** 1.5) * decay
    dur = min(6.0 / d, 1.5)
    n = int(dur * SR); i0 = int(t0 * SR)
    if i0 >= buf.shape[1] or n < 8: return
    n = min(n, buf.shape[1] - i0)
    t = np.arange(n) / SR
    f = f0 * (1 + sigma * d * t)
    ph = 2 * np.pi * np.cumsum(f) / SR + (r.uniform(0, 6.28) if r is not None else 0)
    s = amp * np.sin(ph) * np.exp(-d * t) * np.minimum(1, t / 0.0012)
    put(buf, i0, s, pan)


def click(buf, t0, amp, pan, r, hp=2500):
    n = int(0.004 * SR); i0 = int(t0 * SR)
    if i0 + n >= buf.shape[1]: return
    s = r.standard_normal(n) * np.exp(-np.arange(n) / (0.0007 * SR))
    s = s - np.convolve(s, np.ones(4) / 4, 'same')        # bỏ phần trầm → tiếng tách giọt
    put(buf, i0, amp * s, pan)


def radius_mm(r, lo, hi, small_bias=1.6):
    u = r.random() ** small_bias                              # nhiều bọt nhỏ, ít bọt to
    return np.exp(np.log(lo) + (np.log(hi) - np.log(lo)) * u)


def reverb(buf, r, length=0.7, decay=0.22, wet=0.16, hi=3500):
    """phản xạ ngắn của bờ kè/nhà ven sông (IR nhiễu tắt dần, khác nhau mỗi kênh)"""
    n = int(length * SR); t = np.arange(n) / SR
    out = buf.copy()
    for ch in range(2):
        ir = r.standard_normal(n) * np.exp(-t / decay); ir[:int(0.012 * SR)] = 0
        ir = band(ir, 120, hi); ir /= np.sqrt(np.sum(ir ** 2)) + 1e-9
        m = len(buf[ch]) + n
        y = np.fft.irfft(np.fft.rfft(buf[ch], m) * np.fft.rfft(ir, m), m)[:len(buf[ch])]
        out[ch] = buf[ch] + wet * y
    return out


def finish(buf, peak=0.89, fade_out=0.25):
    buf = buf - buf.mean(axis=1, keepdims=True)
    n = buf.shape[1]; f = int(fade_out * SR)
    if f: buf[:, n - f:] *= np.linspace(1, 0, f) ** 2
    buf[:, :int(0.002 * SR)] *= np.linspace(0, 1, int(0.002 * SR))
    m = np.max(np.abs(buf)); return buf * (peak / m) if m > 0 else buf


def write(name, buf):
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, name)
    pcm = (np.clip(buf, -1, 1) * 32767).astype('<i2').T.copy()
    with wave.open(p, 'wb') as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    print(name, f'{buf.shape[1] / SR:.2f}s', f'{os.path.getsize(p) / 1024:.0f} KB')
    return p


# ------------------------------------------------------------------ 1. cú «ÙM» chạm nước
def impact():
    r = rng(36521); L = 3.0; n = int(L * SR); t = np.arange(n) / SR
    buf = np.zeros((2, n))
    # a) thân xe đập mặt nước: tiếng ục trầm 72→44 Hz (giữa, hai kênh lệch nhẹ cao độ → dày) + vỗ «chát» hai mép đầu xe
    for ch, det in ((0, 1.0), (1, 1.03)):
        f = (72 * np.exp(-t / 0.25) + 44) * det
        buf[ch] += np.sin(2 * np.pi * np.cumsum(f) / SR + ch * 0.9) * env_ad(t, 0.004, 0.16) * 1.15
    for pan, dt, g in ((0.1, 0.0, 2.0), (0.9, 0.007, 1.9), (0.5, 0.003, 0.8)):          # mép trái, mép phải, giữa
        s = band(r.standard_normal(n), 120, 5000, tilt=-0.4) * env_ad(t, 0.0015, 0.035) * g
        put(buf, int(dt * SR), s, pan)
    # b) hai vạt nước tung sang hai bên: trái loang dần ra biên trái, phải trễ chút loang ra biên phải
    for side, dly in ((0, 0.0), (1, 0.035)):
        x = band(r.standard_normal(n), 250, 7000, tilt=-0.7)
        turb = np.clip(1 + 0.55 * smooth_noise(n, 28, r), 0.1, 2.5)
        tt = np.maximum(t - dly, 0)
        e = (env_ad(tt, 0.012, 0.28) * 0.75 + env_ad(tt, 0.03, 0.75) * 0.3) * (t >= dly)
        spread = np.clip(tt / 0.5, 0, 1)
        pan = 0.4 - 0.37 * spread if side == 0 else 0.6 + 0.37 * spread
        add_moving(buf, x * turb * e * 2.5, pan)
    # c) hốc khí sau thân xe sập lại: «bụp» trầm vút lên — giữa, rồi trái, rồi phải
    bubble(buf, 0.16, 95, 0.75, 0.45, sigma=0.35, decay=0.35, r=r)
    bubble(buf, 0.22, 140, 0.5, 0.12, sigma=0.3, decay=0.45, r=r)
    bubble(buf, 0.27, 120, 0.45, 0.88, sigma=0.3, decay=0.45, r=r)
    # d) mây bọt: ~420 bọt 0,8–12 mm, dày nhất 0,05–0,4 s, rải rộng hai bên
    for _ in range(420):
        tt = 0.02 + r.exponential(0.32)
        if tt > 2.2: continue
        rad = radius_mm(r, 0.8, 12.0); f0 = 3.26 / (rad / 1000)
        bubble(buf, tt, f0, 0.05 * (rad / 3) ** 0.7 * np.exp(-tt / 0.9), wide_pan(r, 0.7), sigma=r.uniform(0.02, 0.15), r=r)
    # e) nước bắn lên rồi rơi lại: giọt lộp độp 0,4–2,6 s quanh cả hai tai (đỉnh ~0,95 s — khớp giọt hạt trong game)
    for _ in range(900):
        tt = 0.35 + r.gamma(2.6, 0.24)
        if tt > 2.75: continue
        pan = wide_pan(r, 0.5); a = 0.06 * r.uniform(0.3, 1.0) * np.exp(-max(0, tt - 1.3) / 0.6)
        click(buf, tt, a, pan, r)
        if r.random() < 0.45:
            rad = radius_mm(r, 0.6, 3.2, 1.2); bubble(buf, tt + 0.002, 3.26 / (rad / 1000), a * 1.4, pan, sigma=0.12, r=r)
    # f) sóng vỗ bờ hai bên luân phiên (trái rồi phải) tắt dần
    lap = np.sin(2 * np.pi * 0.9 * t)
    for ch, sg in ((0, 1), (1, -1)):
        buf[ch] += band(r.standard_normal(n), 300, 4000, tilt=-0.5) * env_ad(t, 0.4, 0.9) * 0.2 * np.clip(1 + 0.6 * sg * lap, 0, 2)
    buf = reflections(buf)
    buf = reverb(buf, r, wet=0.2)
    buf = widen(buf, 1.35)
    return finish(buf, fade_out=0.3)


# ------------------------------------------------------------------ 2. xe chìm (còn trên mặt nước) → 3. ngập hẳn
# Thời điểm ngập thay đổi theo tốc độ lao (đo Unity: 0,6 s khi rơi 8 m, ~1,1 s khi trôi xuống chậm) → tách 2 tệp:
# game phát PV_AmXeChim ngay lúc chạm nước, tới khi nóc xe xuống dưới mặt nước thì tắt nhanh nó và phát PV_AmXeNgap.
def sink_above():
    r = rng(1369); L = 2.4; n = int(L * SR); t = np.arange(n) / SR
    buf = np.zeros((2, n))
    # a) nước ào vào khoang qua hai cửa: cửa trái trước, cửa phải trễ 0,18 s; mỗi dòng xoáy riêng, lắc nhẹ tại chỗ
    for pan0, dly in ((0.15, 0.0), (0.85, 0.18)):
        x = band(r.standard_normal(n), 140, 1500, tilt=-0.9)
        turb = np.clip(1 + 0.6 * smooth_noise(n, 9, r), 0.05, 2.5)
        tt = t - dly
        e = np.clip(tt / 0.3, 0, 1) * np.exp(-np.maximum(0, tt - 1.3) / 0.6)
        add_moving(buf, x * turb * e * 1.5, pan0 + 0.08 * smooth_noise(n, 1.5, r))
    # b) ùng ục: từng loạt 1–3 bọt to (10–30 mm → 110–330 Hz) vút lên «ọc», loạt trái rồi loạt phải xen kẽ
    tt = 0.12; side = 0
    while tt < L - 0.4:
        w = np.exp(-max(0, tt - 1.3) / 0.6)
        for k in range(r.integers(1, 4)):
            rad = r.uniform(10, 30); f0 = 3.26 / (rad / 1000)
            pan = r.uniform(0.08, 0.35) if side == 0 else r.uniform(0.65, 0.92)
            bubble(buf, tt + k * r.uniform(0.03, 0.07), f0, 0.55 * w * r.uniform(0.6, 1.0), pan,
                   sigma=r.uniform(0.25, 0.5), decay=r.uniform(0.3, 0.5), r=r)
        tt += r.uniform(0.14, 0.26); side = 1 - side if r.random() < 0.8 else side
    # c) dòng bọt nhỏ sủi liên tục (1,5–7 mm) rải khắp hai bên
    for _ in range(480):
        tt = r.uniform(0, L - 0.3)
        rad = radius_mm(r, 1.5, 7.0)
        bubble(buf, tt, 3.26 / (rad / 1000), 0.035 * np.exp(-max(0, tt - 1.2) / 0.6), wide_pan(r, 0.6), sigma=0.1, r=r)
    # d) thân xe kẽo kẹt khi nước ép (rất nhẹ; hai kênh lệch cao độ → phách lan hai bên)
    for ch, f00 in ((0, 62.0), (1, 63.6)):
        f = f00 - 8 * t / L
        buf[ch] += np.sin(2 * np.pi * np.cumsum(f) / SR) * (0.5 + 0.5 * np.sin(2 * np.pi * 5.5 * t + ch * 1.6)) * np.clip((t - 0.3) / 0.5, 0, 1) * 0.12
    buf = reflections(buf, left=(0.015, 0.25), right=(0.026, 0.22))
    buf = reverb(buf, r, wet=0.15)
    buf = widen(buf, 1.3)
    return finish(buf, peak=0.8, fade_out=0.6)


def sink_under():
    r = rng(2369); L = 2.6; n = int(L * SR); t = np.arange(n) / SR
    buf = np.zeros((2, n))
    # a) «ực» lớn: túi khí cuối thoát ra (trái, rồi phải, rồi trái) + ầm trầm riêng mỗi kênh
    bubble(buf, 0.01, 78, 1.0, 0.27, sigma=0.45, decay=0.22, r=r)
    bubble(buf, 0.10, 112, 0.75, 0.86, sigma=0.4, decay=0.3, r=r)
    bubble(buf, 0.21, 150, 0.5, 0.12, sigma=0.35, decay=0.35, r=r)
    for ch in range(2):
        buf[ch] += band(r.standard_normal(n), 30, 220) * env_ad(t, 0.04, 0.55) * 2.0
    # b) dư âm nước ào bị bóp nghẹt (như nghe dưới nước) xoáy chậm quanh đầu, tắt nhanh
    x = band(r.standard_normal(n), 80, 500, tilt=-0.8) * np.clip(1 + 0.5 * smooth_noise(n, 7, r), 0, 2) * env_ad(t, 0.02, 0.35) * 0.9
    add_moving(buf, x, 0.5 + 0.4 * np.sin(2 * np.pi * 0.8 * t))
    for ch in range(2):
        buf[ch] += band(r.standard_normal(n), 80, 500, tilt=-0.8) * env_ad(t, 0.02, 0.35) * 0.45
    # c) bọt lẻ tẻ nổi lên (nghẹt), luân phiên hai bên, thưa + nhỏ dần tới khi xe nằm đáy (~1,6 s) rồi lặng
    tt = 0.35; side = 0
    while tt < L - 0.4:
        pan = r.uniform(0.1, 0.4) if side == 0 else r.uniform(0.6, 0.9)
        bubble(buf, tt, r.uniform(160, 420), 0.32 * np.exp(-tt / 0.9), pan, sigma=0.3, decay=0.5, r=r)
        tt += r.uniform(0.15, 0.4); side = 1 - side
    for _ in range(140):
        tt = r.exponential(0.6)
        if tt > L - 0.3: continue
        rad = radius_mm(r, 2, 8); bubble(buf, tt, 3.26 / (rad / 1000), 0.04 * np.exp(-tt / 0.8), wide_pan(r, 0.6), sigma=0.1, r=r)
    buf = np.vstack([band(buf[ch], 0, 750) for ch in range(2)])          # dưới nước: mất hết tiếng cao
    buf = reflections(buf, left=(0.021, 0.22), right=(0.034, 0.2), hi=800)
    buf = reverb(buf, r, wet=0.12, hi=900)
    buf = widen(buf, 1.3, lo=120)
    return finish(buf, peak=0.85, fade_out=0.5)


def stereo_stats(buf):
    L, R = buf[0], buf[1]
    return float(np.corrcoef(L, R)[0, 1]), float(np.sqrt(np.mean((L - R) ** 2)) / max(1e-9, np.sqrt(np.mean((L + R) ** 2))))


def preview_mix(parts, hit=0.15, sub=0.62):
    """bản nghe thử từ lúc chạm nước tới khi chìm hẳn (ngập sau 0,62 s như đo Unity khi rơi 8 m): ùm + chìm → ngập «ực»"""
    n = int((hit + sub + 2.7) * SR); buf = np.zeros((2, n)); k = int(hit * SR)
    s = parts['PV_AmNuocVang.wav']; m = min(s.shape[1], n - k); buf[:, k:k + m] += s[:, :m]
    c = parts['PV_AmXeChim.wav'] * 0.9; ks = int((hit + sub) * SR); m = min(c.shape[1], n - k)
    fade = np.ones(m); j = ks - k; fl = int(0.25 * SR); fade[j:j + fl] = np.linspace(1, 0, fl); fade[j + fl:] = 0
    buf[:, k:k + m] += c[:, :m] * fade
    u = parts['PV_AmXeNgap.wav'] * 0.95; m = min(u.shape[1], n - ks); buf[:, ks:ks + m] += u[:, :m]
    buf *= 0.89 / np.max(np.abs(buf))
    os.makedirs(PREV, exist_ok=True); p = os.path.join(PREV, 'nuoc-roi-tron-bo.wav')
    pcm = (np.clip(buf, -1, 1) * 32767).astype('<i2').T.copy()
    with wave.open(p, 'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(SR); f.writeframes(pcm.tobytes())
    print('preview', p, f'{n / SR:.2f}s', 'tương quan T/P %.2f, side/mid %.2f' % stereo_stats(buf))


def spectro_png(name, buf):
    from PIL import Image
    x = buf.mean(axis=0); win = 2048; hop = 256
    frames = [x[i:i + win] * np.hanning(win) for i in range(0, len(x) - win, hop)]
    S = np.abs(np.fft.rfft(np.array(frames), axis=1)).T
    f = np.fft.rfftfreq(win, 1 / SR)
    # trục tần số log 30 Hz – 16 kHz
    rows = 300; fl = np.geomspace(30, 16000, rows); idx = np.searchsorted(f, fl)
    img = 20 * np.log10(S[idx] + 1e-6); img = np.clip((img - img.max() + 80) / 80, 0, 1)[::-1]
    os.makedirs(PREV, exist_ok=True)
    Image.fromarray((img * 255).astype(np.uint8)).resize((900, 300)).save(os.path.join(PREV, name + '.png'))


if __name__ == '__main__':
    parts = {'PV_AmNuocVang.wav': impact(), 'PV_AmXeChim.wav': sink_above(), 'PV_AmXeNgap.wav': sink_under()}
    for k, v in parts.items():
        write(k, v); print('   tương quan T/P %.2f, side/mid %.2f' % stereo_stats(v))
        if '--png' in sys.argv: spectro_png(k[:-4], v)
    if '--mix' in sys.argv: preview_mix(parts)
