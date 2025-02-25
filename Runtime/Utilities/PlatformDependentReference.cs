using System;

namespace Core.Utilities
{
    [Serializable]
    public struct PlatformDependentReference<T>
    {
        public T Android;
        public T Samsung;
        public T Mac;
        public T IOS;
        public T Windows;
        public T UWP;
        public T Web;
        
        public T Get()
        {
#if UNITY_ANDROID
#if SAMSUNG_BUILD
            return Samsung;
#endif
            return Android;
#elif UNITY_IOS
            return IOS;
#elif UNITY_STANDALONE_OSX
            return Mac;
#elif UNITY_STANDALONE_WIN
            return Windows;
#elif UNITY_WSA
            return UWP;
#elif UNITY_WEBGL
            return Web;
#else
            #error Unexpected platform!
#endif
            throw new NotImplementedException();
        }
    }
}