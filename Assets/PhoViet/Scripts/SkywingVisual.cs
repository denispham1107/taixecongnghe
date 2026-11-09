using UnityEngine;

namespace PhoViet
{
    public sealed class SkywingVisual : MonoBehaviour
    {
        [SerializeField] Transform airframe, propeller;
        float age, spin, angle;
        bool wrecked;
        PhoVietGame game;

        public void ResetPose()
        {
            age=angle=0;
            spin=1;
            wrecked=false;
            airframe.localPosition=Vector3.zero;
            airframe.localRotation=propeller.localRotation=Quaternion.identity;
        }

        public void SetWrecked()
        {
            wrecked=true;
            airframe.localPosition=Vector3.zero;
            airframe.localRotation=Quaternion.identity;
        }

        void Awake() => game=GetComponentInParent<PhoVietGame>();

        void LateUpdate()
        {
            if(game==null||game.State==PhoVietGame.RideState.Paused)return;
            float dt=Mathf.Min(Time.deltaTime,.05f);
            age+=dt;
            if(wrecked)spin=Mathf.MoveTowards(spin,0,dt*1.35f);
            angle=Mathf.Repeat(angle+2270*spin*dt,360);
            propeller.localRotation=Quaternion.AngleAxis(angle,Vector3.forward);
            if(wrecked)return;
            airframe.localPosition=Vector3.up*(.085f*Mathf.Sin(age*2.6f));
            airframe.localRotation=Quaternion.Euler(.8f*Mathf.Sin(age*1.9f),0,2.8f*Mathf.Sin(age*1.4f));
        }
    }
}
