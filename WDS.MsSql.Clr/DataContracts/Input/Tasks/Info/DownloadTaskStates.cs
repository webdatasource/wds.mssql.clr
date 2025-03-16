/// <summary>
/// Download task states
/// </summary>
public enum DownloadTaskStates
{
    /// <summary>
    /// Task is handled and its results are available
    /// </summary>
    Handled = 0,
    /// <summary>
    /// Access to a URL is denied by robots.txt
    /// </summary>
    AccessDeniedForRobots = 1,
    /// <summary>
    /// All request gateways (proxy and host IP addresses) were exhausted but no data was received
    /// </summary>
    AllRequestGatesExhausted = 2,
    
    
    /// <summary>
    /// Task has not been started yet
    /// </summary>
    Created = 10,
    /// <summary>
    /// Task is in progress
    /// </summary>
    InProgress = 11,
    /// <summary>
    /// Task has been deleted
    /// </summary>
    Deleted = 12
}