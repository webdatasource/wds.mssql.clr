using System;

namespace WDS.MsSql.Clr.Serialization.DataContracts
{
    public class DataContractPropAssignEnum<TRes, TEnum> : IDataContractAssign<TRes>
        where TEnum : struct
    {
        private readonly Action<TRes, TEnum> _assignProp;

        public DataContractPropAssignEnum(Action<TRes, TEnum> assignProp)
        {
            _assignProp = assignProp;
        }

        public void AssignProp(TRes res, string value)
        {
            if (!Enum.TryParse<TEnum>(value, out var enumValue))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid enum value");
            _assignProp(res, enumValue);
        }
    }
}