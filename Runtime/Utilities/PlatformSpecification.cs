using System;

namespace Core.Utilities
{
    [Flags]
    public enum PlatformSpecification
    {
        None = 0,
        Mac = 1,
        WebMobile = 2,
        WebDesktop = 4,
        Windows = 8,
        IOS = 16,
        GP = 32,
        UWP = 64,
        Samsung = 128,
        
        ALL = 65535
    }
}