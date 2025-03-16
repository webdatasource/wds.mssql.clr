using System;

namespace WDS.MsSql.Clr.Serialization
{
    public static class EnumSerializationExtensions
    {
        public static TEnum ParseEnum<TEnum>(this string value) => (TEnum)Enum.Parse(typeof(TEnum), value);
    }
}