using UnityEngine;
using UnityEditor;
using System.Text;

public static class Weather21317Probe
{
    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (l.type == LightType.Directional) sb.AppendLine($"DirLight {l.name} int={l.intensity} col={l.color} rot={l.transform.eulerAngles} shadows={l.shadows} active={l.isActiveAndEnabled}");
        sb.AppendLine($"ambientMode={RenderSettings.ambientMode} amb={RenderSettings.ambientLight} sky={RenderSettings.ambientSkyColor} eq={RenderSettings.ambientEquatorColor} gnd={RenderSettings.ambientGroundColor} intensity={RenderSettings.ambientIntensity}");
        sb.AppendLine($"skybox={(RenderSettings.skybox ? RenderSettings.skybox.name + " / " + RenderSettings.skybox.shader.name : "none")} sun={(RenderSettings.sun ? RenderSettings.sun.name : "none")}");
        sb.AppendLine($"fog={RenderSettings.fog} mode={RenderSettings.fogMode} col={RenderSettings.fogColor} dens={RenderSettings.fogDensity} start={RenderSettings.fogStartDistance} end={RenderSettings.fogEndDistance}");
        sb.AppendLine($"reflInt={RenderSettings.reflectionIntensity}");
        if (RenderSettings.skybox) { var m = RenderSettings.skybox; foreach (var p in new[] { "_Tint", "_Exposure", "_SkyTint", "_AtmosphereThickness", "_GroundColor", "_SunSize" }) if (m.HasProperty(p)) sb.AppendLine(p + "=" + (m.shader.FindPropertyIndex(p) >= 0 && m.shader.GetPropertyType(m.shader.FindPropertyIndex(p)) == UnityEngine.Rendering.ShaderPropertyType.Color ? m.GetColor(p).ToString() : m.GetFloat(p).ToString())); }
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) sb.AppendLine($"Cam {c.name} clear={c.clearFlags} bg={c.backgroundColor} far={c.farClipPlane} hdr={c.allowHDR}");
        foreach (var r in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (r.parent == null) sb.Append(r.name + "; ");
        return sb.ToString();
    }
}
