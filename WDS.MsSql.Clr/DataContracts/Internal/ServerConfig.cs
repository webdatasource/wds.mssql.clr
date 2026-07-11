// ReSharper disable CheckNamespace

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// API server configuration
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ServerConfig : DataContractBase
{
    /// <summary>
    /// API server Uri
    /// </summary>
    public Uri Uri { get; private set; }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public override void Validate()
    {
        if (Uri is null)
            throw new NullReferenceException(nameof(Uri));
    }

    /// <summary>
    /// Parses a Server Connection String of the following format: wds://user:password@host:port?https=false
    /// </summary>
    public static ServerConfig Parse(SqlString s)
    {
        if (!Uri.TryCreate(s.Value, UriKind.Absolute, out var connectionString))
            throw new ArgumentException("Invalid URL");

        if (connectionString.Scheme.ToLower() != "wds")
            throw new ArgumentException("Invalid scheme. Expected scheme: wds");

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

        var uriBuilder = new UriBuilder(scheme, connectionString.Host, connectionString.Port)
        {
            Path = connectionString.AbsolutePath
        };
        var serverConfig = new ServerConfig
        {
            Uri = uriBuilder.Uri
        };
        return serverConfig;
    }

    private static readonly DataContractSerializer<ServerConfig> _dataContractSerializer = new();

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static ServerConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Uri = new Uri(r.ReadString());
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Uri.ToString());
    }

    #endregion
}