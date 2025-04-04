using System;
using System.Collections;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
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
        var uri = BuildUri(jobConfig.Server.Uri, "/api/v1/jobs/start");
        if (ServerApiXml.TryPost<DownloadTask[]>(uri, jobConfig.ToString(), _downloadTaskArraySerializer, out var tasks, out var error))
            return BuildTasks(tasks, jobConfig.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = error
            }
        };
    }

    /// <summary>
    /// Performs a download task to collect subsequent pages of web resources
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <param name="selector">Selector of links on a web page. Format CSS|XPATH: selector</param>
    /// <param name="attributeName">HTML attribute name to get data from. Optional. By default, href</param>
    /// <returns>Subsequent download tasks array with URLs matched to the selector</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(Crawl),
        FillRowMethodName = nameof(FillIntentTableRow),
        TableDefinition = "Task wds.DownloadTask")]
    public static IEnumerable Crawl(DownloadTask downloadTask, string selector, string attributeName)
    {
        if (downloadTask is null)
            return Array.Empty<DownloadTask>();
        
        if (downloadTask.Error is not null)
            return new[] { downloadTask };
        
        var uri = BuildUri(downloadTask.Server.Uri, $"/api/v1/tasks/{downloadTask.Id}/crawl", selector, attributeName);
        if (ServerApiXml.TryGet<DownloadTask[]>(uri, _downloadTaskArraySerializer, out var tasks, out var error))
            return BuildTasks(tasks, downloadTask.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = error
            }
        };
    }

    /// <summary>
    /// Builds download tasks from an XML string
    /// </summary>
    /// <param name="tasks">Tasks</param>
    /// <param name="serverConfig">Current job server config</param>
    /// <returns>Array of download tasks</returns>
    private static IEnumerable BuildTasks(DownloadTask[] tasks, ServerConfig serverConfig)
    {
        foreach (var task in tasks)
            task.Server = serverConfig;
        return tasks;
    }
}