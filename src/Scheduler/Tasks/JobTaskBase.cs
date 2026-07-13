using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Queries;

using Scheduler.Extensions;

namespace Scheduler.Tasks;

public abstract class JobTaskBase : IJobTask, IDisposable
{
  protected ILogger _logger;
  protected JobStep _taskStep;
  protected ScheduleContext _db;
  protected JobTaskBase()
  {
    this.OutCome = JobStepOutCome.Unknown;
    this.Id = Guid.NewGuid();
  }

  public Guid Id { get; private set; }
  public string Name { get; private set; }
  public int StepId { get; private set; }
  public Nullable<DateTime> Started { get; private set; }
  public Nullable<TimeSpan> Duration { get; private set; }
  public JobStepOutCome OutCome { get; private set; }
  public string OutComeMessage { get; private set; }
  public Nullable<DateTime> Completed { get; private set; }

  public IJobTask Create(ScheduleContext db, JobStep step, ILogger logger)
  {
    _logger = logger;
    _db = db;
    _taskStep = step;
    this.Name = step.Name;
    this.StepId = step.StepId;
    return this;
  }

  public JobStepOutCome Execute(CancellationToken cancelToken)
  {
    _logger.LogTrace("Started :: Executing Step {0} ({1}) for {2}", _taskStep.StepId, _taskStep.Name, _taskStep.SubSystem);
    this.OutCome = JobStepOutCome.Unknown;
    var sw = new Stopwatch();
    sw.Start();
    try
    {
            //  Step the Started DateTime value
            this.Started = DateTime.Now;
      this.OnExecute();
      this.OutCome = JobStepOutCome.Succeeded;
      this.OutComeMessage = "The step succeeded.";
    }
    catch(Exception e)
    {
      _logger.LogError(e, "Error :: Encountered an error while Executing Step {0} ({1}) for {2}", _taskStep.StepId, _taskStep.Name, _taskStep.SubSystem);
      this.OutCome = JobStepOutCome.Failed;
      this.OutComeMessage = "The step Failed. " + e.BuildExceptionMessage();
    }
    finally
    {
      sw.Stop();
      //  Set the Duration to the Elapsed Timespan
      this.Duration = sw.Elapsed;
      //  Set the Completed DateTime value
      this.Completed = DateTime.Now;
      ReportOutCome();
    }
    _logger.LogTrace("Completed :: Executing Step {0} ({1}) for {2} {3}", _taskStep.StepId, _taskStep.Name, _taskStep.SubSystem, this.OutCome);
    return this.OutCome;
  }

  public abstract void OnExecute();

  protected ILogger Logger => _logger;

  protected void ReportOutCome()
  {
    //  Add a JobHistory Entry for this JobStep
    _db.AddJobHistory(_taskStep.JobId, _taskStep.StepId, _taskStep.Name, OutCome, OutComeMessage, this.Started, this.Duration);
    //  Update Activity
    _db.SetActivityLastStepDetails(_taskStep.JobId, _taskStep.StepId, this.Started.Value, this.Duration.Value);
  }

  public void Dispose()
  {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }
  protected virtual void Dispose(bool disposing)
  {
    if(disposing)
    {

    }
  }
}
