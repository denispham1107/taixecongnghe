using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.396 — chế độ «TẬP LUYỆN» (nút dưới LÊN XE THÔI): cùng bản đồ, CSGT, giao thông, thời tiết (trời thường + mưa giông) như
    // chơi thật, nhưng hiểm họa thưa để người mới quen điều khiển (người dùng 25/11/2026):
    // • Không khủng long.
    // • Thỉnh thoảng 1 xe tăng (mốc 250 m, rồi mỗi 500 m) và 1 máy bay — trực thăng trong phố (mốc 500 m, rồi mỗi 500 m) hoặc
    //   máy bay cánh kép ở lề sát biển; cùng lúc tối đa 1 xe tăng và 1 chiếc bay.
    // • Tàu vẫn bắn khi xe ra lề sông/lề biển nhưng chỉ 2 tàu (21.398, trước 3): 2 tàu sông gần xe nhất (chọn lại mỗi 2 s) và 2 tàu biển.
    // Quãng đường vẫn đo nội bộ (để định mốc hiểm họa) nhưng không hiện, không lưu kỷ lục, không gửi bảng xếp hạng (OpenWorldGame).
    public partial class OpenWorldHazards
    {
        [Header("Tập luyện (21.396)")]
        public bool practice;
        public float practiceFirstTank = 250f, practiceFirstHeli = 500f, practiceEvery = 500f;
        public int practiceBoats = 2;                              // 21.398 (người dùng): bớt 1 tàu, 3 → 2

        float TankFirst => practice ? practiceFirstTank : tankEvery;
        float HeliFirst => practice ? practiceFirstHeli : heliEvery;
        float TankEvery => practice ? practiceEvery : tankEvery;
        float HeliEvery => practice ? practiceEvery : heliEvery;
        float DinoFirst => practice ? float.PositiveInfinity : firstAt;
        int CoastBoats => practice ? Mathf.Min(practiceBoats, coastBoats) : coastBoats;
        // Tập luyện: chỉ 1 chiếc bay cùng lúc (trực thăng hoặc máy bay biển).
        int CoastPlanes => practice ? (ActiveHeliCount() > 0 ? 0 : 1) : coastPlanes;
        int TankWant => practice ? Mathf.Min(1, 1 - ActiveTankCount()) : Mathf.Min(TankWave, maxTanks - ActiveTankCount());
        int HeliWant => practice ? (ActiveHeliCount() + ActivePlanes > 0 ? 0 : 1) : Mathf.Min(HeliWave, maxHelis - ActiveHeliCount());

        readonly HashSet<int> practiceRiverGuns = new HashSet<int>();
        float practiceGunsAt = -99f;

        // Tàu sông i được bắn không? Chơi thật: mọi tàu. Tập luyện: chỉ practiceBoats tàu sông còn chạy gần xe nhất.
        bool RiverGunArmed(int i, Vector3 pp)
        {
            if (!practice) return true;
            if (Time.time - practiceGunsAt > 2f)
            {
                practiceGunsAt = Time.time; practiceRiverGuns.Clear();
                var pick = new List<KeyValuePair<float, int>>();
                for (int k = 0; k < boats.Count; k++)
                    if (!boats.IsSea(k) && boats.Alive(k)) pick.Add(new KeyValuePair<float, int>((boats.BoatCenter(k) - pp).sqrMagnitude, k));
                pick.Sort((a, b) => a.Key.CompareTo(b.Key));
                for (int k = 0; k < pick.Count && k < practiceBoats; k++) practiceRiverGuns.Add(pick[k].Value);
            }
            return practiceRiverGuns.Contains(i);
        }
    }
}
