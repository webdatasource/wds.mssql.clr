// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Server;

/// <summary>
/// A special object for fluent configuration of a batch scrape request 
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ScrapeMultipleParams : ResponseDataContractBase
{
    private static readonly XmlSerializer _scrapeResultArraySerializer = new(typeof(ScrapeResult[]));
    private static readonly XmlSerializer _scrapeParamsArraySerializer = new(typeof(ScrapeParams[]));
    private List<ScrapeParams> _scrapeParams = new();
    private DownloadTask _downloadTask;
    private ScrapeResult[] _scrapeResults;

    /// <summary>
    /// Binds a download task
    /// </summary>
    /// <param name="downloadTask">Parent download task</param>
    public ScrapeMultipleParams BindDownloadTask(DownloadTask downloadTask)
    {
        _downloadTask = downloadTask;
        return this;
    }

    /// <summary>
    /// Add a new scrape parameter
    /// </summary>
    /// <param name="name">Scrape parameter name that is used to get scraped data</param>
    /// <param name="selector">Selector of data elements on a web page. CSS|css|XPATH|xpath: selector</param>
    /// <param name="attributeName">HTML attribute from which to get data. Optional. By default, a tag body text is returned</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ScrapeMultipleParams AddScrapeParams(string name, string selector, string attributeName) => AddScrapeParamsWithConvert(name, selector, attributeName, null);

    /// <summary>
    /// Add a new scrape parameter with an optional conversion
    /// </summary>
    private ScrapeMultipleParams AddScrapeParamsWithConvert(string name, string selector, string attributeName, string convert)
    {
        if (_scrapeResults is not null)
            throw new InvalidOperationException("Method Add can not be used when scrape parameters initialization is completed");
        _scrapeParams ??= new List<ScrapeParams>();
        _scrapeParams.Add(new ScrapeParams
        {
            Name = name,
            Selector = selector,
            AttributeName = attributeName,
            Convert = convert
        });
        return this;
    }

    /// <summary>
    /// Returns all scraped values
    /// </summary>
    /// <param name="name">Scrape parameter name</param>
    /// <returns>Scraped data (string) array</returns>
    public StringDataItems GetAll(string name) => new StringDataItems().AddDataItems(GetValues(name));

    /// <summary>
    /// Returns the first scraped value
    /// </summary>
    /// <param name="name">Scrape parameter name</param>
    /// <returns>Scraped data (string) or null if nothing found</returns>
    public SqlString GetFirst(string name) => GetValues(name).FirstOrDefault() ?? SqlString.Null;

    /// <summary>
    /// Performs a batch scrape request and returns all scraped values
    /// </summary>
    /// <param name="name">Scrape parameter name</param>
    /// <returns>Scraped data (string) array</returns>
    private string[] GetValues(string name)
    {
        if (_downloadTask is null)
            return Array.Empty<string>();

        if (_downloadTask.Error is not null)
            return new[] { _downloadTask.Error };

        if (Error is not null)
            return new[] { Error };

        if (_scrapeResults is null)
        {
            var uri = new Uri(_downloadTask.Server.Uri.Append($"/api/mssql/{ClrFunctions.API_VERSION}/tasks/{_downloadTask.Id}/scrape").ToString());
            if (!ServerApiXml.TryPost(uri, _scrapeParams.ToArray(), _scrapeParamsArraySerializer, _scrapeResultArraySerializer, out _scrapeResults, out var error))
            {
                Error = error;
                return new[] { Error };
            }
        }

        var scrapeResult = _scrapeResults.SingleOrDefault(r => r.Name == name);
        return scrapeResult is not null ? scrapeResult.Values : new[] { $"ERROR: Scrape parameter not found: {name}" };
    }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Error is not null)
            return;

        if (_downloadTask is null)
            throw new NullReferenceException("Download task is null");

        if (_scrapeParams is null)
            throw new NullReferenceException("No scrape parameters were added");

        foreach (var scrapeParam in _scrapeParams)
            scrapeParam.Validate();

        if (_scrapeResults is null)
            return;
        foreach (var scrapeResult in _scrapeResults)
            scrapeResult.Validate();
    }

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static ScrapeMultipleParams Null { get; } = new() { IsNull = true };

    public static ScrapeMultipleParams Parse(SqlString _) => throw new InvalidOperationException("Parse method is not supported");

    public override string ToString() => throw new InvalidOperationException("ToString method is not supported");

    protected override void BinaryRead(BinaryReader r)
    {
        _downloadTask = r.ReadNullable<DownloadTask>();
        _scrapeParams = new List<ScrapeParams>(r.ReadArrayNullable<ScrapeParams>());
        _scrapeResults = r.ReadArrayNullable<ScrapeResult>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteNullable(_downloadTask);
        w.WriteNullable(_scrapeParams);
        w.WriteNullable(_scrapeResults);
    }

    #endregion
}