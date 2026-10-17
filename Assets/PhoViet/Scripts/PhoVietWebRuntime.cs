using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        // Invoked by the web shell on visibility loss and before opening its help panel.
        public void PauseForBrowser()
        {
            if(State==RideState.Riding)TogglePause();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ConfigureBrowserGraphics()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var game=Object.FindAnyObjectByType<PhoVietGame>();
            if(game!=null && game.rideCamera!=null)game.rideCamera.allowHDR=false;
            if(Application.isMobilePlatform)
            {
                QualitySettings.shadowDistance=45;
                QualitySettings.shadowResolution=ShadowResolution.Low;
                QualitySettings.shadowCascades=0;
                QualitySettings.antiAliasing=2;
            }
#endif
        }
    }
}
