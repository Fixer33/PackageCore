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
            CurrentPlatform = PlatformSpecification.WebDesktop;
#endif
        }
        
        public static bool IsForCurrentPlatform(PlatformSpecification platformSpecification)
        {
            return platformSpecification.HasFlag(CurrentPlatform);
        }
    }
}
