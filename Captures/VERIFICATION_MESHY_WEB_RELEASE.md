# Phát hành WebGL Phố Việt 1.31.0 — xe Meshy

**Đã phát hành và xác minh live ngày 23/10/2026 (Asia/Bangkok).**

- URL: [https://denispham1107.github.io/taixecongnghe/](https://denispham1107.github.io/taixecongnghe/)
- Source + WebGL commit: `6a0266098bf8fd80cbd6e7b05896ddf04079c3f9`.
- Cache: `4f9dbd6172a2b7df`, build UTC `2026-10-22T20:34:46.3473513Z`.
- Build cuối `build_1e00ac19ccba` Succeeded, 0 lỗi / 1 cảnh báo Pipeline đã biết, thời gian 30,275 ms.
- [GitHub Pages run 39851261543](https://github.com/denispham1107/taixecongnghe/actions/runs/39851261543): completed/success, đúng SHA commit phát hành.
- Native art/gameplay: **15 suite / 4.572 điều kiện** đạt (bằng chứng 21.7).
- Local browser: **83 kiểm tra** đạt = PWA/offline 37, so tài nguyên/WASM 26, fullscreen iPhone/iPad dọc-ngang 20. Static bundle **28 kiểm tra** đạt.
- Live: **26 kiểm tra đạt**, version/cache khớp docs, mọi tài nguyên deploy khớp bản local (binary từng byte, text chuẩn hóa CRLF), Unity WASM khởi tạo thật và start/pause không lỗi. **0 lỗi runtime**.

Xe chính trắng–đen, xe giao thông/quái xế/xe đậu dùng Meshy nhiều màu, mô tô CSGT giữ nguyên. Mã/scene công việc vẫn khóa, Coin/kỷ lục không bị xóa. Đèn/mặt kính phát sáng và khói boost gắn theo rig. Một bản xe mẫu rời ngoài game root được giữ nguyên trong Editor, gán tag EditorOnly để không lọt vào build; packedAssets bản cuối không chứa model/texture gốc MeshyImports.

Đã xem ảnh WebGL desktop, mobile dọc/ngang, offline và ảnh live `meshy-live-game.png`. Profile browser không thay cho kiểm tra phần cứng iPhone/Android thật; chưa đo FPS/nhiệt máy thật. Các file Wave/WaveAlpha/Blender cũ, plugin Bridge và Packages cục bộ được giữ ngoài commit này.

Service worker cache mới chỉ kích hoạt đầy đủ sau khi tải xong và các tab cũ đóng; nếu còn thấy xe cũ, đóng các tab/ứng dụng Phố Việt rồi mở lại. Không yêu cầu xóa dữ liệu vì có thể mất Coin/kỷ lục.

Bằng chứng: `meshy-pages-release.json`, `meshy-live-results.json`, `meshy-release-local-summary.json`, `meshy-release-build-summary.json`, `meshy-release-bundle-checks.json` và các ảnh `meshy-release-web-*.png`.
