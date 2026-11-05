# Phố Việt 1.55.0 (05/11/2026)

Codex build; Claude Code kiểm tra cuối và phát hành, không rebuild. Chi tiết: `huongdan.md` 21.124–21.133.

## Thay đổi so với 1.54.0

- Trực thăng khi bay: 2–5 chiếc theo quãng đường, bắn tên lửa 40 m/s; đổi làn, hạ hoặc lộn để né.
- Tên lửa người chơi: kho 3/3, nút tròn/phím F, vật phẩm mẫu C trên đường, tự khóa máy bay phía trước.
- Lửa/khói tại điểm va chạm; vệt lửa tên lửa rõ hơn; âm lượng tổng +30%.
- Nhảy mobile bật ngay; lần vuốt giữ bay sớm chờ đủ 70% độ cao.
- Công việc đưa đón vẫn khóa.

## Kiểm tra

- Build `Succeeded`, 2 gói mobile/desktop, 0 lỗi. Cache `7a2f7e521dd62b06`.
- 182 preflight mỗi GPU; 43 bundle repo; 33 bundle site; 7 publish parts.
- Trình duyệt 468 đạt, 0 lỗi JS/Unity: landing 387, web/PWA 37, fullscreen 20, năng lượng 13, tên lửa 11.
- Âm lượng trên WASM thật: gain động cơ 0,1768 = 0,136 × 1,3.

## Phát hành

- Source/artifact `86b26ec`, push main.
- Pages run 41983779125 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/41983779125
- Live: version 1.55.0 / cache `7a2f7e521dd62b06`; 27 kiểm tra đạt, 0 lỗi runtime (`release155-live-results.json`, `release155-pages.json`).
- Bài tên lửa/âm lượng chạy trên chính trang live: 11 đạt, 0 lỗi (`release155-live-missiles-checks.json`).

## Chưa làm

- Chưa thử Safari/Android máy thật.
