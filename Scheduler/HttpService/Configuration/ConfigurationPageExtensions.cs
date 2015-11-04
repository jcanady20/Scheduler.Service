using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace Scheduler.HttpService.Configuration
{
	public static class ConfigurationPageExtensions
	{
		public static IAppBuilder UseConfigurationPage(this IAppBuilder builder, ConfigurationPageOptions options)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			return builder.Use(typeof(ConfigurationPageMiddleware), options);
		}

		public static IAppBuilder UseConfigurationPage(this IAppBuilder builder, PathString path)
		{
			return UseConfigurationPage(builder, new ConfigurationPageOptions { Path = path });
		}

		public static IAppBuilder UseConfigurationPage(this IAppBuilder builder)
		{
			return UseConfigurationPage(builder, new ConfigurationPageOptions());
		}

	}
}
