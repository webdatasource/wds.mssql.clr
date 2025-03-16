using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Server;

namespace WDS.MsSql.Clr.Serialization
{
    internal static class BinarySerializationExtensions
    {
        #region IBinarySerialize

        public static void WriteNullable<TValue>(this BinaryWriter w, TValue value)
            where TValue : class, IBinarySerialize
        {
            var isNull = value is null;
            w.Write(isNull);
            if (!isNull)
                value.Write(w);
        }

        public static TValue ReadNullable<TValue>(this BinaryReader r)
            where TValue : class, IBinarySerialize, new()
        {
            var isNull = r.ReadBoolean();
            if (isNull)
                return null;
            var value = new TValue();
            value.Read(r);
            return value;
        }

        public static void WriteNullable<TValue>(this BinaryWriter w, ICollection<TValue> values)
            where TValue : class, IBinarySerialize
        {
            var isNull = values is null;
            w.Write(isNull);
            if (isNull)
                return;
            w.Write(values.Count);
            foreach (var value in values)
                w.WriteNullable(value);
        }

        public static TValue[] ReadArrayNullable<TValue>(this BinaryReader r)
            where TValue : class, IBinarySerialize, new()
        {
            var isNull = r.ReadBoolean();
            if (isNull)
                return null;
            var count = r.ReadInt32();
            var values = new TValue[count];
            for (var i = 0; i < count; i++)
                values[i] = r.ReadNullable<TValue>();
            return values;
        }

        #endregion

        #region Strings

        public static void WriteNullable(this BinaryWriter w, string value)
        {
            var isNull = value is null;
            w.Write(isNull);
            if (!isNull)
                w.Write(value);
        }

        public static string ReadNullableString(this BinaryReader r)
        {
            var isNull = r.ReadBoolean();
            if (isNull)
                return null;
            return r.ReadString();
        }

        public static void WriteNullable(this BinaryWriter w, ICollection<string> values)
        {
            var isNull = values is null;
            w.Write(isNull);
            if (isNull)
                return;
            w.Write(values);
        }
        
        public static void Write(this BinaryWriter w, ICollection<string> values)
        {
            w.Write(values.Count);
            foreach (var value in values)
                w.WriteNullable(value);
        }


        public static string[] ReadNullableStringArray(this BinaryReader r)
        {
            var isNull = r.ReadBoolean();
            if (isNull)
                return null;
            return r.ReadStringArray();
        }
        
        public static string[] ReadStringArray(this BinaryReader r)
        {
            var count = r.ReadInt32();
            var values = new string[count];
            for (var i = 0; i < count; i++)
                values[i] = r.ReadNullableString();
            return values;
        }

        #endregion

        #region Int

        public static void WriteNullable(this BinaryWriter w, int? value)
        {
            w.Write(value.HasValue);
            if (value.HasValue)
                w.Write(value.Value);
        }

        public static int? ReadNullableInt(this BinaryReader r)
        {
            var hasValue = r.ReadBoolean();
            return hasValue ? r.ReadInt32() : null;
        }

        #endregion

        #region DateTime

        public static void WriteNullable(this BinaryWriter w, DateTime? value)
        {
            w.Write(value.HasValue);
            if (value.HasValue)
                w.Write(value.Value.Ticks);
        }

        public static DateTime? ReadNullableDateTime(this BinaryReader r)
        {
            var hasValue = r.ReadBoolean();
            return hasValue ? new DateTime(r.ReadInt64()) : null;
        }

        public static void Write(this BinaryWriter w, DateTime value)
        {
            w.Write(value.Ticks);
        }

        public static DateTime ReadDateTime(this BinaryReader r)
        {
            return new DateTime(r.ReadInt64());
        }

        #endregion

        #region Enums

        public static void WriteEnum<TEnum>(this BinaryWriter w, TEnum value)
            where TEnum : struct
        {
            w.Write(value.ToString());
        }

        public static void WriteNullableEnum<TEnum>(this BinaryWriter w, TEnum? value)
            where TEnum : struct
        {
            w.Write(value.HasValue);
            if (value.HasValue)
                w.Write(value.ToString());
        }


        public static TEnum ReadEnum<TEnum>(this BinaryReader r)
            where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), r.ReadString());
        }
        
        public static TEnum? ReadNullableEnum<TEnum>(this BinaryReader r)
            where TEnum : struct
        {
            var hasValue = r.ReadBoolean();
            if (!hasValue)
                return null;
            return (TEnum)Enum.Parse(typeof(TEnum), r.ReadString());
        }

        #endregion
    }
}