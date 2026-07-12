// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures whether a restarted job keeps previous task data or starts clean.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RestartConfig : DataContractBase
{
    /// <summary>
    /// Selects the job restart mode through its SQL-facing string representation.
    /// </summary>
    public string RestartMode
    {
        get => JobRestartMode.ToString();
        set => JobRestartMode = value.ParseEnum<JobRestartModes>();
    }
    
    /// <summary>
    /// Optional. Controls whether a restarted job keeps previous task data or starts clean. Continue is the default and resumes existing job data; FromScratch removes job tasks and related service data before crawling again from StartUrls.
    /// </summary>
    public JobRestartModes JobRestartMode { get; private set; }

    /// <summary>
    /// Creates a new instance and fills it out from a config string of the following format 'RestartMode: <see cref="RestartMode">Possible values</see>'
    /// </summary>
    /// <param name="s">Config string</param>
    public static RestartConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<RestartConfig> _dataContractSerializer = new DataContractSerializer<RestartConfig>()
        .MapValue<string>(nameof(RestartMode), (jobRestartConfig, jobRestartMode) => jobRestartConfig.RestartMode = jobRestartMode);

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static RestartConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        JobRestartMode = r.ReadEnum<JobRestartModes>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(JobRestartMode);
    }

    #endregion
}
