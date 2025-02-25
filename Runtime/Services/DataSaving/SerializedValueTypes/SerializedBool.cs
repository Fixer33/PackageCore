namespace Core.Services.DataSaving.SerializedValueTypes
{
    public class SerializedBool : SerializedValueBase
    {
        public bool Value;
        
        public override string Serialize()
        {
            return Value.ToString();
        }

        public override void DeserializeFromString(string s)
        {
            if (bool.TryParse(s, out var res))
                Value = res;
        }
    }
}

namespace Core.Services.DataSaving.SerializedValueTypes
{
    public abstract partial class SerializedValueBase
    {
        public static implicit operator SerializedValueBase(bool val) => new SerializedBool() { Value = val };

        public static explicit operator bool(SerializedValueBase val)
        {
            if (val is SerializedBool b)
                return b.Value;
            return false;
        }
        
        public static bool operator ==(SerializedValueBase sv, bool b) => (bool)sv == b;
        public static bool operator !=(SerializedValueBase sv, bool b) => (bool)sv != b;
    }
}