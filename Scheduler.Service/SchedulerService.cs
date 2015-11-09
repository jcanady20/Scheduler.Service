using System;
using System.Diagnostics;
using System.ServiceProcess;
using Scheduler.Logging;
using Scheduler.Logging.NLog;
using Scheduler.Http;
using Scheduler.Extensions;
using Scheduler.Scheduling;
using Scheduler.Jobs;

namespace Scheduler.Service
{
    public class SchedulerService : ServiceBase
	{
		private ILogger m_logger;
		private EventLog m_eventLog;
        private ScheduleEngine m_scheduleEngine;

		#region Public Consts
		public static readonly string EVENTLOGNAME = "Scheduler";
		public static readonly string EVENTLOGSOURCE = "SchedulerService";
		public static readonly string SERVICENAME = "Scheduler";
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
			m_logger = new NLogger("Scheduler.Scheduling.SchedulerService");

            m_scheduleEngine = new ScheduleEngine();
        }

        public string BaseAddress { get; private set; }
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

        public void Start()
		{
			m_logger.Trace("Starting Scheduler Service");
			OwinService.Instance.Start();
			JobEngine.Instance.Start();
            m_scheduleEngine.Start();
		}
		public new void Stop()
		{
			m_logger.Trace("Stopping Scheduler Service");
			OwinService.Instance.Stop();
            m_scheduleEngine.Stop();
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
            m_scheduleEngine.Pasuse();
            JobEngine.Instance.Pause();
			base.OnPause();
		}
		protected override void OnContinue()
		{
            m_scheduleEngine.Continue();
            JobEngine.Instance.Continue();
            base.OnContinue();
		}
		protected override void OnShutdown()
		{
			this.Stop();
			base.OnShutdown();
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
