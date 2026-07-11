using System;
using System.IO;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Crawl parameters sent to the Dapi MSSQL API
/// </summary>
public class CrawlParams : DataContractBase
{
    public string Selector { get; set; }

    public string AttributeName { get; set; }

    public int? MaxDepth { get; set; }

    public override void Validate()
    {
        if (Selector is null)
            throw new NullReferenceException(nameof(Selector));
    }

    private static readonly DataContractSerializer<CrawlParams> _dataContractSerializer = new();

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Selector = r.ReadString();
        AttributeName = r.ReadNullableString();
        MaxDepth = r.ReadNullableInt();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Selector);
        w.WriteNullable(AttributeName);
        w.WriteNullable(MaxDepth);
    }
}
