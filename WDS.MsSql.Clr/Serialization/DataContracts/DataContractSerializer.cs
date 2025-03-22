using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using WDS.MsSql.Clr.Server;

namespace WDS.MsSql.Clr.Serialization.DataContracts
{
    public class DataContractSerializer<TRes>
        where TRes : class, new()
    {
        private static readonly char[] _parseGroupsSeparator = { ';' };

        private readonly XmlSerializer _xmlSerializer = new(typeof(TRes));

        private readonly Dictionary<string, IDataContractAssign<TRes>> _assignProps = new();

        public DataContractSerializer<TRes> MapValue<TProp>(string propName, Action<TRes, TProp> assignProp)
        {
            AddMapping(propName, new DataContractPropAssignValue<TRes, TProp>(assignProp));
            return this;
        }

        public DataContractSerializer<TRes> MapArray<TProp>(string propName, Action<TRes, TProp[]> assignProp)
        {
            AddMapping(propName, new DataContractPropAssignArray<TRes, TProp>(assignProp));
            return this;
        }

        public DataContractSerializer<TRes> MapEnum<TEnum>(string propName, Action<TRes, TEnum> assignProp)
            where TEnum : struct
        {
            AddMapping(propName, new DataContractPropAssignEnum<TRes, TEnum>(assignProp));
            return this;
        }

        public string Serialize(object instance)
        {
            var stringBuilder = new StringBuilder();
            using var writer = new XmlStringWriter(stringBuilder, ServerApiBase.Encoding);
            _xmlSerializer.Serialize(writer, instance);
            return stringBuilder.ToString();
        }

        public TRes Parse(string config)
        {
            if (TryParseXml(config, out var res))
                return res;
            res = new TRes();
            var groups = config.Split(_parseGroupsSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var group in groups)
            {
                var delimiterPos = group.IndexOf(':');
                if (delimiterPos == -1)
                    continue;
                var key = group.Substring(0, delimiterPos);
                var value = group.Substring(delimiterPos + 1).Trim();
                _assignProps[key.Trim().ToLower()].AssignProp(res, value);
            }

            return res;
        }

        private void AddMapping(string propName, IDataContractAssign<TRes> dataContractAssign)
        {
            _assignProps.Add(propName.Trim().ToLower(), dataContractAssign);
        }

        private bool TryParseXml(string xmlString, out TRes res)
        {
            if (!xmlString.TrimStart().StartsWith("<"))
            {
                res = null;
                return false;
            }

            try
            {
                using var reader = new StringReader(xmlString);
                res = (TRes)_xmlSerializer.Deserialize(reader);
                return true;
            }
            catch
            {
                res = null;
                return false;
            }
        }
    }
}