using System;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyShopHideout21197
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static PhoVietGame game;
    static int passed;
    static object Call(string method, params object[] args) =>
        typeof(PhoVietGame).GetMethod(method, Flags).Invoke(game, args);
    static void Set(string name, object value) =>
        typeof(PhoVietGame).GetField(name, Flags).SetValue(game, value);
    static void Check(bool condition, string error)
    {
        if (!condition) throw new InvalidOperationException(error);
        passed++;
    }

    static void Tick(int frames)
    {
        for (int i = 0; i < frames; i++) Call("TickOnFoot", .02f);
    }

    static void ClearTraffic()
    {
        foreach (var vehicle in game.vehicles)
        {
            vehicle.active = false;
            vehicle.model.gameObject.SetActive(false);
        }
        Set("spawnTimer", 1000f);
    }

    static void Tap(Vector2 pixel)
    {
        game.ApplyHudTap(FormattableString.Invariant(
            $"{{\"x\":{pixel.x/Screen.width},\"y\":{pixel.y/Screen.height}}}"));
    }

    static (Vector3 approach, Vector3 inward) FindShop()
    {
        var doors = game.roadRows.Where(r => r.root.gameObject.activeInHierarchy)
            .SelectMany(r => r.tiles)
            .SelectMany(tile => tile.Cast<Transform>())
            .Where(t => t.gameObject.activeInHierarchy && t.name.EndsWith("TẠP HÓA"))
            .Select(t => ((Vector3)Call("ShopApproach", t), (Vector3)Call("ShopInward", t)))
            .Where(p => !game.OverRiver(p.Item1))
            .OrderBy(p => (p.Item1 - game.FootCharacter.localPosition).sqrMagnitude).ToArray();
        Check(doors.Length > 0, "The authored scene has an accessible grocery entrance.");
        return doors[0];
    }

    static void EnterAtShop(bool walk)
    {
        var (approach, inward) = FindShop();
        Check(Mathf.Abs(inward.y) < .001f && Mathf.Abs(approach.x) < 60, "The authored door uses a horizontal world orientation.");
        game.FootCharacter.localPosition = approach - inward * (walk ? .42f : .18f);
        if (walk)
        {
            Set("footMove", new Vector2(inward.x, inward.z));
            for (int i = 0; i < 12 && !game.InsideShop; i++) Tick(1);
            Set("footMove", Vector2.zero);
        }
        else Check(game.TryEnterShop(), "E interaction enters through a nearby grocery door.");
        Check(game.InsideShop && game.PlayerOnFoot && game.ShopHideRemaining > 2.98f, "The walk-in starts a new three-second shelter session.");
    }

    static PhoVietGame.TrafficVehicle TakeDifferentCar()
    {
        int index = game.vehicles.FindIndex(v => !v.active && !v.eventOnly && !v.playerClaimed &&
            v.halfWidth > .7f && v.model.GetComponent<MeshyCarVariants>() != null);
        var car = game.SpawnTraffic(2, 9, index);
        Check(car != null && car.halfWidth > .7f, "A replacement car is available from normal traffic.");
        car.model.SetLocalPositionAndRotation(new Vector3(PhoVietGame.LaneX[2], 0, 9), Quaternion.identity);
        Call("DetonateGrenadeAt", car.model.localPosition, true);
        var door = typeof(PhoVietGame).GetMethod("LeftDoorPosition", BindingFlags.Static | BindingFlags.NonPublic);
        game.FootCharacter.localPosition = (Vector3)door.Invoke(null, new object[] { car });
        Check(game.TryTakeVehicle(), "The stopped replacement can be entered from its left door.");
        Call("TickVehicleEntry", .79f);
        Check(game.State == PhoVietGame.RideState.Riding && car.model.parent == game.rider && game.rider.gameObject.activeSelf,
            "The exact replacement car becomes the player's visible car.");
        return car;
    }

    static void VerifySearchWindow()
    {
        game.RestartRide();
        ClearTraffic();
        Check(game.TryExitVehicle() && game.FootEscapeRemaining == 10, "Manual exit starts the ten-second search immediately.");
        Check(Call("NearbyShop", 96f, true) is Transform && Call("NearbyShop", 96f, false) == null,
            "Guide can identify a real shop during the exit animation without entering it early.");
        Tick(29);
        Check(Mathf.Abs(game.FootEscapeRemaining - 9.42f) < .001f, "The countdown includes the jump out of the car.");
        game.TogglePause();
        Tick(75);
        Check(game.State == PhoVietGame.RideState.Paused && Mathf.Abs(game.FootEscapeRemaining - 9.42f) < .001f,
            "Pausing holds the search countdown.");
        game.TogglePause();
        game.policeBike.localPosition = game.FootCharacter.localPosition;
        Tick(470);
        game.Arrest();
        Check(game.PlayerOnFoot && game.IsChased && game.FootEscapeRemaining > 0, "Police cannot arrest just before ten seconds.");
        Tick(1);
        Check(game.FootEscapeRemaining == 0, "The search window expires at ten seconds of play.");
        game.Arrest();
        Check(game.State == PhoVietGame.RideState.Arrested, "Arrest can proceed after the deadline.");

        var bearing = typeof(PhoVietGame).GetMethod("ShopGuideBearing", BindingFlags.Static | BindingFlags.NonPublic);
        foreach (var (direction, expected) in new[] { (Vector3.forward, Vector2.down), (Vector3.back, Vector2.up),
                     (Vector3.right, Vector2.right), (Vector3.left, Vector2.left) })
        {
            var result = (Vector2)bearing.Invoke(null, new object[] { direction, Vector3.right, Vector3.forward });
            Check((result - expected).sqrMagnitude < .001f, "Guide arrow uses camera-relative GUI coordinates.");
        }
    }

    static void VerifyFullLoop()
    {
        game.RestartRide();
        ClearTraffic();
        Check(!game.TryEnterShop() && game.IsChased, "The shop cannot be entered while driving.");
        Check(game.TryExitVehicle(), "Leave the original car.");
        Tick(29);
        Check(!game.TryEnterShop(), "There is no doorway interaction in the middle of the road.");
        EnterAtShop(true);
        Check(!game.TryLeaveShop() && !game.TryTakeVehicle() && !game.TryThrowFootGrenade(),
            "A hiding character cannot exit early, enter a car, or throw grenades from indoors.");
        game.Arrest();
        Check(game.PlayerOnFoot && game.InsideShop, "The police cannot arrest inside the shop.");
        float distance = game.Distance;
        var initial = game.FootCharacter.localPosition;
        Set("footMove", new Vector2(-1,0));
        Tick(50);
        Set("footMove", Vector2.zero);
        Check(game.FootCharacter.localPosition.x < initial.x-1 &&
              game.FootCharacter.localPosition.x >= initial.x-1.95f, "The character can move indoors without crossing shelving.");
        Check(game.rider.localPosition.y > .04f, "The tow truck lifts the abandoned car onto its bed.");
        Check(game.rider.gameObject.activeSelf && game.ShopHideRemaining > 1.98f && game.ShopHideRemaining < 2.02f,
            "The abandoned car is being recovered while the countdown runs.");
        var area = (Rect)Call("GameGuiArea");
        bool portrait = Screen.width < Screen.height;
        var pause = typeof(PhoVietGame).GetMethod("ShopPausePixels", BindingFlags.Static | BindingFlags.NonPublic);
        Tap(((Rect)pause.Invoke(null, new object[] { area, portrait })).center);
        Check(game.State == PhoVietGame.RideState.Paused, "The shop pause button is reachable via mobile HUD tap.");
        Tick(60);
        Check(Mathf.Abs(game.ShopHideRemaining - 2) < .02f, "Pause freezes the countdown.");
        game.TogglePause();
        Tick(99);
        Check(game.IsChased && game.rider.gameObject.activeSelf && !game.EvadedPoliceInShop, "The escape cannot complete before 3 seconds.");
        Tick(1);
        Check(game.InsideShop && game.EvadedPoliceInShop && game.ShopHideRemaining == 0 &&
              !game.IsChased && !game.policeBike.gameObject.activeSelf && !game.rider.gameObject.activeSelf,
            "At three seconds both the patrol and the abandoned car are gone.");
        Check(game.Distance == distance, "The hideout and tow do not score distance.");
        float pixelRatio = (float)typeof(PhoVietGame).GetField("browserPixelRatio", Flags).GetValue(game);
        Vector2 exit = PhoVietGame.ExitVehicleLayout(area, portrait, pixelRatio).center;
        var exitChoice = Call("HudButtonAt", new Vector2(exit.x, Screen.height-exit.y));
        Tap(exit);
        Check(!game.InsideShop, $"The mobile exit button returns to the original storefront (choice {exitChoice}, point {exit}, area {area}, screen {Screen.width}x{Screen.height}, state {game.State}).");
        Tick(100);
        Check(!game.IsChased && !game.TryEnterShop() && !game.rider.gameObject.activeSelf, "The chase stays ended while looking for another car.");
        var replacement = TakeDifferentCar();
        Check(!game.IsChased && !game.EvadedPoliceInShop && !game.policeBike.gameObject.activeSelf, "Taking a different car schedules pursuit after hiding.");
        for (int i = 0; i < 149; i++) Call("TickChase", .02f);
        Check(!game.IsChased, "The patrol stays away until the full three seconds elapse.");
        Call("TickChase", .02f);
        Check(game.IsChased && game.PoliceDistance >= 37.9f, "The patrol returns at three seconds.");

        Check(game.TryExitVehicle(), "The player can leave the taken car.");
        Tick(29);
        EnterAtShop(false);
        Tick(150);
        Check(!replacement.model.gameObject.activeInHierarchy && replacement.playerClaimed,
            "The taken car can also be impounded and stays out of traffic until reset.");
        game.RestartRide();
        Check(!game.InsideShop && !game.EvadedPoliceInShop && game.IsChased && game.rider.gameObject.activeSelf &&
              !replacement.playerClaimed, "Restart clears the hideout, tow, and previous car ownership.");
    }

    public static object Main()
    {
        game = UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if (game == null || !Application.isPlaying) throw new InvalidOperationException("Open Phố Việt in Play Mode.");
        float originalScale = Time.timeScale;
        bool river = game.riversEnabled, dinosaur = game.dinosaursEnabled, tank = game.tankBarrageEnabled;
        try
        {
            Time.timeScale = 0;
            game.riversEnabled = game.dinosaursEnabled = game.tankBarrageEnabled = false;
            passed = 0;
            foreach (var path in new[] { "ShopHideout/PV_ShopInterior", "ShopHideout/PV_PoliceTow" })
            {
                var model = Resources.Load<GameObject>(path);
                Check(model != null && model.GetComponentInChildren<MeshRenderer>() != null, $"The Blender model is importable at {path}.");
            }
            VerifySearchWindow();
            VerifyFullLoop();
            return new { passed, seconds = PhoVietGame.ShopHideSeconds };
        }
        finally
        {
            game.riversEnabled = river; game.dinosaursEnabled = dinosaur; game.tankBarrageEnabled = tank;
            Time.timeScale = originalScale;
            game.ResetRide();
        }
    }
}
