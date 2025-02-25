namespace Core.Utilities
{
    public static class PlatformUtility
    {
        public static PlatformSpecification CurrentPlatform { get; private set; }

        static PlatformUtility()
        {
#if UNITY_STANDALONE_OSX
            CurrentPlatform = PlatformSpecification.Mac;
#elif UNITY_IOS
            CurrentPlatform = PlatformSpecification.IOS;
#elif UNITY_ANDROID
    #if SAMSUNG_BUILD
            CurrentPlatform = PlatformSpecification.Samsung;
    #else
            CurrentPlatform = PlatformSpecification.GP;
    #endif
#elif UNITY_STANDALONE_WIN
            CurrentPlatform = PlatformSpecification.Windows;
#elif UNITY_WSA
            CurrentPlatform = PlatformSpecification.UWP;
#elif !UNITY_EDITOR && UNITY_WEBGL
#error Needs correcting!
        if (_specification.HasFlag(PlatformSpecification.WebMobile) || _specification.HasFlag(PlatformSpecification.WebDesktop))
        {
            if (PlatformDefiner.Mobile && _specification.HasFlag(PlatformSpecification.WebMobile) == false)
            {
                Destroy(gameObject);
            }
            
            if (PlatformDefiner.Mobile == false &&
                     _specification.HasFlag(PlatformSpecification.WebDesktop) == false)
            {
                Destroy(gameObject);
            }
            return;
        }

        Destroy(gameObject);
        return;
#endif
        }
        
        public static bool IsForCurrentPlatform(PlatformSpecification platformSpecification)
        {
            return platformSpecification.HasFlag(CurrentPlatform);
        }
    }
}