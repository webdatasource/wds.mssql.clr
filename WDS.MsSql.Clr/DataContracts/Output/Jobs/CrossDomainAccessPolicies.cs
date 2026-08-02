// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

/// <summary>
/// Cross-domain access policies enumeration
/// </summary>
public enum CrossDomainAccessPolicies
{
    /// <summary>
    /// No subdomain or cross-domain access. Only the main domain is allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The subdomains of the main domain are allowed (e.g., "example.com", "sub.example.com)
    /// </summary>
    Subdomains = 1,

    /// <summary>
    /// Allows access to any domain (e.g., "example.com", "sub.example.com, another.com")
    /// </summary>
    CrossDomains = 2
}