using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Net;

using Scheduler.Logging;

namespace Scheduler.HttpService.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheFilter : ActionFilterAttribute
    {
        private ILogger m_logger;
        private string m_cacheKey;
        public CacheFilter(string cacheKey)
        {
            m_logger = new NLogger();
            m_cacheKey = cacheKey;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (IgnoreCache(actionContext.Request))
            {
                return;
            }
            var cache = MemoryCache.Default;
            var results = cache.Get(m_cacheKey);
            if (results != null)
            {
                m_logger.Info("Found existing cache Results for CacheKey {0}", m_cacheKey);
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.OK,
                    results,
                    actionContext.ControllerContext.Configuration.Formatters.JsonFormatter
                    );
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            if (actionContext.Response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }
            var objectContent = actionContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var value = objectContent.Value;
                if (value != null)
                {
                    var cache = MemoryCache.Default;
                    var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(8) };
                    cache.Add(m_cacheKey, value, policy);
                }
            }
        }

        private bool IgnoreCache(HttpRequestMessage request)
        {
            var result = false;
            var parms = request.GetQueryNameValuePairs().ToDictionary(x => x.Key.ToLower(), x => x.Value);
            if (parms.ContainsKey("ignorecache"))
            {
                Boolean.TryParse(parms["ignorecache"], out result);
            }

            return result;
        }
    }
}
