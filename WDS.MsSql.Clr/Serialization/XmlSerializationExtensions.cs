using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WDS.MsSql.Clr.Serialization
{
    internal static class XmlSerializationExtensions
    {
        public static T Deserialize<T>(this string xml, XmlSerializer serializer)
        {
            using var reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }
    }
}