/// <summary>
/// Download error handling policies
/// </summary>
public enum DownloadErrorHandlingPolicies
{
    
    /// <summary>
    /// Skip an error and continue crawling
    /// </summary>
    Skip = 0,
    
    /// <summary>
    /// Try again
    /// </summary>
    Retry = 1
}