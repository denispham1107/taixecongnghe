using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        Texture2D guiStar;
        static string PortableGuiText(string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Web players have no OS font fallback for these symbols. ASCII stays readable.
            return value.Replace("←","<").Replace("→",">").Replace("Ⅱ","II").Replace("▶",">");
#else
            return value;
#endif
        }

        void DrawGuiStar(Rect rect,Color color)
        {
            if(guiStar==null)
            {
                const int size=64;
                var points=new Vector2[10];
                for(int i=0;i<points.Length;i++)
                {
                    float a=(90+i*36)*Mathf.Deg2Rad,r=i%2==0?29:29*.382f;
                    points[i]=new Vector2(32+Mathf.Cos(a)*r,32+Mathf.Sin(a)*r);
                }
                var pixels=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    bool inside=false;float px=x+.5f,py=y+.5f;
                    for(int i=0,j=9;i<10;j=i++)
                    {
                        var a=points[i];var b=points[j];
                        if((a.y>py)!=(b.y>py)&&px<(b.x-a.x)*(py-a.y)/(b.y-a.y)+a.x)inside=!inside;
                    }
                    pixels[y*size+x]=inside?Color.white:Color.clear;
                }
                guiStar=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Five point GUI star",filterMode=FilterMode.Bilinear};
                guiStar.SetPixels(pixels);guiStar.Apply(false,true);
            }
            GUI.DrawTexture(rect,guiStar,ScaleMode.ScaleToFit,true,0,color,0,0);
        }

        // Invoked by the web shell on visibility loss and before opening its help panel.
        [UnityEngine.Scripting.Preserve]
        public void PauseForBrowser()
        {
            if(State==RideState.Riding)TogglePause();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ConfigureBrowserGraphics()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game!=null && game.rideCamera!=null)game.rideCamera.allowHDR=false;
            if(Application.isMobilePlatform)
            {
                QualitySettings.shadowDistance=45;
                QualitySettings.shadowResolution=ShadowResolution.Low;
                QualitySettings.shadowCascades=0;
                QualitySettings.antiAliasing=2;
            }
#endif
        }
    }
}
