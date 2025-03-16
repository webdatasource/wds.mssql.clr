/// <summary>
/// Job restart mode enum
/// </summary>
public enum JobRestartModes
{
    /// <summary>
    /// Reuse cached data and continue crawling and parsing new data
    /// </summary>
    Continue = 0,
    
    /// <summary>
    /// Clear cached data and start from scratch 
    /// </summary>
    FromScratch = 1
}