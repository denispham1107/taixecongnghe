using UnityEngine;
using UnityEngine.Rendering;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public static class RecklessArt
    {
        public static Transform Exciter(Transform parent)
        {
            Color blue=Hex("246BDD"),dark=Hex("252F34");
            var r=Motorcycle(parent,"Quái xế • Exciter",blue,dark,false);
            foreach(string name in new[]{"Yếm trước","Ốp trắng","Sàn để chân"})
            {var t=r.Find(name);if(t!=null)Object.DestroyImmediate(t.gameObject);}
            Box("Bình xăng thể thao",r,new Vector3(0,1.02f,.28f),new Vector3(.48f,.39f,.7f),blue,new Vector3(-22,0,0));
            Box("Động cơ",r,new Vector3(0,.61f,.15f),new Vector3(.44f,.32f,.6f),Ink);
            foreach(float s in new[]{-1f,1f})
            {
                Box("Ốp gió góc cạnh",r,new Vector3(s*.27f,.85f,.46f),new Vector3(.13f,.36f,.8f),blue,new Vector3(-22,0,s*-12));
                Box("Sọc vàng thể thao",r,new Vector3(s*.345f,.91f,.34f),new Vector3(.025f,.07f,.57f),Gold,new Vector3(-22,0,0));
            }
            Rod("Pô thể thao",r,new Vector3(.4f,.42f,-.9f),new Vector3(.4f,.55f,-.12f),.09f,dark);
            Box("Kính mũ kín mặt",r,new Vector3(0,2.2f,-.005f),new Vector3(.4f,.19f,.075f),Ink);
            Text(r,"Tên xe","EXCITER",new Vector3(0,.88f,-.88f),.07f,Cream);
            return r;
        }
        public static Transform Truck(Transform parent)
        {
            var r=Group("Xe tải cỡ trung • lấn làn",parent);Color paint=Hex("D68B31"),metal=Hex("B4BDBB"),glass=Hex("355963");
            Box("Khung gầm",r,new Vector3(0,.65f,0),new Vector3(2,.3f,6.5f),Ink);
            Box("Thùng kín",r,new Vector3(0,2,-.95f),new Vector3(2.25f,2.25f,4.5f),Cream);
            Box("Nóc thùng",r,new Vector3(0,3.17f,-.95f),new Vector3(2.32f,.12f,4.6f),metal);
            Box("Cabin tải",r,new Vector3(0,1.63f,2.23f),new Vector3(2.18f,2.05f,2.05f),paint);
            Box("Kính chắn gió",r,new Vector3(0,2.1f,3.265f),new Vector3(1.87f,.73f,.04f),glass);
            Box("Lưới tản nhiệt",r,new Vector3(0,1.1f,3.29f),new Vector3(1.1f,.35f,.06f),Ink);
            Box("Cản trước",r,new Vector3(0,.68f,3.32f),new Vector3(2.24f,.21f,.17f),metal);
            Box("Cửa thùng sau",r,new Vector3(0,1.98f,-3.22f),new Vector3(2.1f,2.05f,.06f),metal);
            Text(r,"Vận tải Việt","VẬN TẢI VIỆT",new Vector3(0,2.4f,-3.27f),.19f,Ink);
            foreach(float s in new[]{-1f,1f})
            {
                Box("Kính cửa tải",r,new Vector3(s*1.1f,2.1f,2.2f),new Vector3(.035f,.72f,1.45f),glass);
                Box("Gương tải",r,new Vector3(s*1.34f,2.13f,2.87f),new Vector3(.2f,.43f,.25f),Ink);
                Box("Đèn pha tải",r,new Vector3(s*.8f,.98f,3.3f),new Vector3(.43f,.23f,.05f),Gold);
                Box("Sọc phản quang",r,new Vector3(s*1.135f,1.2f,-.95f),new Vector3(.025f,.13f,4.3f),paint);
                foreach(float z in new[]{-2.05f,2.12f})
                {
                    Shape("Lốp tải",r,PrimitiveType.Cylinder,new Vector3(s*1.05f,.53f,z),new Vector3(1.05f,.18f,1.05f),Ink,new Vector3(0,0,90));
                    Shape("Mâm tải",r,PrimitiveType.Cylinder,new Vector3(s*1.23f,.53f,z),new Vector3(.57f,.025f,.57f),metal,new Vector3(0,0,90));
                }
                for(int i=0;i<6;i++)Box("Gân thùng",r,new Vector3(s*1.14f,2,-2.9f+i*.75f),new Vector3(.055f,2.1f,.055f),metal);
            }
            return r;
        }
        public static Transform CrashFx(Transform parent,out ParticleSystem smoke,out ParticleSystem fire)
        {
            var root=Group("Khói lửa • xe đâm nhà",parent);
            var smokeTexture=Resources.Load<Texture2D>("Wreck/PV_WreckSmoke");var fireTexture=Resources.Load<Texture2D>("Wreck/PV_WreckFlame");
            if(smokeTexture==null||fireTexture==null)throw new System.InvalidOperationException("Missing Blender Mantaflow wreck flipbooks");
            smoke=Particles(root,"Khói đen • Blender",smokeTexture,false);fire=Particles(root,"Lửa cháy • Blender",fireTexture,true);
            smoke.transform.localPosition=Vector3.up*.8f;
            fire.transform.localPosition=Vector3.up*.4f;
            var glow=Group("Ánh lửa",root,new Vector3(0,.6f,0)).gameObject.AddComponent<Light>();
            glow.type=LightType.Point;glow.color=Hex("FF8A29");glow.range=5;glow.intensity=1.8f;glow.shadows=LightShadows.None;
            root.gameObject.AddComponent<WreckFireVisual>().glow=glow;
            root.gameObject.SetActive(false);return root;
        }
        static ParticleSystem Particles(Transform parent,string name,Texture texture,bool flame)
        {
            var p=Group(name,parent).gameObject.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=p.main;main.loop=true;main.playOnAwake=false;main.duration=5;main.maxParticles=flame?100:160;
            main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=flame?48:40;
            main.startLifetime=new ParticleSystem.MinMaxCurve(flame?.45f:2.4f,flame?.8f:3.6f);
            main.startSpeed=new ParticleSystem.MinMaxCurve(flame?.4f:1.3f,flame?.9f:1.8f);
            main.startSize=new ParticleSystem.MinMaxCurve(flame?1.3f:1.2f,flame?1.9f:1.7f);
            main.startRotation=new ParticleSystem.MinMaxCurve(-.12f,.12f);
            var emission=p.emission;emission.rateOverTime=flame?16:12;
            var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=flame?8:16;shape.radius=flame?.32f:.45f;
            shape.rotation=new Vector3(-90,0,0);
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(flame?Hex("FFAE69"):Hex("9B9B98"),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(flame?.85f:.8f,.15f),new GradientAlphaKey(flame?.7f:.6f,.55f),new GradientAlphaKey(0,1)});
            var color=p.colorOverLifetime;color.enabled=true;color.color=gradient;
            var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,flame?.9f:.7f,1,flame?1.15f:2.1f));
            var sheet=p.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=4;sheet.numTilesY=2;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,1));sheet.startFrame=new ParticleSystem.MinMaxCurve(0,.875f);sheet.cycleCount=1;
            var noise=p.noise;noise.enabled=true;noise.strength=flame?.12f:.3f;noise.frequency=.8f;noise.scrollSpeed=.5f;noise.quality=ParticleSystemNoiseQuality.Low;
            var mat=new Material(Shader.Find(flame?"PhoViet/Wreck Flame":"PhoViet/Soft Particle")){name=flame?"PV_MantaflowFire":"PV_MantaflowSmoke",mainTexture=texture};
            mat.renderQueue=3000;
            var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;
            renderer.renderMode=ParticleSystemRenderMode.VerticalBillboard;renderer.sortMode=ParticleSystemSortMode.Distance;
            renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
            return p;
        }
    }
}
