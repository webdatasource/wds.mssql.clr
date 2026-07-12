// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures whether the downloader stores response cookies and sends them on later requests.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CookiesConfig : DataContractBase
{
    /// <summary>
    /// Optional. When true, persist and reuse cookies between requests for this job. False is the default.
    /// </summary>
    public bool UseCookies { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'UseCookies: true|false'
    /// </summary>
    /// <param name="s">Config string</param>
    public static CookiesConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CookiesConfig> _dataContractSerializer = new DataContractSerializer<CookiesConfig>()
        .MapValue<bool>(nameof(UseCookies), (cookiesConfig, useCookies) => cookiesConfig.UseCookies = useCookies);

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
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
