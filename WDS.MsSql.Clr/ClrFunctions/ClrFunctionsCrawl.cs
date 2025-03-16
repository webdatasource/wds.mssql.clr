using System;
using System.Collections;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;
using WDS.MsSql.Clr.Server;

public static partial class ClrFunctions
{
    private static readonly XmlSerializer _downloadTaskArraySerializer = new(typeof(DownloadTask[]));

    /// <summary>
    /// Starts a new job with the specified configuration
    /// </summary>
    /// <param name="jobConfig">Job configuration</param>
    /// <returns>Download tasks array (of the count of the StartUrls list)</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(Start),
        FillRowMethodName = nameof(FillIntentTableRow),
        TableDefinition = "Task wds.DownloadTask")]
    public static IEnumerable Start(JobConfig jobConfig)
    {
        const string pathAndQuery = "/api/v1/jobs/start";
        if (_serverApiXml.TryPost(jobConfig.Server.Uri, pathAndQuery, jobConfig.ToString(), out var statusCode, out var responseString))
            if (!string.IsNullOrWhiteSpace(responseString))
                return BuildTasks(responseString, jobConfig.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = ServerApi.BuildApiRequestError(statusCode, responseString)
            }
        };
    }

    /// <summary>
    /// Performs a download task to collect subsequent pages of web resources
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of links on a web page. Format CSS|XPATH: selector</param>
    /// <returns>Subsequent download tasks array with URLs matched to the selector</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(Crawl),
        FillRowMethodName = nameof(FillIntentTableRow),
        TableDefinition = "Task wds.DownloadTask")]
    public static IEnumerable Crawl(DownloadTask downloadTask, string selector)
    {
        if (downloadTask.Error is not null)
            return new[] { downloadTask };
        var pathAndQuery = $"/api/v1/tasks/{downloadTask.Id}/crawl?selector={selector}";
        if (_serverApiXml.TryGet(downloadTask.Server.Uri, pathAndQuery, out var statusCode, out var responseString))
            if (!string.IsNullOrWhiteSpace(responseString))
                return BuildTasks(responseString, downloadTask.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = ServerApi.BuildApiRequestError(statusCode, responseString)
            }
        };
    }

    /// <summary>
    /// Builds download tasks from an XML string
    /// </summary>
    /// <param name="responseString">Tasks XML string</param>
    /// <param name="serverConfig">Current job server config</param>
    /// <returns>Array of download tasks</returns>
    private static IEnumerable BuildTasks(string responseString, ServerConfig serverConfig)
    {
        var tasks = responseString.Deserialize<DownloadTask[]>(_downloadTaskArraySerializer);
        foreach (var task in tasks)
            task.Server = serverConfig;
        return tasks;
    }

    #region MS SQL CLR Required methods and properties

    private static void FillIntentTableRow(object obj, out DownloadTask intentRow) => intentRow = (DownloadTask)obj;

    #endregion
}