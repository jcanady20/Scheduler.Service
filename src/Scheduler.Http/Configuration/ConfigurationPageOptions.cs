namespace Scheduler.Http.Configuration;

/// <summary>
/// Options for the ConfigurationPageMiddleware
/// </summary>
public class ConfigurationPageOptions
{
  /// <summary>
  /// Specifies which requests paths will be responded to. Exact matches only. Leave null to handle all requests.
  /// </summary>
  public PathString RoutePath { get; set; }
}
