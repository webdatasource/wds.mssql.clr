using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// HTTP header
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class HttpHeader : DataContractBase
{
    /// <summary>
    /// Header name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Header values
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

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'Name: name; Values: value1, value2;'
    /// </summary>
    /// <param name="s">Config string</param>
    public static HttpHeader Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);
    private static readonly DataContractSerializer<HttpHeader> _dataContractSerializer = new DataContractSerializer<HttpHeader>()
        .MapValue<string>("Name", (requestHeader, name) => requestHeader.Name = name)
        .MapArray<string>("Values", (requestHeader, values) => requestHeader.Values = values);

    #region MS SQL CLR Required methods and properties
    
    public static HttpHeader Null { get; } = new() { IsNull = true };
    
    public override string ToString() => _dataContractSerializer.Serialize(this);
    
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