using System;
using System.Configuration;

namespace Scheduler.Http
{
	public static class ServiceUrl
	{
		public static string GetServiceUrl()
		{
			return ConfigurationManager.AppSettings["baseAddress"];
		}
	}
}
