#!/bin/sh
# 21.338 — xuất nút mẫu A (người dùng chọn) vào game: Assets/PhoViet/Resources/TouchControls/PV_Btn_*.png 512², nền trong suốt.
# Chạy trong thư mục này:  sh xuat_game.sh            (đủ 9 nút mẫu A + 8 ảnh nút trang chủ)
#                          sh xuat_game.sh laser 3    (thêm nút LAZER kiểu ?lz=3 sau khi người dùng chọn)
# Sau đó trong Unity: nhập lại ảnh (AssetDatabase.ImportAsset ForceUpdate) — .meta giữ nguyên (RGBA32 không nén + mipmap, 21.284).
CH="/c/Program Files/Google/Chrome/Application/chrome.exe"
P=$(cygpath -m "$PWD"); OUT="../../../Assets/PhoViet/Resources/TouchControls"
shot() { "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --default-background-color=00000000 --window-size=512,512 --virtual-time-budget=2000 --screenshot="$P/raw/game-$1.png" "file:///$P/nut.html?mau=A&nut=$2$3" 2>/dev/null && cp "raw/game-$1.png" "$OUT/PV_Btn_$1.png"; }
if [ "$1" = "laser" ]; then shot Laser laser "&lz=${2:-1}"; exit; fi
shot Left left; shot Right right; shot Missile missile; shot Jump jump; shot Reverse reverse
shot Laser laser "&lz=3"; shot Gas gas; shot Stop stop; shot Shield shield            # 21.339 LAZER kiểu 3; 21.340 GA, P/DRIFT, KHIÊN
# 21.340: ảnh nút trang chủ (Assets/WebGLTemplates/PhoVietPWA/controls/btn-*.png, 160², 8 nút — trang chủ không có P/DRIFT)
python -X utf8 -c "
from PIL import Image
for k in ['Left','Right','Shield','Reverse','Gas','Jump','Missile','Laser']:
    Image.open('raw/game-%s.png' % k).convert('RGBA').resize((160, 160), Image.LANCZOS).save('../../../Assets/WebGLTemplates/PhoVietPWA/controls/btn-%s.png' % k.lower(), optimize=True)
print('trang chu ok')"

