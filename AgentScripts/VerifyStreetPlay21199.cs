using System;
using System.Linq;
using System.Reflection;
using PhoViet;
using UnityEngine;

public static class VerifyStreetPlay21199
{
    const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static PhoVietGame game;
    static int passed;
    static object Call(string name, params object[] args) => typeof(PhoVietGame).GetMethod(name, Members).Invoke(game, args);
    static T Read<T>(string name) => (T)typeof(PhoVietGame).GetField(name, Members).GetValue(game);
    static void Set(string name, object value) => typeof(PhoVietGame).GetField(name, Members).SetValue(game, value);
    static void Check(bool condition, string error)
    {
        if (!condition) throw new InvalidOperationException(error);
        passed++;
    }

    static void Restart()
    {
        game.RestartRide();
        foreach (var v in game.vehicles)
        {
            v.active = false;
            v.model.gameObject.SetActive(false);
        }
        Set("spawnTimer", 999f);
        game.policeBike.localPosition = new Vector3(0, 0, -85);
    }

    static PhoVietGame.TrafficVehicle Spawn(bool car, int lane, float z, float speed)
    {
        int index = game.vehicles.FindIndex(v => !v.active && !v.eventOnly &&
            (car ? v.halfWidth > .7f && v.model.GetComponent<MeshyCarVariants>() != null : v.halfWidth < .7f));
        var v = game.SpawnTraffic(lane, z, index);
        if (v == null) throw new InvalidOperationException("No test vehicle available.");
        Place(v, lane, z, speed);
        return v;
    }

    static void Place(PhoVietGame.TrafficVehicle v, int lane, float z, float speed)
    {
        v.model.SetLocalPositionAndRotation(new Vector3(PhoVietGame.LaneX[lane], 0, z), Quaternion.Euler(0, speed < 0 ? 180 : 0, 0));
        v.previousPosition = v.model.localPosition;
        v.speed = v.baseTrafficSpeed = speed;
        v.motionVelocity = Vector3.forward * speed;
    }

    static PhoVietGame.TrafficVehicle SpawnContainer(int lane, float speed)
    {
        int index = game.vehicles.FindIndex(v => !v.active && !v.eventOnly && v.model.TryGetComponent<MeshyCarVariants>(out var variants) &&
            variants.variants.Any(x => x.asset == "PV_MeshyContainer"));
        if (index < 0) throw new InvalidOperationException("No container truck pool entry.");
        var truck = game.SpawnTraffic(lane, 15, index);
        var choices = truck.model.GetComponent<MeshyCarVariants>();
        choices.Select(Array.FindIndex(choices.variants, v => v.asset == "PV_MeshyContainer"), 0, truck);
        Place(truck, lane, 15, speed);
        return truck;
    }

    static void MakeHeadOnContact(PhoVietGame.TrafficVehicle a, PhoVietGame.TrafficVehicle b)
    {
        float gap = a.halfLength + b.halfLength;
        b.previousPosition = a.model.localPosition + Vector3.forward * (gap + .30f);
        b.model.localPosition = a.model.localPosition + Vector3.forward * (gap - .22f);
        a.previousPosition = a.model.localPosition - Vector3.forward * .18f;
    }

    static void VerifyGentleAndContainerContacts()
    {
        foreach (float speed in new[] { 50.9f, 51f })
        {
            Restart();
            var a = Spawn(true, 2, 15, speed / 3.6f);
            var b = Spawn(true, 2, 25, -50.9f / 3.6f);
            MakeHeadOnContact(a, b);
            Call("ResolveVehicleCollisions");
            var fire = Read<ContactFireVisual>("contactFire");
            fire.Render();
            if (speed < 51)
            {
                Check(!a.knocked && !b.knocked && a.grenadeSlowTime > 0 && b.grenadeSlowTime > 0 &&
                      fire.CountFor(a.model) == 0 && fire.CountFor(b.model) == 0,
                    "Two cars driving below 51 km/h keep driving without contact fire, even head-on.");
                Check(b.model.localPosition.z-a.model.localPosition.z >= a.halfLength+b.halfLength,
                    "The gentle response separates the actual footprints.");
            }
            else Check(a.knocked && b.knocked && fire.CountFor(a.model) > 0 && fire.CountFor(b.model) > 0,
                "Reaching 51 km/h on either car restores the ordinary heavy-contact response.");
        }
        foreach (bool armored in new[] { false, true })
        {
            Restart();
            var truck = SpawnContainer(2, 24);
            var other = Spawn(true, 2, 30, -20);
            other.armored = armored;
            MakeHeadOnContact(truck, other);
            Call("ResolveVehicleCollisions");
            var fire = Read<ContactFireVisual>("contactFire");
            fire.Render();
            if (armored) Check(truck.knocked && !other.knocked && fire.CountFor(truck.model) > 0,
                "Container protection does not apply against the tank.");
            else Check(!truck.knocked && truck.grenadeSlowTime > 0 && truck.model.localPosition.y == 0 &&
                       other.knocked && fire.CountFor(truck.model) == 0 && fire.CountFor(other.model) > 0,
                "At high speed the container stays upright and only the other car takes a heavy impact.");
        }

        Restart();
        var patrol = Read<PhoVietGame.TrafficVehicle>("policeBody");
        var civilian = Spawn(true, 2, 20, -12);
        patrol.model.SetLocalPositionAndRotation(new Vector3(PhoVietGame.LaneX[2], 0, 14), Quaternion.identity);
        MakeHeadOnContact(patrol, civilian);
        typeof(PhoVietGame).GetProperty("PoliceSpeed", Members).SetValue(game, 13f);
        Call("ResolvePoliceContact", patrol.previousPosition, Vector3.forward * 13, true);
        Check(game.PoliceSpeed > 0 && game.PoliceSpeed < 13 && !game.PoliceFallen && !civilian.knocked &&
              civilian.grenadeSlowTime > 0, "A low-speed patrol collision slows both cars without interrupting the pursuit.");

        Restart();
        var target = Spawn(true, 2, 8, -13);
        var player = Read<PhoVietGame.TrafficVehicle>("riderBody");
        MakeHeadOnContact(player, target);
        typeof(PhoVietGame).GetProperty("Speed", Members).SetValue(game, 13f);
        Check((bool)Call("ResolveRiderContacts", game.rider.localPosition.x, .02f) && game.State == PhoVietGame.RideState.Riding &&
              game.Speed < 13 && !target.knocked && target.grenadeSlowTime > 0, "The player car uses the same low-speed rule.");

        StartFoot();
        var claimed = SpawnContainer(2, 0);
        Call("TakeVehicle", claimed);
        Set("takeoverGrace", 0f);
        var car = Spawn(true, 2, 25, -22);
        player = Read<PhoVietGame.TrafficVehicle>("riderBody");
        MakeHeadOnContact(player, car);
        typeof(PhoVietGame).GetProperty("Speed", Members).SetValue(game, 24f);
        Check((bool)Call("ResolveRiderContacts", game.rider.localPosition.x, .02f) && game.State == PhoVietGame.RideState.Riding &&
              game.Speed < 24 && car.knocked, "A claimed container retains its protection against a fast ordinary car.");
    }

    static void VerifyTrafficContacts()
    {
        foreach (float leadSpeed in new[] { 7f, -20f })
        {
            Restart();
            Set("timeRiding", 8f);
            var rear = Spawn(true, 2, 15, 23);
            var lead = Spawn(false, 2, leadSpeed > 0 ? 27 : 43, leadSpeed);
            for (int f = 0; f < 100 && !lead.knocked; f++)
            {
                Set("timeRiding", 8f + f * .02f);
                Call("TickTrafficMotion", .02f);
                if (f == 5)
                {
                    Check(Mathf.Abs(rear.model.localPosition.z - (15 + 23 * .12f)) < .01f &&
                          Mathf.Abs(rear.model.localPosition.x - PhoVietGame.LaneX[2]) < .01f &&
                          !rear.changingLane && !lead.changingLane,
                        "Nearby NPCs follow their ordinary speed and lane until contact.");
                }
            }
            var fire = Read<ContactFireVisual>("contactFire");
            fire.Render();
            Check(!rear.knocked && lead.knocked && rear.grenadeSlowTime > 0 && game.TrafficCollisions > 0,
                "The car slows and keeps going after both rear-end and head-on motorcycle impacts.");
            Check(fire.CountFor(rear.model) == 0 && fire.CountFor(lead.model) > 0,
                "Only the motorcycle gets contact fire.");
        }
        Restart();
        var patrol = Read<PhoVietGame.TrafficVehicle>("policeBody");
        var motorcycle = Spawn(false, 2, 10, -13);
        Vector3 from = motorcycle.model.localPosition - Vector3.forward * (patrol.halfLength + motorcycle.halfLength + .4f);
        game.policeBike.SetLocalPositionAndRotation(from + Vector3.forward * .7f, Quaternion.identity);
        patrol.previousPosition = from;
        patrol.previousYaw = 0;
        typeof(PhoVietGame).GetProperty("PoliceSpeed", Members).SetValue(game, 21f);
        Call("ResolvePoliceContact", from, Vector3.forward * 21, true);
        var contactFire = Read<ContactFireVisual>("contactFire");
        contactFire.Render();
        Check(!game.PoliceFallen && game.IsChased && game.PoliceSpeed > 0 && game.PoliceSpeed < 21 && motorcycle.knocked,
            "A motorcycle hit does not put the patrol car into recovery or stop the chase.");
        Check(contactFire.CountFor(patrol.model) == 0 && contactFire.CountFor(motorcycle.model) > 0,
            "Patrol contact fire is attached only to the motorcycle.");
    }

    static void VerifyRiverAtActiveTile()
    {
        game.riversEnabled = true;
        Restart();
        foreach (var row in game.roadRows)
            foreach (var tile in row.tiles)
                tile.GetComponent<RiverBlock>().SetBridge(false);
        var bridge = game.blocks.First(tile => tile.parent.gameObject.activeInHierarchy);
        bridge.GetComponent<RiverBlock>().SetBridge(true);
        var car = Spawn(true, 2, 25, 0);
        var water = bridge.TransformPoint(new Vector3(RiverBlock.DeckHalf + 1.55f, 1, (RiverBlock.Near + RiverBlock.Far) * .5f));
        car.model.localPosition = game.transform.InverseTransformPoint(water);
        Check(game.OverRiver(car.model.localPosition), "The river fixture is tied to the current tile transform.");
        Call("ActivateCollisionBody", car, Vector3.zero, Vector3.zero, Vector2.zero, Vector2.zero, false, true, 6f);
        for (int i = 0; i < 175; i++) Call("TickTrafficMotion", .02f);
        Check(car.riverFalling && car.riverSplashed && car.recovery == null && car.model.localPosition.y < RiverBlock.WaterY,
            "An impacted vehicle sinks during regular traffic updates.");
    }

    static void VerifyWreckEntryAndCameras()
    {
        Restart();
        var camera = Read<Camera>("rideCamera");
        Call("PositionRideCamera", true, .02f);
        float fov = camera.fieldOfView;
        Check(game.TryExitVehicle(), "Manual exit begins.");
        for (int i = 0; i < 29; i++) Call("TickOnFoot", .02f);
        Call("PositionRideCamera", true, .02f);
        Vector3 cameraOffset = camera.transform.localPosition - game.FootCharacter.localPosition;
        Check(Mathf.Abs(fov - camera.fieldOfView) < .001f && Mathf.Abs(cameraOffset.z + 14) < .01f && Mathf.Abs(cameraOffset.y - 7.8f) < .01f,
            "The foot camera matches the driving field of view and chase distance.");

        var door = (Transform)Call("NearbyShop", 96f, true);
        game.FootCharacter.localPosition = (Vector3)Call("ShopApproach", door);
        Check(game.TryEnterShop(), "Enter an authored shop for camera validation.");
        float oldShopFov=Screen.width<Screen.height?87:76;
        float shopZoom=Mathf.Tan(oldShopFov*Mathf.Deg2Rad*.5f)/Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
        Check(Mathf.Abs(shopZoom/.70f-.60f)<.001f && camera.transform.localPosition.y < 3.1f, "Shop projection is 40% smaller than before and the camera stays inside.");

        Restart();
        Check(game.TryExitVehicle(), "Exit again for the wreck test.");
        for (int i = 0; i < 29; i++) Call("TickOnFoot", .02f);
        var car = Spawn(true, 2, 10, 0);
        var striker = Spawn(true, 2, 0, 17);
        striker.model.localPosition = car.model.localPosition;
        Call("ResolveVehicleCollisions");
        Check(car.knocked && car.trafficCrashTakeable && car.footStopRemaining == 0, "A real NPC contact makes a wreck eligible without a grenade.");
        striker.active = false;
        striker.model.gameObject.SetActive(false);
        car.knockVelocity = Vector3.up * 2;
        Check(!(bool)Call("CanTakeVehicle", car), "An airborne/bouncing wreck cannot be entered.");
        car.knockVelocity = car.angularVelocity = Vector3.zero;
        car.model.SetLocalPositionAndRotation(new Vector3(PhoVietGame.LaneX[2], 0, 10), Quaternion.identity);
        Vector3 approach = (Vector3)Call("LeftDoorPosition", car);
        game.FootCharacter.localPosition = approach + Vector3.right * (2 * (car.halfWidth + .54f));
        Check(!game.TryTakeVehicle(), "The passenger side does not enter the wreck.");
        game.FootCharacter.localPosition = approach;
        Check(game.TryTakeVehicle(), "The stopped wreck can be entered at the left door.");
        Call("TickTrafficMotion", .02f);
        Check(car.model.localPosition.z == 10 && car.knocked, "The wreck holds position through entry.");
        Call("TickVehicleEntry", .79f);
        var fire = Read<ContactFireVisual>("contactFire");
        fire.Render();
        Check(game.State == PhoVietGame.RideState.Riding && car.model.parent == game.rider && !car.knocked &&
              car.recovery == null && fire.CountFor(car.model) > 0, "The same wreck is drivable and retains its local contact fire.");

        int oldFire = fire.CountFor(car.model);
        Set("takeoverGrace", 0f);
        var bike = Spawn(false, 2, 0, -20);
        Vector3 p = game.rider.localPosition;
        float gap = bike.halfLength + (float)typeof(PhoVietGame).GetProperty("PlayerHalfLength", Members).GetValue(game);
        bike.previousPosition = p + Vector3.forward * (gap + .5f);
        bike.model.localPosition = p + Vector3.forward * (gap - .3f);
        typeof(PhoVietGame).GetProperty("Speed", Members).SetValue(game, 18f);
        Check((bool)Call("ResolveRiderContacts", p.x, .05f) && game.State == PhoVietGame.RideState.Riding && game.Speed > 0 && game.Speed < 18 && bike.knocked,
            "A hijacked car also deflects without entering a forced exit when it hits a motorcycle.");
        fire.Render();
        Check(fire.CountFor(car.model) == oldFire && fire.CountFor(bike.model) > 0, "The collision does not add fire to the hijacked car.");
    }

    static Transform StartFoot()
    {
        Restart();
        Check(game.TryExitVehicle(), "Exit for the pedestrian fixture.");
        for (int i = 0; i < 29; i++) Call("TickOnFoot", .02f);
        return game.FootCharacter;
    }

    static void VerifyStationaryTrafficAndPose()
    {
        foreach (bool car in new[] { false, true })
        {
            var actor = StartFoot();
            var vehicle = Spawn(car, 2, 12, 0);
            if (car) vehicle.footStopRemaining = 8;
            actor.localPosition = vehicle.model.localPosition - Vector3.forward * (vehicle.halfLength + .85f);
            float startZ = actor.localPosition.z;
            Set("footMove", Vector2.up);
            for (int i = 0; i < 28; i++) Call("TickOnFoot", .02f);
            float bumper = vehicle.model.localPosition.z - vehicle.halfLength;
            Check(actor.localPosition.z > startZ + .4f && Mathf.Abs(actor.localPosition.z - (bumper - .32f)) < .03f &&
                  game.PlayerOnFoot && Read<float>("footFallRemaining") == 0,
                "A stopped vehicle blocks walking at its body without knocking the character down.");
            Check(Read<float>("footRunBlend") == 0, "Holding the walk button against a stationary body still settles to standing.");
            Set("footMove", Vector2.down);
            for (int i = 0; i < 8; i++) Call("TickOnFoot", .02f);
            Check(actor.localPosition.z < startZ && Read<float>("footFallRemaining") == 0,
                "The character can back away from the body immediately.");
        }

        var foot = StartFoot();
        var parked = game.blocks.SelectMany(tile => tile.Cast<Transform>()).First(t =>
            t.name == "Xe máy đậu trên vỉa hè" && t.gameObject.activeInHierarchy &&
            Mathf.Abs(game.transform.InverseTransformPoint(t.position).z - 10) < 18);
        Vector3 parkedPosition = game.transform.InverseTransformPoint(parked.position);
        float side = Mathf.Sign(parkedPosition.x);
        foot.localPosition = new Vector3(parkedPosition.x - side * 1.5f, 0, parkedPosition.z);
        Set("footMove", new Vector2(side, 0));
        for (int i = 0; i < 24; i++) Call("TickOnFoot", .02f);
        Check(Mathf.Abs(foot.localPosition.x - parkedPosition.x) > .40f && Mathf.Abs(foot.localPosition.x) < 9.1f &&
              Read<float>("footFallRemaining") == 0, "Sidewalk display scooters are solid, stationary obstacles too.");

        foot = StartFoot();
        foot.localPosition = new Vector3(0, 0, 9);
        Set("footMove", Vector2.up);
        for (int i = 0; i < 18; i++) Call("TickOnFoot", .02f);
        Check(Mathf.Approximately(Read<float>("footRunBlend"), 1), "Walking uses the authored running pose.");
        Set("footMove", Vector2.zero);
        Call("TickOnFoot", .02f);
        Check(Read<float>("footRunBlend") > 0 && Read<float>("footRunBlend") < 1, "Stopping blends out of the last stride.");
        Vector3 stopped = foot.localPosition;
        for (int i = 0; i < 10; i++) Call("TickOnFoot", .02f);
        var bones = foot.GetComponentsInChildren<Transform>();
        Vector3 Joint(string name) => foot.InverseTransformPoint(bones.Single(b => b.name == "mixamorig:" + name).position);
        Check(foot.localPosition == stopped && Read<float>("footRunBlend") == 0 &&
              Mathf.Abs(Joint("LeftFoot").y - Joint("RightFoot").y) < .08f &&
              Joint("LeftHand").y < Joint("LeftArm").y - .45f && Joint("RightHand").y < Joint("RightArm").y - .45f,
            "At rest both feet are level and the hands hang at the sides.");
    }

    static void VerifyFootRoadAndContacts()
    {
        var camera = Read<Camera>("rideCamera").transform;
        foreach (int side in new[] { -1, 1 })
        {
            var foot = StartFoot();
            foot.localPosition = new Vector3(0, 0, 25.99f);
            Call("PositionRideCamera", true, .02f);
            foot.localPosition += Vector3.forward * .04f;
            Vector3 relative = camera.localPosition - foot.localPosition;
            Quaternion before = camera.localRotation;
            Call("RecenterFootStreet");
            Check((camera.localPosition - foot.localPosition - relative).sqrMagnitude < .00001f && Quaternion.Angle(before, camera.localRotation) < .01f,
                "Scrolling the street under the runner preserves the relative camera position and rotation.");
            int from = Read<int>("mainRow"), branch = Read<int>(side < 0 ? "leftRow" : "rightRow");
            foot.localPosition = new Vector3(side * 8.9f, 0, game.roadRows[branch].root.localPosition.z);
            Call("PositionRideCamera", true, .02f);
            Vector3 oldOffset = camera.localPosition - foot.localPosition;
            Set("footMove", new Vector2(side, 0));
            float distance = game.Distance;
            for (int i = 0; i < 30 && Read<int>("mainRow") == from; i++) Call("TickOnFoot", .02f);
            Check(Read<int>("mainRow") == branch && game.Distance == distance, "The pedestrian can enter either side street without scoring distance.");
            Check(Mathf.Abs((camera.localPosition - foot.localPosition).magnitude - oldOffset.magnitude) < 2.4f,
                "Changing the street frame does not teleport the camera away from the pedestrian.");
        }

        var actor = StartFoot();
        actor.localPosition = new Vector3(PhoVietGame.LaneX[2], 0, 12);
        var car = Spawn(true, 2, 7, 16);
        Set("timeRiding", 8f);
        for (int i = 0; i < 24 && Read<float>("footFallRemaining") <= 0; i++) Call("TickOnFoot", .02f);
        Check(game.PlayerOnFoot && Mathf.Approximately(Read<float>("footFallRemaining"), 1.5f), "Moving traffic knocks the pedestrian down for 1.5 seconds.");
        car.active = false;
        Vector3 impact = actor.localPosition;
        Set("footMove", Vector2.up);
        float peak = 0;
        for (int i = 0; i < 74; i++)
        {
            Call("TickOnFoot", .02f);
            peak = Mathf.Max(peak, actor.localPosition.y);
        }
        Check(actor.localPosition.z > impact.z + 1 && peak > .18f && Read<float>("footFallRemaining") > 0,
            "A nonfatal moving impact still launches the character in the impact direction.");
        Vector3 landed = actor.localPosition;
        Call("TickOnFoot", .02f);
        Call("TickOnFoot", .02f);
        Check(actor.localPosition.z > landed.z && Read<float>("footFallRemaining") == 0, "The pedestrian can move after recovering.");

        actor = StartFoot();
        actor.localPosition = new Vector3(PhoVietGame.LaneX[2], 0, 12);
        var slow = Spawn(true, 2, 8, 4);
        Set("timeRiding", 8f);
        for (int i = 0; i < 50 && Read<float>("footFallRemaining") <= 0; i++) Call("TickOnFoot", .02f);
        Check(Read<float>("footFallRemaining") > 0, "A slow moving car can still knock the pedestrian down.");
        slow.active = false;
        impact = actor.localPosition;
        peak = 0;
        for (int i = 0; i < 30; i++)
        {
            Call("TickOnFoot", .02f);
            peak = Mathf.Max(peak, actor.localPosition.y);
        }
        Check(Vector3.Distance(impact, actor.localPosition) < .25f && peak < .01f, "A light collision falls in place, without a launch.");

        actor = StartFoot();
        actor.localPosition = new Vector3(PhoVietGame.LaneX[2], 0, 12);
        var tank = Spawn(true, 2, 7, 22);
        tank.armored = true;
        Set("timeRiding", 8f);
        for (int i = 0; i < 24 && game.PlayerOnFoot; i++) Call("TickOnFoot", .02f);
        Check(game.State == PhoVietGame.RideState.GameOver && !game.IsChased && Read<string>("footFatalTitle") == "Va vào xe tăng!",
            "Tank contact ends the round without waiting for an arrest.");

        actor = StartFoot();
        var dinosaur = game.SpawnDinosaur();
        Check(dinosaur != null, "A dinosaur is available for the pedestrian collision fixture.");
        dinosaur.root.localPosition = new Vector3(0, 0, 12);
        actor.localPosition = game.transform.InverseTransformPoint(dinosaur.visual.leftFoot.position);
        actor.localPosition = new Vector3(actor.localPosition.x, 0, actor.localPosition.z);
        Call("TickOnFoot", .02f);
        Check(game.State == PhoVietGame.RideState.GameOver && game.HitByDinosaur && !game.IsChased && Read<string>("footFatalTitle") == "Va vào khủng long!",
            "Dinosaur contact ends the round without waiting for an arrest.");
    }

    static void VerifyFootThrow()
    {
        var foot = StartFoot();
        foot.localPosition = new Vector3(0, 0, 12);
        for(int i=0;i<12;i++)Call("TickOnFoot",.02f);
        var arm = foot.GetComponentsInChildren<Transform>().Single(b => b.name == "mixamorig:RightArm");
        Quaternion standing = arm.localRotation;
        Check(game.TryThrowFootGrenade(), "The on-foot input starts the overhand throw.");
        var grenades = Read<System.Collections.IList>("grenades");
        var grenade = grenades[0];
        var grenadeFields = grenade.GetType();
        var model = (Transform)grenadeFields.GetField("model", Members).GetValue(grenade);
        float Age() => (float)grenadeFields.GetField("age", Members).GetValue(grenade);
        for(int i=0;i<6;i++)Call("TickOnFoot",.02f);
        Check(Age()<0 && Quaternion.Angle(arm.localRotation, standing)>35 &&
              (model.localPosition-(Vector3)Call("FootGrenadeHandPosition")).sqrMagnitude<.0001f,
            "The right arm winds up with the object held in the hand.");
        for(int i=0;i<6;i++)Call("TickOnFoot",.02f);
        Check(Age()>0 && (model.localPosition-(Vector3)Call("FootGrenadeHandPosition")).sqrMagnitude>.0025f,
            "The object leaves the right hand at the release point.");
        for(int i=0;i<25;i++)Call("TickOnFoot",.02f);
        Check(grenades.Count==0 && Quaternion.Angle(arm.localRotation, standing)<.01f, "The throw follows through and returns to standing.");
    }

    static void VerifyFootHazardsAndPickups()
    {
        var foot=StartFoot();
        var camera=Read<Camera>("rideCamera");
        var icons=game.roadRows.SelectMany(row=>row.tiles).Where(t=>t.gameObject.activeInHierarchy)
            .SelectMany(t=>t.Cast<Transform>()).Where(t=>t.name.StartsWith("Nạp tên lửa")&&t.gameObject.activeInHierarchy).Take(6).ToArray();
        Check(icons.Length>=2,"Active road tiles have rocket pickups.");
        foreach(var icon in icons)icon.rotation=Quaternion.identity;
        camera.transform.position=foot.position+new Vector3(10,8,14);
        int collected=game.MissilePickupsCollected;
        Call("PoseMissilePickups",.02f);
        foreach(var icon in icons)
            Check(Vector3.Dot(icon.forward,(camera.transform.position-icon.position).normalized)>.9999f,"Rocket front faces the foot camera.");
        camera.transform.position=foot.position+new Vector3(-10,8,-14);
        Call("PoseMissilePickups",.02f);
        foreach(var icon in icons)
            Check(Vector3.Dot(icon.forward,(camera.transform.position-icon.position).normalized)>.9999f,"Rocket front follows the camera after reversing sides.");
        Check(game.MissilePickupsCollected==collected,"Foot visuals do not collect rocket inventory.");

        foot=StartFoot();
        foot.localPosition=new Vector3(1.55f,0,12);
        game.policeBike.gameObject.SetActive(false);
        Call("StopPursuitForFatalImpact");
        Set("shopEvaded",true);
        Set("tankSpawnTimer",.01f);Set("dinosaurTimer",.01f);
        Call("TickOnFoot",.02f);
        var tank=game.vehicles.Single(v=>v.active&&v.armored);
        Check(game.TanksScheduled==1&&game.DinosaursSpawned==1&&tank.model.localPosition.z-foot.localPosition.z<57,
            "Both hazards spawn on the pedestrian's street, ahead of the pedestrian.");
        Check(tank.previousPosition==tank.model.localPosition,"The new tank does not sweep across the pedestrian on spawn.");
        game.TogglePause();
        float tankClock=game.TankSpawnCountdown,dinoClock=game.DinosaurCountdown;
        Call("TickOnFoot",.05f);
        Check(game.TankSpawnCountdown==tankClock&&game.DinosaurCountdown==dinoClock,"Pause freezes both spawn clocks.");
        game.TogglePause();
        float score=game.Distance;
        for(int i=0;i<215&&game.PlayerOnFoot;i++)
        {
            foot.localPosition=new Vector3(8.98f,0,12);
            Call("TickOnFoot",.05f);
        }
        Check(game.TanksScheduled>=2&&game.DinosaursSpawned>=2&&game.Distance==score,"The time-based hazards repeat without adding walking metres.");
        Check(game.ShellsFired>0&&game.PlayerOnFoot,"Tanks automatically fire on the street and the sidewalk stays beyond the blast.");

        foreach(bool direct in new[]{false,true})
        {
            foot=StartFoot();
            foot.localPosition=new Vector3(1.55f,0,10);
            Set("footFrameStart",foot.localPosition);
            var gun=game.SpawnScheduledTank();
            Check(gun!=null,"A tank can fire on the pedestrian's street.");
            var target=foot.localPosition+(direct?Vector3.zero:Vector3.right*1.6f);
            var shell=(PhoVietGame.TankShell)Call("LaunchTankShell",gun,2,target,false);
            if(direct)
            {
                shell.start=foot.localPosition+new Vector3(-5,1,0);shell.target=foot.localPosition+new Vector3(5,1,0);
                shell.flight=1;shell.age=.35f;shell.apex=0;
                Call("TickShells",.30f);
            }
            else Call("TickShells",PhoVietGame.ShellFlight+.01f);
            Check(game.State==PhoVietGame.RideState.GameOver&&game.KilledByShell&&!game.IsChased&&
                Read<string>("footFatalTitle")=="Trúng đạn xe tăng!","A shell hit or blast ends the pedestrian's round without arrest.");
        }

        foot=StartFoot();
        var door=(Transform)Call("NearbyShop",96f,true);
        var outside=(Vector3)Call("ShopApproach",door);
        foot.localPosition=outside;
        var outsideTank=game.SpawnScheduledTank();
        Check(game.TryEnterShop(),"Pedestrian enters shelter before the shell arrives.");
        var shelteredShell=(PhoVietGame.TankShell)Call("LaunchTankShell",outsideTank,2,outside,false);
        Call("TickShells",shelteredShell.flight+.01f);
        Check(game.InsideShop&&game.PlayerOnFoot&&!game.KilledByShell,"A blast at the entrance cannot hit someone inside.");
    }

    static void VerifyFatalFootSpeed()
    {
        foreach(bool car in new[]{false,true})
        foreach(float kph in new[]{59.9f,60f,60.1f,88f})
        {
            var actor=StartFoot();
            actor.localPosition=new Vector3(PhoVietGame.LaneX[2],0,12);
            Set("footFrameStart",actor.localPosition);
            float speed=kph/3.6f;
            var v=Spawn(car,2,8,speed);
            v.motionVelocity=Vector3.forward*speed;
            v.model.localPosition=actor.localPosition-Vector3.forward*(v.halfLength+.15f);
            v.previousPosition=v.model.localPosition-Vector3.forward*speed*.02f;
            Call("ResolveFootTrafficContacts",game.policeBike.localPosition,.02f);
            Check(Read<float>("footFallRemaining")>0,"Vehicle contact starts a fall at "+kph+" km/h.");
            bool fatal=kph>60;
            Check((Read<string>("footFatalTitle")!=null)==fatal,"Only traffic over 60 km/h queues a fatal fall.");
            if(!fatal)continue;
            Check(game.PlayerOnFoot&&!game.IsChased,"A high-speed impact starts the fall and stops the chase before showing the result.");
            v.active=false;
            for(int i=0;i<65&&game.PlayerOnFoot;i++)Call("TickOnFoot",.02f);
            Check(game.State==PhoVietGame.RideState.GameOver&&actor.localPosition.y<.01f,"The character lands before the fatal result.");
        }
        var foot=StartFoot();
        foot.localPosition=new Vector3(1.55f,0,10);
        Set("footFrameStart",foot.localPosition);
        game.policeBike.SetLocalPositionAndRotation(foot.localPosition-Vector3.forward*1.95f,Quaternion.identity);
        Call("ResolveFootTrafficContacts",game.policeBike.localPosition-Vector3.forward*.40f,.02f);
        Check(Read<string>("footFatalTitle")!=null&&!game.IsChased,"A patrol car hitting at 72 km/h also starts a fatal fall.");
    }

    static void VerifyFootAircraftContact()
    {
        foreach(float height in new[]{6f,1f})
        {
            var foot=StartFoot();
            foot.localPosition=new Vector3(1.55f,0,10);
            Set("footFrameStart",foot.localPosition);
            var aircraft=Read<System.Collections.IList>("helicopters")[0];
            var kind=aircraft.GetType();
            var root=(Transform)kind.GetField("root",Members).GetValue(aircraft);
            root.SetLocalPositionAndRotation(foot.localPosition+new Vector3(0,height,.5f),Quaternion.identity);
            kind.GetField("previousContact",Members).SetValue(aircraft,root.localPosition+Vector3.forward*5);
            kind.GetField("contactVelocity",Members).SetValue(aircraft,Vector3.back*18);
            Call("ResolveAircraftFootContact",aircraft);
            Check((Read<string>("footFatalTitle")!=null)==(height<2),"Fast aircraft hits only when its physical height overlaps the character.");
        }
    }

    public static object Main()
    {
        game = UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
        if (game == null || !Application.isPlaying) throw new InvalidOperationException("Play Mode required.");
        float time = Time.timeScale;
        bool rivers = game.riversEnabled, surprises = game.trafficSurprises;
        try
        {
            Time.timeScale = 0;
            game.riversEnabled = game.trafficSurprises = false;
            passed = 0;
            VerifyTrafficContacts();
            VerifyGentleAndContainerContacts();
            VerifyWreckEntryAndCameras();
            VerifyStationaryTrafficAndPose();
            VerifyFootRoadAndContacts();
            VerifyFootThrow();
            VerifyFootHazardsAndPickups();
            VerifyFatalFootSpeed();
            VerifyFootAircraftContact();
            VerifyRiverAtActiveTile();
            return new { passed };
        }
        finally
        {
            game.riversEnabled = rivers;
            game.trafficSurprises = surprises;
            Time.timeScale = time;
            game.ResetRide();
        }
    }
}
