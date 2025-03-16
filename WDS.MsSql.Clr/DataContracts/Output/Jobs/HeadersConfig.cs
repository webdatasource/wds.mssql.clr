using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Default configuration for ALL requests
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class HeadersConfig : DataContractBase
{
    /// <summary>
    /// Http headers that will be added to all requests
    /// </summary>
    public HttpHeader[] DefaultRequestHeaders { get; set; }

    /// <summary>
    /// Adds or replaces a header
    /// </summary>
    /// <param name="header">Http request header</param>
    /// <returns></returns>
    public HeadersConfig AddHeader(HttpHeader header)
    {
        var existingHeader = DefaultRequestHeaders?.FirstOrDefault(h => h.Name == header.Name);
        if (existingHeader == null)
        {
            if (DefaultRequestHeaders is null)
            {
                DefaultRequestHeaders = new[] { header };
            }
            else
            {
                var headers = DefaultRequestHeaders;
                Array.Resize(ref headers, DefaultRequestHeaders.Length + 1);
                headers[headers.Length - 1] = header;
                DefaultRequestHeaders = headers;
            }
        }
        else
        {
            existingHeader.Values = header.Values;
        }

        return this;
    }

    /// <summary>
    /// Adds a new header or a value to an existing header
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns></returns>
    public HeadersConfig AddHeader(string name, string value)
    {
        var header = DefaultRequestHeaders?.FirstOrDefault(h => h.Name == name);
        if (header == null)
        {
            header = new HttpHeader
            {
                Name = name,
                Values = new[] { value }
            };
            if (DefaultRequestHeaders is null)
            {
                DefaultRequestHeaders = new[] { header };
            }
            else
            {
                var headers = DefaultRequestHeaders;
                Array.Resize(ref headers, DefaultRequestHeaders.Length + 1);
                headers[headers.Length - 1] = header;
                DefaultRequestHeaders = headers;
            }
        }
        else if (!header.Values.Contains(value))
        {
            var values = header.Values;
            Array.Resize(ref values, values.Length + 1);
            values[values.Length - 1] = value;
            header.Values = values;
        }

        return this;
    }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (DefaultRequestHeaders is not null)
            foreach (var defaultRequestHeader in DefaultRequestHeaders)
                defaultRequestHeader.Validate();
    }

    /// <summary>
    /// Creates a new instance of RequestDefaultsConfig
    /// </summary>
    /// <param name="s">This value is ignored</param>
    public static HeadersConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<HeadersConfig> _dataContractSerializer = new();

    #region MS SQL CLR Required methods and properties

    private static readonly XmlSerializer _headersConfigSerializer = new(typeof(HeadersConfig));

    public static HeadersConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        DefaultRequestHeaders = r.ReadArrayNullable<HttpHeader>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteNullable(DefaultRequestHeaders);
    }

    #endregion
}