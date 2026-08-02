// ReSharper disable CheckNamespace

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures proxy routing, proxy pools, and direct-request fallback behavior.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ProxiesConfig : DataContractBase
{
    private static readonly char[] _availableHostsSeparator = { ',' };

    /// <summary>
    /// Optional. When true, route download requests through the configured proxy pool. False is the default.
    /// </summary>
    public bool UseProxy { get; set; }

    /// <summary>
    /// Optional. When true, fall back to a direct non-proxy request after all eligible proxies fail. False is the default.
    /// </summary>
    public bool SendOvertRequestsOnProxiesFailure { get; set; }

    /// <summary>
    /// Optional. HTTP status codes that cause a retry through the next proxy, separated by commas, semicolons, pipes, or ampersands. Null uses 401 and 403.
    /// </summary>
    public string IterateProxyResponseCodes { get; set; }

    /// <summary>
    /// Optional. Proxy endpoints used when UseProxy is true. Null or empty provides no eligible proxies.
    /// </summary>
    public ProxyConfig[] Proxies { get; private set; }

    /// <summary>
    /// Adds a new proxy configuration
    /// </summary>
    /// <param name="proxy">Proxy</param>
    public ProxiesConfig AddProxyConfig(ProxyConfig proxy)
    {
        if (Proxies is null)
            Proxies = new[] { proxy };
        else if (!Proxies.Any(p => p.Protocol == proxy.Protocol && p.Host == proxy.Host && p.Port == proxy.Port && p.UserName == proxy.UserName))
        {
            var proxies = Proxies;
            Array.Resize(ref proxies, proxies.Length + 1);
            proxies[proxies.Length - 1] = proxy;
            Proxies = proxies;
        }

        return this;
    }

    /// <summary>
    /// Adds a new proxy
    /// </summary>
    /// <param name="protocol">Proxy protocol (http|https|socks5)</param>
    /// <param name="host">Proxy host</param>
    /// <param name="port">Proxy port</param>
    /// <param name="userName">Proxy username</param>
    /// <param name="password">Proxy password</param>
    /// <param name="connectionsLimit">Proxy connections limit (how many connections can be established through this proxy at one time)</param>
    /// <param name="availableHosts">A list of available hosts that can be accessed through this proxy</param>
    /// <returns></returns>
    public ProxiesConfig AddProxy(string protocol, string host, int port, string userName, string password, int? connectionsLimit, string availableHosts)
    {
        AddProxyConfig(new ProxyConfig
        {
            Protocol = protocol,
            Host = host,
            Port = port,
            UserName = userName,
            Password = password,
            ConnectionsLimit = connectionsLimit,
            AvailableHosts = availableHosts?.Split(_availableHostsSeparator, StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()).ToArray()
        });
        return this;
    }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public override void Validate()
    {
        if (Proxies is null)
            return;
        foreach (var proxy in Proxies)
            proxy.Validate();
    }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'UseProxy: true|false; SendOvertRequestsOnProxiesFailure: true|false; IterateProxyResponseCodes: 200, 404, 500'
    /// </summary>
    /// <param name="s">Config string</param>
    public static ProxiesConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<ProxiesConfig> _dataContractSerializer = new DataContractSerializer<ProxiesConfig>()
        .MapValue<bool>(nameof(UseProxy), (proxiesConfig, useProxy) => proxiesConfig.UseProxy = useProxy)
        .MapValue<bool>(nameof(SendOvertRequestsOnProxiesFailure), (proxiesConfig, sendOvertRequestOnProxiesFailure) => proxiesConfig.SendOvertRequestsOnProxiesFailure = sendOvertRequestOnProxiesFailure)
        .MapArray<int>(nameof(IterateProxyResponseCodes), (proxiesConfig, iterateProxyResponseCodes) => proxiesConfig.IterateProxyResponseCodes = string.Join(", ", iterateProxyResponseCodes));

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static ProxiesConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        UseProxy = r.ReadBoolean();
        SendOvertRequestsOnProxiesFailure = r.ReadBoolean();
        IterateProxyResponseCodes = r.ReadNullableString();
        Proxies = r.ReadArrayNullable<ProxyConfig>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(UseProxy);
        w.Write(SendOvertRequestsOnProxiesFailure);
        w.WriteNullable(IterateProxyResponseCodes);
        w.WriteNullable(Proxies);
    }

    #endregion
}
