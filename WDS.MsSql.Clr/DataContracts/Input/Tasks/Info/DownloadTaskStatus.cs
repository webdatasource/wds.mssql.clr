using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Download task execution status
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class DownloadTaskStatus : ResponseDataContractBase
{
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
    /// Download result
    /// </summary>
    public DownloadInfo Result { get; set; }

    /// <summary>
    /// Intermediate requests download results stack
    /// </summary>
    public DownloadInfo[] IntermedResults { get; set; }


    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<DownloadTaskStatus> _dataContractSerializer = new();

    public static DownloadTaskStatus Null { get; } = new() { IsNull = true };

    public static DownloadTaskStatus Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        State = r.ReadEnum<DownloadTaskStates>();
        Result = r.ReadNullable<DownloadInfo>();
        IntermedResults = r.ReadArrayNullable<DownloadInfo>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(State);
        w.WriteNullable(Result);
        w.WriteNullable(IntermedResults);
    }

    #endregion
}