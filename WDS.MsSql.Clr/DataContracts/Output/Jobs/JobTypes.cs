/// <summary>
/// Job type
/// </summary>
public enum JobTypes
{
    /// <summary>
    /// Crawl data from internet sources via request gateways (Proxy addresses, Host IP addresses, etc.)
    /// </summary>
    Internet = 0,
    
    /// <summary>
    /// Crawl data from intranet sources with no limits
    /// </summary>
    Intranet = 1
}