using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Scheduler.Extensions;
using Scheduler.Logging;
using Scheduler.Data.Entities;
using Scheduler.Tasks;


namespace Scheduler.Jobs
{
    public class JobEngine
	{
        #region Private Fields
        private static readonly TimeSpan WORKER_DELAY = TimeSpan.FromSeconds(1);
        private ILogger m_logger;
		private static object m_lockObj = new object();
		private static JobEngine m_instance;
		private bool m_cancel;
        private bool m_paused;
		private Thread m_worker;
		private ConcurrentQueue<IJobExecutioner> m_jobQueue;
		private ICollection<IJobExecutioner> m_activeJobs;
        #endregion

        private JobEngine()
		{
			m_jobQueue = new ConcurrentQueue<IJobExecutioner>();
			m_activeJobs = new List<IJobExecutioner>();
            m_logger = Logging.LogProvider.Instance.Logger;
            TaskManager = new TaskManager(m_logger);
            Start();
		}

        public static JobEngine Instance
        {
            get
            {
                lock (m_lockObj)
                {
                    if (m_instance == null)
                    {
                        m_instance = new JobEngine();
                    }
                    return m_instance;
                }
            }
        }

        /// <summary>
        /// Performance Counters
        /// </summary>
        public int QueuedJobs
		{
			get
			{
				return m_jobQueue.Count();
			}
		}
		public int TotalJobsExecuted { get; private set; }
		public IEnumerable<IJobExecutioner> Queue
		{
			get
			{
				return m_jobQueue.AsEnumerable();
			}
		}
		public IEnumerable<IJobExecutioner> CurrentActivity
		{
			get
			{
				return m_activeJobs;
			}
		}

        public TaskManager TaskManager { get; private set; }

        public void Add(Job job, JobSchedule jobSchedule)
        {
            if (m_activeJobs.Any(x => x.JobId == job.Id))
            {
                return;
            }
            m_logger.Info("Enqueueing {0}", job.Name);
            var je = new JobExecutioner(job, jobSchedule, this.TaskManager);
            m_activeJobs.Add(je);
            m_jobQueue.Enqueue(je);
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
            m_logger.Trace("Starting Job Engine");
            m_cancel = false;
            m_paused = false;
            if (m_worker != null && m_worker.IsAlive)
			{
				return;
			}
			m_worker = new Thread(new ThreadStart(DoWork));
			m_worker.Start();
        }

		public void Stop()
		{
			m_cancel = true;
		}

        public void Pause()
        {
            m_paused = true;
        }

        public void Continue()
        {
            m_paused = false;
        }

		private void DoWork()
		{
            while(m_cancel == false)
            {
                if(m_paused)
                {
                    Sleep();
                    continue;
                }

                DeQueueAllJobs();

                if (m_cancel)
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
                if(m_jobQueue.TryDequeue(out exec))
                {
                    if(exec.Status == Data.JobStatus.Cacnceled)
                    {
                        continue;
                    }
                    ExecuteJob(exec);
                }

            } while (m_jobQueue.Count != 0);
		}

		private void ExecuteJob(IJobExecutioner job)
		{
			if (job == null)
			{
				return;
			}
            m_logger.Info("Starting job execution for {0}", job.Name);
			TotalJobsExecuted++;
			ThreadPool.QueueUserWorkItem(o => {
                job.Execute();
                job.Dispose();
                this.RemoveJob(job);
            });
		}

		private void RemoveJob(IJobExecutioner job)
		{
			lock (m_lockObj)
			{
				m_activeJobs.Remove(job);
			}
		}

        private void Sleep()
        {
            Thread.Sleep(WORKER_DELAY);
        }
    }
}
