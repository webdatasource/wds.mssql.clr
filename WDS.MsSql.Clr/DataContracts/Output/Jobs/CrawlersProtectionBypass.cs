// ReSharper disable CheckNamespace

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures per-download response, redirect, timeout, and per-host pacing limits.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CrawlersProtectionBypass : DataContractBase
{
    /// <summary>
    /// Optional. Maximum response content buffered for one download, in kilobytes. Null uses 1000 KB.
    /// </summary>
    public int? MaxResponseSizeKb { get; set; }

    /// <summary>
    /// Optional. Maximum number of redirects followed for one download. Null uses 10; 0 disables redirects.
    /// </summary>
    public int? MaxRedirectHops { get; set; }

    /// <summary>
    /// Optional. Request timeout in seconds. Null uses 30 seconds.
    /// </summary>
    public int? RequestTimeoutSec { get; set; }

    /// <summary>
    /// Optional. Per-host delay overrides. Null or empty uses the robots.txt crawl-delay when available, otherwise no extra delay.
    /// </summary>
    public CrawlDelay[] CrawlDelays { get; private set; }

    /// <summary>
    /// Adds a new crawl delay
    /// </summary>
    /// <param name="crawlDelay">CrawlDelay instance</param>
    public void AddCrawlDelay(CrawlDelay crawlDelay)
    {
        if (CrawlDelays is null)
            CrawlDelays = new[] { crawlDelay };
        else if (CrawlDelays.All(cd => cd.Host != crawlDelay.Host))
        {
            var crawlDelays = CrawlDelays;
            Array.Resize(ref crawlDelays, crawlDelays.Length + 1);
            crawlDelays[crawlDelays.Length - 1] = crawlDelay;
            CrawlDelays = crawlDelays;
        }
    }

    /// <summary>
    /// Adds a new crawl delay
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="delay">Delay string <see cref="CrawlDelay"/></param>
    public void AddDelay(string host, string delay)
    {
        AddCrawlDelay(new CrawlDelay { Host = host, Delay = delay });
    }


    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'MaxResponseSizeKb: 1000; MaxRedirectHops: 3; RequestTimeoutSec: 1'
    /// </summary>
    /// <param name="s">Config string</param>
    public static CrawlersProtectionBypass Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CrawlersProtectionBypass> _dataContractSerializer = new DataContractSerializer<CrawlersProtectionBypass>()
        .MapValue<int>(nameof(MaxResponseSizeKb), (crawlersProtectionBypass, maxResponseSizeKb) => crawlersProtectionBypass.MaxResponseSizeKb = maxResponseSizeKb)
        .MapValue<int>(nameof(MaxRedirectHops), (crawlersProtectionBypass, maxRedirectHops) => crawlersProtectionBypass.MaxRedirectHops = maxRedirectHops)
        .MapValue<int>(nameof(RequestTimeoutSec), (crawlersProtectionBypass, requestTimeoutSec) => crawlersProtectionBypass.RequestTimeoutSec = requestTimeoutSec);


    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static CrawlersProtectionBypass Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        MaxResponseSizeKb = r.ReadNullableInt();
        MaxRedirectHops = r.ReadNullableInt();
        RequestTimeoutSec = r.ReadNullableInt();
        CrawlDelays = r.ReadArrayNullable<CrawlDelay>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteNullable(MaxResponseSizeKb);
        w.WriteNullable(MaxRedirectHops);
        w.WriteNullable(RequestTimeoutSec);
        w.WriteNullable(CrawlDelays);
    }

    #endregion
}
