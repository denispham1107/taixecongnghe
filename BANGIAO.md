# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈500 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **30/10/2026 — Codex**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.49.0** — commit`faa345d`; Pages`40927522323`success; cache`e37aaa3a16abef99`; **28liveđạt/0runtime** |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Simulator iPhone | **21.71:** đã thêm **Apple iPhone 14 Pro / 14 Pro Max**, Dynamic Island + Safe Area dọc/ngang; đang chọn Pro Max ngang, Safe Area bật. Asset chỉ trong Editor, chưa commit/push |
| Chế độ chơi | Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Người chơi trong source/scene | **Blue Petshop Supercar** xanh, **đã thu nhỏ15%** thay người áo đỏ+xe máy; Q/chạm hai lần áp sát+húc ngang, Space/vuốt lên nhảy4jet trắng, W/vuốt xuống boost2bô (21.64–21.65). **Đã có trên web1.40.0** |
| Điều khiển/nhảy trong source | **21.94 đã có trên web1.49.0:** lane pace thêm10%→1,3915; nhảy cao/vận tốc bật thêm10%, rơi vẫn22m/s²; đầu17m/s~4,00m. Giữ scale0,85/curveSpeed/W chốt lúc cất cánh. Thông số21.92 đã được thay bằng21.94 |
| NPC xe máy trong source21.67 | **Urban Rider mới:31actor(17traffic+14parked)+56baked nhánh**, rig19xương/3LOD42k/18,5k/6,7k, cổ+tay+thân đồngbộ; **đã có trên web1.41.0**. PlayerSupercar/CSGT giữ nguyên |
| Cài đặt đồ họa trong source | **21.78 đã có trên web1.45.0:** bánh răng trênWelcome → Cao/Trung bình/Thấp, áp dụng ngay/lưuPlayerPrefs; Cao giữ đúng baseline từng nền tảng.155kiểmtra+6ảnh+chuột/phím/lưuquaPlay đạt |
| Trang chủ HTML trong source | **21.83 đã có trên web1.47.0:** giao diện trắng–xanh/ảnhxeđua sángcósương, hếtchồng315kiểmtra12viewport; thêm37web+20fullscreen/0runtime;1.47.0có28liveđạt |
| Phản hồi đổi làn trong source | **21.81 đã có trên web1.46.0:** gain đầu quỹ đạo car + yawattack0,05s; nhận phím/vuốt ngay, giữ nhịp hoàn tất.779kiểm tra đạt/4ảnh đã xem; chi tiết huongdan21.81 |
| Đợt bổ sung sau rẽ trong source | **21.80 đã có trên web1.46.0:** hết cua → Dino ở giao lộ phía trước → Tank sau2s; giữ lịch10s/9s.389kiểmtra đạt/2ảnhđãxem; xem huongdan21.80 |
| Bộ nhà mới đang chờ duyệt | **21.86:**19mẫu Blender/6ảnh/board tại`ArtSource/Concepts/City-20261029/`; `approval.json=false`, **chưa tích hợp Unity/web**, cây giữ mesh hiện tại. Cầu21.85 vẫn chờ chọn riêng |
| Trang trí cầu hiện tại trong source | **21.87 đã sửa:** bỏ12chậuhoa/cầu,8cờ dùng16tayđỡngangxanhcóđaikẹp,đủ3LOD;45Blender+27assetUnity+101sceneđạt/6ảnhđãxem. **Đã có trên web1.48.0**,khôngápdụngmẫucầu21.85 |
| Đèn giao thông/điều hòa/biển thuê nhà | Signal2mặt/ACxámtrắng đã có trên web1.48.0. **21.94 đã có trên web1.49.0:** 24bảng thuê/3LOD rộng/chữ thêm20% so1.48.0 (tổng×1,44 so21.91), giữ tầng/vịtrí/màu/chiều cao bảng |
| Cờ ban công trong source | **21.88 đã sửa:** bỏ thanhđen cạnh cờ ở8biếnthểnhà/24LOD,giữcờ-sao/hoa/lan can;64Blenderđạt/24mesh+510refsUnitygiữnguyên,4ảnhđãxem. **Đã có trên web1.48.0** |
| Thay đổi cục bộ ngoài phát hành | Simulator21.71/Packages, Art/Wave/.blend và Captures lịch sử giữ cục bộ, không reset/stage toàn bộ. **21.70/21.72–21.74 đã có trên web1.43.0**; **21.76 kéo điểm xuất phát và21.77 sửa Safari/bộ nhớ đã lên1.44.0**; xem mục2 |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

- **21.96 source đã xong, chưa phát hành:** người dùng xác nhận xe tăng miễn nhiễm thân/chân/đuôi khủng long, tiếp tục chạy và bắn. `ResolveDinosaurHits` bỏqua`v.armored` trước dò/xung lực, không đụng speed/route/timer/pháo; xe khác/CSGT giữ như cũ. **61kiểm thực+69Dino+69Tank=199đạt**,0lỗi/warningcompile;6case body/tail×20/30/60FPS đều chạy>30m/bắn2phát sau1,5s. SHA3106file chỉ1filegame đổi;scene/asset/webgiữnguyên. Chi tiếthuongdan21.96.

- **21.95 đã phát hành1.49.0:** commit`faa345d`/Pages`40927522323`success/cache`e37aaa3a16abef99`. Gồm21.94 bảngthuê/chữ+20%,lane+10%→1,3915,đỉnh/v0nhảy+10%/fall22. Build2variant530,736s/0lỗi/1warningPipeline;107preflight/500filebytegiữ,43repo+34site+35staged+315landing+37web+20fullscreen+28liveđạt/0runtime. Đãđốichiếu toàn bộtài nguyênPages/khởiđộngWASMthật;khônghạđồhọa. Chi tiếthuongdan21.95.

- **21.94 đã có trên web1.49.0:** bảngthuê/chữ thêm20% (BlenderMCP riêng,24part/3LOD); lane thêm10%→1,3915; nhảy cao/vận tốc bật+10%, rơi22m/s² theo lựa chọn người dùng. **193Blender+920Unity đạt**,0lỗi/warningcompile,3ảnhbảngngày/đêmđãxem;72mesh/3248refs/2779LOD0missing. SHA3106filechỉ4filegameđổidựkiến;start1,92/jobsfalse/scene sạch. Nguồn`PhoViet_Rental2194.blend`riêng, khôngđụng .blend người dùng; xemhuongdan21.94.

- **21.93 đã phát hành1.48.0:** commit`a424235`/Pages`40905048845`success/cache`72d4794dd7d8a0a4`. Gồm21.87–21.92: trangtrícầu/cờ, menuPause, signal2mặt,ACxámtrắng,biểnthuê+20%,lane pace1,265. Build2variant/636,698s/0lỗi/1warningPipeline;102preflight/255source byte-stable;43repo+34site+35Gitstaged+315landing+37web+20fullscreen+28live đạt/0runtime. Đã đối chiếu toàn bộ tài nguyên trênPages và khởi độngWASM/thao tácPause thật. Chưa kiểm máy thật; giữDPR/texture/AA4/bóng/jobsfalse/start1,92. Chi tiết huongdan21.93.


- **21.92 đã xong:** signal2mặt đối xứng cả housing/visor/đèn đổi màu (gộp lens2mặt dùng chung1mesh/renderer), 24bảng thuê rộng/chữ+20%, chuyển làn thêm10% (1,265cumulative). **201Blender+505Unity đạt**,75mesh/3668refs/2779LOD0missing,10ảnh đã chụp/xem chi tiết. SHA3104file chỉ4assetFBX/JSON+PhoVietGame đổi, thêmpartialSignalFaces.cs/meta; scene/cầu/cây/AC/web giữ byte cũ. Nguồn mới`PhoViet_SignalRental2192.blend`, không ghi đè .blend21.91 đang có sửa cục bộ. Edit/scenesạch/start1,92/jobsfalse. Commit cục bộ kèm2tài liệu; **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.92.

- **21.91 nâng chi tiết cảnh đã xong:** Blender MCP dựng/xuất riêng26asset (24shop thường/Petshop+signal+river apartments),3LOD/chung vật liệu, không thêm renderer; đèn giữ socket/420bóng runtime, AC xám trắng, mỗi tòa ven sông1biển CHO THUÊ NHÀ ở tầng cao thấp khác nhau theo người dùng chọn. **175kiểm Blender đạt;147mesh/5138refs và10983renderer/7670obstacle giữ nguyên;2779LOD/0missing**,11ảnh Unity ngày/đêm/LOD đã xem. SHA3104file chỉ52FBX/JSON dự kiến đổi, scene/cầu/cây/gameplay giữ byte cũ. Nguồn`PhoViet_FixtureDetails.blend`;Unity Edit/scene sạch/start1,92/jobsfalse. Đã commit cục bộ kèm tài liệu/bằng chứng, **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.91.

- **21.90 menu Pause/kết thúc đã sửa:** Pause thêm **VỀ ĐẦU PHỐ** gọi ResetRide như khi thua; bỏ mũi tên sau **ĐI TIẾP** và **THỬ MỘT CHUYẾN NỮA**. Compile0lỗi/0warning; 6ảnh dọc/ngang/desktop đã xem, không chồng; kiểm Pause→Riding và Pause→Ready/0m/0speed/giữ kỷ lục đạt. Edit/scene sạch/start1,92/jobsfalse. Commit cục bộ cùng tài liệu/bằng chứng; **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.90.

- **21.89 đổi làn xe đua nhanh thêm15% đã xong:** `AdvanceLaneCurve` nhân pace car ×1,15, giữ đường cong/phản hồi đầu/steering damping và W giữa chừng. **399 điều kiện Unity đạt** (217 input/motion +182 scale/pace), compile0lỗi/0warning. Đo60FPS quãng tới làn22,4237→19,5149m, tỷ lệ1,1491 do lượng tử frame. Edit/scenesạch/start1,92/jobsfalse; đã commit cục bộ cùng tài liệu và báo cáo, **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.89.

- **21.88 bỏ thanhđen cờban công đãxong:** chỉxóa1cylở`flag()`;4tiệm0/3/6/9+4Petshop/3LODđãexport/import.64kiểmBlenderđạt,24mesh/510refs,vậtliệu/scene/obstaclegiữnguyên;4ảnhmôhìnhUnityđãxem. SHA3104filechỉđổi8FBX+8JSONnhà,cầu21.87vàcâykhôngđổi. Nguồn`PhoViet_BalconyFlags.blend`,khônginstaller; đãpháthành1.48.0/21.93;Edit/scenesạch/start1,92/jobsfalse. Chi tiếthuongdan21.88.

- **21.87 trang trí cầu hiện tại đã sửa:** bỏchậuhoa/cầu,thaythanhchéoqua8cờbằng2tayđỡngangxanh/1cờcóđaikẹp vàbulôngLOD0;3LOD,140meshref/vậtliệugiữnguyên.45Blender+27Unityasset+101sceneđạt,6ảnhday/night/2mặt/LODđãxem. SHA3104filechỉđổiFBX+JSONcầu,scene/đèn/nhà/cây/config/docswebkhôngđổi. Lưu`PhoViet_BridgeFixtures.blend`riêng,khônginstaller; đãpháthành1.48.0/21.93;web1.48.0. Chi tiếthuongdan21.87.

- **21.86 bộ nhà Blender đã dựng xong, chờ duyệt:**19mẫu(13tiệm+2góc+4căn hộ),6ảnh thật vàboard/previewHTML đã xem; 42Blender+34filechecks/3viewportviewerđạt,3104filegameSHAkhôngđổi. Cây/pets/xeđua dùng nguồn hiện tại, chưaLOD/collider/Unityimport/build/push/pháthành. Fileblend/render/script ở`ArtSource/Concepts/City-20261029/`; chỉsau người dùngduyệtmớitíchhợpnhà. Chi tiếthuongdan21.86.

- **21.85 dựng mẫu cầu đã xong, chờ chọn:** 3sceneBlenderA/B/C,6render1600×1000vàboard1800×1250 đãxem; kếtcấu/lan can/đènchi tiết. Chỉthamkhảo,tạingoàiAssets/khônggameimport/build/push/pháthành;2993fileAssets/config/docsđốichiếuSHAkhôngđổi. File/ảnhởArtSource/Concepts/Bridge-20261028,selectionnull/false; người dùng phải xácnhậntrướckhithaycầu. Chi tiếthuongdan21.85.

- **1.47.0/21.84 đã phát hành:** commit`ec06629`, Pages`40689965684`success, cache`d26fbf3bf46d9ea7`; **28liveđạt/0runtime**. Trangchủ21.83/2ảnhsươngđãcótrênweb; ảnhlandingvàgamewebthậtđãxem.315landing+37web/offline/touch+20fullscreen,101finalscene+43repo+34site+35Gitbytesđạt;196filenguồn/scene/templategiữnguyên. Build296,451s/0lỗi/1warningPipeline; UnityEdit/scenesạch/start1,92/jobsfalse,server8771đãdừng. Chi tiếthuongdan21.84.

- **21.83 trang chủ mới đã có trên web1.47.0:** nền trắng–xanh, tiêu đềgọn/xeđua/Qhúc,2ảnhgame mới sángsương; footer/nhãnảnh trongflow, headerwrap vàdialogsafearea. **315layout+37web/offline/touch+20fullscreenđạt/0runtime**, ảnhdesktop/phone2hướngđãxem. Chỉfrontend/editorhelper,5runtimefilesbytekhớp1.46.0. Đãfullbuild/push/Pages1.47.0/21.84,28liveđạt. Xemhuongdan21.83.

- **1.46.0/21.82 đã phát hành:** commit`6ae2f3f`, Pages`40662116798`success, cache`7b8dfa6dc4f391bd`; **27liveđạt/0runtime**, ảnhwebthậtđãxem. Bao gồm21.80Dino→Tanksaurẽ và21.81phảnhồiđầulàn.101finalscene+42repo+33site+26graphics+37web/offline/touch+20fullscreen+34indexđạt,194filesource/scene/templategiữnguyên. Mobile461,795s/0lỗi/3warning, desktop108,640s/0lỗi/1warning; lỗighimetaTankPalette tạmthờiđãtiếptụcResumethànhcông. UnityEdit/scenesạch/start1,92/jobsfalse; server8771đãdừng. Chi tiếthuongdan21.82.

- **21.81 đã có trên web1.46.0:** input vốn nhận ngay; thêm gain đầu quỹ đạo car, triệt tiêu cuối để giữ nhịp hoàn tất, yawattack0,08→0,05s/return0,13s giữ.60FPS: frame đầu0,057→1,409mm,1cm100→50ms, quãng tới làn22,4237m khớp cũ. **779kiểm tra đạt**,4ảnh đã xem. UnityEdit/scenesạch/start1,92/jobsfalse. Đã build/phát hành cùng21.80 trong1.46.0/21.82. Chi tiết huongdan21.81.

- **21.80 đã có trên web1.46.0:** mỗi lần rẽ hoàn tất thêmDino ở giao lộ kế tiếp, Tank sau2giây; giữ lịch định kỳ10s/9s. Wave độc lập cho từng cua, pause/reset/pool/vị trí bị chắn xử lý đủ; Tank tính cả vị trí sau nhập làn cầu. **219kiểmtra mới+170hồi quy=389đạt**,2ảnhcameraGame đãxem. UnityEdit/scenesạch/start1,92/jobsfalse. Đã phát hành1.46.0/21.82. Chi tiết huongdan21.80.

- **1.45.0/21.79 đã phát hành:** commit `9dc3aa5`, Pages `40571941077` success, cache `c0f81245589b65f2`; **27 livechecks đạt/0 runtime**. Cài đặt Cao/Trung bình/Thấp21.78 đã có trên web. 101finalscene+42repo/33site+26settingsbrowser+37web/offline/touch+20fullscreen+34Gitbytes đạt; ảnh dọc/ngang/desktop/live đã xem. Sửa output hash cũ và ổn định importGPU trước build; giữ texture/DPR/heap128/512MiB, scene/start1,92/jobsfalse. Unity Edit, server8771 đã dừng. Chi tiết huongdan21.79.

- **21.78 cài đặt đồ họa đã lên web1.45.0:** icon đườngWelcome→bánh răng, modal3mứcCao/Trung bình/Thấp, áp dụng/lưu ngay; Cao khôi phục snapshot từng nền tảng. Medium/Low giảmAA/bóng/LODxa/gương, giữDPR/texture/camera/cảnh vật.85graphics+39gesture+31distance=155đạt,6ảnhdọc/ngang/desktop vàchuột/phím/reloadPlay đãxem. UnityEdit/scenesạch/start(1,55;0;1,92)/jobsfalse/High. **Đãpháthành1.45.0/21.79**,27liveđạt/0runtime. Chi tiết huongdan21.78.

- **1.44.0/21.77 đã triển khaiPages:** commitb0a31f8/run40535422845success/cachedb471371e548dd93. TextureGPU44maps ETC2/EACchođiệnthoại, DXTchodesktop, mỗi máy chỉ1data; gói mobile129,8→92,7MB, heap550,69→397,875MiB. Bỏpreloadbuildởlanding/tảitrùng, giữDPR3/2K/AA4/bóng/cảnh. 101finalpreflight+18memory+15cachefixture+42repo/33site+37web+20fullscreen+34indexchecksđạt/0runtime; 27livechecksđạt/0runtime. UnityEdit/scenesạch/gốcxe1,92/jobsfalse, serverskiểmtrađãdừng. Chi tiết huongdan21.77.

- **21.76 đặt điểm xuất phát xe bằng kéo thả:** Edit Mode kéo xe/model → gốc xe và collider cùng đi → Ctrl+S; Play Ready cũng kéo được và thoát Play giữ vị trí về Edit để Save. Menu **Phố Việt → Điểm xuất phát → Chọn xe để kéo vị trí**. Đã giữ đúng vị trí người dùng (1,55;0;4,39), không tự trả về0; camera/đường ngang/cầu/nhảy đồng bộ. **686 điều kiện đạt + kéo gizmo thật + save/reload/Play/reset**; scene sạch/jobsfalse. Đã có trênweb1.44.0/21.77; người dùng sauđó tựđặtZ=1,92 vàbảnpháthành giữgiátrịmới này. Chi tiết huongdan21.76.

- **1.43.0 đã phát hành (21.75):** commit7b19e84/Pages40487519674 success/cache1b0d45bcf76b698e; gồmâmđộngcơ+70%,Petshop/thúcưng/phụđề/phốkhácmàu. Đã sửaOOM512MB:initial128/max1024MB, đo550,69MiB. Build321,196s/0lỗi/1warning;20preflight+34repo+29site+37web+20fullscreen+31staged+26live đạt/0runtime. Server8765 đãdừng; UnityEditMode/khôngcompile/jobsfalse; thayđổiEditor chưa lưuxem3. Chi tiết huongdan21.75.

- **21.74 đã có trên web1.43.0:** 12mẫu tiệm dùng12màu riêng, xanh dương cốđịnh choPetshop; **2.590cặp biển kề phố/river đều khácmàu**, biển dọc/2mặt/3LOD đồngbộ. XếpPetshop không kề nhau, vẫn147/490 và693/2310=30%; phụđề/thú giữ nguyên. 10.983renderer/770set/7.670box, khônghạđồhọa. **16.128điều kiện đạt**,10ảnh gồmTIỆMBÁNH–Petshop đãxem; installer byte-identical, EditMode/scenesạch/jobsfalse. Source cuối cùng dựa trên c5d873c; chi tiết huongdan21.74. Đã phát hành1.43.0/21.75.

- **21.73 đã có trên web1.43.0:** Petshop thêm **“Lưu Trú - Spa Pet”**, bỏ bàn ghế/quầy/tủ trưng bày, mỗi cửa hàng **9chó/mèo/thỏ** trong+ngoài. Dựng thật bằng **Blender MCP**,3LOD956/838/864tri tốiđa mỗi loài; ghép vào facade, giữ10.983renderer/30%/đồhọa. Bỏ1.231ghostbox trong147Petshop, scene770set/7.659box. **3.274điều kiện đạt**,9ảnh Unityđãxem; installer lặp byte-identical, EditMode/scenesạch/0missing/jobsfalse. Đã phát hành1.43.0/21.75. Chi tiết huongdan21.73.

- **21.72 đã có trên web1.43.0:** Cù Lao Petshop nềnxanh/chữtrắng **30%**:147/490shop phố,693/2310gian river; river cóbiển **cả mặtđường/mặtsông, đủ3LOD** (1386/4620biển).12variantgiữfacade+riverbase/1partbổsung; cùngmaterial, không thêmrenderer (10983), collider giữ ởmốc21.72, đã thay đồ cũ ở21.73; LOD/độnétgiữ nguyên. **682asset+1326performance+160river=2168điềukiện đạt**,9ảnh đãxem, installer repeatbyte-identical. UnityEditMode/scenesạch/jobsfalse; Blenderbackground riêng đãthoát. Đã phát hành1.43.0/21.75. Chi tiết huongdan21.72.

- **21.71 Simulator đã dùng được:** hai cấu hình iPhone 14 Pro1179×2556 / Pro Max1290×2796, overlay Dynamic Island, Safe Area đủ4hướng tại `Assets/PhoViet/Editor/DeviceSimulator/`. Unity đã import/menu có cả hai; xem cả hai dọc/ngang+khungSafeArea đạt; kiểm JSON/kích thước/alpha2mẫu/8hướng đạt (`Captures/iphone14-simulator-checks.json`). Giữ package1.0.1, không đổi gameplay/scene/web; chưa commit/push. Chi tiết huongdan21.71.

- **21.70 đã có trên web1.43.0:** tiếng động cơ player cả cruise/boost tăng70%, AudioSource0,08→**0,136** (chungnguồn, không nhân boost lần2). ChỉPhoVietGame.CreateSound; âm khác/pitch/gameplay giữ nguyên. Compile0lỗi/0warning; PlayMode đọcnormal/boost đều0,136 đạt; đãthoátPlay. Đã build/phát hành1.43.0/21.75. Chi tiết huongdan21.70.

- **1.42.0 đã phát hành (21.69):** cache bố cục sông, broadphase va chạm theo tile, gom ánh sáng sau physics;700group shop/tree dùng LOD xa sớm hơn, giữ DPR/texture/AA4/bóng/phảnchiếu. **3.810Unity+16preflight+34repo+29site+31staged+71browser+DPR3+26live đạt**,0runtime. CPUriver0,856→0,037ms, controlledrender61,13→52,80ms (không phải FPS gameplay); ảnh gần45% dướiMAE0. Commitbe0c6ff/Pages40445290840/cached7e9bf2a636e37f7. UnityEditMode/scenesạch/jobsfalse; server đã dừng. Chi tiết huongdan21.69.

- **1.41.0 đã phát hành (21.68):** commit648b163/Pages40420198461 thành công/cachec9db437e230d3824; build0lỗi/3warning đã biết;14preflight+34repo+29site+37web/offline/touch+20fullscreen+31Gitbytechecks+26live đạt,0lỗi runtime. Urban Rider31actor+56baked/cổ xe/tay lái đã có trên web. Unity Edit Mode/scene sạch/jobs khóa, server kiểmtra đã dừng.

- **21.67 đã có trên web1.41.0:** Urban Rider thay31xe máy có người ngoàiCSGT+56baked nhánh; cổ/ghiđông/bánhtrước/tay/IK/thân đồngbộ, đổi lànquintic theo tốcđộNPC, nghiêng/về cân bằng giảmchấn. 7suite/**1.493điều kiện đạt**, ảnh4màu/grip/cua/đêm đãxem; map2K cópadding, đèn không sánglan yếm. Jobs khóa; giữSupercar/CSGT. File/parameter/checks chi tiết huongdan21.67.

- **1.40.0 đã phát hành (21.66):** commit2c118c3/Pages40396554304/cache10d5883fe6b0325f;0lỗi build/3warning. 34repo bundle+29site+7parts,37web/offline/touch+20fullscreen+5offline cuối+27staged bytes+26live đều đạt,0lỗi runtime. Data chia4phần32MiB/ghép SHA trênPages; source/cảnh/gameplay giữ đúng21.62–21.65. Unity Edit Mode/scene sạch, jobs khóa.

- **21.65, đã có trong1.40.0:** Supercar nhỏ15%, đồng bộ collider/axle/đèn/jet; đổi làn nhanh tỷ lệ Speed, W giữa cú cũng tăng nhịp. **5suite/634 điều kiện đạt**, đã lưu/reload scene giữ đúng scale0,85; ảnh dọc đã xem. Unity Edit Mode/scene sạch; chi tiết huongdan21.65.

- **21.64, đã có trong1.40.0:** Blue Petshop Supercar player xanh, 3LOD28k/12,3k/4,5k; yaw collider đúng hướng (nhịp lane đã thay ở21.65); nhảy4jet trắng, W2ống bô. Q=húc như nhánh ô tô đã chốt. **8suite/1.602 điều kiện đạt**, ảnh dọc/ngang/FX/đêm đã xem; Unity Edit Mode/scene sạch; NPC/CSGT/jobs giữ nguyên. Chi tiết huongdan21.64.

- **21.63, đã có trong1.40.0:** mọi xe va chạm văng sông hủy recovery/course, rơi/chìm cả khi đang bay hoặc đã GameOver; tra mọi nhánh/đổi khung sau cua. CSGT chìm thì hết truy đuổi; người chơi đã té thì hết lượt. **8 suite/1.166 điều kiện đạt**, ảnh xác nhận chìm. Unity Edit Mode/scene sạch, jobs khóa; chi tiết huongdan21.63.

- **21.62, đã có trong1.40.0:** Emerald Tyrant rộng4,65m/cao6,081m, rig/Walk/TailSweep bằng BlenderMCP, lịch10s/ngược chiều3m/s; đuôi thật quét4làn, nhảy né, hất cả xe tăng/CSGT ở bản cũ; source21.96 đã miễn xe tăng theo yêu cầu mới, chưa phát hành. Sửa CSGT tìm đường tới người chơi bị hất sang đường ngang. **69 kiểm tra +96 cuộc gặp +681 hồi quy =846 đạt**; chi tiết huongdan21.62.

- **1.39.0 đã phát hành (21.61):** commit17aa0b2, Pages40268861830 thành công; build0lỗi/3warning,29bundle +37web/offline/touch +20fullscreen +26live đạt,0lỗi runtime. Unity Edit Mode/scene sạch, jobs khóa.

1. **21.60, đã có trên web1.39.0:** đổi làn0,72 s và phục hồi nghiêng chậm; nhảy thấp25% ở tốc độ đầu, tăng đều theo tốc độ lúc cất cánh/W; W giữa không trung giữ cú đang bay. **6 suite/541 điều kiện Unity đạt**, xem trace/ảnh; chi tiết huongdan21.60.
2. **21.58, đã có trên web1.39.0:** khôi phục đúng hero áo đỏ sao vàng/ngón nắm v4 và xe máy Meshy, sơn xanh dương–đen; Q trở lại đạp, nhảy/boost/đèn/khói cũ. NPC28/CSGT giữ nguyên. **4 suite / 229 điều kiện Unity đạt**, xem ảnh trước/sau; người/sao và phần đen trong atlas không đổi. Thay lựa chọn player Petshop21.57.
3. **Đã có trên web1.39.0:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
4. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
5. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
6. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết

- **21.96 đã xong source/kiểm tra, chưa build/phát hành.** Xe tăng miễn nhiễm thân/đuôi khủng long, vẫn chạy/bắn;199điều kiện Unity đạt/compile0lỗi-warning. UnityEdit/scenesạch/start1,92/jobsfalse,2779LOD0missing; không còn bước sửa đangdở. Webvẫn1.49.0/cònluậtcũ; giữthayđổicụcbộkháckhôngliênquan ngoài commit.

- **21.94–21.95 đã phát hành1.49.0**, khôngcònbướcbuild/push/liveđangdở. Server8771đãdừng;UnityEdit/scenesạch/start1,92/jobsfalse. GiữthayđổicụcbộArt/Wave/Packages/Simulator/.blendkháckhôngliênquan ngoài commit;chưa kiểmđiệnthoạithật.

- **21.87–21.93 đã phát hành1.48.0**, không còn bước build/push/live đang dở. Server kiểmtra8771 đã dừng; Unity giữEdit/scenesạch/start1,92/jobsfalse. Thayđổi Art/Wave/Packages/Simulator/.blend/Captures lịch sử vẫn giữ cục bộ ngoài commit. NguồnBlender21.91 và21.92 ởphiên riêng, khôngghiđè`PhoViet_FixtureDetails.blend` đangcó sửa người dùng; không còntácvụ dựng/import đang chạy.

- **21.86 chờ người dùng duyệt bộ nhà:** đãxem6rendercuối/board, file`PhoViet_NewCity_Approval.blend`, tranggallery`preview.html`, bảng`city-approval-board.png`;19mẫu/3scene, approvalfalse/installedfalse. **Chưađượcduyệt→khôngtíchhợpUnity/web.** Nguồn/render chỉthamkhảođãlưuGit,khôngđónggóivàogame. Câygiữnguồnhiện tại; sau duyệt phải làmLOD/gộpmesh/atlas/collider/kiểmhiệunăng. BlenderđangStreet_Preview/cameraStreet_Wide,renderđãxong306,06s;Scene gốc0/filepathtrống. MCPclientTransportclosednhưngBlender/addoncònmở; helper`call_blender_addon.py`gọiAPIaddon9876đãkiểm, khôngcàiđè/restart. 42Blender+34file/3viewportđạt,3104filegamegiữnguyên;tạithờiđiểmdựngweb1.47.0. Cầu21.85vẫnchờchọnriêng.

- **21.85 chờ người dùng chọn và xác nhận cầu A/B/C:** đã dựng thật BlenderMCP3mẫu/6ảnh, board`ArtSource/Concepts/Bridge-20261028/bridge-comparison.png`vàfile`PhoViet_Bridge_Concepts.blend`. A bê tông3vòm/đồng;Bvòmthépxanh;Cdâyvăng.50,4m/road6,2m/deck7,4m, chưaLOD/collider/merge. **Không tự tíchhợp hoặc đổiUnity trước xácnhận**;selection.jsonvẫnnull/false.2993filegamebytegiữnguyên;tạithờiđiểmdựngweb1.47.0. BlenderđangsceneA/rootA,cácfile.blendcũkhôngđổi. Chi tiếthuongdan21.85.

- **21.83–21.84 đã lênweb1.47.0**, khôngcònchờbuild/pháthành. Giữthayđổicụcbộkháckhôngliênquan; chưađoSafari/Androidmáythật.



- **21.80–21.82 đã lênweb1.46.0**, khôngcònchờbuild/pháthành. Bảnmớiđượckiểm27live/0runtime; chưađođộtrễ/FPS trênmáythật. Giữcácthayđổicụcbộkháckhôngliênquan.











> Mẫu: `Yêu cầu: … · Đã xong: … · Bước kế tiếp: … · File đang sửa: … · Unity/Blender: … · Cập nhật: <ngày giờ>, <agent>`

- **21.78/21.79 đã phát hành1.45.0:** không còn phần cài đặt đồ họa chờ build. Hai lỗi pipeline outputcũ/importer đã sửa; dùng builder mới và GPUformat ổn định. Server8771 đã dừng; Unity Edit/scene sạch/jobs khóa.
- **21.70/21.72–21.74 đã phát hành1.43.0:** không còn tính năng này chờ build. Khi sửaPetshop dùng generator/installer hiện hành, không chạy công cụ dựng phố/cầu cũ. Cấu hình1024MB là lịch sử1.43.0; hiện dùng128/512MiB+GPUcompression/growth10%/cap32MiB theo21.77. Khôngpháthành lại rawartifactlỗiOOM ở21.75.
- **21.69 đã phát hành1.42.0:** không còn phần tối ưu chờ build; không tự hạ DPR/texture/bóng để tăng FPS. Phép đo là Editor/camera readback và browser giả lập; chưa đo Safari/điện thoại thật. Các probe shadow/instancing chỉ thử tạm, không áp dụng.
- **21.67 đã phát hành1.41.0:** asset/meta+scene/code/2tài liệu nằm trong commit648b163. Khi bảo trì dùng đúng bộ Urban Rider hiện hành. RawMeshyFBX120,7MB giữ cục bộ, không stage/push mù quáng; dependencies không còn raw FBX/FBX dân sự cũ, không thiếu script; game dùng FBX4MB. Không chạy lạiMeshyRiders/Wave installer cũ.
- Không còn chức năng21.62–21.65 chờ phát hành: **đã có trên web1.40.0**. Dữ liệu119,6MB lưu trongGit dạng4parts; Pages ghép/kiểmSHA trước upload. Trên checkout mới, materialize ra_site trước khi chạy server local (15.2–15.4/21.66).
- Bán tải/installer Petshop21.57 đã ngưng dùng từ21.58; không tự cài lại. **Nhánh húc được tái dùng cho Blue Supercar21.64–21.65**. Test/giới hạn cua cực muộn của pickup cũ không đại diện player hiện tại. Chưa thử Safari trên điện thoại thật.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- **Source21.96:** xe tăng không bị thân/chân/đuôi khủng long hất, vẫn chạy/bắn theo quy tắc pháo hiện có; các phương tiện khác/player/CSGT giữ luật cũ. Chưa lênweb1.49.0, thay quyết định hất xe tăng ở21.62.

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi. **CSGT rơi sông → hết truy đuổi; người chơi đã té thì hết lượt**, còn đang chạy thì tiếp tục (21.63).
- Húc/đạp quái xế tự vệ không gọi mới CSGT; húc/đạp xe thường thì bị truy đuổi.
- Phím: **Q húc, Space nhảy, W tăng tốc +125%/4s/hồi2s**; mobile: chạm hai lần húc, vuốt lên nhảy, vuốt xuống tăng tốc; nhận ngay khi đủ ngưỡng.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- **Source21.65: player Blue Petshop Supercar** xanh scale0,85, Q=húc thân xe; đã có trên web1.40.0; hero xe máy xanh–đen21.58 là lịch sử. NPC xe máy source21.67 dùngUrbanRider nhiều màu xe/áo, CSGT giữ cả người+xe; NPC4mẫu ô tô×8màu, xe tăng giữ nguyên.
- Phong cách bán hiện thực vui nhộn, tối ưu điện thoại/WebGL.
- Không bật lại công việc khi nâng cấp/sửa lỗi/build. Giữ dữ liệu Coin và mã jobs.
- Không ghi đè/reset file Blender cục bộ, không dựng lại cả scene bằng builder lịch sử.

## 5. Tra cứu nhanh: sửa gì → đọc gì

| Muốn đụng tới | Mục `huongdan.md` | File chính |
| --- | --- | --- |
| Welcome/bánh răng/cài đặt đồ họa3mức/lưu lựa chọn | **21.78**,12 | `PhoVietGraphicsSettings.cs`, `PhoVietWelcome.cs`, `PhoVietGame.cs`, `PhoVietWebRuntime.cs`, `AgentScripts/VerifyGraphicsSettings.cs`, `CaptureGraphicsSettings.cs` |
| Điểm xuất phát xe, kéo thả Edit/Play Ready | **21.76**,6.1 | `PhoVietPlayerStart.cs`, `Editor/PhoVietPlayerStartEditor.cs`, `PhoVietGame.cs`, `PhoVietRoutes.cs`, `AgentScripts/VerifyPlayerStart*.cs` |
| Menu Pause/kết thúc/về đầu phố | **21.90**, 2.1 | `PhoVietGame.cs` → `EndCard`, `TogglePause`, `ResetRide` |
| Điều khiển, đổi làn, camera, HUD | 2.2, 12, **21.94**,21.92/21.89/21.81/21.55/21.60/21.65 | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoViet*Hud.cs`, `AgentScripts/VerifyLaneResponse.cs`, `VerifySupercarScalePace.cs`, `CaptureLaneResponse.cs` |
| Giao thông, va chạm, văng/té | 7, 21.33 | `PhoVietTraffic.cs`, `PhoVietCollisions.cs`, `PhoVietRiderCrash.cs` |
| Quái xế, xe tải ngủ gật | 2.4, 7.2 | `PhoVietReckless.cs` |
| Đợt Dino→Tank sau cua, vị trí giao lộ, chờ2s/pool | **21.80**,21.62/21.38 | `PhoVietTurnHazards.cs`, `PhoVietRoutes.cs`, `PhoVietDinosaurs.cs`, `PhoVietTankFire.cs`, `AgentScripts/VerifyTurnHazards.cs`, `CaptureTurnHazards.cs` |
| Khủng long, rig/quất đuôi, nhảy né/miễn xe tăng | **21.96**,2.12,7.5,21.62 | `PhoVietDinosaurs.cs`, `DinosaurVisual.cs`, `Editor/PhoVietDinosaurUpgrade.cs`, `pho_viet_dinosaur.py` |
| Xe tăng, đạn pháo | 21.38–21.40 | `PhoVietTankFire.cs`, `Editor/PhoVietTankFxUpgrade.cs` |
| Đạp, CSGT, bị bắt | 8 | `PhoVietActions.cs`, `PhoVietKickAssist.cs`, `PhoVietPolice.cs`, `PhoVietCrashPoliceNavigation.cs` |
| Nhảy, nóc xe, tăng tốc | 19, 21.60 | `PhoVietJump.cs`, `PhoVietBoost.cs` |
| Giao lộ, rẽ, rebase tọa độ | 6.1, 9 | `PhoVietRoutes.cs`, `PhoVietRoadDrivers.cs`, `PhoVietExitTraffic.cs` |
| Chọn mẫu cầu mới (chưa tích hợp) | **21.85** | `ArtSource/Concepts/Bridge-20261028/README.md`, `bridge-comparison.png/html`, `PhoViet_Bridge_Concepts.blend`, `selection.json`; chờ người dùng xác nhận |
| Trang trí cầu hiện tại/chậuhoa/giá cờ | **21.87** | `ArtSource/Blender/pho_viet_river_bridge.py`, `patch_bridge_fixtures.py`, `PhoViet_BridgeFixtures.blend`, `Art/River/PV_RiverBridge.fbx/json`, `AgentScripts/VerifyBridgeFixtures.cs`, `CaptureBridgeFixtures.cs`; không chạy installer sông/cầu cũ |
| Đèn giao thông, điều hòa xám trắng, bảng thuê nhà ven sông | **21.94**,21.92/21.91 | `pho_viet_fixture_details.py`, `upgrade_fixture_details.py`, `pho_viet_saigon_streets.py`, `pho_viet_river_bridge.py`, `AgentScripts/FixtureDetailsImport.cs`, `CaptureFixtureDetails.cs`;21.92 thêm`upgrade_signal_rental.py`, `PhoVietSignalFaces.cs`, `SignalRentalUpdate.cs`, `VerifySignalFaces.cs`, `CaptureSignalRental.cs`; chỉ export/import signal+river, không installer |
| Thanh đen cạnh cờ ban công nhà phố | **21.88** | `pho_viet_saigon_streets.py`→`flag()`, `remove_balcony_flag_poles.py`, `PhoViet_BalconyFlags.blend`, `AgentScripts/BalconyFlagCleanup.cs`, `Captures/balcony-flags-*`; chỉ8mẫu0/3/6/9 thường+Petshop |
| Duyệt bộ nhà phố mới (chưa tích hợp) | **21.86** | `ArtSource/Concepts/City-20261029/README.md`, `preview.html`, `city-approval-board.png`, `PhoViet_NewCity_Approval.blend`, `approval.json`; chỉ sau duyệt mới thay nhà, giữ cây |
| Sông, cầu, căn hộ ven sông, xe/CSGT rơi chìm | 21.35–21.37, 21.45–21.54, 21.63 | `PhoVietRiver.cs`, `RiverBlock.cs`, `RiverReflection.cs`, `River*.shader`, `Editor/PhoVietRiverUpgrade.cs`, `AgentScripts/VerifyRiverCrashSinking.cs` |
| Cảnh phố, nhà, cây | 11.4, 21.19–21.28, 21.43 | `VietnamStreet.cs`, `StreetArt.cs`, `Editor/PhoVietStreetUpgrade.cs` |
| Ngày–đêm, đèn, sương | 20, 21.41 | `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs`, `MorningLooks.cs` |
| Xe đua người chơi/khói4góc+2bô/chuyển làn | 21.64–21.65, 19, 21.60 | `PlayerCarVisual.cs`, `PhoVietSupercarUpgrade.cs`, `CarJumpJetVisual.cs`, `BoostSmokeVisual.cs`, `integrate_supercar_player.py`, `VerifySupercarScalePace.cs` |
| Khôi phục player xe máy xanh–đen (lịch sử) | 21.58, 21.9–21.16 | `RestoreBlueBlackPlayer.cs`, `prepare_hero_blue_black.py`, `BikeVisualAnimator.cs` |
| Xe máy dân sự Urban Rider/cổ xe/tay nắm/đổi làn | **21.67**,7.1,11 | `UrbanRiderMotion.cs`, `BikeVisualAnimator.cs`, `UrbanRider.shader`, `PhoVietTraffic.cs`, `PhoVietRoutes.cs`, `Editor/PhoVietUrbanRiderUpgrade.cs`, `integrate_urban_rider.py`, `prepare_urban_rider_maps.py`, `AgentScripts/InstallUrbanRider.cs`, `VerifyUrbanRider.cs` |
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Màu bảng không trùng, Petshop30%, phụđề/thú cưng/nội thất | **21.72–21.74**,11.4 | `pho_viet_petshop_signs.py`, `pho_viet_river_bridge.py`, `Editor/PhoVietPetshopSignsUpgrade.cs`, `AgentScripts/InstallPetshopSigns.cs`, `VerifyPetshopSigns.cs`, `VerifyPetshopDetails.cs`, `VerifySignColours.cs`, `pho_viet_petshop_layout.py`, `pho_viet_pet_animals.py`, `Art/Pets/PV_PetGeometry.json` |
| Âm động cơ player/cruise/boost | **21.70** | `PhoVietGame.cs` (`CreateSound`, `engine`, cuối `Update`) |
| Simulator iPhone 14 Pro/Pro Max/Dynamic Island/Safe Area | **21.71** | `Assets/PhoViet/Editor/DeviceSimulator/` (`README.md`, hai `.device`, `Overlays/`, `LICENSE.md`); `Captures/iphone14-simulator-checks.json` |
| Hiệu năng/lag/cache sông/va chạm/đèn/LODxa | **21.69** | `PhoVietObstacleBatches.cs`, `PhoVietRiver.cs`, `DayNightLighting.cs`, `Editor/PhoVietPerformanceUpgrade.cs`, `AgentScripts/ProfileRuntimeCost.cs`, `ProfileStreetDetail.cs`, `VerifyPerformanceSafety.cs`, `VerifyPerformanceLighting.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Trang chủ HTML / ảnh sương / responsive | **21.83**,15.2 | `Assets/WebGLTemplates/PhoVietPWA/index.html`, `style.css`, `app.js`, `poster-morning*.jpg`; `AgentScripts/CaptureLandingMorning.cs`, `RefreshLandingPreview.cs`, `VerifyLanding.cjs` |
| WebGL/Safari/OOM/textureGPU/cache/offline/Pages | 15.2–15.4, **21.77**,21.66 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/BuildWebRelease.cs`, `VerifyRelease145Scene.cs`, `ConfigureWebRelease.cs`, `PublishWebArtifact.py`, `PhoVietWebMemorySettings.cs`, `VerifyWebMemory.cjs`, `VerifyWebCache.cjs`, `VerifyStagedWeb.py`, `.github/workflows/pages.yml` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.

## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình. Việc dài: ghi “Tiến độ việc đang làm” ở mục 3 sau mỗi mốc, để phiên khác (tài khoản Claude khác hoặc Codex) làm tiếp nếu phiên này bị dừng giữa chừng.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
