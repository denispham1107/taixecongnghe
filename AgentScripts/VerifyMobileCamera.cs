using System;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyMobileCamera
{
    public static object Main()
    {
        if(!Application.isPlaying||Screen.width>=Screen.height)throw new Exception("Requires portrait Play Mode");
        var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        var camera=game.rideCamera;
        var position=typeof(PhoVietGame).GetMethod("PositionCamera",BindingFlags.Instance|BindingFlags.NonPublic);
        int count=0;
        try
        {
            game.ResetRide();camera.aspect=(float)Screen.width/Screen.height;
            foreach(bool chased in new[]{false,true})
            {
                typeof(PhoVietGame).GetProperty("IsChased").SetValue(game,chased);
                foreach(float laneX in PhoVietGame.LaneX)
                {
                    game.rider.localPosition=new Vector3(laneX,0,0);position.Invoke(game,new object[]{true});
                    foreach(var renderer in game.rider.GetComponentsInChildren<Renderer>())
                    {
                        var b=renderer.bounds;
                        for(int mask=0;mask<8;mask++)
                        {
                            var p=new Vector3((mask&1)==0?b.min.x:b.max.x,(mask&2)==0?b.min.y:b.max.y,(mask&4)==0?b.min.z:b.max.z);
                            var v=camera.WorldToViewportPoint(p);
                            if(v.z<=0||v.x<.02f||v.x>.98f||v.y<.02f||v.y>.98f)
                                throw new Exception("Rider clipped: chased="+chased+", laneX="+laneX+", viewport="+v);
                        }
                    }
                    count++;
                }
            }
            return new {success=true,count,width=Screen.width,height=Screen.height,coverage="All rider renderer bounds, four lanes, ordinary and police chase cameras"};
        }
        finally {camera.ResetAspect();game.ResetRide();}
    }
}
