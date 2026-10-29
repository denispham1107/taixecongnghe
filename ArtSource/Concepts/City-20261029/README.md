# Phố Việt — Bộ nhà phố theo ảnh tham khảo, 29/10/2026

**Chỉ là mô hình để duyệt. Chưa được đưa vào Unity hoặc web.**

Người dùng yêu cầu dựng lại toàn bộ nhóm nhà theo ảnh phố hiện đại nhiều màu;
cây cối vẫn giữ nguyên và chỉ tích hợp khi người dùng duyệt rõ ràng.
Yêu cầu này độc lập với ba mẫu cầu21.85 còn đang chờ chọn.

## Thiết kế

- 13 mẫu cửa hàng tương ứng các nhóm hiện có, kể cả Cù Lao Petshop xanh dương,
  chữ trắng và phụ đề `Lưu Trú - Spa Pet`.
- 2 mẫu nhà góc có cửa kính và biển hiệu chạy vòng mặt bên.
- 4 mẫu căn hộ ven sông, 6–9 tầng trên tầng cửa hàng; mặt đường và mặt sông đều có chi tiết.
- Khung mặt tiền xám–xanh đậm, sàn/viền trắng, kính xanh, nội thất vàng ấm,
  mái hiên sọc, biển đứng và billboard thú cưng nổi có chiều sâu.
- Mặt tiền nhà phố8,36m, chiều sâu7,8m; căn hộ16,72m/sâu3m.
- Cây, chó/mèo/thỏ dùng mesh nguồn hiện tại, không sửa hình học nguồn.
  Đường, vỉa hè, ánh sáng, cây và các vị trí đặt trong scene preview chỉ là ngữ cảnh xem mẫu.

## Nguồn và ảnh

- `build_city_study.py`: dựng bằng Blender MCP, không import/export Unity.
- `refine_city_study.py`: sửa font/kính/camera sau khi xem ảnh; builder tự gọi cuối.
- `render_city_study.py`: queue6render thật qua timer Blender.
- `PhoViet_NewCity_Approval.blend`: savecopy riêng, ảnh atlas nguồn được pack.
- `study.json`: catalogue19mẫu và camera/render.
- `approval.json`: chưa duyệt/chưa tích hợp.
- `protected-game-fingerprint.json`: SHA bảo vệ Assets/ProjectSettings/Packages/docs trước tác vụ.

Đã hoàn tất và xem cả6ảnh: `street-wide.png`, `street-portrait.png`,
`petshop-detail.png`, `architecture-close.png`, `building-catalogue.png`, `riverside.png`.
Mở `preview.html` để xem bộ ảnh, `city-approval-board.png` để xem bảng duyệt nhanh.
Ảnh là render Blender thật, chưa phải gameplay sau tích hợp.

Kiểm tra:42Blender +34filecheck đạt;3104filegame không đổi/không thêm.
Gallery không tràn ngang ở1440×900/390×844/844×390; đủ6card/7ảnh có ảnhthamkhảo.
MCP client bịTransportclosed sau lượt render đầu; helper`call_blender_addon.py`
dùng JSON API của chính addon Blender MCP đang mở tạilocalhost9876 để hoàn tất,
không đổi config/restart/cài addon. Đợt ảnh cuối32EEVEE samples hoàn thành306,06s.

Đây là mẫu hình ảnh. Chưa thiết kế LOD, gộp mesh, collider, atlas runtime hoặc đo hiệu năng.
Không chạy installer phố/sông/cầu cũ để nhập mẫu. Chỉ sau khi duyệt mới lập bước tích hợp
từng nhóm nhà, giữ cây, ranh đường/sông và gameplay hiện tại.
