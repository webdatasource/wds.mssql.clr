using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// API server configuration
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ServerConfig : DataContractBase
{
    private Uri _uri;
    private string _url;
    
    /// <summary>
    /// API server Uri
    /// </summary>
    public Uri Uri => _uri;

    /// <summary>
    /// API server URL
    /// </summary>
    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            _uri = new Uri(value);
        }
    }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public override void Validate()
    {
        if (Url is null)
            throw new NullReferenceException(nameof(Url));

        if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            throw new InvalidDataException($"Invalid {nameof(Url)} value: {Url}. It should be a valid absolute URL");
    }

    /// <summary>
    /// Parses a Server Connection String of the following format: wds://user:password@host:port?https=false
    /// </summary>
    public static ServerConfig Parse(SqlString s)
    {
        if (!Uri.TryCreate(s.Value, UriKind.Absolute, out var connectionString))
            throw new ArgumentException("Invalid URL");

        if (connectionString.Scheme.ToLower() != "wds")
            throw new ArgumentException("Invalid scheme. Expected 'wds' scheme");

        //var userInfo = connectionString.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

        var prms = connectionString.Query
            .TrimStart('?')
            .Split('&')
            .Select(i => i.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(i => i.Length == 2)
            .ToDictionary(i => i[0].Trim().ToLower(), i => i[1].Trim());

        var scheme = "http";
        if (prms.TryGetValue("https", out var https) && https.ToLower() == "true")
            scheme = "https";

        var serverConfig = new ServerConfig
        {
            Url = new UriBuilder(scheme, connectionString.Host, connectionString.Port).Uri.AbsoluteUri
        };
        return serverConfig;
    }

    private static readonly DataContractSerializer<ServerConfig> _dataContractSerializer = new();

    #region MS SQL CLR Required methods and properties

    public static ServerConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Url = r.ReadString();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Url);
    }

    #endregion
}