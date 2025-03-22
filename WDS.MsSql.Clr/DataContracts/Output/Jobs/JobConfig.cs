using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Job settings
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
    public string JobType
    {
        get => Type?.ToString();
        set => Type = value?.ParseEnum<JobTypes>();
    }

    /// <summary>
    /// Job name (if not set a random value is used each run)
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// Job start URLs
    /// </summary>
    public string[] StartUrls { get; set; }

    /// <summary>
    /// Job type <see cref="JobTypes"/>
    /// </summary>
    public JobTypes? Type { get; set; }

    /// <summary>
    /// Headers settings
    /// </summary>
    public HeadersConfig Headers { get; set; }

    /// <summary>
    /// Job restart settings
    /// </summary>
    public JobRestartConfig JobRestart { get; set; }

    /// <summary>
    /// HTTPS settings
    /// </summary>
    public HttpsConfig Https { get; set; }

    /// <summary>
    /// Cookies settings
    /// </summary>
    public CookiesConfig Cookies { get; set; }

    /// <summary>
    /// Proxy settings
    /// </summary>
    public ProxiesConfig Proxy { get; set; }

    /// <summary>
    /// Download errors handling settings
    /// </summary>
    public DownloadErrorHandlingPolicy DownloadErrorHandlingPolicy { get; set; }

    /// <summary>
    /// Crawlers protection countermeasure settings
    /// </summary>
    public CrawlersProtectionBypass CrawlersProtectionBypass { get; set; }

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
        if(Server is null)
            throw new NullReferenceException(nameof(Server));
        
        if (StartUrls is null)
            throw new NullReferenceException(nameof(StartUrls));
        if (StartUrls.Length == 0)
            throw new InvalidDataException("StartUrls can't be empty");

        if (Headers is not null)
            Headers.Validate();

        if (JobRestart is not null)
            JobRestart.Validate();

        if (Https is not null)
            Https.Validate();

        if (Cookies is not null)
            Cookies.Validate();

        if (Proxy is not null)
            Proxy.Validate();

        if (DownloadErrorHandlingPolicy is not null)
            DownloadErrorHandlingPolicy.Validate();

        if (CrawlersProtectionBypass is not null)
            CrawlersProtectionBypass.Validate();
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
        .MapValue<string>("Server", (jobConfig, serverCs) => jobConfig.Server = ServerConfig.Parse(serverCs))
        .MapArray<string>("StartUrls", (jobConfig, startUrls) => jobConfig.StartUrls = startUrls)
        .MapValue<string>("JobName", (jobConfig, jobName) => jobConfig.JobName = jobName);

    #region MS SQL CLR Required methods and properties

    public static JobConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        JobName = r.ReadString();
        Server = r.ReadNullable<ServerConfig>();
        StartUrls = r.ReadNullableStringArray();
        Type = r.ReadNullableEnum<JobTypes>();
        Headers = r.ReadNullable<HeadersConfig>();
        JobRestart = r.ReadNullable<JobRestartConfig>();
        Https = r.ReadNullable<HttpsConfig>();
        Cookies = r.ReadNullable<CookiesConfig>();
        Proxy = r.ReadNullable<ProxiesConfig>();
        DownloadErrorHandlingPolicy = r.ReadNullable<DownloadErrorHandlingPolicy>();
        CrawlersProtectionBypass = r.ReadNullable<CrawlersProtectionBypass>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(JobName);
        w.WriteNullable(Server);
        w.WriteNullable(StartUrls);
        w.WriteNullableEnum(Type);
        w.WriteNullable(Headers);
        w.WriteNullable(JobRestart);
        w.WriteNullable(Https);
        w.WriteNullable(Cookies);
        w.WriteNullable(Proxy);
        w.WriteNullable(DownloadErrorHandlingPolicy);
        w.WriteNullable(CrawlersProtectionBypass);
    }

    #endregion
}