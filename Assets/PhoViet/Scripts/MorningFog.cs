using System;
using UnityEngine;

namespace PhoViet
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class MorningFog : MonoBehaviour
    {
        [Serializable] sealed class Profile
        {
            public float durationSeconds=6,density=.065f,heightFalloff=.24f,maximumDistance=100;
            public string color="E1ECF1";
            public int gridSize=32,atlasColumns=8,atlasRows=4,tileSize=34;
            public float[] period={72,12,144};public float[] fade;
        }
        public DayNightLighting lighting;
        public TextAsset authoredProfile;
        public Texture2D densityAtlas;
        public Shader volumeShader;
        public float Strength {get;private set;}
        public float ElapsedSeconds {get;private set;}
        public float Duration=>profile.durationSeconds;
        public Matrix4x4 WorldToField=>referenceFromLocal*Matrix4x4.Translate(Vector3.forward*(RoadDistance-referenceDistance))*Root.worldToLocalMatrix;
        Transform Root=>lighting!=null?lighting.transform:transform;
        float RoadDistance=>lighting!=null&&lighting.game!=null?lighting.game.Distance:0;
        Profile profile=new Profile();Camera view;Material volume;bool ready,ownedDepth;
        Matrix4x4 referenceFromLocal=Matrix4x4.identity;float referenceDistance;
        readonly Vector3[] corners=new Vector3[4];

        void Prepare()
        {
            if(ready)return;
            view=GetComponent<Camera>();
            if(authoredProfile!=null)profile=JsonUtility.FromJson<Profile>(authoredProfile.text);
            if(profile.fade==null||profile.fade.Length<2)profile.fade=new[]{1f,0f};
            if(volumeShader!=null){volume=new Material(volumeShader){hideFlags=HideFlags.HideAndDontSave};volume.SetTexture("_DensityAtlas",densityAtlas);}
            ready=true;
        }
        void OnEnable()
        {
            Prepare();ownedDepth=(view.depthTextureMode&DepthTextureMode.Depth)==0;
            view.depthTextureMode|=DepthTextureMode.Depth;
        }
        void OnDisable(){if(view!=null&&ownedDepth)view.depthTextureMode&=~DepthTextureMode.Depth;ownedDepth=false;}
        public void ResetFrame(){referenceFromLocal=Matrix4x4.identity;referenceDistance=RoadDistance;}
        public void Rebase(Quaternion inverse,Vector3 origin)
        {
            referenceFromLocal=referenceFromLocal*Matrix4x4.Translate(Vector3.forward*(RoadDistance-referenceDistance))*Matrix4x4.TRS(origin,Quaternion.Inverse(inverse),Vector3.one);
            referenceDistance=RoadDistance;
        }
        public void SetTime(float seconds)
        {
            Prepare();ElapsedSeconds=Mathf.Max(0,seconds);
            float at=Mathf.Clamp01(ElapsedSeconds/profile.durationSeconds)*(profile.fade.Length-1);
            int first=Mathf.Min(profile.fade.Length-1,Mathf.FloorToInt(at));
            Strength=ElapsedSeconds>=profile.durationSeconds?0:Mathf.Lerp(profile.fade[first],profile.fade[Mathf.Min(first+1,profile.fade.Length-1)],at-first);
            bool visible=Strength>0&&volume!=null&&lighting!=null&&lighting.game!=null&&lighting.game.State!=PhoVietGame.RideState.Jailed;
            if(enabled!=visible)enabled=visible;
        }
        public void Hide(){Strength=0;enabled=false;}
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(volume==null||Strength<=0){Graphics.Blit(source,destination);return;}
            // Positive camera-local Z from CalculateFrustumCorners matches linear eye depth.
            view.CalculateFrustumCorners(new Rect(0,0,1,1),1,Camera.MonoOrStereoscopicEye.Mono,corners);
            volume.SetVector("_RayBL",transform.TransformVector(corners[0]));volume.SetVector("_RayTL",transform.TransformVector(corners[1]));
            volume.SetVector("_RayTR",transform.TransformVector(corners[2]));volume.SetVector("_RayBR",transform.TransformVector(corners[3]));
            volume.SetVector("_CameraPosition",transform.position);volume.SetMatrix("_WorldToField",WorldToField);
            volume.SetVector("_FogParams",new Vector4(profile.density*Strength,profile.heightFalloff,profile.maximumDistance,ElapsedSeconds));
            volume.SetVector("_Atlas",new Vector4(profile.gridSize,profile.atlasColumns,profile.atlasRows,profile.tileSize));
            volume.SetVector("_Period",new Vector4(profile.period[0],profile.period[1],profile.period[2],0));
            volume.SetColor("_FogColor",StreetArt.Hex(profile.color));
            Graphics.Blit(source,destination,volume);
        }
        void OnDestroy(){if(volume!=null){if(Application.isPlaying)Destroy(volume);else DestroyImmediate(volume);}}
    }
}
