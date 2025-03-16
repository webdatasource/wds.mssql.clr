using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Cookies settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CookiesConfig : DataContractBase
{
    /// <summary>
    /// Defines if cookies should be saved and reused between requests
    /// </summary>
    public bool UseCookies { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'UseCookies: true|false'
    /// </summary>
    /// <param name="s">Config string</param>
    public static CookiesConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CookiesConfig> _dataContractSerializer = new DataContractSerializer<CookiesConfig>()
        .MapValue<bool>("UseCookies", (cookiesConfig, useCookies) => cookiesConfig.UseCookies = useCookies);

    #region MS SQL CLR Required methods and properties

    public static CookiesConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        UseCookies = r.ReadBoolean();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(UseCookies);
    }

    #endregion
}