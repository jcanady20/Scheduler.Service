using System.Diagnostics;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Data.Queries;
using Scheduler.Tasks;
using Microsoft.Extensions.Logging;



namespace Scheduler.Jobs;

public class JobExecutioner : IJobExecutioner
{
  #region Private Methods
  private IJobTask m_lastJobTask;
  private ILogger _logger;
  private ScheduleContext _db;
  private Job _job;
  private Stopwatch _stopwatch;
  private JobSchedule _jobSchedule;
  private CancellationTokenSource _cancelToken;
    private TaskManager _taskManager;
  private JobStatus m_status;
  #endregion

  public JobExecutioner(ILogger logger, ScheduleContext scheduleContext, Job job, JobSchedule jobSchedule, TaskManager taskManager)
  {
    _db = scheduleContext;
    _job = job;
    _jobSchedule = jobSchedule;
    _stopwatch = new Stopwatch();
    _cancelToken = new CancellationTokenSource();
    _taskManager = taskManager;
    _logger = logger;
    this.OutCome = JobStepOutCome.Unknown;
    this.Status = JobStatus.WaitingForWorkerThread;
    ReportQueuedDateTime();
  }

  public int JobId { get { return _job.Id; } }
  public string Name { get { return _job.Name; } }
  public JobStepOutCome OutCome { get; private set; }
  public Nullable<DateTime> StartDateTime { get; private set; }
  public Nullable<DateTime> CompletedDateTime { get; private set; }
  public Nullable<TimeSpan> Duration { get; private set; }
  public JobStatus Status
  {
    get { return m_status; }
    private set
    {
      if(value != m_status)
      {
        m_status = value;
        ReportStatusActivity(value);
      }
    }
  }

  public void Cancel()
  {
        Status = JobStatus.Canceled;
    _cancelToken.Cancel();
  }

  public void Execute()
  {
    StartExecute();

    foreach(var task in JobTasks())
    {
            if(task == null)
            {
                continue;
            }
      if (_cancelToken.IsCancellationRequested)
      {
        CancelExecute();
        break;
      }
      this.m_lastJobTask = task;

            this.OutCome = task.Execute(_cancelToken.Token);

            if (this.OutCome != JobStepOutCome.Succeeded)
      {
        break;
      }
    }
    CompleteExecute();
  }

  private void StartExecute()
  {
    _logger.LogTrace("Started :: Job ({0}) has started Execution", _job.Name);
    this.StartDateTime = DateTime.Now;
    _stopwatch.Start();
    this.Status = JobStatus.Executing;
  }

  private void CancelExecute()
  {
    this.OutCome = JobStepOutCome.Cancelled;
    _logger.LogTrace("Completed :: Job ({0}) has been Cancelled", _job.Name);
    CompleteExecute();
  }

  private void CompleteExecute()
  {
    _stopwatch.Stop();
    this.Duration = _stopwatch.Elapsed;
    this.CompletedDateTime = DateTime.Now;
    this.Status = JobStatus.Idle;
    ReportOutcome();
    ReportOutComeActivity();
    _logger.LogTrace("Completed :: Job ({0}) has completed Execution", _job.Name);
  }

  private IEnumerable<IJobTask> JobTasks()
  {
    var tasks = _job.JobSteps.OrderBy(x => x.StepId);
    foreach(var t in tasks)
    {
      yield return _taskManager.CreateTask(_db, t);
    }
  }

  private string GetOutComeMessage()
  {
    var msg = String.Empty;
    msg = String.Format("The job {0}.", this.OutCome);
    if (_jobSchedule != null)
    {
      msg += String.Format("The job was Invoked by Schedule ({0})", _jobSchedule.Schedule.Name);
    }
    var lastStepName = (this.m_lastJobTask != null) ? this.m_lastJobTask.Name : "";
    var lastStepId = (this.m_lastJobTask != null) ? this.m_lastJobTask.StepId : 0;
    msg += String.Format("The last step to run was step {0} ({1}).", lastStepId, lastStepName);

    return msg;
  }

  private void ReportQueuedDateTime()
  {
    var item = _db.GetActivity(this.JobId);
    item.QueuedDateTime = DateTime.Now;
    _db.SaveChanges();
  }

  private void ReportOutcome()
  {
    if (_jobSchedule != null)
    {
      //  Record the Last Run Details to the JobSchedule object
      _db.UpdateJobScheduleLastRun(_job.Id, _jobSchedule.Schedule.Id, this.CompletedDateTime.Value);
    }
    //  Add a Job History Entry
    _db.AddJobHistory(_job.Id, 0, "(Job OutCome)", this.OutCome, this.GetOutComeMessage(), this.CompletedDateTime.Value, this.Duration.Value);
  }

  private void ReportOutComeActivity()
  {
    if (_db == null) { return;  }
    var item = _db.GetActivity(this.JobId);
    item.StartDateTime = this.StartDateTime;
    item.CompletedDateTime = this.CompletedDateTime.Value;
    item.LastRunOutCome = this.OutCome;
    item.LastOutComeMessage = this.GetOutComeMessage();
    item.LastRunDateTime = this.StartDateTime.Value;
    item.LastRunDuration = this.Duration.Value;
    if (_jobSchedule != null)
    {
      item.NextRunDateTime = _jobSchedule.Schedule.CalculateNextRun(DateTime.Now);
    }

    _db.SaveChanges();
  }

  private void ReportStatusActivity(JobStatus status)
  {
    if(_db == null)
    {
      return;
    }
    _db.SetActivityStatus(this.JobId, status);
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
      if(_cancelToken != null)
      {
        _cancelToken.Dispose();
        _cancelToken = null;
      }
      if(_db != null)
      {
        _db.Dispose();
        _db = null;
      }
    }
  }
}
