using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;


using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Extensions;
using Scheduler.Logging;
using Scheduler.HttpService;

namespace Scheduler.Scheduling
{
    public class SchedulerService : ServiceBase
	{
		#region Private variables
		private ILogger m_logger;
		private Thread m_worker;
		private volatile bool m_cancel;
		private volatile bool m_paused;
		private const int ONE_MINUTE = 1000 * 60;
		private const long REFRESH_INTERVAL = ONE_MINUTE * 5;
		private const int WORKER_DELAY = ONE_MINUTE;
		private long m_lastrefresh;
		private EventLog m_eventLog;
		private ICollection<JobSchedule> m_jobSchedules;
		#endregion

		#region Public Consts
		public static readonly string EVENTLOGNAME = "VisualutionsScheduler";
		public static readonly string EVENTLOGSOURCE = "VisualutionsSchedulerService";
		public static readonly string SERVICENAME = "Visualutions Scheduler";
		#endregion

		public SchedulerService()
		{
#if (DEBUG == false)
			//	Auto Log to the Event Log in Release mode only
			this.AutoLog = true;
#endif
			this.ServiceName = SERVICENAME;
			this.CanShutdown = true;
			this.CanStop = true;
			this.CanPauseAndContinue = true;
			this.CanHandlePowerEvent = false;
			this.CanHandleSessionChangeEvent = false;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			m_logger = new NLogger("Visualutions.Scheduler.Scheduling.SchedulerService");
			m_cancel = false;
		}
		public string BaseAddress { get; private set; }

		public void Start()
		{
			m_logger.Trace("Starting Scheduler Service");
			m_cancel = false;
			m_paused = false;
		
			if(m_worker != null && m_worker.IsAlive)
			{
				return;
			}
			m_worker = new Thread(new ThreadStart(DoWork));
			m_worker.Start();
			OwinService.Instance.Start();
			JobEngine.Instance.Start();

			QueueStartUpJobs();
		}
		public new void Stop()
		{
			m_logger.Trace("Stopping Scheduler Service");
			m_cancel = true;
			OwinService.Instance.Stop();
			JobEngine.Instance.Stop();
			base.Stop();
		}
		protected override void OnStart(string[] args)
		{
			this.Start();
			base.OnStart(args);
		}
		protected override void OnPause()
		{
			m_paused = true;
			base.OnPause();
		}
		protected override void OnContinue()
		{
			m_paused = false;
			base.OnContinue();
		}
		protected override void OnShutdown()
		{
			this.Stop();
			base.OnShutdown();
		}

		private void DoWork()
		{
			while (m_cancel == false)
			{
				if(m_paused)
				{
					Sleep();
					continue;
				}

				//	Get the Current Schedules from the Database
				var schedules = GetActiveSchedules();
				if (schedules == null)
				{
					Sleep();
					continue;
				}

				foreach (var schedule in schedules)
				{
					ProcessSchedule(schedule);
				}

				Sleep();
			} 
		}

		public override EventLog EventLog
		{
			get
			{
				if (m_eventLog == null)
				{
					m_eventLog = new EventLog(EVENTLOGNAME, Environment.MachineName, EVENTLOGSOURCE);
				}
				return m_eventLog;
			}
		}

		private void ProcessSchedule(JobSchedule jobSchedule)
		{
			var nextRunDateTime = jobSchedule.CalculateNextRun(jobSchedule.LastRunDateTime);
			if(nextRunDateTime <= DateTime.Now)
			{
				Job job = GetJob(jobSchedule.JobId);
				if(job != null && job.Enabled)
				{
					JobEngine.Instance.Add(job, jobSchedule);
				}				
			}
		}

		private Job GetJob(Guid id)
		{
			Job job = null;
			try
			{
				using (var db = ContextFactory.CreateContext())
				{
					db.Configuration.ProxyCreationEnabled = false;
					db.Configuration.AutoDetectChangesEnabled = false;
					job = db.Jobs.AsNoTracking().Include(r => r.JobSteps).AsNoTracking().FirstOrDefault(x => x.Id == id);
				}
			}
			catch(Exception e)
			{
				m_logger.Error(e);
			}
			return job;
		}

		private ICollection<JobSchedule> GetActiveSchedules()
		{
			try
			{
				if ((DateTime.Now.Ticks - REFRESH_INTERVAL) > m_lastrefresh)
				{
					using (var db = ContextFactory.CreateContext())
					{
						m_jobSchedules = db.JobSchedules.AsNoTracking().Where(x => x.Enabled == true && ((int)x.Type & 61) != 0).ToList();
					}
					m_lastrefresh = DateTime.Now.Ticks;
				}
			}
			catch(Exception e)
			{
				m_logger.Error(e);
			}
			return m_jobSchedules;
		}

		private void QueueStartUpJobs()
		{
			ICollection<JobSchedule> schedules = null;
			using(var db = ContextFactory.CreateContext())
			{
				schedules = db.JobSchedules
					.AsNoTracking()
					.Where(x => x.Type == FrequencyType.OnStartup)
					.ToList();
			}
			foreach(var js in schedules)
			{

				var job = GetJob(js.JobId);
				if(job != null)
				{
					JobEngine.Instance.Add(job, js);
				}
			}
		}

		private void Sleep()
		{
			Thread.Sleep(WORKER_DELAY);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;
			if (exception == null)
			{
				return;
			}
			var msg = exception.BuildExceptionMessage();

			Trace.WriteLine(msg);

			this.EventLog.WriteEntry(msg, EventLogEntryType.Error, 1002);
		}

	}
}
