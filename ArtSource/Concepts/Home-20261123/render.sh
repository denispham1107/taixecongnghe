#!/bin/sh
# 21.330 — chụp 3 mẫu bằng Chrome headless: điện thoại 390 px (×2) và máy tính 1440 px
CH="/c/Program Files/Google/Chrome/Application/chrome.exe"
P=$(cygpath -m "$PWD"); mkdir -p raw
for m in ${@:-A B C}; do
 "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --force-device-scale-factor=2 --window-size=600,3600 --virtual-time-budget=3000 --screenshot="$P/raw/$m-phone.png" "file:///$P/trangchu.html?mau=$m&shot=1&w=390" 2>/dev/null
 "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --force-device-scale-factor=1 --window-size=1440,2600 --virtual-time-budget=3000 --screenshot="$P/raw/$m-pc.png" "file:///$P/trangchu.html?mau=$m&shot=1" 2>/dev/null
done
