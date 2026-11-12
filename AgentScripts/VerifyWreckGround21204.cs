using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhoViet;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public static class VerifyWreckGround21204
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static PhoVietGame game;
    static int checks;
    static float lowestGround, highestGround;
    static readonly Vector3[] RolledPoses = {
        new Vector3(0,10,78), new Vector3(24,20,-78), new Vector3(55,100,0),
        new Vector3(-78,50,0), new Vector3(8,10,35), new Vector3(24,120,63),
        new Vector3(-25,-45,-60)
    };

    public static object Main()
    {
        game = UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        checks = 0;
        lowestGround = float.PositiveInfinity;
        highestGround = float.NegativeInfinity;
        float time = Time.timeScale;
        bool rivers = game.riversEnabled;
        try
        {
            Time.timeScale = 0;
            PlayerWreck();
            TrafficWrecks();
            AircraftWreck("helicopters");
            AircraftWreck("skywings");
            BridgeAndWater();
            return new { checks, lowestGround, highestGround };
        }
        finally
        {
            game.riversEnabled = rivers;
            game.ResetRide();
            Time.timeScale = time;
        }
    }

    static void Restart()
    {
        game.RestartRide();
        game.riversEnabled = false;
        foreach (var v in game.vehicles) v.active = false;
        foreach (var v in game.vehicles) v.model.gameObject.SetActive(false);
        game.policeBike.localPosition = new Vector3(0,0,-85);
        Set("spawnTimer", 999f);
    }

    static void PlayerWreck()
    {
        foreach (bool claimed in new[] { false, true })
        {
            Restart();
            var car = game.rider.GetComponent<PlayerCarVisual>();
            if (claimed)
            {
                var truck = game.vehicles.First(v => !v.eventOnly &&
                    v.model.GetComponent<MeshyCarVariants>()?.variants.Any(a => a.asset == "PV_MeshyContainer") == true);
                var donor = game.SpawnTraffic(2,5,game.vehicles.IndexOf(truck));
                Select(donor,"PV_MeshyContainer");
                Call("TakeVehicle",donor);
                Check(donor.playerClaimed,"Container is now the player car");
            }
            var target = game.vehicles.First(v => !v.playerClaimed && !v.eventOnly &&
                v.model.GetComponent<MeshyCarVariants>()?.variants.Any(a => a.asset == "PV_MeshySport") == true);
            var hit = game.SpawnTraffic(1,1,game.vehicles.IndexOf(target));
            Select(hit,"PV_MeshySport");
            hit.motionVelocity = Vector3.left*20;
            var point = game.rider.localPosition + Vector3.right;
            Call("BeginRiderImpact",hit,Vector2.right,point,Vector3.forward*21,false);
            Check(game.State == PhoVietGame.RideState.Falling,"Severe traffic collision entered the falling state");
            hit.active = false;
            hit.model.gameObject.SetActive(false);
            var claimedVisual = claimed ? game.rider.GetComponentInChildren<CarVisualAnimator>() : null;
            var visual = claimed ? claimedVisual.visual : car.visual;
            for (int i=0; i<155 && !game.PlayerOnFoot; i++) Call("TickRiderCrash",.02f);
            Check(game.PlayerOnFoot,"Collision forced the player out of the car");
            foreach(var angle in RolledPoses)
            {
                game.rider.SetLocalPositionAndRotation(new Vector3(1.6f,0,7),Quaternion.Euler(angle));
                car.Advance(.016f);
                claimedVisual?.Advance(.016f);
                Ground(visual,claimed?"claimed car wreck":"abandoned player car wreck",.18f);
            }
            game.TogglePause();
            var before = visual.position;
            car.Advance(.016f);
            claimedVisual?.Advance(.016f);
            Check((visual.position-before).sqrMagnitude<.000001f,"Pausing preserves the supported pose");
            game.TogglePause();
            SetState(PhoVietGame.RideState.GameOver);
            car.Advance(.016f);
            claimedVisual?.Advance(.016f);
            Ground(visual,"wreck after game over",.18f);
            game.ResetRide();
            car.Advance(.016f);
            Check(Mathf.Abs(car.visual.localPosition.y)<.001f,"Restart clears the wreck lift");
        }
    }

    static void TrafficWrecks()
    {
        Restart();
        var visited = new HashSet<string>();
        foreach (var v in game.vehicles)
        {
            v.active=true;
            v.model.gameObject.SetActive(true);
            var variants = v.model.GetComponent<MeshyCarVariants>();
            var bike = v.model.GetComponent<BikeVisualAnimator>();
            if(variants!=null)
            {
                foreach (var variant in variants.variants)
                {
                    if(!visited.Add(variant.asset)) continue;
                    Select(v,variant.asset);
                    v.knocked=true;
                    v.knockAge=2;
                    foreach (var a in variant.armored ? new[] { Vector3.zero } : RolledPoses)
                    {
                        v.model.SetLocalPositionAndRotation(new Vector3(0,0,10),Quaternion.Euler(a));
                        variants.driver.Advance(.17f);
                        Ground(variant.visual,variant.asset,.20f);
                    }
                    if (variant.armored) continue;
                    v.wallCrashed=true;
                    variants.driver.Advance(.1f);
                    Ground(variant.visual,variant.asset+" wall wreck",.20f);
                    v.wallCrashed=false;
                }
            }
            else if(bike!=null && visited.Add(bike.AssetName))
            {
                v.knocked=true;
                v.knockAge=2;
                foreach(var a in RolledPoses)
                {
                    v.model.SetLocalPositionAndRotation(new Vector3(0,0,10),Quaternion.Euler(a));
                    bike.Advance(.016f);
                    Ground(bike.visual,bike.AssetName,.16f);
                }
            }
            v.active=false;
            v.model.gameObject.SetActive(false);
        }
        Check(visited.Contains("PV_MeshyTank") && visited.Contains("PV_MeshyContainer") && visited.Count>=9,"All road model families are represented");
        Call("TickPoliceRecovery",new Vector3(-1.6f,0,5),.02f);
        Ground(game.policeBike.Find("Vietnam Traffic Police Cruiser"),"police sedan during recovery",.02f);
    }

    static void AircraftWreck(string fleet)
    {
        Restart();
        Check((bool)(fleet=="helicopters"?Call("SpawnHelicopter"):Call("LaunchSkywing",0,1)),fleet+" spawned");
        var actor = ((IList)Read(fleet))[0];
        var root = (Transform)actor.GetType().GetField("root").GetValue(actor);
        root.SetLocalPositionAndRotation(new Vector3(0,5,12),Quaternion.Euler(18,160,-42));
        Call("KnockHelicopter",actor,Vector3.zero,Vector3.right);
        var velocity=actor.GetType().GetField("velocity");
        velocity.SetValue(actor,Vector3.down*2);
        for(int i=0;i<120;i++) Call("TickHelicopterWreck",actor,.02f);
        Check(root.gameObject.activeSelf && Mathf.Abs(((Vector3)velocity.GetValue(actor)).y)<.001f,fleet+" remains on dry ground");
        Ground(root.Find("Airframe"),fleet,.14f);
    }

    static void BridgeAndWater()
    {
        Restart();
        game.riversEnabled=true;
        foreach(var row in game.roadRows)
            foreach(var tile in row.tiles) tile.GetComponent<RiverBlock>().SetBridge(false);
        Transform bridge = game.blocks.First(b => b.gameObject.activeInHierarchy && b.parent.gameObject.activeInHierarchy);
        bridge.GetComponent<RiverBlock>().SetBridge(true);
        Vector3 deck = game.transform.InverseTransformPoint(bridge.TransformPoint(new Vector3(1,0,20)));
        Vector3 water = game.transform.InverseTransformPoint(bridge.TransformPoint(new Vector3(5,0,20)));
        Check(!game.OverRiver(deck) && game.OverRiver(water),"Bridge fixture has a real deck and an exposed river edge");
        var car=game.rider.GetComponent<PlayerCarVisual>();
        ((PhoVietGame.TrafficVehicle)Read("riderBody")).knocked=true;
        SetState(PhoVietGame.RideState.Falling);
        game.rider.SetLocalPositionAndRotation(deck,Quaternion.Euler(14,0,78));
        car.Advance(.016f);
        Ground(car.visual,"wreck on bridge deck",.12f);
        game.rider.localPosition=water+Vector3.up*(RiverBlock.WaterY-1);
        car.Advance(.016f);
        Check(car.visual.position.y<RiverBlock.WaterY,"Player skin is not lifted above the river");
        var sample = game.vehicles.First(v => !v.eventOnly && v.model.GetComponent<BikeVisualAnimator>()!=null);
        var bike=game.SpawnTraffic(2,5,game.vehicles.IndexOf(sample));
        bike.model.localPosition=bike.previousPosition=water;
        Call("ActivateCollisionBody",bike,Vector3.zero,Vector3.down*2,Vector2.right*200,Vector2.zero,false,true,8f);
        Check((bool)Call("TryBeginVehicleRiverFall",bike,water,.02f),"Bike crosses exposed edge and enters river physics");
        for(int i=0;i<170;i++) Call("TickVehicleRiverFall",bike,.02f,false);
        bike.model.GetComponent<BikeVisualAnimator>().Advance(.016f);
        Check(bike.riverSplashed && bike.model.localPosition.y<RiverBlock.WaterY-1 &&
            bike.model.GetComponent<BikeVisualAnimator>().visual.position.y<RiverBlock.WaterY,"NPC keeps sinking below water");
    }

    static void Select(PhoVietGame.TrafficVehicle v,string model)
    {
        var variants=v.model.GetComponent<MeshyCarVariants>();
        variants.Select(Array.FindIndex(variants.variants,entry=>entry.asset==model),0,v);
    }
    static object Call(string name,params object[] args) => typeof(PhoVietGame).GetMethod(name,Flags).Invoke(game,args);
    static object Read(string name) => typeof(PhoVietGame).GetField(name,Flags).GetValue(game);
    static void Set(string name,object value) => typeof(PhoVietGame).GetField(name,Flags).SetValue(game,value);
    static void SetState(PhoVietGame.RideState state) => typeof(PhoVietGame).GetProperty("State",Flags).SetValue(game,state);
    static void Check(bool condition,string message)
    {
        if(!condition) throw new InvalidOperationException(message);
        checks++;
    }
    static void Ground(Transform root,string name,float upper)
    {
        float min=MinimumMeshY(root)-game.transform.position.y;
        Check(min>=-.025f && min<=upper,name+" has mesh bottom "+min.ToString("0.000")+" m above ground");
        lowestGround=Mathf.Min(lowestGround,min);
        highestGround=Mathf.Max(highestGround,min);
    }
    static float MinimumMeshY(Transform root)
    {
        float min=float.PositiveInfinity;
        var bake=new Mesh();
        foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(!renderer.enabled)continue;
            renderer.BakeMesh(bake);
            foreach(var point in bake.vertices) min=Mathf.Min(min,renderer.transform.TransformPoint(point).y);
        }
        UnityEngine.Object.DestroyImmediate(bake);
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
        {
            if(filter.name.Contains("blur",StringComparison.OrdinalIgnoreCase)||!filter.TryGetComponent<MeshRenderer>(out var renderer)||!renderer.enabled)continue;
            using var data=MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
            using var points=new NativeArray<Vector3>(data[0].vertexCount,Allocator.Temp);
            data[0].GetVertices(points);
            foreach(var point in points) min=Mathf.Min(min,filter.transform.TransformPoint(point).y);
        }
        return min;
    }
}
