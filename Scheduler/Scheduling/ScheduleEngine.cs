using Scheduler.Extensions;
using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Scheduler.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Scheduler.Scheduling;

public class ScheduleEngine
{
  private Thread _worker;
  private bool _paused;
  private bool _canceled;
  private ILogger _logger;
  private JobEngine _jobEngine;
  private static readonly TimeSpan WORKER_DELAY = TimeSpan.FromMinutes(1);

  public ScheduleEngine(ILogger logger, JobEngine jobEngine)
  {
    _jobEngine = jobEngine;
    _logger = logger;
  }

  public void Start()
  {
    _canceled = false;
    _paused = false;
    if(_worker != null && _worker.IsAlive)
    {
      return;
    }
    _worker = new Thread(new ThreadStart(DoWork));
    _worker.Start();

    QueueStartUpJobs();
  }
  public void Stop()
  {
    _canceled = true;
  }
  public void Pause()
  {
    _paused = true;
  }
  public void Continue()
  {
    _paused = false;
  }
  private void DoWork()
  {
    while(_canceled == false)
    {
      if(_paused)
      {
        Sleep();
        continue;
      }
      try
      {
        using (var db = ContextFactory.CreateContext())
        {
          db.Configuration.ProxyCreationEnabled = false;
          db.Configuration.AutoDetectChangesEnabled = false;
          var schedules = GetActiveSchedules(db);
          ProcessSchedules(schedules, db);
        }
      }
      catch(Exception e)
      {
        _logger.LogError(e.Message, e);
      }

      Sleep();
    }
  }

  private void ProcessSchedules(IEnumerable<Schedule> schedules, ScheduleContext db)
  {
    if (schedules == null)
    {
      return;
    }
    schedules.ForEach(sch => {
      ProcessSchedule(sch, db);
    });
  }
  private void ProcessSchedule(Schedule schedule, ScheduleContext db)
  {
    var jobSchedules = db.JobSchedules.Where(x => x.ScheduleId == schedule.Id);
    jobSchedules.ForEach(js =>
    {
      js.Schedule = schedule;
      var nextRunDateTime = schedule.CalculateNextRun(js.LastRunDateTime);
      if (nextRunDateTime <= DateTime.Now)
      {
        db.Jobs
          .AsNoTracking()
          .Include(r => r.JobSteps)
          .Where(x => x.Id == js.JobId)
          .ForEach(j =>
          {
            js.Job = j;
            _jobEngine.Add(j, js);
          });
      }
    });
  }
  private IEnumerable<Schedule> GetActiveSchedules(ScheduleContext db)
  {
    IEnumerable<Schedule> schedules = null;
    try
    {
      schedules = db.Schedules.Where(x => x.Enabled == true && ((int)x.Type & 61) != 0).ToList();
    }
    catch (Exception e)
    {
      _logger.LogError(e.Message, e);
    }
    return schedules;
  }
  private void QueueStartUpJobs()
  {
    _logger.LogTrace("Queueing Startup Jobs");
    using (var db = ContextFactory.CreateContext())
    {
      db.Configuration.ProxyCreationEnabled = false;
      db.Configuration.AutoDetectChangesEnabled = false;

      db.Schedules
        .AsNoTracking()
        .Where(x => x.Type == FrequencyType.OnStartup)
        .ToList()
        .ForEach(s => {
          ProcessSchedule(s, db);
        });
    }
  }
  private void Sleep()
  {
    Thread.Sleep(WORKER_DELAY);
  }
}
