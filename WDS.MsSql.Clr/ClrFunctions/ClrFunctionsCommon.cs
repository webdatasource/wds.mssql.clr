using System;
using System.Data.SqlTypes;
using WDS.MsSql.Clr;

public static partial class ClrFunctions
{
    public const string API_VERSION = "v2";

    #region Utils

    private static Uri BuildUri(Uri baseUri, string path, string selector = null, string attributeName = null)
    {
        var builder = baseUri.Append(path);
        if (selector is not null)
            builder.AppendFormat("?selector={0}", selector);
        if (attributeName is null)
            return new Uri(builder.ToString());
        if (selector is null)
            throw new InvalidOperationException("Selector must be specified if attributeName is specified");
        builder.AppendFormat("&attributeName={0}", attributeName);
        return new Uri(builder.ToString());
    }

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