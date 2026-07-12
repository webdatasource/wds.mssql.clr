// ReSharper disable CheckNamespace

using System;
using System.IO;
using WDS.MsSql.Clr.Serialization;

/// <summary>
/// Represents values extracted for one named scrape field.
/// </summary>
public class ScrapeResult: ResponseDataContractBase
{
    /// <summary>
    /// Optional. Field name copied from the matching ScrapeParams entry.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Scraped values
    /// </summary>
    public string[] Values { get; set; }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Name is null)
            throw new NullReferenceException(nameof(Name));
        if (Values is null)
            throw new NullReferenceException(nameof(Values));
    }

    #region MS SQL CLR Required methods and properties
    
    protected override void BinaryRead(BinaryReader r)
    {
        Name = r.ReadString();
        Values = r.ReadNullableStringArray();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Name);
        w.WriteNullable(Values);
    }

    #endregion
}
