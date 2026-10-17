using System;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifySafeArea
{
    public static object Main()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        const BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;
        var areaMethod=typeof(PhoVietGame).GetMethod("GameGuiArea",f);
        var tap=typeof(PhoVietGame).GetField("lastTapTime",f);
        var old=typeof(PhoVietGame).GetField("browserInsets",f).GetValue(game);
        var had=typeof(PhoVietGame).GetField("hasBrowserInsets",f).GetValue(game);
        int count=0;
        Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception(name);count++;};
        try
        {
            game.RestartRide();
            game.ApplyBrowserInsets("{\"top\":0.06,\"bottom\":0.04,\"left\":0.08,\"right\":0.08}");
            var area=(Rect)areaMethod.Invoke(game,null);
            check(area.xMin>0&&area.yMin>0&&area.xMax<Screen.width&&area.yMax<Screen.height,"HUD must be inset on all sides");
            check(game.rideCamera.rect==new Rect(0,0,1,1),"Camera still renders the entire screen");
            foreach(var point in new[]{new Vector2(Screen.width*.5f,Screen.height-1),new Vector2(Screen.width*.5f,1),new Vector2(1,Screen.height*.5f),new Vector2(Screen.width-1,Screen.height*.5f)})
            {
                tap.SetValue(game,1f);game.RegisterTap(point,1.2f);
                check((float)tap.GetValue(game)==-100,"Cutouts and home-indicator regions must not launch kicks");
            }
            var centre=new Vector2(Screen.width*.5f,Screen.height*.5f);
            tap.SetValue(game,-100f);game.RegisterTap(centre,5f);
            check((float)tap.GetValue(game)==5,"Gameplay-area tap remains accepted");
            var before=area;
            game.ApplyBrowserInsets("{\"top\":-0.5}");
            check((Rect)areaMethod.Invoke(game,null)==before,"Invalid insets must not corrupt layout");
            game.ApplyBrowserInsets("{\"left\":0,\"right\":0,\"top\":0,\"bottom\":0}");
            check((Rect)areaMethod.Invoke(game,null)==new Rect(0,0,Screen.width,Screen.height),"Devices without cutouts retain full-screen HUD layout");
            return new{success=true,count};
        }
        finally
        {
            typeof(PhoVietGame).GetField("browserInsets",f).SetValue(game,old);
            typeof(PhoVietGame).GetField("hasBrowserInsets",f).SetValue(game,had);
            game.ResetRide();
        }
    }
}
