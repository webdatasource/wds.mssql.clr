using System;
using System.Data.SqlTypes;
using System.Text;

public static partial class ClrFunctions
{
    #region Utils
    
    private static Uri BuildUri(Uri baseUrl, string path)
    {
        return BuildUri(baseUrl, path, null, null);
    }

    private static Uri BuildUri(Uri baseUrl, string path, string selector, string attributeName)
    {
        var queryBuilder = new StringBuilder(path);
        if (selector is not null)
            queryBuilder.AppendFormat("?selector={0}", selector);
        if (attributeName is not null)
        {
            if (selector is null)
                throw new InvalidOperationException("Selector must be specified if attributeName is specified");
            queryBuilder.AppendFormat("&attributeName={0}", attributeName);
        }

        return new Uri(baseUrl, queryBuilder.ToString());
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