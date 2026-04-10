using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Windows.UI.ViewManagement;
#endif

namespace Core.Services
{
    [CreateAssetMenu(fileName = "UWP Fullscreen services", menuName = "Services/UWP Fullscreen", order = 0)]
    public class UWPFullscreenServices : ServiceScriptableObject
    {
        [SerializeField] private float _initDelay = 0.2f;
        
        public override void InitializeService()
        {
            _ = InitializeDelayed(_initDelay);
        }

        private async Awaitable InitializeDelayed(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
#if ENABLE_WINMD_SUPPORT
        Resolution res = Screen.currentResolution;
        Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
#endif
            IsServiceInitialized = true;
        }
    }
}