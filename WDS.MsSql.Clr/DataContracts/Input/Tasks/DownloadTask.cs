using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Download task
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class DownloadTask : ResponseDataContractBase
{
    /// <summary>
    /// Server configuration associated with the job
    /// </summary>
    [XmlIgnore]
    public ServerConfig Server { get; set; }

    /// <summary>
    /// Download task ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Download task URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Error is not null)
            return;

        if (Id is null)
            throw new NullReferenceException(nameof(Id));

        if (Url is null)
            throw new NullReferenceException(nameof(Url));

        if (Server is null || Server.IsNull)
            throw new NullReferenceException(nameof(Server));
    }

    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<DownloadTask> _dataContractSerializer = new();

    public static DownloadTask Null { get; } = new() { IsNull = true };

    public static DownloadTask Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Server = r.ReadNullable<ServerConfig>();
        Id = r.ReadNullableString();
        Url = r.ReadNullableString();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteNullable(Server);
        w.WriteNullable(Id);
        w.WriteNullable(Url);
    }

    #endregion
}