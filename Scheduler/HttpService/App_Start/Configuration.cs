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
            ConfigureFilters();
            ConfigureFormatters();
        }

		private void ConfigureRoutes()
		{
			this.MapHttpAttributeRoutes();
			Routes.MapHttpRoute("DefaultApiWithId", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
		}
        private void ConfigureFilters()
        {
            Filters.Add(new Filters.LoggingFilter());
        }
        private void ConfigureFormatters()
        {
            Formatters.Add(new Formatting.BsonMediaTypeFormatter());
        }
        private void ConfigureJsonSerialization()
		{
			var jsonSettings = Formatters.JsonFormatter.SerializerSettings;
			jsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			jsonSettings.Formatting = Newtonsoft.Json.Formatting.None;
			jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}
	}
}
