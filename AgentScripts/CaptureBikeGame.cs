using UnityEngine;
using System.IO;
public static class CaptureBikeGame
{
    public static object Main()
    {
        var g=Object.FindAnyObjectByType<PhoViet.PhoVietGame>();
        var camera=g.rideCamera;var old=camera.targetTexture;
        var rt=new RenderTexture(1440,900,24);var image=new Texture2D(1440,900,TextureFormat.RGB24,false);
        var active=RenderTexture.active;
        try
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();
            File.WriteAllBytes("Captures/blender-bikes-game.png",image.EncodeToPNG());
            return new{success=true,path="Captures/blender-bikes-game.png"};
        }
        finally{camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
    }
}
