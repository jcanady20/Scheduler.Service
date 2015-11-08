using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Scheduler.Http.Configuration;

namespace Scheduler.Http.App_Start
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);
			HttpConfiguration httpConfig = new Configuration();

			httpConfig.MessageHandlers.Add(new MessageHandlers.WebApiCorsHandler());
			app.UseWebApi(httpConfig);

			app.UseConfigurationPage(new Microsoft.Owin.PathString("/configuration"));
		}
	}
}
