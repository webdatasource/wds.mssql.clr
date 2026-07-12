// ReSharper disable CheckNamespace

using System;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;

/// <summary>
/// Defines one named field extracted from pages by selector, attribute, and optional conversion.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class ScrapeParams : DataContractBase
{
    /// <summary>
    /// Name of the output field populated from this extraction rule. Names must be unique within one scrape request or Traversal level.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Selector used to read matching elements. Use <c>CSS: &lt;selector&gt;</c> or <c>XPATH: &lt;selector&gt;</c>. Use <c>*</c> to target the whole page; when used, AttributeName is ignored.
    /// </summary>
    public string Selector { get; set; }

    /// <summary>
    /// Optional. Attribute read from each matched element. Use <c>val</c>, or pass null or empty, to return inner text.
    /// </summary>
    public string AttributeName { get; set; }

    /// <summary>
    /// Optional. Conversion applied to scraped values. Use <c>md()</c> for Markdown or <c>sr()</c> for main readable content using Mozilla Readability. Null applies no conversion.
    /// </summary>
    public string Convert { get; set; }

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
        // Added in V3
        Convert = r.TryReadNullableString(null);
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Name);
        w.Write(Selector);
        w.WriteNullable(AttributeName);
        w.WriteNullable(Convert);
    }

    #endregion
}
