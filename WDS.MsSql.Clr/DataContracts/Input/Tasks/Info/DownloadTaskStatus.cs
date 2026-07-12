// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Reports the current state and request history for a download task.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class DownloadTaskStatus : ResponseDataContractBase
{
    /// <summary>
    /// Downloaded URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Enum filed wrapper for State
    /// </summary>
    [XmlIgnore]
    public string TaskState => State.ToString();

    /// <summary>
    /// Task state
    /// </summary>
    public DownloadTaskStates State { get; set; }

    /// <summary>
    /// Optional. Final download attempt details, or null when no attempt result is available.
    /// </summary>
    public DownloadInfo Result { get; set; }

    /// <summary>
    /// Optional. Earlier attempt details such as redirects or proxy retries. Null means no attempt history is available; an empty array means only the final result exists.
    /// </summary>
    public DownloadInfo[] IntermedResults { get; set; }

    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<DownloadTaskStatus> _dataContractSerializer = new();

    public static DownloadTaskStatus Null { get; } = new() { IsNull = true };

    // ReSharper disable once UnusedMember.Global
    public static DownloadTaskStatus Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        State = r.ReadEnum<DownloadTaskStates>();
        Result = r.ReadNullable<DownloadInfo>();
        IntermedResults = r.ReadArrayNullable<DownloadInfo>();
        // Added in V3
        Url = r.TryReadString(string.Empty);
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(State);
        w.WriteNullable(Result);
        w.WriteNullable(IntermedResults);
        w.Write(Url);
    }

    #endregion
}
