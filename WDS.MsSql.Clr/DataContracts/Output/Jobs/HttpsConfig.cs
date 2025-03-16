using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Https settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class HttpsConfig : DataContractBase
{
    /// <summary>
    /// Defines whether to suppress HTTPS certificate validation of a web resource
    /// </summary>
    public bool SuppressHttpsCertificateValidation { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'SuppressHttpsCertificateValidation: true|false'
    /// </summary>
    /// <param name="s">Config string</param>
    public static HttpsConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<HttpsConfig> _dataContractSerializer = new DataContractSerializer<HttpsConfig>()
        .MapValue<bool>("SuppressHttpsCertificateValidation", (httpsConfig, suppressHttpsCertificateValidation) => httpsConfig.SuppressHttpsCertificateValidation = suppressHttpsCertificateValidation);

    #region MS SQL CLR Required methods and properties
    
    public static HttpsConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        SuppressHttpsCertificateValidation = r.ReadBoolean();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(SuppressHttpsCertificateValidation);
    }

    #endregion
}