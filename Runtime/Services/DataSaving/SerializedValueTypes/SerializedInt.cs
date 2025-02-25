using System;

namespace Core.Services.DataSaving.SerializedValueTypes
{
    [Serializable]
    public class SerializedInt : SerializedValueBase
    {
        public int Value;
        
        public override string Serialize()
        {
            return Value.ToString();
        }

        public override void DeserializeFromString(string s)
        {
            if (int.TryParse(s, out var res))
                Value = res;
        }
    }
}

namespace Core.Services.DataSaving.SerializedValueTypes
{
    public abstract partial class SerializedValueBase
    {
        public static implicit operator SerializedValueBase(int val) => new SerializedInt() { Value = val };

        public static explicit operator int(SerializedValueBase val)
        {
            if (val is SerializedInt b)
                return b.Value;
            return 0;
        }
        
        public static bool operator ==(SerializedValueBase sv, int b) => (int)sv == b;
        public static bool operator !=(SerializedValueBase sv, int b) => (int)sv != b;
    }
}