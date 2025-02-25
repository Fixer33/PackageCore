using System;

namespace Core.Services.DataSaving.SerializedValueTypes
{
    [Serializable]
    public abstract partial class SerializedValueBase
    {
        public abstract string Serialize();
        public abstract void DeserializeFromString(string s);
    }
}