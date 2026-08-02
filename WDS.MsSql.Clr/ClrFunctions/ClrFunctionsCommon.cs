using System;
using System.Data.SqlTypes;
using WDS.MsSql.Clr;

// ReSharper disable once CheckNamespace
public static partial class ClrFunctions
{
    public const string API_VERSION = "v3";

    #region Utils

    private static Uri BuildUri(Uri baseUri, string path) => new(baseUri.Append(path).ToString());

    #endregion

    #region MS SQL CLR Specific methods

    private static void FillStringTableRow(object obj, out SqlString result) => result = obj?.ToString() ?? SqlString.Null;

    private static void FillIntentTableRow(object obj, out DownloadTask result)
    {
        if (obj is DownloadTask downloadTask)
            result = downloadTask;
        else
            result = DownloadTask.Null;
    }

    private static void FillStatusResultTableRow(object obj, out SqlChars name, out SqlChars value, out SqlChars description)
    {
        if (obj is StatusResultRow row)
        {
            name = new SqlChars(row.Name);
            value = new SqlChars(row.Value);
            description = new SqlChars(row.Description);
        }
        else
        {
            name = SqlChars.Null;
            value = SqlChars.Null;
            description = SqlChars.Null;
        }
    }

    #endregion
}
