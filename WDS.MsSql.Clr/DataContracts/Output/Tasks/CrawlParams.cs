using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Crawl parameters sent to the Dapi MSSQL API
/// </summary>
public class CrawlParams
{
    public string Selector { get; set; }
    public string AttributeName { get; set; }

    private static readonly DataContractSerializer<CrawlParams> _dataContractSerializer = new();

    public override string ToString() => _dataContractSerializer.Serialize(this);
}