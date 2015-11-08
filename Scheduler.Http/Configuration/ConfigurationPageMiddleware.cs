using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

using Scheduler.Http.Configuration.Views;

namespace Scheduler.Http.Configuration
{
	
	using AppFunc = Func<IDictionary<string, object>, Task>;

	public class ConfigurationPageMiddleware
	{
		private readonly AppFunc m_next;
		private readonly ConfigurationPageOptions m_options;
		public ConfigurationPageMiddleware(AppFunc next, ConfigurationPageOptions options)
		{
			m_next = next;
			m_options = options;
		}

		public Task Invoke(IDictionary<string, object> environment)
		{
			IOwinContext context = new OwinContext(environment);
			if (!m_options.Path.HasValue || m_options.Path == context.Request.Path)
			{
				var page = new ConfigurationPage();
				page.Execute(context);
				return Task.FromResult(0);
			}
			return m_next(environment);
		}		
	}
}
