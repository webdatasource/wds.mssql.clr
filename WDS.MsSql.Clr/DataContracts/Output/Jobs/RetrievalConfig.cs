using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Retrieval enrollment settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RetrievalConfig : DataContractBase
{
    public bool EnrollInIndex { get; set; }

    public bool Force { get; set; }

    public int? MaxTokensPerChunk { get; set; }

    public RetrievalContentScope[] ContentScopes { get; set; }

    public bool WaitForEnrolled { get; set; }

    public RetrievalConfig AddContentScope(RetrievalContentScope contentScope)
    {
        if (ContentScopes is null)
            ContentScopes = new[] { contentScope };
        else
        {
            var contentScopes = ContentScopes;
            Array.Resize(ref contentScopes, contentScopes.Length + 1);
            contentScopes[contentScopes.Length - 1] = contentScope;
            ContentScopes = contentScopes;
        }
        return this;
    }

    public override void Validate()
    {
        if (ContentScopes is null)
            return;
        foreach (var contentScope in ContentScopes)
            contentScope?.Validate();
    }

    private static readonly DataContractSerializer<RetrievalConfig> _dataContractSerializer = new DataContractSerializer<RetrievalConfig>()
        .MapValue<bool>(nameof(EnrollInIndex), (config, value) => config.EnrollInIndex = value)
        .MapValue<bool>(nameof(Force), (config, value) => config.Force = value)
        .MapValue<int>(nameof(MaxTokensPerChunk), (config, value) => config.MaxTokensPerChunk = value)
        .MapValue<bool>(nameof(WaitForEnrolled), (config, value) => config.WaitForEnrolled = value);

    public static RetrievalConfig Null { get; } = new() { IsNull = true };

    public static RetrievalConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        EnrollInIndex = r.ReadBoolean();
        Force = r.ReadBoolean();
        MaxTokensPerChunk = r.ReadNullableInt();
        ContentScopes = r.ReadArrayNullable<RetrievalContentScope>();
        WaitForEnrolled = r.ReadBoolean();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(EnrollInIndex);
        w.Write(Force);
        w.WriteNullable(MaxTokensPerChunk);
        w.WriteNullable(ContentScopes);
        w.Write(WaitForEnrolled);
    }
}
