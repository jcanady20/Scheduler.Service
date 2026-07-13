namespace Scheduler.Http.Configuration;

public static class ConfigurationPageExtensions
{
  public static IApplicationBuilder UseConfigurationPage(this IApplicationBuilder app, ConfigurationPageOptions options)
  {
    if (app is null) throw new ArgumentNullException("builder");
    return app.UseMiddleware<ConfigurationPageMiddleware>(options);
  }

  public static IApplicationBuilder UseConfigurationPage(this IApplicationBuilder app, PathString path)
  {
    return app.UseConfigurationPage(opts =>
    {
      opts.RoutePath = path;
    });
  }

  public static IApplicationBuilder UseConfigurationPage(this IApplicationBuilder app, Action<ConfigurationPageOptions> configOptions)
  {
    var options = new ConfigurationPageOptions();
    configOptions(options);
    return UseConfigurationPage(app, options);
  }
}
