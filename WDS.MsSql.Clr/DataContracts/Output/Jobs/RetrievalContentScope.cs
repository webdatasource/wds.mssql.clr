using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Indexed content scope
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RetrievalContentScope : DataContractBase
{
    public string PathPattern { get; set; }

    public string Selector { get; set; }

    public override void Validate()
    {
        if (PathPattern is null)
            throw new NullReferenceException(nameof(PathPattern));
        if (Selector is null)
            throw new NullReferenceException(nameof(Selector));
    }

    private static readonly DataContractSerializer<RetrievalContentScope> _dataContractSerializer = new DataContractSerializer<RetrievalContentScope>()
        .MapValue<string>(nameof(PathPattern), (scope, value) => scope.PathPattern = value)
        .MapValue<string>(nameof(Selector), (scope, value) => scope.Selector = value);

    public static RetrievalContentScope Null { get; } = new() { IsNull = true };

    public static RetrievalContentScope Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        PathPattern = r.ReadString();
        Selector = r.ReadString();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(PathPattern);
        w.Write(Selector);
    }
}
