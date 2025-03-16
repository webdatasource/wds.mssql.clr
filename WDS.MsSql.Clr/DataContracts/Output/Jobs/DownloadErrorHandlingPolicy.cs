using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Download error handling policy
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class DownloadErrorHandlingPolicy : DataContractBase
{
    /// <summary>
    /// Enum filed wrapper for Policy
    /// </summary>
    [XmlIgnore]
    public string ErrorHandlingPolicy
    {
        get => Policy.ToString();
        set => Policy = value.ParseEnum<DownloadErrorHandlingPolicies>();
    }
    
    /// <summary>
    /// Download error handling policy
    /// </summary>
    public DownloadErrorHandlingPolicies Policy { get; set; }

    /// <summary>
    /// Retry settings (if not set 0 values are used)
    /// </summary>
    public RetryPolicyParams RetryPolicyParams { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'ErrorHandlingPolicy: <see cref="DownloadErrorHandlingPolicies">Possible values</see>'
    /// </summary>
    /// <param name="s">Config string</param>
    public static DownloadErrorHandlingPolicy Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<DownloadErrorHandlingPolicy> _dataContractSerializer = new DataContractSerializer<DownloadErrorHandlingPolicy>()
        .MapValue<string>("ErrorHandlingPolicy", (downloadErrorHandlingPolicy, errorHandlingPolicy) => downloadErrorHandlingPolicy.ErrorHandlingPolicy = errorHandlingPolicy);

    #region MS SQL CLR Required methods and properties

    public static DownloadErrorHandlingPolicy Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Policy = r.ReadEnum<DownloadErrorHandlingPolicies>();
        RetryPolicyParams = r.ReadNullable<RetryPolicyParams>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(Policy);
        w.WriteNullable(RetryPolicyParams);
    }

    #endregion
}