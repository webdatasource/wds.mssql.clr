// ReSharper disable CheckNamespace

using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures request pacing for one target host.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CrawlDelay : DataContractBase
{
    private const string _robots = "robots";
    private static readonly char[] _delayRangeSeparator = { '-' };

    /// <summary>
    /// Lowercase target host, or host:port for a non-default port, to which this delay applies.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Per-host wait between requests. Use <c>0</c> for no extra delay, an integer for fixed seconds, a range such as <c>1-5</c> for a random value from 1 inclusive to 5 exclusive, or <c>robots</c> to use the robots.txt crawl-delay when available.
    /// </summary>
    public string Delay { get; set; }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public override void Validate()
    {
        if (Host is null)
            throw new NullReferenceException(nameof(Host));

        if (Delay is null)
            throw new NullReferenceException(nameof(Delay));

        if (_robots.Equals(Delay, StringComparison.InvariantCultureIgnoreCase) || int.TryParse(Delay, out _))
            return;
        var delayRange = Delay.Split(_delayRangeSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (delayRange.Length == 2 && int.TryParse(delayRange[0], out _) && int.TryParse(delayRange[1], out _))
            return;
        throw new InvalidDataException($"Invalid delay {Delay}");
    }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'Host: host.com; Delay: 0|1-5|robots'
    /// </summary>
    /// <param name="s">Config string</param>
    public static CrawlDelay Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CrawlDelay> _dataContractSerializer = new DataContractSerializer<CrawlDelay>()
        .MapValue<string>(nameof(Host), (crawlDelay, host) => crawlDelay.Host = host)
        .MapValue<string>(nameof(Delay), (crawlDelay, delay) => crawlDelay.Delay = delay);

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static CrawlDelay Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Host = r.ReadString();
        Delay = r.ReadString();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Host);
        w.Write(Delay);
    }

    #endregion
}
