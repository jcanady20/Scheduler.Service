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
using System.Net;

namespace Scheduler.Http.App_Start
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
            HttpListener listener = (HttpListener)app.Properties["System.Net.HttpListener"];
            listener.AuthenticationSchemes = AuthenticationSchemes.IntegratedWindowsAuthentication;
            
			app.UseCors(CorsOptions.AllowAll);
			HttpConfiguration httpConfig = new Configuration();

			httpConfig.MessageHandlers.Add(new MessageHandlers.WebApiCorsHandler());
            app.UseWebApi(httpConfig);

			app.UseConfigurationPage(new Microsoft.Owin.PathString("/configuration"));
		}
	}
}
