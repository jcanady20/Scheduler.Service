using Scheduler.Data.Queries;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Data.Context;
using Microsoft.EntityFrameworkCore;
using Scheduler.Jobs;


namespace Scheduler.Http.Api;

public class JobController : BaseApiController
{
  private readonly JobEngine _jobEngine;
  internal JobController(ILogger logger, ScheduleContext context, JobEngine jobEngine) : base(logger, context)
  {
    _jobEngine = jobEngine;
  }

  [HttpGet]
  public async Task<IActionResult> Get()
  {
    var items = await _db.Jobs.ToListAsync();
    return Ok(items);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(int id)
  {
    var item = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
    return Ok(item);
  }

  [HttpGet("history")]
  public async Task<IActionResult> GetJobHistory([FromQuery] Data.Models.JobHistorySearch model)
  {
    var items = await Task.FromResult(_db.JobHistorySearch(model));
    return Ok(items);
  }

  [HttpGet("activity")]
  public async Task<IActionResult> GetJobActivity()
  {
    var items = await _db.JobActivity.ToListAsync();
    return Ok(items);
  }

  [HttpGet("subsystems")]
  public async Task<IActionResult> GetSubSystems()
  {
    var items = await Task.FromResult(_jobEngine.TaskManager.TaskPlugins.ToList());
    return Ok(items);
  }

  [HttpGet("{id}/steps")]
  public async Task<IActionResult> GetJobSteps(int id)
  {
    var items = await _db.JobSteps.Where(x => x.JobId == id).ToListAsync();
    return Ok(items);
  }

  [HttpGet("{id}/schedules")]
  public async Task<IActionResult> GetJobSchedules(int id)
  {
    var items = await _db.GetJobSchedules(id).ToListAsync();
    return Ok(items);
  }

  [HttpGet("{id}/start")]
  public async Task<IActionResult> ExecuteJob(int id)
  {
    var job = await _db.Jobs.Include(r => r.JobSteps).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    _jobEngine.Add(job, null);
    return Ok(new { success = true });
  }

  [HttpPost("{id}/cancel")]
  public IActionResult CacnelJob(int id)
  {
    var jobExecution = _jobEngine.CurrentActivity.FirstOrDefault(x => x.JobId == id);
    _jobEngine.CancelExecution(jobExecution);
    return Ok(new { success = true });
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Data.Entities.Job job)
  {
    if (ModelState.IsValid) return BadRequest(ModelState);
    _db.Jobs.Add(job);
    await _db.SaveChangesAsync();
    return Ok(job);
  }

  [HttpPut("{id}"), HttpPatch("{id}")]
  public async Task<IActionResult> Put(Guid id, [FromBody] Data.Entities.Job job)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    _db.Jobs.Attach(job);
    _db.Entry(job).State = EntityState.Modified;
    await _db.SaveChangesAsync();
    return Ok(job);
  }

  [HttpDelete("activity/{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var item = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
    if (item == null)
    {
      return NotFound();
    }

    _db.Jobs.Remove(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }
}
