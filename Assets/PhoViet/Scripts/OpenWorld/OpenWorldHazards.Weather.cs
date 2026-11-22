using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.317 — sét của thời tiết mưa giông (OpenWorldWeather) gây hại như đạn pháo (người dùng chọn «có hại, có cảnh báo»):
    // • Xe người chơi trong bán kính sét: bốc cháy rồi nổ tung, hết lượt («SÉT ĐÁNH TRÚNG»); đang bật KHIÊN thì chặn được, chỉ giảm tốc.
    // • Xe NPC trong bán kính: bốc cháy 0,45 s rồi nổ tung thành mảnh (đường LAZER: OpenWorldTraffic.LightningAround); xe sát bên bị hất.
    // • Xe CSGT gần: khựng 1,5 s.
    public partial class OpenWorldHazards
    {
        public int LightningHits { get; private set; }
        public int LightningNpc { get; private set; }

        public void LightningBlast(Vector3 at, float radius, bool hurtPlayer)
        {
            if (traffic)
            {
                int burned = traffic.LightningAround(at, radius);
                LightningNpc += burned; NpcKnocked += burned + traffic.KnockAround(at, radius * 1.8f);
            }
            foreach (var pol in OpenWorldPolice.All) if (pol && Flat(pol.transform.position - at) < radius + 2f) pol.Stun(1.5f, "sét đánh");
            if (!hurtPlayer || !player || player.Crashed || player.Sinking) return;
            if (Flat(player.transform.position - at) > radius + 0.5f) return;
            LightningHits++;
            if (player.Shielded)
            {   // khiên năng lượng chặn sét như chặn đạn (Crash tự đổi thành ShieldBump)
                player.Crash(at, "SÉT ĐÁNH TRÚNG", 10f); ShieldBlocks++; Note("sét bị khiên chặn");
            }
            else
            {
                player.Crash(at, "SÉT ĐÁNH TRÚNG", 10f); Note("sét đánh trúng người chơi");
                if (player.Crashed) player.Ignite(BurnFx(player.transform, new Vector3(0, 0.7f, 0)), 0.55f);
            }
        }

        // Điểm trên lề sát biển gần `near` (≤ maxDist tới kè): chân kè gần nhất ± dọc kè, lùi vào bờ trong dải lề biển.
        public static bool CoastPoint(Vector3 near, float maxDist, float along, out Vector3 p)
        {
            p = near;
            NearestSide(near, out var foot, out var outN, out var dirAlong);
            float d = Vector2.Distance(new Vector2(near.x, near.z), foot);
            if (d > maxDist) return false;
            bool ne = outN == Vector2.up || outN == Vector2.right;
            float inward = ne ? Random.Range(1.5f, CoastNE - 2f) : Random.Range(0.8f, CoastSW - 0.8f);
            var q = foot + dirAlong * along - outN * inward;
            p = new Vector3(q.x, 0f, q.y);
            return IsCoast(p);
        }
    }
}
