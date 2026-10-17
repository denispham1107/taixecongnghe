using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public static class PursuitArt
    {
        public static Transform Police(Transform parent)
        {
            Color khaki=Hex("D9BB7B"),helmet=Hex("E7CF8D");
            var r=Motorcycle(parent,"Mô tô Cảnh sát giao thông",Cream,khaki,false);
            foreach(var renderer in r.GetComponentsInChildren<Renderer>())
            {
                string n=renderer.name;
                if(n=="Quần"||n=="Ống quần")renderer.sharedMaterial=Mat(khaki);
                if(n=="Giày")renderer.sharedMaterial=Mat(Ink);
                if(n=="Mũ bảo hiểm")renderer.sharedMaterial=Mat(helmet);
                if(n=="Sọc mũ")renderer.sharedMaterial=Mat(Teal);
            }
            Box("Đai mũ CSGT",r,new Vector3(0,2.22f,-.46f),new Vector3(.4f,.11f,.025f),Cream);
            Text(r,"Chữ CSGT trên mũ","CSGT",new Vector3(0,2.22f,-.48f),.075f,Teal);
            foreach(float s in new[]{-1f,1f})
            {
                Box("Cầu vai đỏ",r,new Vector3(s*.26f,1.96f,-.25f),new Vector3(.18f,.045f,.3f),Red);
                Box("Viền cầu vai vàng",r,new Vector3(s*.26f,1.988f,-.25f),new Vector3(.13f,.01f,.22f),Gold);
                Box("Túi ngực",r,new Vector3(s*.17f,1.67f,-.044f),new Vector3(.19f,.19f,.03f),Hex("C5A66A"));
                Sphere("Găng tay trắng",r,new Vector3(s*.45f,1.46f,.61f),new Vector3(.16f,.14f,.16f),Cream);
                Box("Thùng mô tô tuần tra",r,new Vector3(s*.48f,.96f,-.65f),new Vector3(.36f,.49f,.64f),Cream);
            }
            Box("Thắt lưng công vụ",r,new Vector3(0,1.25f,-.29f),new Vector3(.67f,.1f,.43f),Ink);
            Box("Khóa thắt lưng",r,new Vector3(0,1.25f,-.52f),new Vector3(.12f,.09f,.025f),Gold);
            Box("Bảng CSGT sau áo",r,new Vector3(0,1.66f,-.49f),new Vector3(.53f,.2f,.025f),khaki);
            Text(r,"Tên lực lượng","C.S.G.T",new Vector3(0,1.66f,-.512f),.105f,Ink);
            Box("Thùng sau",r,new Vector3(0,1.14f,-1.12f),new Vector3(.84f,.56f,.55f),Cream);
            Text(r,"Chữ sau mô tô","C.S.G.T",new Vector3(0,1.2f,-1.405f),.13f,Teal);
            Box("Kính chắn gió",r,new Vector3(0,1.78f,.82f),new Vector3(.72f,.49f,.055f),Hex("A9D0CF"),new Vector3(-15,0,0));
            Box("Ốp mô tô lớn",r,new Vector3(0,.94f,.64f),new Vector3(.9f,.51f,.53f),Cream);
            Sphere("Đèn ưu tiên đỏ",r,new Vector3(-.47f,1.63f,.75f),new Vector3(.2f,.18f,.19f),Red).GetComponent<Renderer>().sharedMaterial=Mat(Hex("FF423D"),true);
            Sphere("Đèn ưu tiên xanh",r,new Vector3(.47f,1.63f,.75f),new Vector3(.2f,.18f,.19f),Teal).GetComponent<Renderer>().sharedMaterial=Mat(Hex("4CBAFF"),true);
            r.gameObject.SetActive(false);return r;
        }
        public static Transform Jail(Transform parent)
        {
            var r=Group("Buồng giam • cảnh kết thúc",parent,new Vector3(300,0,0));
            Color wall=Hex("8D9C94"),floor=Hex("65756E"),steel=Hex("394F51"),white=Hex("E3E2CE"),black=Hex("303D3F"),skin=Hex("D99A6B");
            Box("Sàn buồng giam",r,new Vector3(0,-.14f,1),new Vector3(8,.28f,7),floor);
            Box("Tường sau",r,new Vector3(0,2.2f,3.9f),new Vector3(8,4.4f,.3f),wall);
            Box("Tường trái",r,new Vector3(-4,2.2f,1),new Vector3(.3f,4.4f,6),wall);
            Box("Tường phải",r,new Vector3(4,2.2f,1),new Vector3(.3f,4.4f,6),wall);
            Box("Trần buồng giam",r,new Vector3(0,4.45f,1),new Vector3(8.2f,.2f,6.3f),wall);
            Box("Hành lang nhà giam",r,new Vector3(0,-.35f,0),new Vector3(30,.3f,30),Hex("596C65"));
            for(int i=0;i<9;i++)Box("Mạch sàn",r,new Vector3(-4+i,.008f,1),new Vector3(.018f,.012f,6.5f),Hex("7B8980"));
            for(int i=0;i<7;i++)Box("Mạch gạch",r,new Vector3(0,.01f,-2+i),new Vector3(7.8f,.012f,.018f),Hex("7B8980"));
            Box("Cửa sổ tối",r,new Vector3(-1,3,3.72f),new Vector3(2.2f,1.3f,.06f),Hex("263F42"));
            for(int i=0;i<5;i++)Rod("Song sắt cửa sổ",r,new Vector3(-1.8f+i*.4f,2.38f,3.65f),new Vector3(-1.8f+i*.4f,3.62f,3.65f),.035f,steel);
            Box("Bệ nằm bê tông",r,new Vector3(-1.1f,.28f,1.7f),new Vector3(3.2f,.56f,2.5f),Hex("A0ABA0"));
            Box("Chiếu",r,new Vector3(-1.1f,.58f,1.7f),new Vector3(3,.07f,2.3f),Hex("B9A879"));
            Box("Gối",r,new Vector3(-2,.69f,2.2f),new Vector3(.7f,.17f,.5f),Hex("8C9D86"));
            var man=Group("Người chơi mặc áo tù ngồi trong buồng giam",r,new Vector3(-.85f,0,.9f));
            Box("Áo tù sọc đen trắng",man,new Vector3(0,1.24f,0),new Vector3(.74f,.85f,.45f),white);
            for(int i=0;i<4;i++)
            {
                float x=-.29f+i*.19f;
                Box("Sọc áo trước",man,new Vector3(x,1.24f,-.237f),new Vector3(.095f,.85f,.02f),black);
                Box("Sọc áo sau",man,new Vector3(x,1.24f,.237f),new Vector3(.095f,.85f,.02f),black);
            }
            foreach(float s in new[]{-1f,1f})
            {
                Vector3 a=new Vector3(s*.4f,1.53f,0),b=new Vector3(s*.43f,1.02f,-.35f);
                for(int stripe=0;stripe<5;stripe++)Rod("Tay áo sọc",man,Vector3.Lerp(a,b,stripe/5f),Vector3.Lerp(a,b,(stripe+1)/5f),.115f,stripe%2==0?white:black);
                Sphere("Bàn tay trên đầu gối",man,new Vector3(s*.38f,.96f,-.48f),new Vector3(.19f,.13f,.23f),skin);
                Rod("Đùi ngồi",man,new Vector3(s*.2f,.85f,.02f),new Vector3(s*.3f,.81f,-.57f),.16f,white);
                for(int stripe=0;stripe<5;stripe++)Rod("Quần sọc",man,new Vector3(s*.3f,.8f-stripe*.13f,-.57f),new Vector3(s*.3f,.8f-(stripe+1)*.13f,-.57f),.14f,stripe%2==0?white:black);
                Box("Dép",man,new Vector3(s*.3f,.09f,-.68f),new Vector3(.29f,.09f,.43f),black);
            }
            Rod("Cổ",man,new Vector3(0,1.65f,0),new Vector3(0,1.8f,-.035f),.12f,skin);
            Sphere("Đầu nhân vật",man,new Vector3(0,1.96f,-.05f),new Vector3(.48f,.52f,.46f),skin);
            Sphere("Tóc",man,new Vector3(0,2.13f,-.025f),new Vector3(.49f,.24f,.47f),black);
            foreach(float s in new[]{-1f,1f})Box("Mắt",man,new Vector3(s*.095f,2.0f,-.268f),new Vector3(.065f,.035f,.02f),black);
            Box("Miệng",man,new Vector3(0,1.84f,-.266f),new Vector3(.1f,.022f,.018f),Hex("8A5347"));
            Box("Biển buồng giam",r,new Vector3(1.9f,3.2f,3.71f),new Vector3(1.4f,.65f,.05f),Cream);
            Text(r,"Tên buồng","BUỒNG GIAM",new Vector3(1.9f,3.2f,3.67f),.16f,Ink);
            for(int i=0;i<13;i++)Rod("Song sắt nhà tù",r,new Vector3(-3.9f+i*.65f,0,-1.7f),new Vector3(-3.9f+i*.65f,4.35f,-1.7f),.042f,steel);
            foreach(float y in new[]{.15f,2.8f,4.25f})Box("Giằng cửa sắt",r,new Vector3(0,y,-1.7f),new Vector3(8,.09f,.12f),steel);
            Box("Khóa buồng giam",r,new Vector3(2.55f,1.7f,-1.77f),new Vector3(.21f,.28f,.1f),Hex("B39A54"));
            var lamp=Group("Đèn buồng giam",r,new Vector3(0,3.8f,.6f));var light=lamp.gameObject.AddComponent<Light>();light.type=LightType.Point;light.color=Hex("FFE2B5");light.intensity=1.6f;light.range=8;
            r.gameObject.SetActive(false);return r;
        }
    }
}
