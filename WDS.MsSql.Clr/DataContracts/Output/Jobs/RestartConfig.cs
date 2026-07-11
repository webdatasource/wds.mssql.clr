// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Defines what to do if a failed or completed job is restarted.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class RestartConfig : DataContractBase
{
    /// <summary>
    /// String wrapper for JobRestartMode
    /// </summary>
    public string RestartMode
    {
        get => JobRestartMode.ToString();
        set => JobRestartMode = value.ParseEnum<JobRestartModes>();
    }
    
    /// <summary>
    /// Job restart mode. <see cref="JobRestartModes"/>
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