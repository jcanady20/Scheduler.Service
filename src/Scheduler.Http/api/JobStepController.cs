using Scheduler.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Scheduler.Http.Api;

public class JobStepController : BaseApiController
{
  internal JobStepController(ILogger logger, ScheduleContext context) : base(logger, context)
  { }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(int id)
  {
    var item = await _db.JobSteps.FirstOrDefaultAsync(x => x.Id == id);
    return Ok(item);
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Data.Entities.JobStep taskStep)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.JobSteps.Add(taskStep);
    await _db.SaveChangesAsync();
    return Ok(taskStep);
  }

  [HttpPut("{id}"), HttpPatch("{id}")]
  public async Task<IActionResult> Put(Guid id, [FromBody] Data.Entities.JobStep taskStep)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.JobSteps.Attach(taskStep);
    _db.Entry(taskStep).State = EntityState.Modified;
    await _db.SaveChangesAsync();

    return Ok(taskStep);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var item = await _db.JobSteps.FirstOrDefaultAsync(x => x.Id == id);
    if (item == null)
    {
      return NotFound();
    }

    _db.JobSteps.Remove(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }
}
