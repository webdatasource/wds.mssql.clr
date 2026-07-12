// ReSharper disable UnusedMember.Global

using System;
using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Server;

// ReSharper disable once CheckNamespace
public static partial class ClrFunctions
{
    private static readonly XmlSerializer _jobConfigSerializer = new(typeof(JobConfig));
    private static readonly XmlSerializer _crawlParamsSerializer = new(typeof(CrawlParams));
    private static readonly XmlSerializer _stringArraySerializer = new(typeof(string[]));
    private static readonly XmlSerializer _downloadTaskArraySerializer = new(typeof(DownloadTask[]));

    /// <summary>
    /// Starts or restarts the job named in the supplied configuration after applying its crawler settings, then returns its initial download tasks.
    /// </summary>
    /// <param name="jobConfig">Supplies the server connection, job name, start URLs, and crawler settings. Its job name selects the MSSQL-compatible job to configure and start.</param>
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
        var uri = BuildUri(jobConfig.Server.Uri, $"/api/mssql/{API_VERSION}/jobs/{jobConfig.JobName}/start");
        if (ServerApiXml.TryPost<JobConfig, string[]>(uri, jobConfig, _jobConfigSerializer, _stringArraySerializer, out var taskIds, out var error))
            return BuildInitialTasks(taskIds, jobConfig.StartUrls, jobConfig.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = error
            }
        };
    }

    /// <summary>
    /// Extracts links from the supplied task's downloaded page and creates child download tasks, automatically following pending server-side results.
    /// </summary>
    /// <param name="downloadTask">Supplies the download task ID and server connection for the crawl request. A null task returns no rows; a task that already contains an error is returned unchanged.</param>
    /// <param name="selector">Selector used to find link-bearing elements. Use <c>CSS: &lt;selector&gt;</c>, <c>XPATH: &lt;selector&gt;</c>, or <c>*</c> to select all anchor elements.</param>
    /// <param name="attributeName">Optional. Attribute read from each matched element to obtain a link. Use <c>val</c> for inner text; null or empty uses <c>href</c>.</param>
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

        var uri = BuildUri(downloadTask.Server.Uri, $"/api/mssql/{API_VERSION}/tasks/{downloadTask.Id}/crawl");
        var crawlParams = new CrawlParams
        {
            Selector = selector,
            AttributeName = attributeName
        };
        if (ServerApiXml.TryPost<CrawlParams, DownloadTask[]>(uri, crawlParams, _crawlParamsSerializer, _downloadTaskArraySerializer, out var tasks, out var error))
            return BuildTasks(tasks, downloadTask.Server);
        return new[]
        {
            new DownloadTask
            {
                Error = error
            }
        };
    }

    private static IEnumerable BuildInitialTasks(string[] taskIds, string[] startUrls, ServerConfig serverConfig) => taskIds.Select((t, i) => new DownloadTask
    {
        Id = t,
        Url = startUrls != null && i < startUrls.Length ? startUrls[i] : string.Empty,
        Server = serverConfig
    });


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
