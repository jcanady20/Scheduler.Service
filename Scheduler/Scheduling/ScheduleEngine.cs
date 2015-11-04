using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading;

using Scheduler.Extensions;
using Scheduler.Logging;
using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Data.Context;

namespace Scheduler.Scheduling
{
    public class ScheduleEngine
    {
        #region Private Fields
        private const int ONE_MINUTE = 1000 * 60;
        private const long REFRESH_INTERVAL = ONE_MINUTE * 5;
        private long m_lastrefresh;
        private Thread m_worker;
        private bool m_paused;
        private bool m_canceled;
        private ILogger m_logger;
        private static readonly TimeSpan WORKER_DELAY = TimeSpan.FromSeconds(1);
        private ICollection<JobSchedule> m_jobSchedules;
        #endregion

        public ScheduleEngine()
        {
            m_logger = new NLogger("Scheduler.Scheduling.ScheduleEngine");
        }

        public void Start()
        {
            m_canceled = false;
            m_paused = false;
            if(m_worker != null && m_worker.IsAlive)
            {
                return;
            }
            m_worker = new Thread(new ThreadStart(DoWork));
            m_worker.Start();

            QueueStartUpJobs();
        }
        public void Stop()
        {
            m_canceled = true;
        }
        public void Pasuse()
        {
            m_paused = true;
        }
        public void Continue()
        {
            m_paused = false;
        }

        private void DoWork()
        {
            while(m_canceled == false)
            {
                if(m_paused)
                {
                    Sleep();
                    continue;
                }
                using (var db = ContextFactory.CreateContext())
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.AutoDetectChangesEnabled = false;
                    var schedules = GetActiveSchedules(db);
                    ProcessSchedules(schedules, db);
                }

                Sleep();
            }
        }

        private void ProcessSchedules(IEnumerable<JobSchedule> schedules, IContext db)
        {
            if (schedules == null)
            {
                return;
            }
            schedules.ForEach(sch => {
                ProcessSchedule(sch, db);
            });
        }

        private void ProcessSchedule(JobSchedule jobSchedule, IContext db)
        {
            var nextRunDateTime = jobSchedule.CalculateNextRun(jobSchedule.LastRunDateTime);
            if (nextRunDateTime <= DateTime.Now)
            {
                var job = GetJob(jobSchedule.JobId, db);
                if (job != null && job.Enabled)
                {
                    JobEngine.Instance.Add(job, jobSchedule);
                }
            }
        }

        private ICollection<JobSchedule> GetActiveSchedules(IContext db)
        {
            try
            {
                if ((DateTime.Now.Ticks - REFRESH_INTERVAL) > m_lastrefresh)
                {
                    m_jobSchedules = db.JobSchedules.Where(x => x.Enabled == true && ((int)x.Type & 61) != 0).ToList();
                    m_lastrefresh = DateTime.Now.Ticks;
                }
            }
            catch (Exception e)
            {
                m_logger.Error(e);
            }
            return m_jobSchedules;
        }

        private void QueueStartUpJobs()
        {
            m_logger.Trace("Queueing Startup Jobs");
            ICollection<JobSchedule> schedules = null;
            using (var db = ContextFactory.CreateContext())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;

                schedules = db.JobSchedules
                    .AsNoTracking()
                    .Where(x => x.Type == FrequencyType.OnStartup)
                    .ToList();
                m_logger.Trace("Found {0} enabled start-up jobs", schedules.Count);

                foreach (var js in schedules)
                {
                    var job = GetJob(js.JobId, db);
                    if (job != null)
                    {
                        JobEngine.Instance.Add(job, js);
                    }
                }
            }
        }

        private Job GetJob(Guid id, IContext db)
        {
            Job job = null;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                job = db.Jobs
                    .AsNoTracking()
                    .Include(r => r.JobSteps)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == id);
            }
            catch (Exception e)
            {
                m_logger.Error(e);
            }
            return job;
        }

        private void Sleep()
        {
            Thread.Sleep(WORKER_DELAY);
        }
    }
}
