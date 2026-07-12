// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using WDS.MsSql.Clr.Serialization;
using WDS.MsSql.Clr.Serialization.DataContracts;

/// <summary>
/// Represents an HTTP cookie observed during a download request or response.
/// </summary>
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 8000, IsByteOrdered = true)]
public class Cookie : ResponseDataContractBase
{
    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie#attributes">cookie-name</see>
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional. Cookie value, or null when absent.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Optional. Cookie domain attribute, or null when absent.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// Optional. Cookie path attribute, or null when absent.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie#httponly">HttpOnly</see>
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie#secure">Secure</see>
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Optional. Cookie expiration date and time, or null for a session cookie or unknown expiration.
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Validates required fields
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public override void Validate()
    {
        if (Name is null)
            throw new NullReferenceException(nameof(Name));
    }

    #region MS SQL CLR Required methods and properties

    private static readonly DataContractSerializer<Cookie> _dataContractSerializer = new();

    // ReSharper disable once UnusedMember.Global
    public static Cookie Null { get; } = new() { IsNull = true };

    // ReSharper disable once UnusedMember.Global
    public static Cookie Parse(SqlString s) => _dataContractSerializer.Parse(s.Value);

    public override string ToString() => _dataContractSerializer.Serialize(this);

    protected override void BinaryRead(BinaryReader r)
    {
        Name = r.ReadString();
        Value = r.ReadNullableString();
        Domain = r.ReadNullableString();
        Path = r.ReadNullableString();
        HttpOnly = r.ReadBoolean();
        Secure = r.ReadBoolean();
        Expires = r.ReadNullableDateTime();
    }

    protected override void BinaryWrite(BinaryWriter w)
    {
        w.Write(Name);
        w.WriteNullable(Value);
        w.WriteNullable(Domain);
        w.WriteNullable(Path);
        w.Write(HttpOnly);
        w.Write(Secure);
        w.WriteNullable(Expires);
    }

    #endregion
}
