# Phố Việt 1.35.0 — đổi làn đánh lái, nhà cao tầng kính, zoom dọc (24/10/2026)

## Thay đổi so với 1.34.0

- Đổi làn (A/D/←/→, vuốt ngang): đường cong, đầu xe/người quay theo hướng đi, nghiêng theo hướng đó, vừa chạy tới vừa trả lái và dựng xe từ từ (~0,6 s). Hộp va chạm giữ hướng đường. `huongdan.md` 21.29, 21.31.
- Nhà cao tầng phía sau: vách kính trong suốt theo tầng, văn phòng sáng đèn phía sau, hông có cửa sổ khung, mái có thiết bị. 21.28.
- Màn hình dọc zoom gần hơn 10% (`PortraitWorldScale` .525 → .5775, FOV ≈95,62°). 21.28.
- Sửa test VerifyRoofRiding/VerifyTruckJump cho xe tải Meshy (chỉ test). 21.30.
- Công việc vẫn khóa.

## Kiểm tra

- Native (Play Mode): 22 suite / 5.544 điều kiện đạt, gồm VerifyLaneSteering 123, RoofRiding 3.072, TruckJump 125, PortraitHud 21, MobileCamera 8.
- Preflight Edit Mode: `release-1.35-preflight.json`.
- Build `build_8f58593f4444` Succeeded, 250.356 ms, 0 lỗi / 3 cảnh báo đã biết. Cache `d06d08624dba54fc`.
- 29 kiểm tra bundle: `release-1.35-bundle-checks.json`.
- Trình duyệt local 77 điều kiện, 0 lỗi runtime: DPR3 14 (`release-1.35-mobile-results.json`, ảnh `release-1.35-mobile-portrait.png`/`-landscape.png`), tài nguyên + WASM 26 (`release-1.35-local-results.json`), PWA/offline/cảm ứng 37.

## Phát hành

Đang thực hiện — xem cập nhật bên dưới.
