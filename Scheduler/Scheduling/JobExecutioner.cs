using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Data.Queries;

using Scheduler.Logging;


namespace Scheduler.Scheduling
{
    public class JobExecutioner : IJobExecutioner
	{
		#region Private Methods
		private IJobTask m_lastJobTask;
		private ILogger m_logger;
		private IContext m_db;
		private Job m_job;
		private Stopwatch m_stopwatch;
		private JobSchedule m_jobSchedule;
		private CancellationTokenSource m_cancelToken;
		private JobStatus m_status;
		#endregion

		public JobExecutioner(Job job, JobSchedule jobSchedule)
		{
			m_db = ContextFactory.CreateContext();
			m_job = job;
			m_jobSchedule = jobSchedule;
			m_stopwatch = new Stopwatch();
			m_cancelToken = new CancellationTokenSource();
			m_logger = new NLogger("Scheduler.Scheduling.JobExecutioner");
			this.OutCome = JobStepOutCome.Unknown;
			this.Status = JobStatus.WaitingForWorkerThread;
			ReportQueuedDateTime();
		}

		public Guid JobId { get { return m_job.Id; } }
		public string Name { get { return m_job.Name; } }
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
					ReportStatusActivity();
				}				
			}
		}

		public void Cancel()
		{
			m_cancelToken.Cancel();
		}

		public void Execute()
		{
			SetupExecute();

			foreach(var task in JobTasks())
			{
				if (m_cancelToken.IsCancellationRequested)
				{
					CancelExecute();
					break;
				}
				this.m_lastJobTask = task;
				this.OutCome = JobStepOutCome.Succeeded;
				if (this.OutCome != JobStepOutCome.Succeeded)
				{
					break;
				}
			}
			
			CompleteExecute();
		}

		private void SetupExecute()
		{
			m_logger.Trace("Started :: Job ({0}) has started Execution", m_job.Name);
			this.StartDateTime = DateTime.Now;
			m_stopwatch.Start();
			this.Status = JobStatus.Executing;
		}

		private void CancelExecute()
		{
			this.OutCome = JobStepOutCome.Cancelled;
			m_logger.Trace("Completed :: Job ({0}) has been Cancelled", m_job.Name);
			CompleteExecute();
		}

		private void CompleteExecute()
		{
			m_stopwatch.Stop();
			this.Duration = m_stopwatch.Elapsed;
			this.CompletedDateTime = DateTime.Now;
			this.Status = JobStatus.Idle;
			ReportOutcome();
			ReportOutComeActivity();
			m_logger.Trace("Completed :: Job ({0}) has completed Execution", m_job.Name);
		}

		private IEnumerable<IJobTask> JobTasks()
		{
			var tasks = m_job.JobSteps.OrderBy(x => x.StepId);
			foreach(var t in tasks)
			{
				yield return JobTaskFactory.Instance.CreateTask(m_db, t);
			}
		}

		private string GetOutComeMessage()
		{
			var msg = String.Empty;
			msg = String.Format("The job {0}.", this.OutCome);
			if (m_jobSchedule != null)
			{
				msg += String.Format("The job was Invoked by Schedule ({0})", m_jobSchedule.Name);
			}
			var lastStepName = (this.m_lastJobTask != null) ? this.m_lastJobTask.Name : "";
			var lastStepId = (this.m_lastJobTask != null) ? this.m_lastJobTask.StepId : 0;
			msg += String.Format("The last step to run was step {0} ({1}).", lastStepId, lastStepName);

			return msg;
		}

		private void ReportQueuedDateTime()
		{
			var item = m_db.GetActivity(this.JobId);
			item.QueuedDateTime = DateTime.Now;
			m_db.SaveChanges();
		}

		private void ReportOutcome()
		{
			if (m_jobSchedule != null)
			{
				//	Record the Last Run Details to the JobSchedule object
				m_db.UpdateJobScheduleLastRun(m_jobSchedule.Id, this.CompletedDateTime.Value);
			}
			//	Add a Job History Entry
			m_db.AddJobHistory(m_job.Id, 0, "(Job OutCome)", this.OutCome, this.GetOutComeMessage(), this.CompletedDateTime.Value, this.Duration.Value);
		}

		private void ReportOutComeActivity()
		{
			if (m_db == null) { return;  }
			var item = m_db.GetActivity(this.JobId);
			item.StartDateTime = this.StartDateTime;
			item.CompletedDateTime = this.CompletedDateTime.Value;
			item.LastRunOutCome = this.OutCome;
			item.LastOutComeMessage = this.GetOutComeMessage();
			item.LastRunDateTime = this.StartDateTime.Value;
			item.LastRunDuration = this.Duration.Value;
			if (m_jobSchedule != null)
			{
				item.NextRunDateTime = m_jobSchedule.CalculateNextRun(DateTime.Now);
			}
			
			m_db.SaveChanges();
		}

		private void ReportStatusActivity()
		{
			if(m_db == null)
			{
				return;
			}
			m_db.SetActivityStatus(this.JobId, this.Status);
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
				if(m_cancelToken != null)
				{
					m_cancelToken.Dispose();
					m_cancelToken = null;
				}
				if(m_db != null)
				{
					m_db.Dispose();
					m_db = null;
				}
			}
		}
	}
}
