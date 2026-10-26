using UnityEngine;
namespace PhoViet
{
 // Nhiễu giá trị lặp liền mạch cho mặt sông (25/10/2026, huongdan.md 21.54): lưới 64×64 ô, 4 texel/ô (256×256 RGBA32),
 // R = giá trị (nội suy smoothstep như hàm Noise cũ của shader), G/B = đạo hàm theo x/y (mã hóa 0,5 + d/3, |d| ≤ 1,5).
 // Shader River Water Mirror đọc một lần texture thay cho 4 phép băm + nội suy mỗi lần gọi Noise, và lấy luôn đạo hàm
 // thay cho 2 lần gọi thêm để tính sai phân — gợn nước giữ nguyên dạng, rẻ hơn nhiều trên GPU điện thoại/WebGL.
 public static class RiverNoise
 {
  public const int Period=64,PerCell=4,Size=Period*PerCell;
  static readonly int Id=Shader.PropertyToID("_RiverNoise");
  static Texture2D texture;
  public static Texture2D Ensure()
  {
   if(texture==null)
   {
    var lattice=new float[Period,Period];var rng=new System.Random(20261025);
    for(int y=0;y<Period;y++)for(int x=0;x<Period;x++)lattice[x,y]=(float)rng.NextDouble();
    var px=new Color32[Size*Size];
    for(int ty=0;ty<Size;ty++)for(int tx=0;tx<Size;tx++)
    {
     float fx=(float)tx/PerCell,fy=(float)ty/PerCell;int ix=Mathf.FloorToInt(fx),iy=Mathf.FloorToInt(fy);fx-=ix;fy-=iy;
     float a=lattice[ix%Period,iy%Period],b=lattice[(ix+1)%Period,iy%Period],c=lattice[ix%Period,(iy+1)%Period],d=lattice[(ix+1)%Period,(iy+1)%Period];
     float u=fx*fx*(3-2*fx),v=fy*fy*(3-2*fy),du=6*fx*(1-fx),dv=6*fy*(1-fy);
     float value=Mathf.Lerp(Mathf.Lerp(a,b,u),Mathf.Lerp(c,d,u),v);
     float dx=du*((b-a)*(1-v)+(d-c)*v),dy=dv*((c-a)*(1-u)+(d-b)*u);
     px[ty*Size+tx]=new Color32((byte)Mathf.RoundToInt(value*255),(byte)Mathf.RoundToInt(Mathf.Clamp01(.5f+dx/3)*255),(byte)Mathf.RoundToInt(Mathf.Clamp01(.5f+dy/3)*255),255);
    }
    texture=new Texture2D(Size,Size,TextureFormat.RGBA32,false,true){name="River noise",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear,hideFlags=HideFlags.DontSave};
    texture.SetPixels32(px);texture.Apply(false,true);
   }
   Shader.SetGlobalTexture(Id,texture);return texture;
  }
 }
}
