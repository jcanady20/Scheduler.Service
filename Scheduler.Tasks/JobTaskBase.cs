using System;
using System.Diagnostics;
using System.Threading;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Queries;
using Scheduler.Logging;
using Scheduler.Extensions;

namespace Scheduler.Tasks
{
    public abstract class JobTaskBase : IJobTask, IDisposable
	{
		protected ILogger m_logger;
        protected JobStep m_taskStep;
        protected IContext m_db;
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

		public IJobTask Create(IContext db, JobStep step, ILogger logger)
		{
            m_logger = logger;
			m_db = db;
			m_taskStep = step;
			this.Name = step.Name;
			this.StepId = step.StepId;
			return this;
		}

		public JobStepOutCome Execute(CancellationToken cancelToken)
		{
			m_logger.Trace("Started :: Executing Step {0} ({1}) for {2}", m_taskStep.StepId, m_taskStep.Name, m_taskStep.SubSystem);
			this.OutCome = JobStepOutCome.Unknown;
			var sw = new Stopwatch();
			sw.Start();
			try
			{
                //	Step the Started DateTime value
                this.Started = DateTime.Now;
				this.OnExecute();
				this.OutCome = JobStepOutCome.Succeeded;
				this.OutComeMessage = "The step succeeded.";
			}
			catch(Exception e)
			{
				m_logger.Error("Error :: Encountered an error while Executing Step {0} ({1}) for {2}", m_taskStep.StepId, m_taskStep.Name, m_taskStep.SubSystem);
				m_logger.Error(e);
				this.OutCome = JobStepOutCome.Failed;
				this.OutComeMessage = "The step Failed. " + e.BuildExceptionMessage();
			}
			finally
			{
				sw.Stop();
				//	Set the Duration to the Elapsed Timespan
				this.Duration = sw.Elapsed;
				//	Set the Completed DateTime value
				this.Completed = DateTime.Now;
				ReportOutCome();
			}
			m_logger.Trace("Completed :: Executing Step {0} ({1}) for {2} {3}", m_taskStep.StepId, m_taskStep.Name, m_taskStep.SubSystem, this.OutCome);
			return this.OutCome;
		}

		public abstract void OnExecute();

        protected ILogger Log
        {
            get
            {
                return m_logger;
            }
        }

        protected void ReportOutCome()
		{
			//	Add a JobHistory Entry for this JobStep
			m_db.AddJobHistory(m_taskStep.JobId, m_taskStep.StepId, m_taskStep.Name, OutCome, OutComeMessage, this.Started, this.Duration);
			//	Update Activity
			m_db.SetActivityLastStepDetails(m_taskStep.JobId, m_taskStep.StepId, this.Started.Value, this.Duration.Value);
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
}
