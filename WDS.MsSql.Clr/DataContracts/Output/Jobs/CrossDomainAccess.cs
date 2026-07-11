// ReSharper disable CheckNamespace

using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Cross-domain access policy
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class CrossDomainAccess : DataContractBase
{
    /// <summary>
    /// Enum filed wrapper for CrossDomainAccess
    /// </summary>
    [XmlIgnore]
    public string AccessPolicy
    {
        get => Policy.ToString();
        set => Policy = value.ParseEnum<CrossDomainAccessPolicies>();
    }
    
    /// <summary>
    /// Cross-domain access policy
    /// </summary>
    private CrossDomainAccessPolicies Policy { get; set; }
    
    /// <summary>
    /// Creates a new instance and fills it out from a config string of the following format 'ErrorHandlingPolicy: <see cref="Policy">Possible values</see>'
    /// </summary>
    /// <param name="s">Config string</param>
    // ReSharper disable once UnusedMember.Global
    public static CrossDomainAccess Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    private static readonly DataContractSerializer<CrossDomainAccess> _dataContractSerializer = new DataContractSerializer<CrossDomainAccess>()
        .MapValue<string>(nameof(AccessPolicy), (crossDomainAccessPolicy, accessPolicy) => crossDomainAccessPolicy.AccessPolicy = accessPolicy);

    #region MS SQL CLR Required methods and properties

    // ReSharper disable once UnusedMember.Global
    public static CrossDomainAccess Null { get; } = new() { IsNull = true };

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Policy = r.ReadEnum<CrossDomainAccessPolicies>();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.WriteEnum(Policy);
    }

    #endregion
}