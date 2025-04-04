using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Server;

public static partial class ClrFunctions
{
    private static readonly XmlSerializer _downloadTaskStatusSerializer = new(typeof(DownloadTaskStatus));

    /// <summary>
    /// Internal data contract to build status results table
    /// </summary>
    private struct StatusResultRow
    {
        /// <summary>
        /// Indicator name. Is mapped to the Name column on the result table 
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Indicator value. Is mapped to the Value column on the result table 
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Indicator description. Is mapped to the Description column on the result table 
        /// </summary>
        public readonly string Description;

        public StatusResultRow(string name, string value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }
    }

    /// <summary>
    /// Returns the server status
    /// </summary>
    /// <param name="serverConfig">Server config <see cref="ServerConfig"/></param>
    /// <returns>A table of status indicators</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ServerStatus),
        FillRowMethodName = nameof(FillStatusResultTableRow),
        TableDefinition = "Name NVARCHAR(255), Value NVARCHAR(MAX), Description NVARCHAR(MAX)")]
    public static IEnumerable ServerStatus(ServerConfig serverConfig)
    {
        var uri = BuildUri(serverConfig.Uri, "/ready");
        var isReady = ServerApiTxt.TryGet(uri, out var responseString, out var error);
        var statusResultRows = new List<StatusResultRow>
        {
            new("Ready", isReady.ToString(), isReady ? responseString : error)
        };
        return statusResultRows.ToArray();
    }

    /// <summary>
    /// Returns a status of a download task 
    /// </summary>
    /// <param name="downloadTask">Download task</param>
    /// <returns>A task status object <see cref="DownloadTaskStatus"/></returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(TaskStatus))]
    public static DownloadTaskStatus TaskStatus(DownloadTask downloadTask)
    {
        if (downloadTask == null)
            return DownloadTaskStatus.Null;
        
        if (downloadTask.Error is not null)
            return new DownloadTaskStatus
            {
                Error = downloadTask.Error
            };
        
        var uri = BuildUri(downloadTask.Server.Uri, $"/api/v1/tasks/{downloadTask.Id}/info");
        if (ServerApiXml.TryGet<DownloadTaskStatus>(uri, _downloadTaskStatusSerializer, out var downloadTaskStatus, out var error))
            return downloadTaskStatus;
        return new DownloadTaskStatus
        {
            Error = error
        };
    }
}