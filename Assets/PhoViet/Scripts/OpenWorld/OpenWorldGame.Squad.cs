using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.301 — người dùng 21/11/2026: «mỗi khi đi thêm 300 m cho thêm 1 xe CSGT (xuất hiện cùng chiều chạy lên truy đuổi, hoặc
    // khác chiều chạy xuống để bắt người chơi); lần trước cùng chiều thì lần sau ngược chiều».
    // • Mốc 300, 600, 900 m… (quãng đường của lượt) → nhân bản xe CSGT gốc (cùng mô hình, đèn, còi, lưới điện), luân phiên
    //   NGƯỢC CHIỀU (phía trước ~95 m, quay đầu về phía người chơi) rồi CÙNG CHIỀU (phía sau ~75 m) — xe gốc đã đuổi cùng chiều
    //   nên xe tăng viện đầu tiên tới từ phía trước. Tối đa 10 xe tăng viện (giữ nhẹ máy điện thoại).
    // • Xe nào bắt được cũng vào phòng giam; thanh thông tin «CSGT» hiện xe gần nhất; thẻ thông báo khi có xe mới.
    // • Chơi lại / về đầu phố: xóa hết xe tăng viện. Hết lượt (GameOver): dừng mọi xe.
    public partial class OpenWorldGame
    {
        public const float SquadEvery = 300f; public const int SquadMax = 10;
        readonly List<OpenWorldPolice> squad = new List<OpenWorldPolice>();
        float nextSquadAt = SquadEvery; bool nextSquadAhead = true;
        float squadToastAt = -99f; string squadToastText;
        public int SquadCount => squad.Count;

        void ClearSquad()
        {
            foreach (var p in squad) if (p) Destroy(p.gameObject);
            squad.Clear(); nextSquadAt = SquadEvery; nextSquadAhead = true;
        }

        void HaltSquad() { foreach (var p in squad) if (p) { p.Halt(); p.enabled = false; } }

        void TickSquad()
        {
            if (!police || Mode != State.Riding) return;
            squad.RemoveAll(p => !p);
            if (Practice) return;                                           // 21.397: tập luyện chỉ 1 xe CSGT (xe gốc đuổi phía sau), không tăng viện
            while (Score >= nextSquadAt)
            {
                nextSquadAt += SquadEvery;
                if (squad.Count >= SquadMax) continue;
                bool ahead = nextSquadAhead; nextSquadAhead = !nextSquadAhead;
                SpawnSquad(ahead);
            }
        }

        public OpenWorldPolice SpawnSquad(bool ahead)
        {
            if (!police || !player) return null;
            var f = player.transform.forward; f.y = 0; f.Normalize();
            var go = Instantiate(police.gameObject, player.transform.position + (ahead ? f * 95f : -f * 75f) + Vector3.up * 40f, police.transform.rotation);
            go.name = "CSGT_TangVien_" + (squad.Count + 1);
            var p = go.GetComponent<OpenWorldPolice>();
            p.deploy = ahead ? 1 : 0; p.drawJail = false; p.showDistance = false; p.enabled = true;
            squad.Add(p);
            squadToastText = ahead ? "phía trước, chạy ngược chiều" : "phía sau, đuổi cùng chiều";
            squadToastAt = Time.time;
            return p;
        }

        bool SquadJailed() { foreach (var p in squad) if (p && p.Jailed) return true; return false; }

        // CSGT gần nhất đang đuổi (xe gốc hoặc tăng viện) cho thanh thông tin
        OpenWorldPolice NearestPolice()
        {
            OpenWorldPolice best = null; float bd = float.MaxValue;
            if (police && police.enabled && police.Mode != OpenWorldPolice.State.Waiting) { best = police; bd = police.Distance; }
            foreach (var p in squad)
                if (p && p.enabled && p.Mode != OpenWorldPolice.State.Waiting && p.Distance < bd) { best = p; bd = p.Distance; }
            return best;
        }

        void SquadToast()
        {
            float age = Time.time - squadToastAt; if (age > 2.6f || string.IsNullOrEmpty(squadToastText)) return;
            OpenWorldToast.Show("TĂNG VIỆN", "Thêm 1 xe CSGT!", squadToastText + " · tổng " + (squad.Count + 1) + " xe", OpenWorldToast.Kind.Danger, age, 2.6f - age);
        }
    }
}
