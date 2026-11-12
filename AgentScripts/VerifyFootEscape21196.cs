using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyFootEscape21196
{
    const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static PhoVietGame game;
    static int passed;

    static object Call(string name, params object[] values) =>
        typeof(PhoVietGame).GetMethod(name, Members).Invoke(game, values);

    static T Read<T>(string name) => (T)typeof(PhoVietGame).GetField(name, Members).GetValue(game);
    static void Set(string name, object value) => typeof(PhoVietGame).GetField(name, Members).SetValue(game, value);

    static void Check(bool result, string message)
    {
        if (!result) throw new InvalidOperationException(message);
        passed++;
    }

    static void Restart()
    {
        game.RestartRide();
        foreach (var vehicle in game.vehicles)
        {
            vehicle.active = false;
            vehicle.model.gameObject.SetActive(false);
        }
        Set("spawnTimer", 999f);
        Set("nextIntersectionDistance", float.PositiveInfinity);
        game.policeBike.localPosition = new Vector3(0, 0, -65);
    }

    static PhoVietGame.TrafficVehicle Spawn(bool car, float z, int lane = 2)
    {
        int index = game.vehicles.FindIndex(v => !v.active && !v.eventOnly && !v.playerClaimed &&
            (car ? v.halfWidth > .7f && v.model.GetComponent<MeshyCarVariants>() != null : v.halfWidth < .7f));
        Check(index >= 0, "Traffic fixture available");
        var vehicle = game.SpawnTraffic(lane, z, index);
        Check(vehicle != null, "Traffic fixture spawned");
        return vehicle;
    }

    static void Tick(string method, int frames)
    {
        for (int i = 0; i < frames; i++) Call(method, .02f);
    }

    static void ExitAndTakeVehicle()
    {
        Restart();
        Check(game.TryExitVehicle() && game.PlayerOnFoot, "Voluntary exit");
        Check(!game.TryThrowFootGrenade(), "Grenade unavailable during the exit animation");
        Tick("TickOnFoot", 12);
        Check(game.FootCharacter.localPosition.y > 1, "Exit jumps outward in an arc");
        Tick("TickOnFoot", 17);
        var foot = game.FootCharacter;
        var skin = foot.GetComponentInChildren<SkinnedMeshRenderer>();
        Check(skin != null && skin.sharedMaterial.mainTexture != null && game.footRunClip.name == "Running",
            "The jail character's original Meshy rig, texture and running clip are loaded");
        Check(foot.localPosition.y < .02f, "Character lands on the street");
        game.TogglePause();
        Call("TickOnFoot", .05f);
        Check(game.State == PhoVietGame.RideState.Paused, "Pause works on foot");
        game.TogglePause();

        var area = (Rect)Call("GameGuiArea");
        var stick = PhoVietGame.FootJoystickLayout(area, Screen.width < Screen.height, Read<float>("browserPixelRatio"));
        SendPointer(43, 0, stick.center - Vector2.up * (stick.width * .51f));
        Check(Read<Vector2>("footStick") == Vector2.zero, "Touches outside the expanded joystick are ignored");
        SendPointer(43, 0, stick.center - Vector2.up * (stick.width * .46f));
        Check(Read<Vector2>("footStick").y > .95f, "The expanded edge can capture a touch");
        SendPointer(43, 2, stick.center);
        SendPointer(41, 0, stick.center);
        SendPointer(41, 1, stick.center + Vector2.right * (stick.width * .34f));
        SendPointer(42, 2, stick.center);
        Check(Read<Vector2>("footStick").x > .95f, "A second finger does not cancel the joystick");
        SendPointer(41, 3, stick.center);
        Check(Read<Vector2>("footStick") == Vector2.zero, "Touch cancellation releases the joystick");

        float distance = game.Distance;
        Vector3 start = foot.localPosition;
        float walkSide = Mathf.Sign(start.x - game.rider.localPosition.x);
        Set("footMove", new Vector2(walkSide, 0));
        Tick("TickOnFoot", 10);
        Set("footMove", Vector2.zero);
        Check((foot.localPosition.x-start.x)*walkSide > .9f && game.Distance == distance, "Walking away from the abandoned car never earns metres");
        var bones = foot.GetChild(0).GetComponentsInChildren<Transform>().Where(b => b.name.StartsWith("mixamorig:")).ToArray();
        Call("PoseFootCharacter", 0f, false);
        var rest = bones.Select(b => b.localRotation).ToArray();
        Set("footRunClock", 0f);
        Call("PoseFootCharacter", .14f, true);
        Check(bones.Where((bone, i) => Quaternion.Angle(bone.localRotation, rest[i]) > 1).Any(), "The rig changes pose during the run cycle");

        var vehicle = Spawn(true, 10);
        vehicle.model.localPosition = new Vector3(PhoVietGame.LaneX[2], 0, 13);
        vehicle.model.localRotation = Quaternion.identity;
        foot.localPosition = vehicle.model.localPosition - Vector3.forward * 5.4f;
        Set("footFacing", Vector3.forward);
        Check(game.TryThrowFootGrenade(), "Throw in the selected direction");
        var grenades = Read<IList>("grenades");
        Check(grenades.Count == 1 && !game.TryThrowFootGrenade(), "One grenade with a shared cooldown");
        Call("TickGrenades", .70f);
        Check(grenades.Count == 0 && game.ActiveCraterCount == 1, "Grenade makes a crater");
        Check(Mathf.Approximately(vehicle.footStopRemaining, 8), "Cars stop for eight seconds");
        Vector3 parked = vehicle.model.localPosition;
        vehicle.speed = vehicle.baseTrafficSpeed = 12;
        Call("TickTrafficMotion", 7.9f);
        Check(vehicle.model.localPosition == parked && vehicle.footStopRemaining > 0, "Traffic remains still up to the deadline");
        Call("TickTrafficMotion", .11f);
        Call("TickTrafficMotion", .1f);
        Check(vehicle.footStopRemaining == 0 && vehicle.model.localPosition != parked, "Traffic resumes after the deadline");

        vehicle.model.localPosition = parked;
        vehicle.model.localRotation = Quaternion.identity;
        Call("DetonateGrenadeAt", parked, true);
        foot.localPosition = parked + Vector3.right * (vehicle.halfWidth + .6f);
        Check(!game.TryTakeVehicle(), "Cannot take the car from the right door");
        foot.localPosition = (Vector3)Call("LeftDoorPosition", vehicle);
        Check(game.TryTakeVehicle(), "Driver door is available");
        Call("TickVehicleEntry", .4f);
        var door = Read<Transform>("footDoor");
        Check(door != null && door.gameObject.activeSelf && door.localEulerAngles.y > 15, "The driver door swings open");
        Check(game.IsChased && Read<float>("policeTakeoverRemaining") == 0, "Opening the door does not start the three-second wait.");
        Call("TickVehicleEntry", .39f);
        Check(game.State == PhoVietGame.RideState.Riding && vehicle.model.parent == game.rider &&
            vehicle.playerClaimed && !vehicle.active && !foot.gameObject.activeSelf, "Drive the exact car taken off the street");
        Check(game.Distance == distance && game.Speed == 0, "Driving resumes from the existing score and zero speed");
        Check(!game.IsChased && !game.policeBike.gameObject.activeSelf &&
              Read<float>("policeTakeoverRemaining") == 3, "Finishing entry suspends an existing pursuit for three seconds.");
        Call("StartChase");
        game.Arrest();
        Check(!game.IsChased && game.State == PhoVietGame.RideState.Riding, "Other pursuit and arrest paths respect the wait.");
        Tick("TickRide", 12);
        Check(game.Distance > distance && !game.IsChased, "Driving earns metres while pursuit is suspended.");
        float remaining = Read<float>("policeTakeoverRemaining");
        game.TogglePause();
        Tick("TickChase", 50);
        Check(!game.IsChased && Read<float>("policeTakeoverRemaining") == remaining, "Pause does not consume the takeover wait.");
        game.TogglePause();
        Tick("TickChase", 137);
        Check(!game.IsChased, "Pursuit remains suspended just before three seconds.");
        Tick("TickChase", 1);
        Check(game.IsChased && game.policeBike.gameObject.activeSelf && game.PoliceDistance == 38,
            "Pursuit starts once at the three-second deadline.");

        var otherCar = Spawn(true, 14);
        Call("BeginRiderImpact", otherCar, Vector2.up, game.rider.localPosition + Vector3.forward, Vector3.forward * 9, false);
        Tick("TickRiderCrash", 149);
        Check(game.State == PhoVietGame.RideState.Falling, "A crash in the taken car allows three seconds");
        Tick("TickRiderCrash", 3);
        Check(game.PlayerOnFoot, "Crash in the taken car forces a new exit");
        Restart();
        Check(!vehicle.playerClaimed && vehicle.model.parent != game.rider, "Restart restores the original car and returns the borrowed model to its pool");
    }

    static void ExitAfterLanding()
    {
        bool narrow = Screen.width < Screen.height;
        var area = (Rect)Call("GameGuiArea");
        var center = PhoVietGame.ExitVehicleLayout(area, narrow, Read<float>("browserPixelRatio")).center;
        Vector2 touch = new Vector2(center.x, Screen.height - center.y);
        int ExitButton() => (int)Call("FootControlAt", touch);
        void StepJump() { Call("TickJump", .02f); Call("ResolveJumpSurface", .02f); }

        Restart();
        Check(ExitButton() == 6 && game.TryToggleFlightButton(), "The exit button is available on the ground before takeoff.");
        Check(game.RiderFlightPending && ExitButton() != 6 && !game.TryExitVehicle(), "Exit is blocked as soon as takeoff begins.");
        for (int i = 0; i < 90 && !game.RiderFlying; i++) StepJump();
        Check(game.RiderFlying && game.RiderJumpHeight > 4 && ExitButton() != 6 && !game.TryExitVehicle(),
            "Touch and keyboard exit are unavailable while hovering.");
        Check(game.TryToggleFlightButton() && !game.RiderFlying && game.RiderAirborne && !game.TryExitVehicle(),
            "Turning off flight does not unlock the exit while descending.");
        for (int i = 0; i < 90 && game.RiderAirborne; i++) StepJump();
        Check(game.RiderJumpHeight == 0 && game.RiderJumpActive && ExitButton() == 6 && game.TryExitVehicle(),
            "The exit unlocks at ground contact, including during the landing pose.");

        Restart();
        Check(game.TryJump() && !game.TryExitVehicle(), "A normal jump also locks the exit.");
        var roof = Spawn(true, 10);
        Call("LandJump", 1.7f, roof);
        Check(game.RiderOnRoof && !game.RiderAirborne && ExitButton() != 6 && !game.TryExitVehicle(),
            "Landing on another vehicle does not count as ground.");

        Restart();
        Check(game.TryToggleFlightButton(), "Flight begins before an airborne collision.");
        for (int i = 0; i < 90 && !game.RiderFlying; i++) StepJump();
        var otherCar = Spawn(true, 14);
        Call("BeginRiderImpact", otherCar, Vector2.up, game.rider.localPosition + Vector3.forward, Vector3.forward * 9, false);
        Set("fallTime", 3f);
        Call("TickRiderCrash", .02f);
        Check(game.State == PhoVietGame.RideState.Falling && game.rider.localPosition.y > 0,
            "Forced exit waits for the road even when the three-second crash timer has elapsed.");
        for (int i = 0; i < 90 && game.State == PhoVietGame.RideState.Falling; i++) Call("TickRiderCrash", .02f);
        Check(game.PlayerOnFoot && game.rider.localPosition.y == 0, "The forced exit occurs when the wreck touches the road.");
    }

    static void SendPointer(int id, int phase, Vector2 pixel)
    {
        string payload = FormattableString.Invariant(
            $"{{\"id\":{id},\"phase\":{phase},\"x\":{pixel.x / Screen.width},\"y\":{pixel.y / Screen.height}}}");
        game.ApplyFootPointer(payload);
    }

    static void ExitBlockedByTraffic()
    {
        foreach (float step in new[] { .02f, .05f })
        {
            Restart();
            var striker = Spawn(true, 12);
            Call("BeginRiderImpact", striker, Vector2.up, game.rider.localPosition + Vector3.forward, Vector3.forward * 9, false);
            Tick("TickRiderCrash", 151);
            Check(game.PlayerOnFoot, "Crash starts the forced exit.");
            striker.active = false;
            striker.model.gameObject.SetActive(false);
            Tick("TickOnFoot", step < .03f ? 27 : 26);
            var landing = Read<Vector3>("footJumpEnd");
            var actor = game.FootCharacter;
            var blocker = Spawn(true, landing.z);
            blocker.model.SetLocalPositionAndRotation(landing, Quaternion.identity);
            blocker.footStopRemaining = 8;
            Check(actor.localPosition.y > .04f && Read<float>("footJumpTime") > 0, "A stationary car blocks the last airborne step.");
            Call("TickOnFoot", step);
            Check(Read<float>("footJumpTime") == 0 && Mathf.Abs(actor.localPosition.y) < .001f,
                $"Ground contact keeps the road height even when traffic blocks the sweep. Actual Y: {actor.localPosition.y}");
            Check((bool)Call("ClearFootLanding", actor.localPosition), "The new landing remains outside the blocking car and wreck.");
            blocker.active = false;
            blocker.model.gameObject.SetActive(false);
            Vector3 onRoad = actor.localPosition;
            Set("footMove", Vector2.up);
            Tick("TickOnFoot", 9);
            Check(Mathf.Abs(actor.localPosition.y) < .001f && FlatDistance(onRoad, actor.localPosition) > .6f,
                "The character walks along the road after the blocked landing.");
            Set("footMove", Vector2.zero);
            actor.localPosition += Vector3.up * 1.2f;
            Call("TickOnFoot", .02f);
            Check(actor.localPosition.y == 0, "An idle street character also recovers from a stale height.");
        }
    }

    static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    static void PursuitAndForcedExit()
    {
        Restart();
        var car = Spawn(true, 12);
        Call("BeginRiderImpact", car, Vector2.up, game.rider.localPosition + Vector3.forward, Vector3.forward * 9, false);
        float distance = game.Distance;
        Tick("TickRiderCrash", 151);
        Check(game.PlayerOnFoot && game.Distance == distance, "Traffic impact forces exit from the original car and freezes score");
        Check(game.FootEscapeRemaining == 10, "Forced exit starts the same ten-second search as a manual exit");
        Tick("TickOnFoot", 29);
        Check(Mathf.Abs(game.FootEscapeRemaining - 9.42f) < .001f, "The exit animation counts toward the ten seconds");
        game.policeBike.localPosition = game.FootCharacter.localPosition;
        game.Arrest();
        Check(game.PlayerOnFoot, "Forced exit gives time to react after landing");
        game.policeBike.localPosition = game.FootCharacter.localPosition - Vector3.forward * 12;
        for (int i = 0; i < 550 && game.PlayerOnFoot; i++) Call("TickOnFoot", .02f);
        Check(game.State == PhoVietGame.RideState.Arrested, $"Police can arrest a pedestrian. Gap: {game.PoliceDistance}");
        Call("TickArrest", 1.11f);
        Check(game.State == PhoVietGame.RideState.Jailed && !game.FootCharacter.gameObject.activeSelf, "Arrest transitions to jail");

        Restart();
        game.Crash();
        Set("riderFallVelocity", Vector3.zero);
        game.policeBike.SetLocalPositionAndRotation(game.rider.localPosition + Vector3.left * 2.3f, Quaternion.identity);
        for (int i = 0; i < 400 && game.State == PhoVietGame.RideState.Falling; i++) Call("TickRiderCrash", .02f);
        Check(game.State == PhoVietGame.RideState.Arrested, $"Police closes the gap after a scenery crash. Gap: {game.PoliceDistance}");

        Restart();
        Check(game.TryExitVehicle(), "Start the blocked-cruiser scenario");
        Tick("TickOnFoot", 29);
        game.FootCharacter.localPosition = new Vector3(1.55f, 0, 6);
        game.policeBike.SetLocalPositionAndRotation(new Vector3(1.55f, 0, -5), Quaternion.identity);
        var stoppedCar = Spawn(true, -2);
        stoppedCar.model.SetLocalPositionAndRotation(new Vector3(1.55f, 0, -2), Quaternion.identity);
        stoppedCar.footStopRemaining = 8;
        for (int i = 0; i < 84; i++)
            Call("TickBlockedPoliceOfficer", game.policeBike.localPosition, game.FootCharacter.localPosition, .02f);
        Check(Read<Transform>("policeOfficer")?.gameObject.activeSelf == true, "A blocked cruiser dispatches an officer on foot");
        Set("footEscapeTime", 0d);
        for (int i = 0; i < 400 && game.PlayerOnFoot; i++) Call("TickChase", .02f);
        Check(game.State == PhoVietGame.RideState.Arrested, "Officer routes around the car and reaches the player");
    }

    public static object Main()
    {
        game = UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if (!Application.isPlaying || game == null) throw new InvalidOperationException("Open Phố Việt in Play Mode.");
        float timeScale = Time.timeScale;
        bool rivers = game.riversEnabled, tanks = game.tankBarrageEnabled, dinosaurs = game.dinosaursEnabled;
        try
        {
            Time.timeScale = 0;
            game.riversEnabled = game.tankBarrageEnabled = game.dinosaursEnabled = false;
            passed = 0;
            foreach (var area in new[] { new Rect(0, 0, 393, 852), new Rect(0, 0, 852, 393), new Rect(0, 0, 1440, 900),
                                         new Rect(0, 47, 390, 759), new Rect(47, 0, 750, 369) })
            {
                bool portrait = area.width < area.height;
                var exit = PhoVietGame.ExitVehicleLayout(area, portrait);
                var gas = PhoVietGame.DrivePedalLayout(area, portrait, false);
                Check(exit.yMax < gas.y && exit.x == gas.x && exit.yMin >= area.yMin, "Exit button fits above accelerator");
                foreach (float ratio in new[] { 1f, 2f })
                {
                    var pixels = new Rect(area.position * ratio, area.size * ratio);
                    var brake = PhoVietGame.DrivePedalLayout(pixels, portrait, true, ratio);
                    var stick = PhoVietGame.FootJoystickLayout(pixels, portrait, ratio);
                    Check(Mathf.Abs(stick.width / (1.55f * brake.width) - 1.20f) < .001f && stick.width == stick.height,
                          "Joystick diameter grows 20 percent");
                    Check(Mathf.Abs((stick.xMin-pixels.xMin)-(pixels.xMax-brake.xMax)) < .001f &&
                          Mathf.Abs(stick.yMax-brake.yMax) < .001f && stick.yMin >= pixels.yMin && stick.xMax < brake.xMin,
                          "Joystick respects safe-area bounds and mirrors the foot actions");
                }
            }
            ExitAndTakeVehicle();
            ExitAfterLanding();
            ExitBlockedByTraffic();
            PursuitAndForcedExit();
            return new { passed, forcedExitSeconds = 3, stopSeconds = 8 };
        }
        finally
        {
            game.riversEnabled = rivers; game.tankBarrageEnabled = tanks; game.dinosaursEnabled = dinosaurs;
            Time.timeScale = timeScale;
            game.ResetRide();
        }
    }
}
