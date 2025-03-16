using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Server;

/// <summary>
/// Data contract base for request objects. Those objects might be null
/// </summary>
public abstract class DataContractBase : INullable, IBinarySerialize
{
    /// <summary>
    /// If object is null
    /// </summary>
    [XmlIgnore]
    public bool IsNull { get; protected set; }

    #region MS SQL CLR Required methods and properties
    
    // protected string ToString(XmlSerializer serializer)
    // {
    //     var stringBuilder = new StringBuilder();
    //     using var writer = new XmlStringWriter(stringBuilder, ServerApi.Encoding);
    //     serializer.Serialize(writer, this);
    //     return stringBuilder.ToString();
    // }

    public virtual void Validate()
    {
    }

    public virtual void Read(BinaryReader r)
    {
        IsNull = r.ReadBoolean();
        if (IsNull)
            return;
        BinaryRead(r);
        Validate();
    }

    protected abstract void BinaryRead(BinaryReader r);

    public virtual void Write(BinaryWriter w)
    {
        w.Write(IsNull);
        if (IsNull)
            return;
        Validate();
        BinaryWrite(w);
    }

    protected abstract void BinaryWrite(BinaryWriter w);

    #endregion
}