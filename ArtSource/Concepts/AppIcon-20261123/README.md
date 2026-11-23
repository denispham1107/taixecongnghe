# 21.343 — Mẫu icon ứng dụng mới (chờ duyệt)

- `a-dai-lo-hoang-hon.svg`, `b-huy-hieu-neon.svg`, `c-bay-qua-sam-set.svg`, `d-den-pha-can-canh.svg`: SVG 512², vẽ tay bằng vector.
- `sh render.sh [tên]` → `raw/<tên>.png` 1024² (Chrome headless).
- `so-sanh-4-mau.html` → `so-sanh-4-mau.png/.jpg`: icon lớn + 48/32/16/tròn, cỡ thật iPhone/Android/tab Chrome/thanh tác vụ, cột đầu là icon hiện tại.
- 21.344: bộ mới không xe m1–m4 + `so-sanh-mau-khong-xe.html`; A–D (21.343) bị chê.
- Chưa thay icon trong game/web. Khi được duyệt: xuất 180 Apple, 192/512 any, 512 maskable (thu vào vùng an toàn), cập nhật manifest/index.html và `ArtSource/Web/pho-viet-app-icon.svg` + `AgentScripts/GenerateWebIcons.cjs`.
- 21.345: kiểu D/E xe vàng `kieu-de/` (build.py → k1–k4, `sh ../render.sh`), bảng `so-sanh-kieu-de.html`. m1–m4 (21.344) và A–D (21.343) không dùng.
- 21.346: người dùng chọn mẫu 2 → vẽ lại `kieu-de/k2v2.py` (xe/xe tăng/tên lửa mới + sông, tàu), bảng `so-sanh-mau2-ve-lai.jpg`.
