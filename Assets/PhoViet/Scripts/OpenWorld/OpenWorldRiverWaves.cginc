// 21.271 — sóng mặt sông bản đồ mở (dùng chung cho OpenWorldRiver.shader, OpenWorldBoatWake.shader).
// Phải khớp từng số với OpenWorldBoats.WaveHeight (C#) để tàu nhấp nhô đúng theo mặt nước.
#ifndef PHOVIET_OW_RIVER_WAVES
#define PHOVIET_OW_RIVER_WAVES

float _OWWaveAmp;   // toàn cục, OpenWorldBoats đặt (mặc định 1)

// Ba sóng dài (bước sóng 30 / 17 / 10 m), tổng biên độ ≈ 0,2 m. Trả về độ cao, slope = đạo hàm (dh/dx, dh/dz).
float OWWaveHeight(float2 p, float t, out float2 slope)
{
    float amp = _OWWaveAmp;
    float h = 0; slope = 0;
    float2 d; float k, w, a, ph;
    d = float2(0.94, 0.34);  k = 0.21; w = sqrt(9.8 * k); a = 0.10 * amp;  ph = dot(d, p) * k + t * w; h += a * sin(ph); slope += d * (a * k * cos(ph));
    d = float2(-0.45, 0.89); k = 0.37; w = sqrt(9.8 * k); a = 0.06 * amp;  ph = dot(d, p) * k + t * w + 1.7; h += a * sin(ph); slope += d * (a * k * cos(ph));
    d = float2(0.20, -0.98); k = 0.63; w = sqrt(9.8 * k); a = 0.035 * amp; ph = dot(d, p) * k + t * w + 4.1; h += a * sin(ph); slope += d * (a * k * cos(ph));
    return h;
}

#endif
