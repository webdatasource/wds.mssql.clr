using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Proxies config
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ProxiesConfig : DataContractBase
{
    private static readonly char[] _availableHostsSeparator = { ',' };

    /// <summary>
    /// Use proxies for requests
    /// </summary>
    public bool UseProxy { get; set; }

    /// <summary>
    /// Send a request from a host real IP address if all proxies failed
    /// </summary>
    public bool SendOvertRequestsOnProxiesFailure { get; set; }

    /// <summary>
    /// Comma-separated HTTP response codes to iterate proxies on. Optional. Default value is '401, 403'
    /// </summary>
    public string IterateProxyResponseCodes { get; set; }

    /// <summary>
    /// Proxy configurations
    /// </summary>
    public ProxyConfig[] Proxies { get; set; }

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
        if (Proxies is not null)
            foreach (var proxy in Proxies)
                proxy.Validate();
    }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'UseProxy: true|false; SendOvertRequestsOnProxiesFailure: true|false; IterateProxyResponseCodes: 200, 404, 500'
    /// </summary>
    /// <param name="s">Config string</param>
    public static ProxiesConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<ProxiesConfig> _dataContractSerializer = new DataContractSerializer<ProxiesConfig>()
        .MapValue<bool>("UseProxy", (proxiesConfig, useProxy) => proxiesConfig.UseProxy = useProxy)
        .MapValue<bool>("SendOvertRequestsOnProxiesFailure", (proxiesConfig, sendOvertRequestOnProxiesFailure) => proxiesConfig.SendOvertRequestsOnProxiesFailure = sendOvertRequestOnProxiesFailure)
        .MapArray<int>("IterateProxyResponseCodes", (proxiesConfig, iterateProxyResponseCodes) => proxiesConfig.IterateProxyResponseCodes = string.Join(", ", iterateProxyResponseCodes));

    #region MS SQL CLR Required methods and properties
    
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