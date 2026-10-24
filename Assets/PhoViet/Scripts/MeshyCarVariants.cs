using System;
using UnityEngine;

namespace PhoViet
{
    // One pool/physics root; its geometry, contacts, lights and paint switch together.
    public sealed class MeshyCarVariants : MonoBehaviour
    {
        [Serializable] public sealed class Variant
        {
            public string asset;
            public Transform visual,body,bumperFront,bumperRear;
            public Transform[] wheels,steering;
            public Renderer[] skins;
            public Animation animationPlayer;
            public AnimationClip[] clips;
            public Vector3 size,headlight,taillight;
            public Vector2[] roofProfile;
            public float wheelRadius;
            // Xe tăng: trọng số chọn ngẫu nhiên, không bị xung lực, chạy chậm, giữ một màu gốc.
            public float weight=1,speedScale=1;
            public bool armored,fixedPaint;
        }
        public Variant[] variants;
        public CarVisualAnimator driver;
        public Transform headAnchor;
        public Transform[] frontHalos,tailHalos;
        public int SelectedVariant {get;private set;}
        public int PaintIndex {get;private set;}
        public Vector2[] RoofProfile=>variants[SelectedVariant].roofProfile;
        public float Height=>variants[SelectedVariant].size.y;
        public string ModelAsset=>variants[SelectedVariant].asset;
        static readonly Color[] Paints={new Color(.94f,.96f,1),new Color(.06f,.20f,.48f),new Color(.82f,.08f,.055f),
            new Color(.045f,.55f,.43f),new Color(.92f,.63f,.06f),new Color(.40f,.15f,.65f),new Color(.09f,.10f,.12f),new Color(.94f,.27f,.06f)};
        MaterialPropertyBlock block;
        static readonly int Paint=Shader.PropertyToID("_PaintColor");
        bool prepared;

        public bool Armored=>variants[SelectedVariant].armored;
        public void SelectForSpawn(System.Random random,PhoVietGame.TrafficVehicle vehicle,bool allowArmored=true)
        {
            if(prepared){prepared=false;return;}
            // Chọn theo trọng số; mẫu bọc thép bị loại khi xe phải vượt lên từ phía sau.
            float total=0;foreach(var v in variants)if(allowArmored||!v.armored)total+=Mathf.Max(0,v.weight);
            float pick=(float)random.NextDouble()*total;int index=0;
            for(int i=0;i<variants.Length;i++)
            {
                if(!allowArmored&&variants[i].armored)continue;
                index=i;pick-=Mathf.Max(0,variants[i].weight);if(pick<0)break;
            }
            Select(index,random.Next(Paints.Length),vehicle);
        }
        public void PrepareForSpawn(System.Random random,PhoVietGame.TrafficVehicle vehicle,bool allowArmored=true)
        {if(prepared)return;SelectForSpawn(random,vehicle,allowArmored);prepared=true;}
        public void Select(int index,int paint,PhoVietGame.TrafficVehicle vehicle)
        {
            SelectedVariant=index;PaintIndex=paint;var v=variants[index];
            if(block==null)block=new MaterialPropertyBlock();
            foreach(var variant in variants)variant.visual.gameObject.SetActive(variant==v);
            driver.visual=v.visual;driver.body=v.body;driver.bumperFront=v.bumperFront;driver.bumperRear=v.bumperRear;
            driver.wheels=v.wheels;driver.steering=v.steering;driver.animationPlayer=v.animationPlayer;driver.clips=v.clips;
            driver.wheelRadius=v.wheelRadius;driver.tailX=v.taillight.x;driver.tailY=v.taillight.y;driver.tailZ=v.taillight.z;
            driver.ResetVisualBinding();
            // A pooled visual can still hold an impact pose. Bind lamps only
            // after restoring its rest pose, not relative to a bent bumper.
            if(Application.isPlaying)driver.Initialize();
            // Preserve AssetName's gameplay role; ModelAsset identifies the received shape.
            vehicle.halfWidth=v.size.x*.5f;vehicle.halfLength=v.size.z*.5f;
            vehicle.armored=v.armored;vehicle.speedScale=v.speedScale>0?v.speedScale:1;
            var sign=transform.Find("Biển xe nguy hiểm");if(sign!=null)sign.localPosition=new Vector3(0,v.size.y+.4f,0);
            if(headAnchor!=null)Mount(headAnchor,v.bumperFront,v.headlight);
            for(int i=0;i<frontHalos.Length;i++)Mount(frontHalos[i],v.bumperFront,new Vector3((i==0?-1:1)*v.size.x*.32f,v.headlight.y,v.headlight.z));
            for(int i=0;i<tailHalos.Length;i++)Mount(tailHalos[i],v.bumperRear,new Vector3((i==0?-1:1)*v.taillight.x,v.taillight.y,v.taillight.z));
            if(vehicle.brakeLights!=null)
            {
                for(int i=0;i<vehicle.brakeLights.childCount;i++)vehicle.brakeLights.GetChild(i).localPosition=new Vector3((i==0?-1:1)*v.taillight.x,v.taillight.y,v.taillight.z);
            }
            foreach(var variant in variants)foreach(var skin in variant.skins)
            {skin.GetPropertyBlock(block);block.SetColor(Paint,variant.fixedPaint?Color.white:Paints[paint]);skin.SetPropertyBlock(block);}
        }
        void Mount(Transform anchor,Transform bone,Vector3 local)
        {anchor.SetParent(bone,false);anchor.position=transform.TransformPoint(local);anchor.rotation=transform.rotation;}
        public float RoofAt(float z)
        {
            var points=RoofProfile;
            if(z<=points[0].x)return points[0].y;
            for(int i=1;i<points.Length;i++)if(z<=points[i].x)return Mathf.Lerp(points[i-1].y,points[i].y,Mathf.InverseLerp(points[i-1].x,points[i].x,z));
            return points[points.Length-1].y;
        }
        public float SupportHeight(float a,float b)
        {
            float height=Mathf.Max(RoofAt(a),RoofAt(b));
            foreach(var p in RoofProfile)if(p.x>=Mathf.Min(a,b)&&p.x<=Mathf.Max(a,b))height=Mathf.Max(height,p.y);
            return height;
        }
    }
}
