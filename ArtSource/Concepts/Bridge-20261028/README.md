# Ba mẫu cầu tham khảo Phố Việt

**Chưa được chọn hoặc đưa vào game. Chỉ thay cây cầu sau khi người dùng chọn và xác nhận rõ mẫu.**

- A – Sông Việt: ba vòm bê tông sáng, lan can đồng có họa tiết, đèn lồng gọn.
- B – Vòm Xanh: hai vòm thép xanh, dây treo và bản mã kim loại.
- C – Cánh Buồm: hai tháp dây văng, cáp hình quạt, dầm hộp xanh đậm.

Mở `bridge-comparison.html` hoặc `bridge-comparison.png` để so sánh. Mỗi mẫu có `*-day.png` và `*-street.png` 1600×1000. File gốc `PhoViet_Bridge_Concepts.blend` chứa ba scene PV85_A/B/C; scene gốc trước khi làm vẫn trống. Collection `BRIDGE_ONLY` là kết cấu cầu, `PRESENTATION_NOT_FOR_GAME` chỉ là bối cảnh nước/bờ trơn/ánh sáng/camera để xem mẫu.

Mặt đường rộng6,2m, deck7,4m, dài50,4m. Chưa có LOD, collider hoặc gộp mesh cho runtime. Không copy cả bối cảnh tham khảo sang Unity; mẫu được chọn cần bước tối ưu/kiểm camera và chỉ thay geometry cây cầu theo phạm vi người dùng cho phép.

Các script chỉ chạy bằng Blender MCP trong file/scene mới: `build_bridge_concepts.py` gọi refinement, `render_bridge_concepts.py` xếp6ảnh. Không chạy lại builder vào scene đã có PV85_ (nó sẽ dừng để tránh ghi đè). Không gọi generator phố/cầu Unity cũ. `selection.json` phải giữ selected=null/confirmed=false cho đến khi có câu trả lời của người dùng.

Bằng chứng: `blender-verification.json`, `comparison-checks.json`, `game-unchanged-checks.json`: đủ3mẫu/6ảnh, chiều dài/mặt đường đúng;2993fileAssets/ProjectSettings/Packages/docs byte-identical so với lúc bắt đầu (bao gồm thay đổi cục bộ vốn có). Chưa build/push/phát hành; game vẫn1.47.0.
