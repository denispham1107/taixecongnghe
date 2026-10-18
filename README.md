# PHỐ VIỆT — Chuyến xe không ngừng

Game né xe vô tận bằng Unity 6, lấy cảm hứng từ nhịp chơi endless runner và đường phố Việt Nam. Mô hình 3D được tạo riêng bằng hình học, không dùng tài sản của Subway Surfers.

## Chơi bản web và cài ứng dụng

**[Mở Phố Việt](https://denispham1107.github.io/taixecongnghe/)** · Bản web **1.11.0**.

- Desktop: mở link bằng Chrome/Edge/Firefox/Safari. Chrome và Edge có thể cài thành ứng dụng qua nút cài đặt của trình duyệt.
- Android: mở bằng Chrome, chọn **Cài ứng dụng** trên trang hoặc trong menu trình duyệt. Đây là ứng dụng web PWA cài trực tiếp, không cần tải APK.
- iPhone/iPad: mở link bằng Safari → **Chia sẻ → Thêm vào Màn hình chính**; bật **Mở dưới dạng ứng dụng web** nếu thiết bị hiển thị tùy chọn đó, rồi chọn **Thêm**.
- Cần mạng ở lần đầu. Khi trang báo **Đã lưu để chơi offline trên máy này**, bản game đã được lưu đầy đủ. Trình duyệt có thể xóa dữ liệu offline khi thiết bị thiếu dung lượng hoặc người dùng xóa dữ liệu trang.
- Vuốt/chạm và bàn phím dùng chung luật chơi. Chuyển ứng dụng hoặc mở hướng dẫn khi đang chạy sẽ tạm dừng; nhấn tiếp tục trong game để đi tiếp.
- Khi game mở trên điện thoại/tablet, cảnh chơi phủ toàn bộ khung nhìn dọc và ngang. Vùng tai thỏ và thanh vuốt chỉ đẩy HUD/nút bấm vào trong, không tạo lề quanh cảnh. Nút **☰** mở tùy chọn/cách chơi; thanh tiêu đề không còn chiếm chiều cao khi chơi.
- Android có thể vào fullscreen khi chạm mở game. Trên iPhone/iPad, mở từ biểu tượng Màn hình chính để dùng khung ứng dụng web; Safari vẫn tự quản lý các thanh trình duyệt nếu mở bằng tab thông thường.

## Mã nguồn và triển khai

- Unity **6000.5.6f1**, cần module **Web Build Support**. Mở thư mục dự án bằng Unity Hub, sau đó mở `Assets/PhoViet/Scenes/PhoViet.unity`.
- Toàn bộ scene, mesh/material đã lưu, font kèm giấy phép, mã gameplay, công cụ dựng scene và kiểm tra đều nằm trong repo. `Library`, `Temp`, `Logs`, cấu hình cá nhân và bản Windows được bỏ qua vì có thể tạo lại.
- Chọn **Phố Việt/Cấu hình bản Web PWA** rồi build WebGL tới `Builds/WebGL`, hoặc chạy Unity batchmode với `-executeMethod PhoViet.Editor.PhoVietWebBuild.Build`.
- Template PWA nằm ở `Assets/WebGLTemplates/PhoVietPWA`. Hậu xử lý tự tạo `precache.json`, `build-info.json` và phiên bản service worker từ nội dung build. File nén có bộ giải nén dự phòng để chạy trên GitHub Pages.
- Sau khi build và kiểm tra, sao chép **toàn bộ** `Builds/WebGL` sang `docs`, commit rồi push `main`. Workflow `.github/workflows/pages.yml` triển khai đúng bản đã build lên Pages. Không cần Unity license trong GitHub Actions.
- Giữ `docs/Build` và shell web trong cùng một commit. Service worker chỉ chuyển sang bản mới sau khi đã tải đủ tài nguyên và các tab bản cũ đóng hết.

## Mở và chơi

- Bản Windows: mở `Builds/Windows/PhoViet.exe`. Giữ nguyên các thư mục và DLL đi kèm. **Alt + F4** để thoát.
- Mở `Assets/PhoViet/Scenes/PhoViet.unity` rồi nhấn **Play**.
- Nhấn **LÊN XE THÔI**, **Space** hoặc **Enter** để bắt đầu.
- **← / →** hoặc **A / D**: đổi giữa 4 làn đường. Xe tự tiến.
- Khi đang chạy: **Space** đạp xe máy bên trái/phải trong tầm, cả cùng chiều và ngược chiều. Không thể đạp ô tô, xe tải hoặc xe đang băng ngang. Có hồi chiêu 0,8 giây.
- Điện thoại: vuốt trái/phải hoặc chạm hai nút mũi tên.
- **Rẽ giao lộ:** trong vùng mũi tên trắng trước giao lộ (cách tâm khoảng 12–30 m), làn ngoài cùng trái được chọn rẽ trái, làn ngoài cùng phải được chọn rẽ phải bằng một lần bấm/vuốt đúng hướng. **Trong lòng giao lộ cũng nhận rẽ ngay**, không cần chọn trước: xe ở phía sát lề trái được rẽ trái, phía sát lề phải được rẽ phải (trong chiều rộng 14 m của đường giao nhau). Hai làn giữa tiếp tục đổi làn. Ngoài vùng rẽ điều khiển vẫn đổi làn; đổi sang làn khác hủy lệnh rẽ đã chọn. Xe tự ôm cua, camera chuyển mượt và vẫn giữ vị trí làn sau cua như đã chọn trước đó, kể cả làn ngoài cùng trái.
- Điện thoại: **chạm hai lần liên tiếp** trong vùng chơi để đạp; chạm các nút điều khiển hoặc HUD không kích hoạt cú đạp.
- **P / Esc** hoặc nút **Ⅱ**: tạm dừng / đi tiếp.
- Sau va chạm: **R / Space** hoặc nút chơi lại.

Làn bên trái có xe ngược chiều. Bên phải là xe cùng chiều. Tại giao lộ có xe máy và ô tô **băng ngang từ hai bên**; va vào bất kỳ xe nào sẽ té ngã và kết thúc chuyến đi. Có cảnh báo giao lộ khi đến gần. Kỷ lục quãng đường được lưu trên máy. Đèn giao thông là cảnh vật; luật chơi là né xe.

Bản **1.2.0** tăng nhịp độ khó thêm **50% so với 1.1.0**: tốc độ tăng 0,36 m/s mỗi giây, đạt giới hạn đường thường 30 m/s sau khoảng 36,1 giây; xe theo đôi xuất hiện từ giây thứ 4. Số xe băng ngang tăng từ 1 lên 2 rồi 3 xe mỗi giao lộ; tốc độ xe ngang và tỷ lệ ô tô cũng tăng theo độ khó.

## Đạp xe, dây chuyền và truy đuổi

- Cú đạp tự chọn xe máy gần nhất trong tầm bên cạnh và hoạt ảnh đúng chân; xe ngã ra xa người chơi theo hướng đạp. Xe ngược chiều có cửa sổ tiếp cận dài hơn; nếu có quái xế trong tầm sẽ ưu tiên tự vệ.
- Xe ngã chạm xe máy khác sẽ gây ngã dây chuyền; ô tô chạm hoặc cán xe đã ngã cũng bị hất văng. Xe đã ngã vẫn là chướng ngại trên đường.
- Sau cú đạp thành công đầu tiên vào **xe máy thường**, mô tô CSGT xuất hiện rất nhanh từ phía sau, có đồng phục vàng cát, mũ CSGT, cầu vai, đèn đỏ/xanh và còi.
- Đạp **xe máy quái xế lạng lách** là tự vệ, **không gọi CSGT**. Nếu đã bị truy đuổi vì đạp xe thường trước đó, cuộc truy đuổi đang có vẫn tiếp tục.
- Khi đường thông thoáng, tốc độ người chơi tăng thêm theo thời gian và giữ ưu thế 0,65 m/s sau khi mô tô truy đuổi đã áp sát. Nếu CSGT đuổi kịp trong khoảng 2,2 m và ở sát bên, người chơi bị bắt.
- Bản 1.7.4: **đang bị CSGT truy đuổi mà đụng bất kỳ xe giao thông nào cũng thua ngay**, gồm xe máy, ô tô, xe tải, xe ngược chiều, xe băng ngang và xe đã ngã. Va chạm chuyển ngay sang té ngã/kết thúc lượt; không có giảm tốc để đi tiếp hoặc thời gian miễn va chạm khi truy đuổi.
- Sau khi bị bắt, camera chuyển sang buồng giam: nhân vật mặc đồ sọc đen trắng ngồi sau song sắt. Nhấn **R / Space** hoặc **CHƠI LẠI** để trở lại phố.
- Kỷ lục được giữ nguyên qua các lần chơi. Đạp tiếp không đặt lại khoảng cách CSGT.

## Nội dung

Bản hiện tại **1.11.0**:

- Mũi tên trên hai đường nhánh được dời ra ngoài mép vạch qua đường, có khoảng trống hơn 1,5 m; màu sơn và số lượng mũi tên giữ nguyên ở cả bốn phía.
- Nhận lệnh rẽ trực tiếp trong lòng giao lộ tại đúng phía lề, kể cả đã đi qua vạch người đi bộ và chưa chọn rẽ từ trước. Quỹ đạo nối mượt giúp lệnh rẽ muộn nhập đúng làn đã xác nhận, giữ va chạm và truy đuổi hoạt động.
- Mỗi lần vừa rẽ xong có đợt **6–10 xe ngẫu nhiên**: 3–5 xe cùng chiều từ ngoài màn hình phía sau vượt lên và 3–5 xe ngược chiều từ xa đi tới. Loại xe, làn, vận tốc, vị trí và nhịp xuất hiện thay đổi mỗi lượt. Xe không xuất hiện chồng lên xe khác hay CSGT; nếu bộ xe đang đầy thì chờ chỗ trống, không dời xe đang chạy để lấy chỗ. Khi vượt qua người chơi, xe từ phía sau trở lại hành vi giao thông bình thường.

- Mỗi giao lộ có **16 mũi tên sơn màu vạch qua đường**, chia đều cả bốn hướng tiếp cận. Có đường nhánh thật nối tiếp nhà cửa, quán phở/hủ tiếu/mì gõ, tạp hóa, sửa xe, người đi bộ, cây xanh và xe đậu.
- Rẽ trái/phải theo cung cong 90 độ, góc nhìn phía sau xoay và trở lại mượt mà; quãng đường, độ khó và truy đuổi tiếp tục. Tên đường được chọn từ 48 tên Việt Nam, tránh trùng đường đã ghé trong cùng lượt. Đi thẳng vẫn ở trên cùng một đường.
- Năm dãy đường dùng lại mô hình/mesh chung để giữ cả giao lộ cũ phía sau khi rẽ, chuẩn bị hai nhánh phía trước và tiếp tục hành trình không giới hạn. Xe đang ở đường cũ tiếp tục chuyển động và vẫn có va chạm; CSGT theo dấu đường cong, vẫn tránh xe và ngã 1,2 giây nếu va chạm. Đụng xe khi bị truy đuổi vẫn thua ngay, cả trong lúc cua.
- Chữ biển hiệu dùng vật liệu có kiểm tra độ sâu và sương mù để không hiện xuyên qua nhà ở đường bên cạnh. HUD vẫn dùng font giao diện riêng.

- Toàn bộ **11 xe bốn bánh** được thay bằng sáu mẫu gốc dựng qua Blender MCP: sedan, hatchback, taxi, xe giao hàng thùng nhỏ, xe tải thùng thường và xe tải ngủ gật. Thân xe có đường cong, hốc bánh, mâm/lốp, giảm xóc, kính và gương, gạt mưa, lưới tản nhiệt, biển số; thùng hàng có gân, cửa, bản lề và khóa. Phong cách bán hiện thực vui nhộn đồng bộ với người lái xe máy.
- Mỗi mẫu có **15 xương, 7 clip** (chờ, chạy, phanh, va chạm trước/sau/trái/phải), **3 LOD**, một atlas vật liệu; LOD xa dưới 4.000 tam giác. Bánh quay theo tốc độ, bánh trước đánh lái theo hướng rẽ; thân xe nhún khi chạy/phanh, cản xe và gương phản ứng theo hướng xung lực va chạm. Tạm dừng đóng băng hoạt ảnh.
- Hoạt ảnh chỉ tác động mô hình con, giữ nguyên bộ xe, ngẫu nhiên, kích thước va chạm, vật lý dây chuyền, hành vi xe tải ngủ gật và khói/lửa khi đâm nhà. Khi bị CSGT truy đuổi, đụng bất kỳ xe giao thông nào vẫn thua ngay.

- Toàn bộ 19 xe máy/người lái đang tham gia giao thông và 14 xe đậu vỉa hè dùng bộ model gốc dựng bằng Blender MCP theo phong cách bán hiện thực vui nhộn: xe người chơi, tay ga, xe số, xe thể thao, quái xế và mô tô CSGT. Có thân vỏ cong, vành/lốp/phanh/giảm xóc/động cơ/pô, mặt người, mũ bảo hiểm, quần áo và sao vàng đủ năm cánh.
- Mỗi mẫu có 7 clip có khung xương: đứng chờ, chạy, đạp trái, đạp phải, ngã trái, ngã phải và dựng xe. Bánh quay theo vận tốc đường thực tế, tạm dừng đóng băng hoạt ảnh; phần tiếp đất chỉ chỉnh hình ảnh.
- Mỗi mẫu có 3 mức LOD và một atlas vật liệu. Xe ở xa giảm từ khoảng 24–27 nghìn xuống dưới 6 nghìn tam giác; hình ảnh mới giữ nguyên gốc vật lý và luật va chạm/thua ngay khi đang bị truy đuổi.

- Quái xế/xe tải ngủ gật **cùng chiều bắt đầu hoàn toàn phía sau camera**, chạy từ mép dưới màn hình lên và vượt người chơi. Chúng chỉ bắt đầu lạng lách sau khi tới gần; không sinh ra giữa đường đang nhìn thấy. Xe mới không xuất hiện đè lên xe giao thông hoặc mô tô CSGT.
- Mỗi lượt chơi dùng seed mới, ngẫu nhiên lại số lượng, mẫu xe, làn, vị trí và vận tốc của nhóm xe ban đầu. Xe giao lộ chọn mẫu xe ngẫu nhiên; nhịp sinh xe và tần số/biên độ lạng cũng thay đổi giữa các lượt.
- CSGT dự đoán chuyển động của các xe khác, chọn lối né và phanh nếu bị chặn. Xe đứng yên, xe đã ngã, xe rẽ và xe ngược chiều đều tham gia kiểm tra va chạm của CSGT.
- Nếu vẫn va chạm, mô tô CSGT ngã **1,2 giây**, đứng lại trên đường và không thể bắt người chơi trong thời gian đó. Hết thời gian sẽ dựng xe, tiếp tục cuộc truy đuổi từ vị trí thực tế; không dịch chuyển tức thì về gần người chơi. Tạm dừng cũng dừng thời gian dựng xe.
- Xe nguy hiểm ngược chiều và hiệu ứng khói/lửa khi đâm nhà từ bản trước vẫn được giữ.

## Nguồn model Blender

- Xe bốn bánh: `ArtSource/Blender/PhoViet_Cars.blend`, script `ArtSource/Blender/pho_viet_cars.py` (gọi `build_all()` trong Blender, dùng chung tiện ích mesh/atlas/FBX từ script xe máy). Sáu mẫu có rig/action, bảng màu và font được đóng gói trong file.
- Tài nguyên: `Assets/PhoViet/Art/Cars`. Menu **Phố Việt → Art → Replace all four-wheel vehicles with Blender models** thay đúng các mô hình con; `CarVisualAnimator` lấy mẫu clip, quay bánh và nhận hướng xung lực từ lớp va chạm hiện có.
- File chỉnh sửa: `ArtSource/Blender/PhoViet_Motorcycles.blend`. Sáu mẫu cùng rig/action, bảng màu và font đã được lưu trong file; timeline mặc định phát vòng chạy xe.
- Script dựng gốc: `ArtSource/Blender/pho_viet_bikes.py`. Chạy trong Blender bằng Python/MCP, sau đó gọi `rebuild_all()` để tái tạo bộ mẫu, LOD, FBX và sân trình bày. Script chỉ thay các collection/model do nó sở hữu.
- FBX, texture atlas và material Unity: `Assets/PhoViet/Art/Bikes`. Menu **Phố Việt → Art → Replace all motorcycles with Blender models** cập nhật scene hiện tại mà giữ các gốc xe, pool và cảnh đường phố.
- `BikeVisualAnimator` lấy mẫu clip và điều khiển bánh xe/tiếp đất. Di chuyển, va chạm, cú đạp và các mốc bắt giữ vẫn thuộc các lớp gameplay hiện có.

Bản **1.6.0** có quái xế và **xe tải ngủ gật ở cả hai chiều**: xe cùng chiều từ làn kế bên lạng sang, xe ngược chiều từ hai làn trái lao tới và lấn qua vạch giữa đường sang làn người chơi. Cả hai hướng đều có thể húc xe khác hoặc đâm nhà bên trái/phải, phát khói và lửa. Biển tên có hai mặt để nhận diện cả khi chạy đối diện. Đạp quái xế ngược chiều để tự vệ vẫn không gọi CSGT.

Cơ chế từ bản **1.5.0** được giữ: không có lịch riêng 20 giây / 15–20 giây; xe nguy hiểm xuất hiện thường xuyên trong các đợt xe giao thông bình thường, có thể có cả hai loại đang hoạt động. Xe mới không xuất hiện chồng lên xe đang chạy, và xe đang hoạt động không bị dịch chuyển đi để nhường chỗ cho lần xuất hiện khác.

Bản **1.4.0** bổ sung:

- Xe cùng chiều, gồm xe máy, ô tô, xe giao hàng và xe tải cỡ trung, thường phanh gấp/giảm tốc mạnh. Đèn phanh sáng trong lúc phanh, sau đó xe tăng tốc trở lại.
- Xe bên cạnh có thể bất ngờ tạt sang làn người chơi; xe chạy qua làn và nghiêng/đổi hướng liên tục, không dịch chuyển tức thì. Xe tải đổi làn chậm hơn xe máy do kích thước lớn.
- Tại giao lộ, khoảng 75% xe băng ngang chọn rẽ trái hoặc phải. Xe rẽ theo cung tròn rồi nhập vào đúng làn cùng chiều hoặc ngược chiều; các xe còn lại đi thẳng.
- Tất cả xe trong pool giao thông có thể va chạm lẫn nhau: xe thường, xe ngang/rẽ, quái xế, xe tải và xe đã đổ. Phản lực dựa trên vận tốc tương đối, điểm va chạm, khối lượng và mô men quán tính; có ma sát, độ nảy thấp, trọng lực và giảm xoay.
- Khối lượng quy ước: xe máy kèm người 220 kg, ô tô 1.350 kg, xe giao hàng 2.200 kg, xe tải cỡ trung 4.500 kg. Xe tải tác động mạnh hơn xe máy; va chạm lệch tâm gây xoay, tông đuôi đẩy xe trước và giảm tốc xe sau. Va chạm tự phát không gọi CSGT.
- Đây là mô hình vật lý đơn giản hóa cho gameplay trên mặt đường, dùng kiểm tra va chạm quét theo thân xe và xung lực; không phải mô phỏng biến dạng thân xe. Thay thế kiểu hất mọi ô tô bằng lực cố định của bản cũ.
- Pool hiện có 26 xe thường (thêm hai xe tải cỡ trung) và 2 xe sự kiện riêng. Xe đổ còn là chướng ngại, dừng lại khi chạm mặt tiền nhà và giảm tốc do ma sát. Quái xế đâm nhà vẫn có khói/lửa.

Bản **1.3.0** bổ sung sự kiện giao thông nguy hiểm:

- Lịch 20 giây / 15–20 giây của bản 1.3.0 đã được bỏ. Bản 1.6.0 kết hợp xe nguy hiểm ngược chiều với xe cùng chiều từ làn kế bên, xuất hiện theo luồng giao thông.
- Quái xế chạy Exciter cách điệu, xe tải nguy hiểm có tên **XE TẢI NGỦ GẬT**; có biển tên và cảnh báo trên màn hình.
- Xe nguy hiểm húc và đẩy xe máy/ô tô khác, có thể tạo va chạm dây chuyền làm rối đường. Các va chạm tự phát này không gọi CSGT hay tính thành hành động của người chơi.
- Xe có thể lao vào nhà bên trái hoặc phải, nằm lại cạnh nhà với lửa và khói bốc lên. Giao lộ trống không bị coi là tường nhà. Va vào xe nguy hiểm hoặc xác xe vẫn gây thua.
- Hai mẫu xe sự kiện dùng pool riêng, giữ nguyên 24 xe giao thông thường. Nhịp tăng độ khó của bản 1.2.0 được giữ nguyên.

- Tài xế áo đỏ, sao vàng **5 cánh** ở trước và sau áo; xe máy, mũ bảo hiểm, gương, đèn và biển số.
- Quán Phở, Hủ Tiếu, Mì Gõ, Cơm Tấm, Bún Bò Huế, Bánh Mì, Cà Phê; tạp hóa, bách hóa và tiệm sửa xe có tên tiếng Việt.
- Các đường: Trần Hưng Đạo, Quang Trung, Hai Bà Trưng, Lê Lợi, Lý Thường Kiệt, Lý Nam Đế, Đinh Bộ Lĩnh.
- Vạch giữa đường **vàng đứt nét**, ngã tư, đèn đỏ/vàng/xanh, vạch sang đường, cây xanh, ban công, dây điện và người đi bộ trên vỉa hè.
- Âm động cơ và va chạm được tổng hợp trong game; có nút tắt âm khi chơi.

## Cấu trúc

- `StreetArt.cs`: mô hình xe, người, sao 5 cánh và chi tiết phố.
- `VietnamStreet.cs`: phố, cửa hàng, ngã tư, biển đường.
- `PhoVietGame.cs`: vòng chơi, điều khiển, xe lưu thông, va chạm, âm thanh và giao diện.
- `PhoVietActions.cs`: đạp xe, double tap, xe ngã dây chuyền, truy đuổi và bị bắt.
- `PursuitArt.cs`: mô tô CSGT và buồng giam.
- `PhoVietReckless.cs`: lịch quái xế/xe tải, lạng lách, húc xe, đâm nhà và tự vệ.
- `PhoVietTraffic.cs`: phanh gấp, đèn phanh, tạt làn và cung rẽ giao lộ.
- `PhoVietCollisions.cs`: phát hiện va chạm thân xe, xung lực, khối lượng, ma sát và xoay/trượt sau va chạm.
- `PhoVietPolice.cs`: dự đoán và né giao thông, va chạm mô tô CSGT, ngã và dựng xe sau 1,2 giây.
- `RecklessArt.cs`, `SoftParticle.shader`: xe thể thao, xe tải cỡ trung và hiệu ứng xe cháy.
- `PhoVietBuilder.cs`: menu **Phố Việt → Dựng lại scene chính**. Lệnh này dựng lại scene; lưu công việc khác trước khi sử dụng.

Scene đã lưu sẵn hình học, vật liệu và font. Xe dùng object pool, phố tái sử dụng từng đoạn, hình học được gộp theo vật liệu. Font **Be Vietnam Pro SemiBold** dùng giấy phép OFL, kèm `Assets/PhoViet/Fonts/OFL.txt`.

Tạo hình tham khảo tư liệu của [Bộ Công an về CSGT tuần tra](https://www.bocongan.gov.vn/bai-viet/tang-cuong-tuan-tra-kiem-soat-khong-bo-trong-dia-ban-bao-dam-an-toan-giao-thong-d23-t44777) và [sinh hoạt phạm nhân](https://www.bocongan.gov.vn/bai-viet/ngay-ve-cua-pham-nhan-d23-t44471); các mô hình trong game được dựng mới theo phong cách 3D cách điệu.
