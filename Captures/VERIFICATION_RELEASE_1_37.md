# Phố Việt 1.37.0 — cầu sông, xe tăng bắn đạn, ánh sáng/mũi tên mới, phố nhiều màu (25/10/2026)

## Thay đổi so với 1.36.0

- Cầu vượt sông theo ảnh mẫu, dựng bằng Blender MCP:
  - Không quá 2 khối phố liền nhau, nên chạy một lúc là gặp sông.
  - Cầu chỉ có 2 làn giữa; xe thường tự nhập vào làn cầu.
  - Ra ngoài mặt cầu là rơi xuống sông sâu 5 m dưới mặt đường, bắn nước rồi chìm dần.
  - Người chơi rơi xuống sông thì kết thúc lượt.
  - Mặt nước có sóng, bóng cầu/kè/trụ và thuyền neo.
  - `huongdan.md` 21.35–21.37.
- Xe tăng:
  - Cứ 9 giây có một xe tăng chạy ngược chiều (bỏ xe tăng ngẫu nhiên).
  - Xe tăng bắn đạn vào 1 trong 4 làn phía trước, có vòng cảnh báo dưới đất.
  - Bắn mỗi 1,0 s, chạy càng nhanh càng dày, tối thiểu 0,4 s.
  - Trúng đạn hoặc đứng trong bán kính nổ 2,2 m thì văng ra và kết thúc; xe khác trong bán kính cũng bị hất.
  - Hiệu ứng họng pháo, đạn bay và nổ dựng bằng Blender MCP.
  - `huongdan.md` 21.38–21.40.
- Ánh sáng sáng sớm có sương kiểu D: nắng vàng nhạt, sương xám bạc, hết ám trắng (21.41).
- Mũi tên báo xe phía sau kiểu D: ba mũi tên chạy lên (21.42).
- Cây và mặt tiền (21.43):
  - Cây dời vào khe giữa hai mái che; số đỉnh cây xuyên mái từ 132 còn 0.
  - Mặt tiền 12 tiệm nhiều màu, chỉ còn 1 tiệm vàng.
- Công việc đưa đón vẫn khóa.

## Kiểm tra

- Native (Play Mode): 28 suite / 9.762 điều kiện đạt.
  - Gồm VerifyRiverBridge 136, VerifyTankBarrage 69, RoofRiding 4.206, MeshyTank 1.136, MeshyCars 1.266, PortraitHud 21 và MobileCamera 8.
  - Kết quả: `saigon-street-VerifyRiverBridge/TankBarrage-results.json`, `river-bridge-Verify*-results.json`.
- Sửa test: VerifyRiverBridge giờ tắt xe tăng bắn khi thử cầu, vì đạn pháo từng làm lần chạy đầu trượt. Chạy lại 2 lần, đều 136/136. Không đổi mã game.
- Preflight Edit Mode: `release-1.37-preflight.json`.
- Build `build_9c295f769ef8`: Succeeded, 293.753 ms, 0 lỗi, 3 cảnh báo đã biết. Cache `5018a747f9b8d80c`.
- 29 kiểm tra bundle: `release-1.37-bundle-checks.json`.
- Trình duyệt local: 77 điều kiện, 0 lỗi runtime.
  - DPR3: 14 (`release-1.37-mobile-results.json`, ảnh `release-1.37-mobile-portrait.png` và `-landscape.png`).
  - Tài nguyên + WASM: 26 (`release-1.37-local-results.json`).
  - PWA/offline/cảm ứng: 37.

## Phát hành

- Đang commit/push main. SHA, Pages run và kiểm tra live sẽ ghi tiếp vào đây.
