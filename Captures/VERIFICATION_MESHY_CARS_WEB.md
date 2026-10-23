# Phố Việt 1.33.0 — phát hành ô tô Meshy (24/10/2026)

Yêu cầu trực tiếp của người dùng: build và phát hành web. Source ô tô `ac45183`. Giữ công việc khóa, CSGT/xe máy/nhân vật và dữ liệu người chơi.

- Kế thừa 11 suite / 3.002 điều kiện Unity + 28 kiểm tra nguồn ở `VERIFICATION_MESHY_CARS.md`; không tuyên bố chạy lại chúng trong lần build này.
- Preflight cuối: đúng scene, version1.33.0, 11 ô tô Meshy/33 xe máy/30 đèn, 0 missing scripts, jobsfalse. Đủ 4 mẫu, 3 LOD, 7 clip/material/profile nóc.
- Build cuối `build_6bd4eaf57194`: Succeeded, 6.238 ms incremental, 86.476.927 byte, 0 lỗi/1 warning Pipeline thiếu RuntimePipelineConfig đã biết. Lần đầy đủ trước đó 329.081ms có cảnh báo mã Editor chưa compile; đã chờ ready/reconfigure rồi build lại. Không publish lần thử.
- Cache `f730cee9fbafa12d`, UTC `2026-10-23T20:24:30.7102647Z`. Hai build có cùng tài nguyên/cache; inventory packedAssets từ lần đầy đủ (lần incremental không lặp inventory) xác nhận 0 nguồn MeshyImports vào runtime, đủ material/maps 4 mẫu. Xem `meshy-cars-release-build-summary.json`.
- Bundle: **29** kiểm tra SHA cache, header gzip/WASM/data, đúng 4 hash cuối và mọi file dưới100MB. Gói precache86.475.378byte.
- Browser local: **77** kiểm tra đạt = DPR3 **14** + PWA/offline/cảm ứng **37** + đối chiếu tài nguyên/WASM **26**. Không lỗi runtime. iPhone390×844 →1170×2532; ngang2532×1170; tablet2066×1549; xoay lại phục hồi. Đã xem ảnh dọc/ngang: xe Meshy màu xanh/vàng hiển thị, nhân vật/xe máy giữ nét; shader không bị hồng/lỗi. Màu từng mẫu đã kiểm cận ở Unity trước build.
- Báo cáo: `meshy-cars-mobile-results.json`, `meshy-cars-pwa-results.json`, `meshy-cars-local-results.json`, `meshy-cars-release-bundle-checks.json`, ảnh `meshy-cars-mobile-{portrait,landscape}.png`, `meshy-cars-desktop.png`, `meshy-cars-offline.png`.
- **Đã commit/push/phát hành và xác minh live:** source/artifact `9f5c5210500e169fcda5418d976d1841307e6fb9`, [Pages run40021075002](https://github.com/denispham1107/taixecongnghe/actions/runs/40021075002) completed/success đúng SHA. **26 kiểm tra live đạt, 0 lỗi runtime**: đúng version/cache, từng tài nguyên khớp (binary từng byte/text chuẩn hóaCRLF), WASM khởi tạo/start/pause. Xem `meshy-cars-live-results.json`, `meshy-cars-pages-release.json`, `meshy-cars-live-game.png`. Chưa thử điện thoại vật lý/FPS/nhiệt độ.
- Unity ready/Edit Mode, server kiểm tra docs đã dừng. Đóng hết tab/app rồi mở lại nếu worker còn bản cũ; giữ dữ liệu người chơi. Báo cáo commit/push riêng, không rebuild tài liệu.
- Giữ ngoài commit công việc Wave/Bridge/Packages và các file Blender cục bộ có trước. Không rebuild chỉ cho báo cáo.
