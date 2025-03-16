using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Proxy configuration 
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ProxyConfig : DataContractBase
{
    /// <summary>
    /// Proxy host
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Proxy port
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Proxy username
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Proxy password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Proxy connections limit (how many connections can be established through this proxy at one time)
    /// </summary>
    public int? ConnectionsLimit { get; set; }

    /// <summary>
    /// A list of available hosts that can be accessed through this proxy
    /// </summary>
    public string[] AvailableHosts { get; set; }

    /// <summary>
    /// Adds a new available host
    /// </summary>
    /// <param name="host">Available host</param>
    public ProxyConfig AddAvailableHost(string host)
    {
        if (AvailableHosts is null)
            AvailableHosts = new[] { host };
        else if (!AvailableHosts.Contains(host))
        {
            var availableHosts = AvailableHosts;
            Array.Resize(ref availableHosts, availableHosts.Length + 1);
            availableHosts[availableHosts.Length - 1] = host;
            AvailableHosts = availableHosts;
        }

        return this;
    }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public override void Validate()
    {
        if (Host is null)
            throw new NullReferenceException(nameof(Host));
        if (Port == 0)
            throw new InvalidDataException("Port can't be 0");
    }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'Host: host; Port: port; UserName: userName; Password: password; ConnectionsLimit: connectionsLimit; AvailableHosts: availableHost1, availableHost2;'
    /// </summary>
    /// <param name="s">Config string</param>
    public static ProxyConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);
    private static readonly DataContractSerializer<ProxyConfig> _dataContractSerializer = new DataContractSerializer<ProxyConfig>()
        .MapValue<string>("Host", (proxyConfig, host) => proxyConfig.Host = host)
        .MapValue<int>("Port", (proxyConfig, port) => proxyConfig.Port = port)
        .MapValue<string>("UserName", (proxyConfig, userName) => proxyConfig.UserName = userName)
        .MapValue<string>("Password", (proxyConfig, password) => proxyConfig.Password = password)
        .MapValue<int>("ConnectionsLimit", (proxyConfig, connectionsLimit) => proxyConfig.ConnectionsLimit = connectionsLimit)
        .MapArray<string>("AvailableHosts", (proxyConfig, availableHosts) => proxyConfig.AvailableHosts = availableHosts);

    #region MS SQL CLR Required methods and properties
    
    public static ProxyConfig Null { get; } = new() { IsNull = true };
    
    public override string ToString() => _dataContractSerializer.Serialize(this);
    
    protected override void BinaryRead(BinaryReader r)
    {
        Host = r.ReadString();
        Port = r.ReadInt32();
        UserName = r.ReadNullableString();
        Password = r.ReadNullableString();
        ConnectionsLimit = r.ReadInt32();
        AvailableHosts = r.ReadNullableStringArray();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Host);
        w.Write(Port);
        w.WriteNullable(UserName);
        w.WriteNullable(Password);
        w.Write(ConnectionsLimit ?? -1);
        w.WriteNullable(AvailableHosts);
    }
    
    #endregion
}