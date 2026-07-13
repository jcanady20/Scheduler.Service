using Microsoft.AspNetCore.Mvc;
using Scheduler.Data.Context;

namespace Scheduler.Http.Api;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
  internal ILogger _logger;
  internal ScheduleContext _db;
  internal BaseApiController(ILogger logger, ScheduleContext context)
  {
    _logger = logger;
    _db = context;
  }
}
