// ReSharper disable CheckNamespace
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Defines a job's entry URLs and runtime options for downloading, crawling, scraping, retries, proxies, cross-domain links, and optional retrieval indexing.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class JobConfig : DataContractBase
{
    /// <summary>
    /// Server configuration associated with the job (if not set the following default connection string is used: wds://localhost:2807)
    /// </summary>
    [XmlIgnore]
    public ServerConfig Server { get; set; } = ServerConfig.Parse("wds://localhost:2807");

    /// <summary>
    /// Enum filed wrapper for Type
    /// </summary>
    [XmlIgnore]
    // ReSharper disable once UnusedMember.Global
    public string JobType
    {
        get => Type?.ToString();
        set => Type = value?.ParseEnum<JobTypes>();
    }

    /// <summary>
    /// Job name (if not set a random value is used each run)
    /// </summary>
    [XmlIgnore]
    public string JobName { get; set; }

    /// <summary>
    /// Optional. Initial URLs used to create download tasks on job start. A saved config may omit them, but at least one URL is required to start the job.
    /// </summary>
    public string[] StartUrls { get; set; }

    /// <summary>
    /// Optional. Crawler network profile. Internet uses public-source request gateways; Intranet uses internal-source gateways. Null uses the first service-configured allowed job type.
    /// </summary>
    public JobTypes? Type { get; set; }

    /// <summary>
    /// Optional. Additional HTTP headers for download requests. Null sends no additional default headers.
    /// </summary>
    public HeadersConfig Headers { get; set; }

    /// <summary>
    /// Optional. Behavior when starting a job that has prior tasks or cached data. Null uses Continue.
    /// </summary>
    public RestartConfig Restart { get; set; }

    /// <summary>
    /// Optional. HTTPS certificate-validation behavior. Null validates certificates normally.
    /// </summary>
    public HttpsConfig Https { get; set; }

    /// <summary>
    /// Optional. Cookie persistence behavior. Null does not persist cookies.
    /// </summary>
    public CookiesConfig Cookies { get; set; }

    /// <summary>
    /// Optional. Proxy routing, pool, and direct-request fallback behavior. Null uses non-proxy request gateways.
    /// </summary>
    public ProxiesConfig Proxy { get; set; }

    /// <summary>
    /// Optional. Failed-download policy. Null does not retry failed requests.
    /// </summary>
    public DownloadErrorHandling DownloadErrorHandling { get; set; }

    /// <summary>
    /// Optional. Response, timeout, redirect, and per-host pacing overrides. Null uses downloader defaults.
    /// </summary>
    public CrawlersProtectionBypass CrawlersProtectionBypass { get; set; }

    /// <summary>
    /// Optional. SQL CLR representation of the discovered-link host policy. Its policy value is currently excluded from API XML and therefore is not transmitted to Dapi.
    /// </summary>
    public CrossDomainAccess CrossDomainAccess { get; set; }

    /// <summary>
    /// Optional. Retrieval-index enrollment settings. Null does not enroll downloaded content.
    /// </summary>
    public RetrievalConfig Retrieval { get; set; }

    /// <summary>
    /// Optional. Host lifecycle settings such as robots.txt and cookie refresh cadence. Null uses the 7-day reinitialization default.
    /// </summary>
    public HostConfig Host { get; set; }

    /// <summary>
    /// Adds a new start URL
    /// </summary>
    /// <param name="url">Start Url</param>
    public JobConfig AddStartUrl(string url)
    {
        if (StartUrls is null)
            StartUrls = new[] { url };
        else if (!StartUrls.Contains(url))
        {
            var startUrls = StartUrls;
            Array.Resize(ref startUrls, startUrls.Length + 1);
            startUrls[startUrls.Length - 1] = url;
            StartUrls = startUrls;
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
        if (Server is null)
            throw new NullReferenceException(nameof(Server));

        if (StartUrls is null)
            throw new NullReferenceException(nameof(StartUrls));
        if (StartUrls.Length == 0)
            throw new InvalidDataException("StartUrls can't be empty");

        Headers?.Validate();
        Restart?.Validate();
        Https?.Validate();
        Cookies?.Validate();
        Proxy?.Validate();
        DownloadErrorHandling?.Validate();
        CrawlersProtectionBypass?.Validate();
        CrossDomainAccess?.Validate();
        Retrieval?.Validate();
        Host?.Validate();
    }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'JobName: job; Server: wds://host:port; StartUrls: https://host.com/section1, https://host.com/section2'
    /// </summary>
    /// <param name="s">Config string</param>
    public static JobConfig Parse(SqlString s)
    {
        var jobConfig = _dataContractSerializer.Parse(s.Value);
        jobConfig.JobName ??= Guid.NewGuid().ToString();
        return jobConfig;
    }

    private static readonly DataContractSerializer<JobConfig> _dataContractSerializer = new DataContractSerializer<JobConfig>()
        .MapValue<string>(nameof(Server), (jobConfig, serverCs) => jobConfig.Server = ServerConfig.Parse(serverCs))
        .MapArray<string>(nameof(StartUrls), (jobConfig, startUrls) => jobConfig.StartUrls = startUrls)
        .MapValue<string>(nameof(JobName), (jobConfig, jobName) => jobConfig.JobName = jobName);

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static JobConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        JobName = r.ReadString();
        Server = r.ReadNullable<ServerConfig>();
        StartUrls = r.ReadNullableStringArray();
        Type = r.ReadNullableEnum<JobTypes>();
        Headers = r.ReadNullable<HeadersConfig>();
        Restart = r.ReadNullable<RestartConfig>();
        Https = r.ReadNullable<HttpsConfig>();
        Cookies = r.ReadNullable<CookiesConfig>();
        Proxy = r.ReadNullable<ProxiesConfig>();
        DownloadErrorHandling = r.ReadNullable<DownloadErrorHandling>();
        CrawlersProtectionBypass = r.ReadNullable<CrawlersProtectionBypass>();
        // Added in V3
        CrossDomainAccess = r.TryReadNullable<CrossDomainAccess>(null);
        Retrieval = r.TryReadNullable<RetrievalConfig>(null);
        Host = r.TryReadNullable<HostConfig>(null);
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(JobName);
        w.WriteNullable(Server);
        w.WriteNullable(StartUrls);
        w.WriteNullableEnum(Type);
        w.WriteNullable(Headers);
        w.WriteNullable(Restart);
        w.WriteNullable(Https);
        w.WriteNullable(Cookies);
        w.WriteNullable(Proxy);
        w.WriteNullable(DownloadErrorHandling);
        w.WriteNullable(CrawlersProtectionBypass);
        w.WriteNullable(CrossDomainAccess);
        w.WriteNullable(Retrieval);
        w.WriteNullable(Host);
    }

    #endregion
}
