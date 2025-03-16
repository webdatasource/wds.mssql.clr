using System.IO;
using System.Xml.Serialization;
using WDS.MsSql.Clr.Serialization;

/// <summary>
/// Data contract base for response objects. Those objects might be an error
/// </summary>
public abstract class ResponseDataContractBase : DataContractBase
{
    /// <summary>
    /// Request execution error
    /// </summary>
    public string Error { get; set; }

    #region MS SQL CLR Required methods and properties

    public override void Read(BinaryReader r)
    {
        IsNull = r.ReadBoolean();
        if (IsNull)
            return;
        
        Error = r.ReadNullableString();
        if (Error is null)
        {
            BinaryRead(r);
            Validate();
        }
    }

    public override void Write(BinaryWriter w)
    {
        w.Write(IsNull);
        if (IsNull)
            return;
        
        w.WriteNullable(Error);
        if (Error is null)
        {
            Validate();
            BinaryWrite(w);
        }
    }

    #endregion
}