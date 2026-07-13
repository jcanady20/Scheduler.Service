using Scheduler.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduler.Data.Context;

namespace Scheduler.Http.Api;

public class ScheduleController : BaseApiController
{
  internal ScheduleController(ILogger logger, ScheduleContext context) : base(logger, context)
  { }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(int id)
  {
    var item = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
    return Ok(item);
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Schedule schedule)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.Schedules.Add(schedule);
    await _db.SaveChangesAsync();
    return Ok(schedule);
  }

  [HttpPut("{id}"), HttpPatch("{id}")]
  public async Task<IActionResult> Put(int id, [FromBody] Schedule schedule)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.Schedules.Attach(schedule);
    _db.Entry(schedule).State = EntityState.Modified;
    await _db.SaveChangesAsync();

    return Ok(schedule);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var item = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
    if (item == null)
    {
      return NotFound();
    }

    _db.Schedules.Remove(item);
    _db.SaveChanges();
    return Ok(item);
  }
}
