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
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false){name="PV_SoftParticle",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[64*64];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;float a=Mathf.Pow(Mathf.Clamp01(1-d),1.6f);pixels[y*64+x]=new Color(1,1,1,a);}
            texture.SetPixels(pixels);texture.Apply();
            smoke=Particles(root,"Khói đen",texture,false);fire=Particles(root,"Lửa cháy",texture,true);
            var flames=Group("Ngọn lửa",root);
            foreach(float x in new[]{-.4f,0,.35f})
            {
                var outer=Shape("Lửa cam",flames,PrimitiveType.Capsule,new Vector3(x,.55f,0),new Vector3(.5f,1+(x==0?.35f:0),.5f),Hex("F36B21"));
                outer.GetComponent<Renderer>().sharedMaterial=Mat(Hex("F36B21"),true);
                var core=Sphere("Lõi lửa",flames,new Vector3(x,.28f,-.15f),new Vector3(.35f,.8f,.35f),Gold);
                core.GetComponent<Renderer>().sharedMaterial=Mat(Gold,true);
            }
            var glow=Group("Ánh lửa",root,new Vector3(0,.6f,0)).gameObject.AddComponent<Light>();
            glow.type=LightType.Point;glow.color=Hex("FF8A29");glow.range=5;glow.intensity=1.8f;glow.shadows=LightShadows.None;
            root.gameObject.SetActive(false);return root;
        }
        static ParticleSystem Particles(Transform parent,string name,Texture texture,bool flame)
        {
            var p=Group(name,parent).gameObject.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=p.main;main.loop=true;main.playOnAwake=false;main.duration=5;main.maxParticles=flame?100:160;
            main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.startLifetime=new ParticleSystem.MinMaxCurve(flame?.4f:2.5f,flame?1:4.5f);
            main.startSpeed=new ParticleSystem.MinMaxCurve(flame?1:1.5f,flame?3:2.8f);
            main.startSize=new ParticleSystem.MinMaxCurve(flame?.9f:1.8f,flame?1.6f:2.5f);
            main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            var emission=p.emission;emission.rateOverTime=flame?35:32;
            var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=flame?18:24;shape.radius=flame?.45f:.65f;
            shape.rotation=new Vector3(-90,0,0);
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(flame?Gold:Hex("252D2E"),0),new GradientColorKey(flame?Hex("EC4B20"):Hex("555B59"),1)},new[]{new GradientAlphaKey(.95f,0),new GradientAlphaKey(.7f,.55f),new GradientAlphaKey(0,1)});
            var color=p.colorOverLifetime;color.enabled=true;color.color=gradient;
            var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,flame?1:.5f,1,flame?.2f:2));
            var mat=new Material(Shader.Find("PhoViet/Soft Particle")){name=flame?"PV_Fire":"PV_Smoke",mainTexture=texture};
            mat.renderQueue=3000;
            var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;
            renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position,ParticleSystemVertexStream.Color,ParticleSystemVertexStream.UV});
            return p;
        }
    }
}
