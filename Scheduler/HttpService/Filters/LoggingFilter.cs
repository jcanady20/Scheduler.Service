using System.Diagnostics;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Scheduler.Logging;

namespace Scheduler.HttpService.Filters
{
    public class LoggingFilter : ActionFilterAttribute
    {
        private Stopwatch m_stopWatch;
        private ILogger m_logger;
        public LoggingFilter()
        {
            m_stopWatch = new Stopwatch();
            m_logger = new NLogger();
        }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            m_stopWatch.Start();
            base.OnActionExecuting(actionContext);
        }
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            var actionContext = context.ActionContext;
            m_stopWatch.Stop();
            var elapsed = m_stopWatch.Elapsed;
            var method = context.Request.Method.Method;
            var actionName = actionContext.ActionDescriptor.ActionName;
            var mediaType = context.Request.Content.Headers?.ContentType?.MediaType ?? "Unknown";
            m_logger.Info("[{0}] -- Action: {1:15} -- {2} -- Elapsed:[{3}:{4}:{5}]", method, actionName, mediaType, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            base.OnActionExecuted(context);
        }
    }
}
