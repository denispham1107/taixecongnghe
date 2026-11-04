using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
namespace PhoViet.Editor
{
 public static class PhoVietMissileTrailSetup
 {
  public static void Configure(GameObject root)
  {
   if(Application.isPlaying)throw new InvalidOperationException("Configure missile FX in Edit Mode");
   var visual=root.GetComponent<MissileFlightVisual>();if(visual==null)throw new InvalidOperationException("MissileFlightVisual missing");
   const string path="Assets/PhoViet/Art/Helicopter/Missile/PV_MissileTrail.mat";
   var shader=Shader.Find("PhoViet/Missile Fire Trail");if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Fire trail shader missing/error");
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
   if(visual.fireTrail==null)
   {var child=new GameObject("Vệt lửa sau đuôi • đường bay");child.transform.SetParent(root.transform,false);visual.fireTrail=child.AddComponent<LineRenderer>();}
   var line=visual.fireTrail;line.sharedMaterial=material;line.useWorldSpace=true;line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
   line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.lightProbeUsage=LightProbeUsage.Off;line.reflectionProbeUsage=ReflectionProbeUsage.Off;
   line.numCapVertices=3;line.numCornerVertices=2;line.widthMultiplier=.6f;line.widthCurve=new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.12f,1),new Keyframe(.65f,.6f),new Keyframe(1,.08f));
   var fire=new Gradient();fire.SetKeys(new[]{new GradientColorKey(new Color(1,.88f,.45f),0),new GradientColorKey(new Color(1,.39f,.04f),.45f),new GradientColorKey(new Color(1,.07f,.01f),1)},new[]{new GradientAlphaKey(.96f,0),new GradientAlphaKey(.8f,.65f),new GradientAlphaKey(0,1)});line.colorGradient=fire;line.positionCount=0;line.enabled=false;
   if(visual.flame!=null)visual.flame.localScale=new Vector3(1.8f,2.2f,1.8f);
   if(visual.smoke!=null)
   {
    var ps=visual.smoke;ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.maxParticles=32;main.startLifetime=.6f;main.startSize=new ParticleSystem.MinMaxCurve(.4f,.55f);
    var emission=ps.emission;emission.rateOverTime=50;var shape=ps.shape;shape.angle=10;shape.radius=.06f;
    var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(.25f,2.6f),new Keyframe(1,5.5f)));
    var color=ps.colorOverLifetime;var smoke=new Gradient();smoke.SetKeys(new[]{new GradientColorKey(new Color(1,.83f,.5f),0),new GradientColorKey(new Color(.86f,.88f,.9f),.18f),new GradientColorKey(new Color(.72f,.75f,.78f),1)},new[]{new GradientAlphaKey(.95f,0),new GradientAlphaKey(.85f,.18f),new GradientAlphaKey(.6f,.65f),new GradientAlphaKey(0,1)});color.enabled=true;color.color=smoke;
   }
  }
 }
}
