using Scheduler.Http.Configuration.Views;

namespace Scheduler.Http.Configuration;

public class ConfigurationPageMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ConfigurationPageOptions _options;
  public ConfigurationPageMiddleware(RequestDelegate next, ConfigurationPageOptions options)
  {
    _next = next;
    _options = options;
  }

  public Task InvokeAsync(HttpContext context)
  {
    if (!_options.RoutePath.HasValue || _options.RoutePath == context.Request.Path)
    {
      var page = new ConfigurationPage();
      page.Execute(context);
      return Task.FromResult(0);
    }
    return _next(context);
  }    
}
