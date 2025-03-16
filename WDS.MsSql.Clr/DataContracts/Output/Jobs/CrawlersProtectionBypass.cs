using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Crawlers protection bypass settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CrawlersProtectionBypass : DataContractBase
{
    /// <summary>
    /// Max response size in kilobytes. Optional. Default value is 1000
    /// </summary>
    public int? MaxResponseSizeKb { get; set; }

    /// <summary>
    /// Max redirect hops. Optional. Default value is 10
    /// </summary>
    public int? MaxRedirectHops { get; set; }

    /// <summary>
    /// Max request timeout in seconds. Optional. Default value is 30
    /// </summary>
    public int? RequestTimeoutSec { get; set; }

    /// <summary>
    /// Crawl delays for hosts
    /// </summary>
    public CrawlDelay[] CrawlDelays { get; set; }

    /// <summary>
    /// Adds a new crawl delay
    /// </summary>
    /// <param name="crawlDelay">CrawlDelay instance</param>
    public CrawlersProtectionBypass AddCrawlDelay(CrawlDelay crawlDelay)
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

        return this;
    }

    /// <summary>
    /// Adds a new crawl delay
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="delay">Delay string <see cref="CrawlDelay"/></param>
    public CrawlersProtectionBypass AddDelay(string host, string delay)
    {
        return AddCrawlDelay(new CrawlDelay { Host = host, Delay = delay });
    }


    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'MaxResponseSizeKb: 1000; MaxRedirectHops: 3; RequestTimeoutSec: 1'
    /// </summary>
    /// <param name="s">Config string</param>
    public static CrawlersProtectionBypass Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CrawlersProtectionBypass> _dataContractSerializer = new DataContractSerializer<CrawlersProtectionBypass>()
        .MapValue<int>("MaxResponseSizeKb", (crawlersProtectionBypass, maxResponseSizeKb) => crawlersProtectionBypass.MaxResponseSizeKb = maxResponseSizeKb)
        .MapValue<int>("MaxRedirectHops", (crawlersProtectionBypass, maxRedirectHops) => crawlersProtectionBypass.MaxRedirectHops = maxRedirectHops)
        .MapValue<int>("RequestTimeoutSec", (crawlersProtectionBypass, requestTimeoutSec) => crawlersProtectionBypass.RequestTimeoutSec = requestTimeoutSec);


    #region MS SQL CLR Required methods and properties

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