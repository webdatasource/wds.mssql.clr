using System.IO;
using System.Text;

namespace WDS.MsSql.Clr.Serialization
{
    internal class XmlStringWriter : StringWriter
    {
        public override Encoding Encoding { get; }

        public XmlStringWriter(StringBuilder sb, Encoding encoding) : base(sb)
        {
            Encoding = encoding;
        }
    }
}