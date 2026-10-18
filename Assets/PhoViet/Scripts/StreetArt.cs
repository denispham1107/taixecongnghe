using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet
{
    // Original low-poly geometry. Everything is built in metres with the road along +Z.
    public static class StreetArt
    {
        public static Font Font;
        public static System.Func<Transform,string,Transform> ParkedBikeFactory;
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        public static readonly Color Ink = Hex("203C43"), Cream = Hex("FFF1CF"), Red = Hex("D83833"), Teal = Hex("177E80"), Gold = Hex("FFD35A");
        public static Color Hex(string s) { ColorUtility.TryParseHtmlString("#" + s, out Color c); return c; }
        public static Material Mat(Color c, bool flat = false)
        {
            string key = ColorUtility.ToHtmlStringRGBA(c) + flat;
            if (materials.TryGetValue(key, out Material m) && m != null) return m;
            m = new Material(Shader.Find(flat ? "Unlit/Color" : "Standard"));
            m.name = "PV_" + key; m.color = c;
            if (!flat) { m.SetFloat("_Glossiness", .12f); m.SetFloat("_Metallic", 0); }
            materials[key] = m; return m;
        }
        public static Transform Group(string name, Transform parent, Vector3 position = default)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = position; return t;
        }
        public static Transform Shape(string name, Transform parent, PrimitiveType kind, Vector3 p, Vector3 size, Color c, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(kind); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = p; go.transform.localScale = size; go.transform.localEulerAngles = euler;
            var col = go.GetComponent<Collider>(); if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
            go.GetComponent<Renderer>().sharedMaterial = Mat(c); return go.transform;
        }
        public static Transform Box(string name, Transform p, Vector3 pos, Vector3 size, Color c, Vector3 e = default) => Shape(name, p, PrimitiveType.Cube, pos, size, c, e);
        public static Transform Sphere(string name, Transform p, Vector3 pos, Vector3 size, Color c) => Shape(name, p, PrimitiveType.Sphere, pos, size, c);
        public static Transform Rod(string name, Transform p, Vector3 a, Vector3 b, float radius, Color c)
        {
            var t = Shape(name, p, PrimitiveType.Cylinder, (a+b)*.5f, new Vector3(radius*2, Vector3.Distance(a,b)*.5f, radius*2), c);
            t.localRotation = Quaternion.FromToRotation(Vector3.up, b-a); return t;
        }
        public static void Text(Transform p, string name, string value, Vector3 pos, float size, Color color, Vector3 rotation = default, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var t = Group(name,p,pos); t.localEulerAngles = rotation;
            var mesh = t.gameObject.AddComponent<TextMesh>(); mesh.text=value; mesh.font=Font; mesh.fontSize=64;
            // TextMesh world scale also multiplies by fontSize / 10.
            mesh.characterSize=size*10f/64f; mesh.anchor=anchor; mesh.alignment=TextAlignment.Center; mesh.color=color;
            if (Font != null) t.GetComponent<MeshRenderer>().sharedMaterial=Font.material;
        }
        public static void Star(Transform parent, Vector3 p, float radius, bool back = false)
        {
            var t=Group("Sao vàng • đủ 5 cánh", parent,p); if(back)t.localEulerAngles=new Vector3(0,180,0);
            var vertices=new Vector3[11]; var triangles=new int[30]; vertices[0]=Vector3.zero;
            for(int i=0;i<10;i++) { float a=(90+i*36)*Mathf.Deg2Rad; float r=i%2==0?radius:radius*.382f; vertices[i+1]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0); }
            for(int i=0;i<10;i++){ triangles[i*3]=0;triangles[i*3+1]=(i+1)%10+1;triangles[i*3+2]=i+1; }
            var mesh=new Mesh {name="FivePointVietnamStar"}; mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=Mat(Gold,true);
        }
        public static Transform Motorcycle(Transform parent, string name, Color paint, Color shirt, bool hero)
        {
            if(!hero&&name=="Xe máy đậu trên vỉa hè"&&ParkedBikeFactory!=null)return ParkedBikeFactory(parent,name);
            var root=Group(name,parent); Color rubber=Hex("252F34"), chrome=Hex("A6B5B6"), skin=Hex("D99A6B");
            foreach(float z in new[]{-.76f,.81f})
            {
                Shape("Lốp xe",root,PrimitiveType.Cylinder,new Vector3(0,.39f,z),new Vector3(.74f,.13f,.74f),rubber,new Vector3(0,0,90));
                Shape("Vành hợp kim",root,PrimitiveType.Cylinder,new Vector3(0,.39f,z),new Vector3(.44f,.14f,.44f),chrome,new Vector3(0,0,90));
                Rod("Trục bánh",root,new Vector3(-.17f,.4f,z),new Vector3(.17f,.4f,z),.08f,Ink);
                Box("Chắn bùn",root,new Vector3(0,.76f,z),new Vector3(.36f,.12f,.59f),paint);
            }
            Box("Thân xe",root,new Vector3(0,.76f,-.23f),new Vector3(.62f,.42f,1.15f),paint);
            Box("Sàn để chân",root,new Vector3(0,.56f,.26f),new Vector3(.68f,.1f,.6f),chrome);
            Box("Yên da",root,new Vector3(0,1.015f,-.35f),new Vector3(.62f,.16f,.98f),rubber);
            Box("Yếm trước",root,new Vector3(0,.97f,.65f),new Vector3(.69f,.76f,.31f),paint,new Vector3(-17,0,0));
            Box("Ốp trắng",root,new Vector3(0,1.04f,.83f),new Vector3(.3f,.53f,.055f),Cream,new Vector3(-17,0,0));
            Box("Đầu xe",root,new Vector3(0,1.45f,.73f),new Vector3(.78f,.27f,.31f),paint);
            Box("Đèn pha",root,new Vector3(0,1.46f,.902f),new Vector3(.42f,.14f,.028f),Hex("FFF8DC"));
            Box("Đèn hậu",root,new Vector3(0,.84f,-.84f),new Vector3(.4f,.14f,.045f),Red);
            Box("Biển số",root,new Vector3(0,.6f,-.95f),new Vector3(.35f,.26f,.035f),Cream);
            Text(root,"Biển số xe","59 • VN",new Vector3(0,.6f,-.98f),.045f,Ink);
            Rod("Tay lái",root,new Vector3(-.52f,1.45f,.66f),new Vector3(.52f,1.45f,.66f),.045f,rubber);
            foreach(float s in new[]{-1f,1f})
            {
                Rod("Gương",root,new Vector3(s*.41f,1.47f,.72f),new Vector3(s*.57f,1.84f,.71f),.025f,chrome);
                Sphere("Mặt gương",root,new Vector3(s*.57f,1.86f,.71f),new Vector3(.22f,.14f,.085f),chrome);
                Vector3 hip=new Vector3(s*.2f,1.14f,-.27f);
                var leg=hero?Group(s<0?"Chân đạp trái":"Chân đạp phải",root,hip):root;
                Vector3 offset=hero?hip:Vector3.zero;
                Rod("Quần",leg,hip-offset,new Vector3(s*.34f,.78f,.12f)-offset,.13f,Hex("263E55"));
                Rod("Ống quần",leg,new Vector3(s*.34f,.78f,.12f)-offset,new Vector3(s*.31f,.61f,.38f)-offset,.115f,Hex("263E55"));
                Box("Giày",leg,new Vector3(s*.34f,.58f,.43f)-offset,new Vector3(.22f,.15f,.38f),Cream);
                Rod("Tay áo",root,new Vector3(s*.27f,1.81f,-.21f),new Vector3(s*.43f,1.57f,.12f),.12f,shirt);
                Rod("Cánh tay",root,new Vector3(s*.43f,1.57f,.12f),new Vector3(s*.45f,1.46f,.6f),.073f,skin);
            }
            Box("Áo tài xế",root,new Vector3(0,1.59f,-.27f),new Vector3(.65f,.76f,.4f),shirt,new Vector3(7,0,0));
            Sphere("Vai áo",root,new Vector3(0,1.85f,-.24f),new Vector3(.72f,.28f,.45f),shirt);
            Rod("Cổ",root,new Vector3(0,1.9f,-.25f),new Vector3(0,2.02f,-.24f),.105f,skin);
            Sphere("Đầu",root,new Vector3(0,2.1f,-.2f),new Vector3(.41f,.47f,.4f),skin);
            Sphere("Mũ bảo hiểm",root,new Vector3(0,2.24f,-.23f),new Vector3(.49f,.38f,.47f),hero?Cream:paint);
            Box("Sọc mũ",root,new Vector3(0,2.387f,-.23f),new Vector3(.08f,.06f,.29f),hero?Red:Cream);
            Box("Quai mũ",root,new Vector3(0,2.06f,-.405f),new Vector3(.4f,.045f,.025f),rubber);
            if(hero)
            {
                Star(root,new Vector3(0,1.65f,-.485f),.235f);
                Star(root,new Vector3(0,1.65f,-.055f),.20f,true);
            }
            return root;
        }
        public static Transform Car(Transform parent, Color paint, bool taxi, bool van)
        {
            var r=Group(taxi?"Taxi phố Việt":van?"Xe tải giao hàng":"Ô tô",parent);
            Color glass=Hex("375D69"); float length=van?4.8f:4.1f;
            Box("Thân xe",r,new Vector3(0,.8f,0),new Vector3(1.82f,.64f,length),paint);
            Box("Cabin",r,new Vector3(0,1.38f,-.25f),new Vector3(1.63f,.72f,2.25f),paint);
            Box("Kính sau",r,new Vector3(0,1.45f,-1.39f),new Vector3(1.4f,.48f,.035f),glass);
            Box("Kính trước",r,new Vector3(0,1.45f,.89f),new Vector3(1.4f,.48f,.035f),glass);
            foreach(float x in new[]{-.92f,.92f})
            {
                Box("Kính cửa",r,new Vector3(x*.9f,1.45f,-.25f),new Vector3(.035f,.44f,1.98f),glass);
                Box("Cột cửa",r,new Vector3(x*.92f,1.45f,-.25f),new Vector3(.05f,.51f,.085f),paint);
                foreach(float z in new[]{-1.3f,1.3f})
                    Shape("Bánh xe",r,PrimitiveType.Cylinder,new Vector3(x,.42f,z),new Vector3(.79f,.1f,.79f),Ink,new Vector3(0,0,90));
            }
            foreach(float x in new[]{-.62f,.62f})
            { Box("Đèn trước",r,new Vector3(x,.84f,length*.5f+.015f),new Vector3(.44f,.2f,.035f),Cream);Box("Đèn sau",r,new Vector3(x,.88f,-length*.5f-.015f),new Vector3(.4f,.19f,.035f),Red); }
            Box("Cản xe",r,new Vector3(0,.51f,-length*.5f-.04f),new Vector3(1.74f,.13f,.1f),Cream);
            if(taxi) { Box("Hộp đèn TAXI",r,new Vector3(0,1.88f,-.17f),new Vector3(.65f,.24f,.36f),Gold);Text(r,"Taxi","TAXI",new Vector3(0,1.89f,-.37f),.09f,Ink); }
            if(van) { Box("Thùng hàng",r,new Vector3(0,1.65f,-1.15f),new Vector3(1.84f,1.25f,2.15f),Cream);Text(r,"Giao hàng","GIAO HÀNG",new Vector3(0,1.8f,-2.24f),.16f,Teal); }
            return r;
        }
        public static void Person(Transform p, Vector3 pos, Color shirt, float turn=0)
        {
            var r=Group("Người đi bộ",p,pos);r.localEulerAngles=new Vector3(0,turn,0);
            Sphere("Đầu",r,new Vector3(0,1.56f,0),new Vector3(.29f,.33f,.29f),Hex("D99A6B"));
            Box("Áo",r,new Vector3(0,1.15f,0),new Vector3(.43f,.53f,.24f),shirt);
            foreach(float s in new[]{-1f,1f}) {Rod("Chân",r,new Vector3(.12f*s,.91f,0),new Vector3(.15f*s,.12f,.08f*s),.085f,Ink);Rod("Tay",r,new Vector3(.25f*s,1.36f,0),new Vector3(.3f*s,.88f,0),.06f,Hex("D99A6B"));}
            Shape("Nón lá",r,PrimitiveType.Cylinder,new Vector3(0,1.76f,0),new Vector3(.52f,.04f,.52f),Cream);
        }
        public static void Tree(Transform p, float x, float z)
        {
            var r=Group("Cây me ven đường",p,new Vector3(x,0,z));
            Box("Bồn cây",r,new Vector3(0,.18f,0),new Vector3(1.3f,.35f,1.3f),Hex("C6B18E"));
            Rod("Thân cây",r,new Vector3(0,.3f,0),new Vector3(.1f,3.8f,0),.13f,Hex("776850"));
            Rod("Cành",r,new Vector3(0,2.7f,0),new Vector3(-.8f,4.1f,0),.08f,Hex("776850"));
            Sphere("Tán lá",r,new Vector3(.1f,4.3f,0),new Vector3(2.8f,2.5f,2.6f),Hex("508B66"));
            Sphere("Tán lá sáng",r,new Vector3(-.8f,4.5f,-.2f),new Vector3(1.9f,1.8f,1.9f),Hex("73A879"));
        }
        public static void Sign(Transform p,string title,string sub,Vector3 pos,Color bg,float yaw,float width=5)
        {
            var r=Group("Biển hiệu • "+title,p,pos);r.localEulerAngles=new Vector3(0,yaw,0);
            Box("Viền biển",r,Vector3.zero,new Vector3(width+ .12f,1.12f,.15f),Cream);
            Box("Nền biển",r,new Vector3(0,0,-.09f),new Vector3(width,1,.03f),bg);
            Text(r,"Tên quán",title,new Vector3(0,.12f,-.12f),.40f,Cream);
            Text(r,"Dòng phụ",sub,new Vector3(0,-.31f,-.12f),.115f,Cream);
        }
    }
}
