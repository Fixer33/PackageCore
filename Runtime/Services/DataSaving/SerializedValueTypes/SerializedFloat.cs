using System;

namespace Core.Services.DataSaving.SerializedValueTypes
{
    [Serializable]
    public class SerializedFloat : SerializedValueBase
    {
        public float Value;
        
        public override string Serialize()
        {
            return Value.ToString();
        }

        public override void DeserializeFromString(string s)
        {
            if (float.TryParse(s, out var res))
                Value = res;
        }
    }
}

namespace Core.Services.DataSaving.SerializedValueTypes
{
    public abstract partial class SerializedValueBase
    {
        protected bool Equals(SerializedValueBase other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializedValueBase)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static implicit operator SerializedValueBase(float val) => new SerializedFloat() { Value = val };

        public static explicit operator float(SerializedValueBase val)
        {
            if (val is SerializedFloat b)
                return b.Value;
            return 0;
        }
        
        public static bool operator ==(SerializedValueBase sv, float b) => (float)sv == b;
        public static bool operator !=(SerializedValueBase sv, float b) => (float)sv != b;
    }
}