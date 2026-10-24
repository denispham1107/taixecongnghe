using UnityEngine;
namespace PhoViet
{
 // Các kiểu ánh sáng buổi sáng có sương (25/10/2026, huongdan.md 21.41) để người dùng duyệt.
 // Áp theo độ đậm sương (MorningFog.Strength): đầu lượt đúng kiểu đã chọn, sương tan thì về ánh sáng ngày cũ.
 // Chỉ số -1 = ánh sáng gốc (nắng trắng 1,12, môi trường gần trắng, sương E1ECF1 — bị ám trắng).
 public sealed class MorningLook
 {
  public string name,description;
  public Color sun,ambientSky,ambientEquator,ambientGround,fog,zenith,horizon;
  public float sunIntensity,fogDensity,sunPitch;
  static Color C(string hex)=>StreetArt.Hex(hex);
  public static readonly MorningLook[] All=
  {
   new MorningLook{name="A · Bình minh vàng ấm",description="Nắng sớm vàng cam chiếu xiên, sương màu đào nhạt, bóng đổ dài",
    sun=C("FFC98E"),sunIntensity=1.0f,sunPitch=24,ambientSky=C("A9B8CC"),ambientEquator=C("C9B49A"),ambientGround=C("6E6658"),
    fog=C("E8C9A6"),fogDensity=.85f,zenith=C("8FB2D8"),horizon=C("F2D2AE")},
   new MorningLook{name="B · Sương xanh dịu",description="Ánh sáng mát, sương xanh xám, màu phố rõ và tương phản hơn",
    sun=C("F1F0EA"),sunIntensity=.92f,sunPitch=34,ambientSky=C("9DB4CB"),ambientEquator=C("A9B7C3"),ambientGround=C("67707A"),
    fog=C("A9BCC8"),fogDensity=.9f,zenith=C("8DAACB"),horizon=C("BFCFDA")},
   new MorningLook{name="C · Sương mỏng trong trẻo",description="Giữ tông sáng ban ngày nhưng giảm chói, sương mỏng hơn, màu tươi",
    sun=C("FFF6E6"),sunIntensity=.98f,sunPitch=40,ambientSky=C("AFC6DD"),ambientEquator=C("B8C4CF"),ambientGround=C("7A838D"),
    fog=C("CCDAE2"),fogDensity=.55f,zenith=C("9CC2E6"),horizon=C("D6E4EE")},
   new MorningLook{name="D · Nắng sớm vàng nhạt, sương xám bạc",description="Nắng vàng nhẹ, sương xám bạc trầm, không gian dịu mắt",
    sun=C("FFE3B5"),sunIntensity=.95f,sunPitch=30,ambientSky=C("A2AFBE"),ambientEquator=C("B3AFA4"),ambientGround=C("6B6A66"),
    fog=C("B9BDBF"),fogDensity=.8f,zenith=C("94AECB"),horizon=C("D9D6CC")},
  };
  public static MorningLook Get(int index)=>index>=0&&index<All.Length?All[index]:null;
 }
}
