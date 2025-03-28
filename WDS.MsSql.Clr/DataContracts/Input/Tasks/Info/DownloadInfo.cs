using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Download attempt information
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class DownloadInfo : ResponseDataContractBase
{
    /// <summary>
    /// HTTP method
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Request URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Was the request successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// HTTP status code <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Status"/>
    /// </summary>
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// HTTP reason phrase
    /// </summary>
    public string ReasonPhrase { get; set; }

    /// <summary>
    /// HTTP headers sent with the request
    /// </summary>
    public HttpHeader[] RequestHeaders { get; set; }

    /// <summary>
    /// HTTP headers received in the response
    /// </summary>
    public HttpHeader[] ResponseHeaders { get; set; }

    /// <summary>
    /// Cookies sent with the request
    /// </summary>
    public Cookie[] RequestCookies { get; set; }

    /// <summary>
    /// Cookies received in the response
    /// </summary>
    public Cookie[] ResponseCookies { get; set; }

    /// <summary>
    /// Request date and time in UTC
    /// </summary>
    public DateTime RequestDateUtc { get; set; }

    /// <summary>
    /// Download time in seconds
    /// </summary>
    public double DownloadTimeSec { get; set; }

    /// <summary>
    /// Is the request made via a proxy
    /// </summary>
    public bool ViaProxy { get; set; }

    /// <summary>
    /// What was the delay (in seconds) before the request was executed (crawl latency, etc.)
    /// </summary>
    public double WaitTimeSec { get; set; }

    /// <summary>
    /// A delay in seconds applied to the request
    /// </summary>
    public int CrawlDelaySec { get; set; }
    
    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Method is null)
            throw new NullReferenceException(nameof(Method));
        if (Url is null)
            throw new NullReferenceException(nameof(Url));
    }
    
    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<DownloadInfo> _dataContractSerializer = new();

    public static DownloadInfo Null { get; } = new() { IsNull = true };

    public static DownloadInfo Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Method = r.ReadString();
        Url = r.ReadString();
        IsSuccess = r.ReadBoolean();
        HttpStatusCode = r.ReadInt32();
        ReasonPhrase = r.ReadNullableString();
        RequestHeaders = r.ReadArrayNullable<HttpHeader>();
        ResponseHeaders = r.ReadArrayNullable<HttpHeader>();
        RequestCookies = r.ReadArrayNullable<Cookie>();
        ResponseCookies = r.ReadArrayNullable<Cookie>();
        RequestDateUtc = r.ReadDateTime();
        DownloadTimeSec = r.ReadDouble();
        ViaProxy = r.ReadBoolean();
        WaitTimeSec = r.ReadInt32();
        CrawlDelaySec = r.ReadInt32();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Method);
        w.Write(Url);
        w.Write(IsSuccess);
        w.Write(HttpStatusCode);
        w.WriteNullable(ReasonPhrase);
        w.WriteNullable(RequestHeaders);
        w.WriteNullable(ResponseHeaders);
        w.WriteNullable(RequestCookies);
        w.WriteNullable(ResponseCookies);
        w.Write(RequestDateUtc);
        w.Write(DownloadTimeSec);
        w.Write(ViaProxy);
        w.Write(WaitTimeSec);
        w.Write(CrawlDelaySec);
    }

    #endregion
}