# Phố Việt 1.59.0 — 07/11/2026

## Thay đổi

Gồm source21.142–21.145: W/vuốt xuống bắn tên lửa, boost tạm khóa; bay+20% nhưng không đổi nhảy; kho5/5; Rất cao cố định và Cao mới tự giảm; mỗi phố/cầu6icon năng lượng đất+7bay; một tên lửa dưới đất hạ tank, dừng chạy/bắn/cháy lớn/khói, xác xe vẫn gây va chạm.

## Kiểm tra

- Hai gói ETC2/DXT Succeeded; tổng378,570s, desktop75,799s. Không lỗi; báo cáo desktop có1warning PipelineNoRuntimeConfig (công cụ Editor không có config runtime, như các bản trước).
-193preflight +193postflight;584file nguồn nguyênSHA sau build;43bundle repo +34site +35index.
- 575 browser local đạt,0lỗi JS/Unity, gồm trang chủ/offline/fullscreen/năng lượng/tên lửa/vuốt/nút bay. Xem release159-browser-summary.json.
- Source21.145 đã qua1215Unity; các thay đổi bay805 và đồ họa233 có báo cáo riêng trước build.
- Cache `c6699b769f95f932`; data mobile70.307.165B, desktop124.252.759B; hai gói chung code sau giải nén. Artifact docs đã pack32MiB và materialize để thử.

## Trạng thái

Đang commit/push; chưa xác minh Pages/live. Unity Edit, scene sạch, jobs/boost khóa. Không thử Safari/Android máy thật hoặc cam kết FPS; giữ thay đổi Art/Wave/Packages/Simulator/Captures lịch sử ngoài commit phát hành.
