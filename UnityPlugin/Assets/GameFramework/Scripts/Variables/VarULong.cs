using GameFramework;

namespace UnityGameFramework.Runtime
{
    public class VarULong : Variable<ulong>
    {
        public VarULong()
        {

        }

        public VarULong(ulong value)
            : base(value)
        {

        }

        public static implicit operator VarULong(ulong value)
        {
            return new VarULong(value);
        }

        public static implicit operator ulong(VarULong value)
        {
            return value.Value;
        }
    }
}
