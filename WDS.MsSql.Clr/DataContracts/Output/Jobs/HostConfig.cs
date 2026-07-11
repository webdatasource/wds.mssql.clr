// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Host initialization settings
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class HostConfig : DataContractBase
{
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
