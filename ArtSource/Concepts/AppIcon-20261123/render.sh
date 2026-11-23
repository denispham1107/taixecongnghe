#!/bin/sh
# 21.343 — xuất PNG 1024² từ SVG mẫu icon bằng Chrome headless: sh render.sh [tên-svg-không-đuôi ...]  → raw/<tên>.png
CH="/c/Program Files/Google/Chrome/Application/chrome.exe"
P=$(cygpath -m "$PWD"); mkdir -p raw
for f in ${@:-$(ls *.svg | sed 's/\.svg$//')}; do
 printf '<html><body style="margin:0"><img src="../%s.svg" width="1024" height="1024" style="display:block"></body></html>' "$f" > raw/_$f.html
 "$CH" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --window-size=1024,1024 --virtual-time-budget=1500 --screenshot="$P/raw/$f.png" "file:///$P/raw/_$f.html" 2>/dev/null
 rm -f raw/_$f.html
done
