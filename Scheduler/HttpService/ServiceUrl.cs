using System;
using System.Configuration;

namespace Scheduler.HttpService
{
	public static class ServiceUrl
	{
		public static string GetServiceUrl()
		{
			return ConfigurationManager.AppSettings["baseAddress"];
		}
	}
}
