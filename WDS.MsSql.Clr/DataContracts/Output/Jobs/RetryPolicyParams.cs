using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Retry settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RetryPolicyParams : DataContractBase
{
    /// <summary>
    /// Delay between retries in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; }

    /// <summary>
    /// Maximum number of retries
    /// </summary>
    public int RetriesLimit { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'RetryDelayMs: 1000; RetriesLimit: 3'
    /// </summary>
    /// <param name="s">Config string</param>
    public static RetryPolicyParams Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<RetryPolicyParams> _dataContractSerializer = new DataContractSerializer<RetryPolicyParams>()
        .MapValue<int>("RetryDelayMs", (retryPolicyParams, retryDelayMs) => retryPolicyParams.RetryDelayMs = retryDelayMs)
        .MapValue<int>("RetriesLimit", (retryPolicyParams, retriesLimit) => retryPolicyParams.RetriesLimit = retriesLimit);

    #region MS SQL CLR Required methods and properties
    
    public static RetryPolicyParams Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        RetryDelayMs = r.ReadInt32();
        RetriesLimit = r.ReadInt32();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(RetryDelayMs);
        w.Write(RetriesLimit);
    }

    #endregion
}