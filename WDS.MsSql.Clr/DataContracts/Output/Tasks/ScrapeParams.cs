using System;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;

/// <summary>
/// Scrape parameters
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ScrapeParams : DataContractBase
{
    /// <summary>
    /// Scrape parameter name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Scrape selector
    /// </summary>
    public string Selector { get; set; }
    /// <summary>
    /// HTML attribute from which to get data. Optional. By default, a tag body text is returned
    /// </summary>
    public string AttributeName { get; set; }
    
    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Name is null)
            throw new NullReferenceException(nameof(Name));
        if (Selector is null)
            throw new NullReferenceException(nameof(Selector));
    }

    #region MS SQL CLR Required methods and properties
    
    protected override void BinaryRead(BinaryReader r)
    {
        Name = r.ReadString();
        Selector = r.ReadString();
        AttributeName = r.ReadNullableString();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Name);
        w.Write(Selector);
        w.WriteNullable(AttributeName);
    }

    #endregion
}