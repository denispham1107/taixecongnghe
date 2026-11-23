using UnityEditor;
using UnityEngine;
using PhoViet.OpenWorld;
// 21.338 — nút cảm ứng mẫu A: nhập lại ảnh Resources/TouchControls (xuất ngoài Unity bằng xuat_game.sh) + bật nút cảm ứng để chụp.
//   Reimport (Edit mode) → Play → Show (bắt đầu lượt, hiện nút, bỏ tạm dừng) → capture_game_view source=screen → End
public static class Buttons21338
{
    public static string Reimport()
    {
        foreach (var n in new[] { "Left", "Right", "Missile", "Jump", "Reverse", "Laser", "Gas", "Stop", "Shield" })
            AssetDatabase.ImportAsset("Assets/PhoViet/Resources/TouchControls/PV_Btn_" + n + ".png", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PhoViet/Resources/TouchControls/PV_Btn_Left.png");
        return "ok " + (t ? t.width + "x" + t.height + " " + t.format : "missing");
    }
    public static string Show()
    {
        var g = Object.FindAnyObjectByType<OpenWorldGame>(); var p = Object.FindAnyObjectByType<OpenWorldDriver>();
        OpenWorldWeather.ForceNext = false; g.GoReady(); g.StartRun();
        foreach (var pol in Object.FindObjectsByType<OpenWorldPolice>(FindObjectsInactive.Exclude)) pol.enabled = false;
        p.testTouchUI = true; EditorApplication.isPaused = false;
        return "mode " + g.Mode + " touch " + p.TouchControlsVisible;
    }
    public static void End() { var p = Object.FindAnyObjectByType<OpenWorldDriver>(); if (p) p.testTouchUI = false; }
}
