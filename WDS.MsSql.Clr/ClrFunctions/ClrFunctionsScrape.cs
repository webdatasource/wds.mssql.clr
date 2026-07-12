// ReSharper disable UnusedMember.Global

using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Server;

// ReSharper disable once CheckNamespace
public static partial class ClrFunctions
{
    private const string _singleScrapeName = "Data";
    private static readonly XmlSerializer _scrapeParamsArraySerializer = new(typeof(ScrapeParams[]));
    private static readonly XmlSerializer _scrapeResultArraySerializer = new(typeof(ScrapeResult[]));

    /// <summary>
    /// Performs a scrape task on the server side to extract data from a web page
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of data elements on a web page. Format CSS|XPATH: selector</param>
    /// <param name="attributeName">HTML attribute name to get data from. Optional. By default, an HTML tag inner text is taken</param>
    /// <returns></returns>
    private static string[] Scrape(DownloadTask downloadTask, string selector, string attributeName)
    {
        if (downloadTask is null)
            return Array.Empty<string>();

        if (downloadTask.Error is not null)
            return new[] { downloadTask.Error };

        var uri = BuildUri(downloadTask.Server.Uri, $"/api/mssql/{API_VERSION}/tasks/{downloadTask.Id}/scrape");
        var scrapeParams = new[]
        {
            new ScrapeParams
            {
                Name = _singleScrapeName,
                Selector = selector,
                AttributeName = attributeName
            }
        };
        if (!ServerApiXml.TryPost<ScrapeParams[], ScrapeResult[]>(uri, scrapeParams, _scrapeParamsArraySerializer, _scrapeResultArraySerializer, out var results, out var error))
            return new[] { error };
        return results.SingleOrDefault(result => result.Name == _singleScrapeName)?.Values ?? Array.Empty<string>();
    }

    /// <summary>
    /// Extracts every value matching the selector from the supplied task's downloaded page, automatically following pending server-side results.
    /// </summary>
    /// <param name="downloadTask">Supplies the download task ID and server connection for the scrape request. A null task produces no values; a task that already contains an error produces that error as a value.</param>
    /// <param name="selector">Selector used to read matching elements. Use <c>CSS: &lt;selector&gt;</c>, <c>XPATH: &lt;selector&gt;</c>, or <c>*</c> to target the whole page.</param>
    /// <param name="attributeName">Optional. Attribute read from each matched element. Use <c>val</c>, or pass null or empty, to return inner text.</param>
    /// <returns>A table with one Data column with all the data of the web page elements that match the specified selector in its rows</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ScrapeAll),
        FillRowMethodName = nameof(FillStringTableRow),
        TableDefinition = "Data NVARCHAR(MAX)")]
    public static IEnumerable ScrapeAll(DownloadTask downloadTask, string selector, string attributeName) => Scrape(downloadTask, selector, attributeName);


    /// <summary>
    /// Extracts the first value matching the selector from the supplied task's downloaded page, automatically following pending server-side results.
    /// </summary>
    /// <param name="downloadTask">Supplies the download task ID and server connection for the scrape request. A null task produces no values; a task that already contains an error produces that error as a value.</param>
    /// <param name="selector">Selector used to read matching elements. Use <c>CSS: &lt;selector&gt;</c>, <c>XPATH: &lt;selector&gt;</c>, or <c>*</c> to target the whole page.</param>
    /// <param name="attributeName">Optional. Attribute read from each matched element. Use <c>val</c>, or pass null or empty, to return inner text.</param>
    /// <returns>Scraped data (string) or null if nothing found</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ScrapeFirst))]
    public static SqlString ScrapeFirst(DownloadTask downloadTask, string selector, string attributeName) => Scrape(downloadTask, selector, attributeName).FirstOrDefault() ?? SqlString.Null;


    /// <summary>
    /// Creates a fluent batch-scrape builder bound to a download task. The server request is sent only when the builder retrieves values.
    /// </summary>
    /// <param name="downloadTask">Supplies the download task used by the later batch request. A null task remains unbound and later retrieval produces no values.</param>
    /// <returns><see cref="ScrapeMultipleParams"/></returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ScrapeMultiple))]
    public static ScrapeMultipleParams ScrapeMultiple(DownloadTask downloadTask)
    {
        return new ScrapeMultipleParams().BindDownloadTask(downloadTask);
    }
}
