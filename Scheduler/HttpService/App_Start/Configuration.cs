using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Scheduler.HttpService.App_Start
{
	internal class Configuration : HttpConfiguration
	{
		public Configuration()
		{
			ConfigureRoutes();
			ConfigureJsonSerialization();
		}

		private void ConfigureRoutes()
		{
			this.MapHttpAttributeRoutes();
			Routes.MapHttpRoute("DefaultApiWithId", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
		}
		private void ConfigureJsonSerialization()
		{
			var jsonSettings = Formatters.JsonFormatter.SerializerSettings;
			jsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
			jsonSettings.Formatting = Formatting.None;
			jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}
	}
}
