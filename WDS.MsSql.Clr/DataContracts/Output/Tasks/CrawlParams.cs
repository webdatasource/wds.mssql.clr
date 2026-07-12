// ReSharper disable CheckNamespace
// ReSharper disable UnusedAutoPropertyAccessor.Global

using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Defines link extraction for the MSSQL API. This CLR shape carries Selector and AttributeName but does not expose Dapi's MaxDepth setting.
/// </summary>
public class CrawlParams
{
    /// <summary>
    /// Selector used to find link-bearing elements. Use <c>CSS: &lt;selector&gt;</c>, <c>XPATH: &lt;selector&gt;</c>, or <c>*</c> to select all anchor elements.
    /// </summary>
    public string Selector { get; set; }

    /// <summary>
    /// Optional. Attribute read from each matched element to obtain a link. Use <c>val</c> for inner text. Null or empty uses <c>href</c>.
    /// </summary>
    public string AttributeName { get; set; }

    private static readonly DataContractSerializer<CrawlParams> _dataContractSerializer = new();

    public override string ToString() => _dataContractSerializer.Serialize(this);
}
