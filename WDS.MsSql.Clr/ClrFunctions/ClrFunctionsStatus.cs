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
        var statusResultRows = new List<StatusResultRow>();
        var isReady = _serverApiText.TryGet(serverConfig.Uri, "/ready", out var responseString, out var error);
        var isReadyDescription = isReady ? responseString : error;
        statusResultRows.Add(new StatusResultRow("Ready", isReady.ToString(), isReadyDescription));
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
        if (downloadTask.Error is not null)
            return new DownloadTaskStatus
            {
                Error = downloadTask.Error
            };
        var pathAndQuery = $"/api/v1/tasks/{downloadTask.Id}/info";
        if (_serverApiXml.TryGet<DownloadTaskStatus>(downloadTask.Server.Uri, pathAndQuery, _downloadTaskStatusSerializer, out var downloadTaskStatus, out var error))
            return downloadTaskStatus;
        return new DownloadTaskStatus
        {
            Error = error
        };
    }

    #region MS SQL CLR Required methods and properties

    private static void FillStatusResultTableRow(object obj, out SqlChars name, out SqlChars value, out SqlChars description)
    {
        var row = (StatusResultRow)obj;
        name = new SqlChars(row.Name);
        value = new SqlChars(row.Value);
        description = new SqlChars(row.Description);
    }

    #endregion
}