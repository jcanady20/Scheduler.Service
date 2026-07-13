using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;

namespace Scheduler.Http.Api;

public class JobScheduleController : BaseApiController
{
  internal JobScheduleController(ILogger logger, ScheduleContext context) : base(logger, context)
  { }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(int id)
  {
    var item = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
    return Ok(item);
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] JobSchedule jobSchedule)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.JobSchedules.Add(jobSchedule);
    await _db.SaveChangesAsync();
    return Ok(jobSchedule);
  }

  [HttpPut("{id}"), HttpPatch("{id}")]
  public async Task<IActionResult> Put([FromBody] JobSchedule jobSchedule)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.JobSchedules.Attach(jobSchedule);
    _db.Entry(jobSchedule).State = EntityState.Modified;
    await _db.SaveChangesAsync();

    return Ok(jobSchedule);
  }

  [HttpDelete("{jobId}/{scheduleId}")]
  public async Task<IActionResult> Delete(int jobId, int scheduleId)
  {
    var item = await _db.JobSchedules.FirstOrDefaultAsync(x => x.JobId == jobId && x.ScheduleId == scheduleId);
    if (item == null)
    {
      return NotFound();
    }
    _db.JobSchedules.Remove(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }
}
