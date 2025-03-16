using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Defines what to do if a failed or completed job is restarted.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class JobRestartConfig : DataContractBase
{
    /// <summary>
    /// Enum filed wrapper for RestartMode
    /// </summary>
    public string JobRestartMode
    {
        get => RestartMode.ToString();
        set => RestartMode = value.ParseEnum<JobRestartModes>();
    }
    
    /// <summary>
    /// Job restart mode. <see cref="JobRestartModes"/>
    /// </summary>
    public JobRestartModes RestartMode { get; set; }

    /// <summary>
    /// Creates a new instance and fills out it from a config string of the following format 'JobRestartMode: <see cref="JobRestartModes">Possible values</see>'
    /// </summary>
    /// <param name="s">Config string</param>
    public static JobRestartConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<JobRestartConfig> _dataContractSerializer = new DataContractSerializer<JobRestartConfig>()
        .MapValue<string>("JobRestartMode", (jobRestartConfig, jobRestartMode) => jobRestartConfig.JobRestartMode = jobRestartMode);

    #region MS SQL CLR Required methods and properties

    public static JobRestartConfig Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        RestartMode = r.ReadEnum<JobRestartModes>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(RestartMode);
    }

    #endregion
}