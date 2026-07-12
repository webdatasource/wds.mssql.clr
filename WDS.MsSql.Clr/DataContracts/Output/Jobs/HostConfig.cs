// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Configures host lifecycle settings such as robots.txt and cookie refresh cadence.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class HostConfig : DataContractBase
{
    /// <summary>
    /// Optional. Host reinitialization interval in seconds. Reinitialization refreshes robots.txt content and renews cookies. Null uses 604800 seconds (7 days).
    /// </summary>
    public int? ReinitPeriodSec { get; set; }

    private static readonly DataContractSerializer<HostConfig> _dataContractSerializer = new DataContractSerializer<HostConfig>()
        .MapValue<int>(nameof(ReinitPeriodSec), (config, value) => config.ReinitPeriodSec = value);

    // ReSharper disable once UnusedMember.Global
    public static HostConfig Null { get; } = new() { IsNull = true };

    public static HostConfig Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r) => ReinitPeriodSec = r.ReadNullableInt();

    protected override void BinaryWrite(BinaryWriter w) => w.WriteNullable(ReinitPeriodSec);
}
