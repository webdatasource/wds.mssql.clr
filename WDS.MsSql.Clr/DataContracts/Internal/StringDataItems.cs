using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// A special object for the ToStringsTable function
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class StringDataItems : ResponseDataContractBase
{
    public List<string> DataItems = new();

    /// <summary>
    /// Adds data items
    /// </summary>
    /// <param name="items">Data items</param>
    public StringDataItems AddDataItems(string[] items)
    {
        DataItems.AddRange(items);
        return this;
    }

    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<StringDataItems> _dataContractSerializer = new DataContractSerializer<StringDataItems>();
    
    public static StringDataItems Null { get; } = new() { IsNull = true };

    public static StringDataItems Parse(SqlString s) => throw new InvalidOperationException("Parse method is not supported");

    public override string ToString() => _dataContractSerializer.Serialize(this);
    
    protected override void BinaryRead(BinaryReader r)
    {
        DataItems = new List<string>(r.ReadStringArray());
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(DataItems);
    }
    
    #endregion
}