# Sao vàng ở chính giữa lưng áo hero

23/10/2026. Đã cập nhật texture trong Unity. Chưa commit/build WebGL/push/phát hành.

- bake_hero_back_star.py chiếu sao5cánh vào UV mặt lưng nguồn Golden Star Stance, tâm(0,1.18), R=.125m, tỉ lệ bán kính trong.381966. Mask nguồn và báo cáo tại ArtSource/Blender/MeshyHero_BackStarMask.png, Captures/meshy-back-star-mask.json.
- prepare_meshy_rider_atlas.py chỉ thêm sao vào palette hero, vàng theo bóng/nếp vải. Sao ngực, geometry/rig/clip/ngón v4 giữ nguyên, các LOD dùng cùng material. Không sửa nguồn import.
- Đối chiếu hash: mọi palette NPC/normal/surface/emission không đổi, nửa atlas xe hero nguyên. Texture checks JSON đạt. Đã xem ảnh sau lưng/góc chơi Unity, hình sao hoàn chỉnh, NPC không thêm sao.
- Captures và preflight success,33actor/1CSGT, không thiếu script/đèn, jobsfalse. Chỉ kiểm asset/hash/render/preflight cho thay đổi texture; không chạy lại gameplay suite hoặc build WebGL.
- Ảnh: meshy-back-star-hero-rear.png, meshy-back-star-hero-front.png, meshy-back-star-game.png. Web công khai vẫn1.31.0.
