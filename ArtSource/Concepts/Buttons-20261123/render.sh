#!/bin/sh
# 21.337 — chụp mẫu nút bằng Chrome headless: raw/sheet-{A,B,C}.png (cả hàng) + raw/{mau}-{nut}.png (512², nền trong suốt)
CH="/c/Program Files/Google/Chrome/Application/chrome.exe"
P=$(cygpath -m "$PWD"); mkdir -p raw
for m in ${@:-A B C}; do
 "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --window-size=1450,260 --virtual-time-budget=2000 --screenshot="$P/raw/sheet-$m.png" "file:///$P/nut.html?mau=$m" 2>/dev/null
 for n in left right laser missile jump reverse; do
  "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --default-background-color=00000000 --window-size=512,512 --virtual-time-budget=2000 --screenshot="$P/raw/$m-$n.png" "file:///$P/nut.html?mau=$m&nut=$n" 2>/dev/null
 done
done
