// ReSharper disable CheckNamespace

using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures optional enrollment of downloaded page content into the retrieval index.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RetrievalConfig : DataContractBase
{
    /// <summary>
    /// Optional. When true, convert successfully downloaded page content to markdown chunks and enroll it for retrieval. False is the default. Cached pages are skipped unless Force is true.
    /// </summary>
    public bool EnrollInIndex { get; set; }

    /// <summary>
    /// Optional. When true, enroll cached page content instead of skipping it. Has no effect when EnrollInIndex is false.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Optional. Maximum tokens per indexed chunk. Null uses 512.
    /// </summary>
    public int? MaxTokensPerChunk { get; set; }

    /// <summary>
    /// Optional. Ordered URL-path patterns and selectors for enrolled content. The first matching scope supplies the selector; if none matches, the whole page is enrolled.
    /// </summary>
    public RetrievalContentScope[] ContentScopes { get; private set; }

    /// <summary>
    /// Optional. When true, wait up to one minute per page for its snippets to become queryable before continuing. False is the default and returns after submitting enrollment.
    /// </summary>
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

    // ReSharper disable once UnusedMember.Global
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
