using System.Collections.Generic;
using UnityEngine;
using static PhoViet.StreetArt;

namespace PhoViet
{
    public static class VietnamStreet
    {
        public const float Length=80;
        public const float IntersectionCenter=70;
        public static System.Action<Transform,float,float,int> ShopFactory;
        public static System.Action<Transform,float,float> TreeFactory;
        public static System.Action<Transform,float> PavementFactory;
        public static readonly string[] Streets={"Trần Hưng Đạo","Quang Trung","Hai Bà Trưng","Lê Lợi","Lý Thường Kiệt","Lý Nam Đế","Đinh Bộ Lĩnh"};
        public static readonly string[] Shops={"QUÁN PHỞ","HỦ TIẾU","MÌ GÕ","TẠP HÓA CÔ BA","BÁCH HÓA AN TÂM","SỬA XE TẤN PHÁT","CÀ PHÊ VỈA HÈ","BÁNH MÌ SÀI GÒN","CƠM TẤM","TIỆM SỬA XE HAI LÚA","BÚN BÒ HUẾ","TẠP HÓA MINH CHÂU"};
        static readonly string[] Subs={"BÒ TÁI • NẠM • GẦU   /   TỪ 35K","NƯỚC LÈO THANH • SỢI DAI NGON","NÓNG HỔI MỖI NGÀY • KÍNH MỜI","GẠO • SỮA • BÁNH KẸO • GIA VỊ","ĐỦ ĐẦY MỖI NGÀY • GIÁ BÌNH DÂN","VÁ VỎ • THAY NHỚT • BẢO DƯỠNG","CÀ PHÊ SỮA ĐÁ • TRÀ TẮC","GIÒN THƠM • ĐẬM VỊ VIỆT","SƯỜN • BÌ • CHẢ • TRỨNG","HONDA • YAMAHA • SỬA CHỮA","ĐẬM ĐÀ HƯƠNG VỊ CỐ ĐÔ","MỞ CỬA TỪ 6:00 ĐẾN 22:00"};
        static readonly Color[] Facades={Hex("E7C48C"),Hex("A6C9C2"),Hex("E4AA94"),Hex("DBD5BB"),Hex("A3B9CD"),Hex("D7BC7F"),Hex("B5C4A0")};
        static readonly Color[] SignColors={Hex("A9382E"),Hex("196E72"),Hex("B74B31"),Hex("276563"),Hex("326A8C"),Hex("274A69"),Hex("684B39")};
        public static Transform Build(Transform parent,int index,List<Renderer> lights)
        {
            var r=Group("Khu phố "+(index+1)+" • "+Streets[index%7],parent,new Vector3(0,0,(index-1)*Length));
            Color asphalt=Hex("555D5D"),sidewalk=Hex("CABCA2"),edge=Hex("E7D8B9");
            Box("Mặt đường",r,new Vector3(0,-.15f,40),new Vector3(14,.3f,80),asphalt);
            // A real cross street interrupts sidewalks and center markings every 80 m.
            Box("Đường giao nhau",r,new Vector3(0,-.16f,IntersectionCenter),new Vector3(85,.3f,14),asphalt);
            for(int j=0;j<16;j++)
            {
                float z=2+j*4;
                if(z>61)continue;
                Box("Vạch vàng đứt nét giữa đường",r,new Vector3(0,.012f,z),new Vector3(.17f,.018f,2.25f),Gold);
                foreach(float x in new[]{-3.1f,3.1f}) Box("Phân làn trắng",r,new Vector3(x,.009f,z),new Vector3(.09f,.014f,1.9f),Cream);
            }
            foreach(float side in new[]{-1f,1f})
            {
                if(PavementFactory!=null)PavementFactory(r,side);
                else
                {
                Box("Vỉa hè",r,new Vector3(side*9.1f,.09f,31.5f),new Vector3(4.2f,.28f,63),sidewalk);
                Box("Bó vỉa",r,new Vector3(side*7.1f,.15f,31.5f),new Vector3(.22f,.34f,63),edge);
                Box("Vạch biên",r,new Vector3(side*6.72f,.01f,31),new Vector3(.11f,.015f,61),Cream);
                for(int t=0;t<32;t++)
                {
                    Box("Mạch gạch vỉa hè",r,new Vector3(side*9.1f,.238f,t*2),new Vector3(3.8f,.012f,.025f),Hex("AF9F86"));
                    if(t%2==0) Box("Gạch màu",r,new Vector3(side*7.8f,.242f,t*2+.6f),new Vector3(.75f,.01f,.75f),Hex("B99B7B"),new Vector3(0,45,0));
                }
                }
                if(PavementFactory!=null)Box("Vạch biên",r,new Vector3(side*6.72f,.01f,31),new Vector3(.11f,.015f,61),Cream);
                for(int b=0;b<7;b++) Shop(r,side,4+b*8.4f,index*7+b+(side>0?3:0));
                for(int t=0;t<3;t++) {if(TreeFactory!=null)TreeFactory(r,side*8.2f,10+t*20);else Tree(r,side*8.2f,10+t*20);Person(r,new Vector3(side*8.15f,.26f,15+t*18),SignColors[(index+t)%7],side*65);}
                // Parked scooters, tables and a pavement food cart.
                var parked=Motorcycle(r,"Xe máy đậu trên vỉa hè",Teal,Hex("426367"),false); parked.localPosition=new Vector3(side*9.2f,.27f,21);parked.localEulerAngles=new Vector3(0,side*50,0);parked.localScale=Vector3.one*.86f;
                Cart(r,new Vector3(side*8.5f,.24f,43),side);
                Lamp(r,side*7.6f,28,side);
                Signal(r,side*7.65f,61,index,lights);
                Signal(r,side*7.65f,78,index+1,lights);
                StreetName(r,side*8.15f,61.8f,Streets[index%7]);
                // Electrical wires and concrete poles, familiar to Vietnamese neighbourhoods.
                Rod("Cột điện",r,new Vector3(side*10.1f,.3f,56),new Vector3(side*10.1f,8.8f,56),.12f,Hex("A6ABA1"));
                for(int wire=0;wire<3;wire++)
                    Rod("Dây điện",r,new Vector3(side*10.1f+wire*.13f,8.2f,0),new Vector3(side*10.1f+wire*.13f,8.2f,80),.018f,Ink);
            }
            foreach(float z in new[]{61f,78f}) for(int k=0;k<11;k++)
                Box("Vạch sang đường",r,new Vector3(-6.1f+k*1.22f,.02f,z),new Vector3(.65f,.02f,2.1f),Cream);
            foreach(float x in new[]{-12f,12f}) for(int k=0;k<7;k++)
                Box("Vạch qua đường nhánh",r,new Vector3(x,.02f,64.5f+k*1.7f),new Vector3(2.1f,.02f,.75f),Cream);
            JunctionStreetArt.AddArrows(r);return r;
        }
        static void Shop(Transform p,float s,float z,int seed)
        {
            if(ShopFactory!=null){ShopFactory(p,s,z,seed);return;}
            int i=seed%Shops.Length; float height=7+(seed%4)*2.1f;
            Color facade=Facades[seed%Facades.Length],sign=SignColors[i%7];
            var r=Group(Shops[i],p,new Vector3(s*13.2f,0,z));
            Box("Nhà ống",r,new Vector3(0,height*.5f,0),new Vector3(7.7f,height,7.95f),facade);
            Box("Mái bằng",r,new Vector3(0,height+.12f,0),new Vector3(8,.24f,8.15f),Cream);
            Box("Nền cửa tiệm",r,new Vector3(-s*3.9f,1.5f,0),new Vector3(.055f,2.8f,6.8f),Ink);
            for(int floor=0;floor<(height-3)/2.8f;floor++)
            {
                float y=4.7f+floor*2.65f;
                for(int w=0;w<2;w++)
                {
                    float wz=-1.85f+w*3.4f;
                    Box("Khung cửa",r,new Vector3(-s*3.9f,y,wz),new Vector3(.13f,1.65f,1.65f),Cream);
                    Box("Cửa sổ xanh",r,new Vector3(-s*3.99f,y,wz),new Vector3(.07f,1.42f,1.4f),Hex("507478"));
                    Box("Chia cửa",r,new Vector3(-s*4.05f,y,wz),new Vector3(.07f,1.48f,.055f),Cream);
                    if((seed+floor)%2==0)
                    {
                        Box("Ban công",r,new Vector3(-s*4.25f,y-.96f,wz),new Vector3(.9f,.16f,2),Cream);
                        for(int rail=0;rail<5;rail++) Rod("Lan can",r,new Vector3(-s*4.65f,y-.9f,wz-.85f+rail*.42f),new Vector3(-s*4.65f,y-.23f,wz-.85f+rail*.42f),.025f,Ink);
                        Rod("Tay vịn",r,new Vector3(-s*4.65f,y-.23f,wz-.9f),new Vector3(-s*4.65f,y-.23f,wz+.9f),.04f,Ink);
                    }
                }
                if(floor==0 && seed%3==0)Box("Máy lạnh",r,new Vector3(-s*4.15f,y+1.08f,2.8f),new Vector3(.45f,.52f,.85f),Cream);
            }
            // Sloped striped awnings over an open front.
            for(int a=0;a<10;a++) Box("Mái hiên sọc",r,new Vector3(-s*4.35f,2.9f,-3.35f+a*.74f),new Vector3(1.65f,.1f,.71f),a%2==0?sign:Cream,new Vector3(0,0,s*10));
            Sign(r,Shops[i],Subs[i],new Vector3(-s*4.05f,3.72f,0),sign,s>0?65:-65,6.6f);
            // Merchandise / repair workshop / bowls and stools give each storefront its own use.
            if(i==3||i==4||i==11)
            {
                for(int shelf=0;shelf<3;shelf++)
                {
                    Box("Kệ tạp hóa",r,new Vector3(-s*4.03f,.55f+shelf*.58f,1),new Vector3(.5f,.09f,4.6f),Cream);
                    for(int item=0;item<7;item++)Box("Hàng hóa",r,new Vector3(-s*4.12f,.78f+shelf*.58f,-1+item*.65f),new Vector3(.32f,.36f,.37f),SignColors[(item+shelf)%7]);
                }
            }
            else if(i==5||i==9)
            {
                for(int tire=0;tire<3;tire++)Shape("Lốp sửa xe",r,PrimitiveType.Cylinder,new Vector3(-s*4.15f,.45f,1.5f+tire*.75f),new Vector3(.8f,.13f,.8f),Ink,new Vector3(90,0,0));
                Box("Tủ dụng cụ",r,new Vector3(-s*4.1f,.7f,-1.7f),new Vector3(.6f,1.1f,1.25f),Red);
            }
            else
            {
                Box("Quầy món ăn",r,new Vector3(-s*4.15f,.85f,1.3f),new Vector3(.7f,1.25f,2),Cream);
                Shape("Nồi nước lèo",r,PrimitiveType.Cylinder,new Vector3(-s*4.15f,1.65f,1.3f),new Vector3(.56f,.22f,.56f),Hex("B7C4C1"));
                for(int t=0;t<2;t++)
                {
                    float tz=-1.7f+t*2.5f;
                    Box("Bàn inox",r,new Vector3(-s*4.9f,.87f,tz),new Vector3(1.1f,.08f,1.3f),Hex("CCD5CB"));
                    Rod("Chân bàn",r,new Vector3(-s*4.9f,.28f,tz),new Vector3(-s*4.9f,.86f,tz),.065f,Ink);
                    Shape("Tô phở",r,PrimitiveType.Cylinder,new Vector3(-s*4.9f,.96f,tz),new Vector3(.26f,.06f,.26f),Cream);
                    Box("Ghế nhựa",r,new Vector3(-s*5.65f,.49f,tz),new Vector3(.4f,.43f,.4f),Red);
                }
            }
            if(seed%2==0)
            {
                var flag=Group("Cờ Việt Nam",r,new Vector3(-s*4.95f,5.2f,-3.1f));flag.localEulerAngles=new Vector3(0,s>0?45:-45,0);
                Rod("Cán cờ",flag,new Vector3(-.65f,-.8f,0),new Vector3(-.65f,.7f,0),.025f,Ink);
                Box("Cờ đỏ",flag,Vector3.zero,new Vector3(1.3f,.87f,.025f),Red);
                Star(flag,new Vector3(0,0,-.018f),.26f);Star(flag,new Vector3(0,0,.018f),.26f,true);
            }
            // Rooftop water tanks and a taller second row build a believable skyline.
            if(seed%3==0)Shape("Bồn nước inox",r,PrimitiveType.Cylinder,new Vector3(1,height+1,0),new Vector3(1.2f,.8f,1.2f),Hex("ACBFBB"));
            Box("Nhà phía sau",r,new Vector3(s*8,7+(seed%3),1),new Vector3(7,14+(seed%3)*2,7),Facades[(seed+2)%7]);
        }
        static void StreetName(Transform p,float x,float z,string street)
        {
            var r=Group("Bảng tên đường • "+street,p,new Vector3(x,0,z));
            Rod("Cột tên đường",r,new Vector3(0,.2f,0),new Vector3(0,4.3f,0),.045f,Ink);
            Box("Viền bảng",r,new Vector3(0,4,0),new Vector3(3.9f,.78f,.12f),Cream);
            Box("Bảng xanh",r,new Vector3(0,4,-.07f),new Vector3(3.79f,.67f,.025f),Hex("245C91"));
            Text(r,"ĐƯỜNG","ĐƯỜNG",new Vector3(0,4.2f,-.1f),.095f,Cream);
            Text(r,street,street.ToUpperInvariant(),new Vector3(0,3.94f,-.1f),.215f,Cream);
        }
        static void Signal(Transform p,float x,float z,int group,List<Renderer> list)
        {
            var r=Group("Đèn giao thông",p,new Vector3(x,0,z));
            Rod("Trụ",r,new Vector3(0,.1f,0),new Vector3(0,4.9f,0),.075f,Ink);
            Rod("Cần đèn",r,new Vector3(0,4.9f,0),new Vector3(-Mathf.Sign(x)*1.5f,4.9f,0),.065f,Ink);
            var head=Group("Hộp đèn",r,new Vector3(-Mathf.Sign(x)*1.5f,4.3f,0));
            Box("Hộp đèn đen",head,Vector3.zero,new Vector3(.49f,1.45f,.4f),Ink);
            for(int i=0;i<3;i++)
            {
                var bulb=Sphere("Tín hiệu "+i,head,new Vector3(0,.43f-i*.43f,-.23f),new Vector3(.3f,.3f,.11f),i==2?Hex("72E0A6"):Hex("374543"));
                bulb.GetComponent<Renderer>().sharedMaterial=Mat(i==2?Hex("72E0A6"):Hex("374543"),true);list.Add(bulb.GetComponent<Renderer>());
                Box("Chụp đèn",head,new Vector3(0,.59f-i*.43f,-.3f),new Vector3(.43f,.07f,.34f),Ink);
            }
        }
        static void Lamp(Transform p,float x,float z,float s)
        {
            Rod("Cột đèn đường",p,new Vector3(x,.2f,z),new Vector3(x,6.8f,z),.07f,Ink);
            Rod("Cần đèn đường",p,new Vector3(x,6.8f,z),new Vector3(x-s*1.5f,6.8f,z),.06f,Ink);
            Box("Đèn đường",p,new Vector3(x-s*1.5f,6.72f,z),new Vector3(.8f,.16f,.38f),Cream);
        }
        static void Cart(Transform p,Vector3 pos,float s)
        {
            var r=Group("Xe bánh mì",p,pos);
            Box("Tủ xe",r,new Vector3(0,.8f,0),new Vector3(.85f,1.15f,1.5f),Teal);
            Box("Mái xe",r,new Vector3(0,2,0),new Vector3(1.2f,.14f,1.9f),Red);
            foreach(float z in new[]{-.65f,.65f})Rod("Khung tủ",r,new Vector3(0,1.3f,z),new Vector3(0,2,z),.035f,Cream);
            Sign(r,"BÁNH MÌ","NÓNG GIÒN • 20K",new Vector3(0,1.65f,-.8f),Red,0,1.7f);
        }
    }
}
