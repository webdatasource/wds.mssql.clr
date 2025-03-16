using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Server;

public static partial class ClrFunctions
{
    private static readonly XmlSerializer _stringArraySerializer = new(typeof(string[]));

    /// <summary>
    /// Performs a scrape task on the server side to extract data from a web page
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of data elements on a web page. Format CSS|XPATH: selector</param>
    /// <param name="attributeName">HTML attribute name to get data from. Optional. By default, an HTML tag inner text is taken</param>
    /// <returns></returns>
    private static string[] Scrape(DownloadTask downloadTask, string selector, string attributeName)
    {
        if (downloadTask.Error is not null)
            return new[] { downloadTask.Error };
        var pathAndQuery = $"/api/v1/tasks/{downloadTask.Id}/scrape?selector={selector}";
        if (attributeName is not null)
            pathAndQuery += $"&attributeName={attributeName}";
        if (_serverApiXml.TryGet(downloadTask.Server.Uri, pathAndQuery, out var statusCode, out var responseString))
            if (!string.IsNullOrWhiteSpace(responseString))
                return responseString.Deserialize<string[]>(_stringArraySerializer);
        return new[] { ServerApi.BuildApiRequestError(statusCode, responseString) };
    }

    /// <summary>
    /// Scrapes all data elements from a web page by the specified CSS selector
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of data elements on a web page. Format CSS|XPATH: selector</param>
    /// <param name="attributeName">HTML attribute name to get data from. Optional. By default, an HTML tag inner text is taken</param>
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
    /// Scrapes first data elements from a web page by the specified CSS selector
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of data elements on a web page. Format CSS|XPATH: selector</param>
    /// <param name="attributeName">HTML attribute name to get data from. Optional. By default, an HTML tag inner text is taken</param>
    /// <returns>Scraped data (string) or null if nothing found</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ScrapeFirst))]
    public static SqlString ScrapeFirst(DownloadTask downloadTask, string selector, string attributeName) => Scrape(downloadTask, selector, attributeName).FirstOrDefault();


    /// <summary>
    /// A batch approach of scraping data from a page by the specified CSS selectors. <see cref="ScrapeMultipleParams"/>
    /// </summary>
    /// <param name="downloadTask">Download task</param>
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