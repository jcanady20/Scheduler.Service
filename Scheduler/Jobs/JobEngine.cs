using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Scheduler.Data.Entities;
using Scheduler.Tasks;


namespace Scheduler.Jobs;

public class JobEngine
{
  private static readonly TimeSpan WORKER_DELAY = TimeSpan.FromSeconds(1);
  private ILogger _logger;
  private static object _lockObj = new object();
  private static JobEngine _instance;
  private bool _cancel;
  private bool _paused;
  private Thread _worker;
  private ConcurrentQueue<IJobExecutioner> _jobQueue;
  private ICollection<IJobExecutioner> _activeJobs;

  private JobEngine(ILogger<JobEngine> logger)
  {
    _jobQueue = new ConcurrentQueue<IJobExecutioner>();
    _activeJobs = new List<IJobExecutioner>();
    _logger = logger;
    TaskManager = new TaskManager(_logger);
    Start();
  }


  /// <summary>
  /// Performance Counters
  /// </summary>
  public int QueuedJobs
  {
    get
    {
      return _jobQueue.Count();
    }
  }
  public int TotalJobsExecuted { get; private set; }
  public IEnumerable<IJobExecutioner> Queue
  {
    get
    {
      return _jobQueue.AsEnumerable();
    }
  }
  public IEnumerable<IJobExecutioner> CurrentActivity
  {
    get
    {
      return _activeJobs;
    }
  }

  public TaskManager TaskManager { get; private set; }

  public void Add(Job job, JobSchedule jobSchedule)
  {
    if (_activeJobs.Any(x => x.JobId == job.Id))
    {
      return;
    }
    _logger.LogInformation("Enqueueing {JobName}", job.Name);
    var je = new JobExecutioner(job, jobSchedule, this.TaskManager);
    _activeJobs.Add(je);
    _jobQueue.Enqueue(je);
  }

  public void CancelExecution(IJobExecutioner jobExecution)
  {
    if(jobExecution == null)
    {
      throw new ArgumentNullException(nameof(jobExecution));
    }
    jobExecution.Cancel();
  }

  public void Start()
  {
    _logger.LogTrace("Starting Job Engine");
    _cancel = false;
    _paused = false;
    if (_worker != null && _worker.IsAlive)
    {
      return;
    }
    _worker = new Thread(new ThreadStart(DoWork));
    _worker.Start();
  }

  public void Stop()
  {
    _cancel = true;
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
    while(_cancel == false)
    {
      if(_paused)
      {
        Sleep();
        continue;
      }

      DeQueueAllJobs();

      if (_cancel)
      {
        break;
      }
      Sleep();
    }
  }

  private void DeQueueAllJobs()
  {
    do
    {
      IJobExecutioner exec = null;
      if(_jobQueue.TryDequeue(out exec))
      {
        if(exec.Status == Data.JobStatus.Cacnceled)
        {
          continue;
        }
        ExecuteJob(exec);
      }
    } while (_jobQueue.Count != 0);
  }

  private void ExecuteJob(IJobExecutioner job)
  {
    if (job == null)
    {
      return;
    }
    _logger.LogInformation("Starting job execution for {JobName}", job.Name);
    TotalJobsExecuted++;
    ThreadPool.QueueUserWorkItem(o => {
      job.Execute();
      job.Dispose();
      this.RemoveJob(job);
    });
  }

  private void RemoveJob(IJobExecutioner job)
  {
    lock (_lockObj)
    {
      _activeJobs.Remove(job);
    }
  }

  private void Sleep()
  {
    Thread.Sleep(WORKER_DELAY);
  }
}
