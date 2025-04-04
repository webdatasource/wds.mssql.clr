using System.Collections;
using Microsoft.SqlServer.Server;

public static partial class ClrFunctions
{
    /// <summary>
    /// Represents input items as a table. Is used together with ScrapeMultipleParams.GetAll
    /// </summary>
    /// <param name="items">Input items</param>
    /// <returns>Input items table</returns>
    [SqlFunction(
        DataAccess = DataAccessKind.None,
        SystemDataAccess = SystemDataAccessKind.None,
        IsDeterministic = false,
        IsPrecise = false,
        Name = nameof(ToStringsTable),
        FillRowMethodName = nameof(FillStringTableRow),
        TableDefinition = "Data NVARCHAR(MAX)")]
    public static IEnumerable ToStringsTable(StringDataItems items) => items.DataItems;
}