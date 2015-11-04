using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Scheduler.Logging;
using Scheduler.Data.Entities;

namespace Scheduler.Scheduling
{
    public class JobEngine
	{
		#region Private Fields
		private ILogger m_logger;
		private static object m_lockObj = new object();
		private static JobEngine m_instance;
		private static bool m_cancel;
		private Thread m_worker;
		private Queue<IJobExecutioner> m_jobQueue;
		private ICollection<IJobExecutioner> m_activeJobs;
		#endregion

		private JobEngine()
		{
			m_jobQueue = new Queue<IJobExecutioner>();
			m_activeJobs = new List<IJobExecutioner>();
			m_logger = new NLogger("Visualutions.Scheduler.Scheduling.JobEngine");
			Start();
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

		public void Start()
		{
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

		private void DoWork()
		{
			while(true)
			{
				var exec = DeQueueJob();
				ExecuteJob(exec);

				if(m_cancel)
				{
					break;
				}
				Thread.Sleep(1000);
			}
		}

		private IJobExecutioner DeQueueJob()
		{
			IJobExecutioner exec = null;
			if(m_jobQueue.Count > 0)
			{
				lock(m_lockObj)
				{
					exec = m_jobQueue.Dequeue();
				}
			}
			return exec;
		}

		private void ExecuteJob(IJobExecutioner job)
		{
			if (job == null)
			{
				return;
			}
			TotalJobsExecuted++;
			ThreadPool.QueueUserWorkItem(o => { job.Execute(); job.Dispose(); this.RemoveJob(job); });
		}

		private void RemoveJob(IJobExecutioner job)
		{
			lock (m_lockObj)
			{
				m_activeJobs.Remove(job);
			}
		}

		public static JobEngine Instance
		{
			get
			{
				lock(m_lockObj)
				{
					if(m_instance == null)
					{
						m_instance = new JobEngine();
					}
					return m_instance;
				}
			}
		}

		public void Add(Job job, JobSchedule jobSchedule)
		{
			if(m_activeJobs.Any(x => x.JobId == job.Id))
			{
				return;
			}
			lock (m_lockObj)
			{
				var je = new JobExecutioner(job, jobSchedule);
				m_activeJobs.Add(je);
				m_jobQueue.Enqueue(je);
			}
		}
	}
}
