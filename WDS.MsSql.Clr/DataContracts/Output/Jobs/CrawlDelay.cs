using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Crawl delay for a host
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CrawlDelay : DataContractBase
{
    private const string _robots = "robots";
    private static readonly char[] _delayRangeSeparator = { '-' };

    /// <summary>
    /// Host
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Delay string. Can be either a number, a range of numbers separated by a dash or 'robots' (0|3|0-3|robots)
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
        .MapValue<string>("Host", (crawlDelay, host) => crawlDelay.Host = host)
        .MapValue<string>("Delay", (crawlDelay, delay) => crawlDelay.Delay = delay);

    #region MS SQL CLR Required methods and properties

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