using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        public DayNightLighting daylight;
        void ResetDaylight(){ResetThunderstorm();if(daylight==null)daylight=GetComponent<DayNightLighting>();if(daylight!=null){daylight.Initialize(this);daylight.ResetCycle();}}
        void AdvanceDaylight(float dt){if(daylight!=null)daylight.AdvanceSimulation(dt);}
    }
}
