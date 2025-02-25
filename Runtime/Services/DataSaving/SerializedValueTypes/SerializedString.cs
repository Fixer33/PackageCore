namespace Core.Services.DataSaving.SerializedValueTypes
{
    public class SerializedString : SerializedValueBase
    {
        public string Value;
        
        public override string Serialize()
        {
            return Value;
        }

        public override void DeserializeFromString(string s)
        {
            Value = s;
        }
    }
}

namespace Core.Services.DataSaving.SerializedValueTypes
{
    public abstract partial class SerializedValueBase
    {
        public static implicit operator SerializedValueBase(string val) => new SerializedString() { Value = val };

        public static explicit operator string(SerializedValueBase val)
        {
            if (val is SerializedString b)
                return b.Value;
            return "";
        }
        
        public static bool operator ==(SerializedValueBase sv, string b) => (string)sv == b;
        public static bool operator !=(SerializedValueBase sv, string b) => (string)sv != b;
    }
}