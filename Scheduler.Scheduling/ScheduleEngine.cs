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
        private Thread m_worker;
        private bool m_paused;
        private bool m_canceled;
        private ILogger m_logger;
        private JobEngine m_jobEngine;
        private static readonly TimeSpan WORKER_DELAY = TimeSpan.FromMinutes(1);
        #endregion

        public ScheduleEngine()
        {
            m_jobEngine = JobEngine.Instance;
            m_logger = new NLogger();
        }

        public ScheduleEngine(JobEngine jobEngine, ILogger logger)
        {
            m_jobEngine = jobEngine;
            m_logger = logger;
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
                try
                {
                    using (var db = ContextFactory.CreateContext())
                    {
                        db.Configuration.ProxyCreationEnabled = false;
                        db.Configuration.AutoDetectChangesEnabled = false;
                        var schedules = GetActiveSchedules(db);
                        ProcessSchedules(schedules, db);
                    }
                }
                catch(Exception e)
                {
                    m_logger.Error(e);
                }

                Sleep();
            }
        }

        private void ProcessSchedules(IEnumerable<Schedule> schedules, IContext db)
        {
            if (schedules == null)
            {
                return;
            }
            schedules.ForEach(sch => {
                ProcessSchedule(sch, db);
            });
        }
        private void ProcessSchedule(Schedule schedule, IContext db)
        {
            var jobSchedules = db.JobSchedules.Where(x => x.ScheduleId == schedule.Id);
            jobSchedules.ForEach(js =>
            {
                js.Schedule = schedule;
                var nextRunDateTime = schedule.CalculateNextRun(js.LastRunDateTime);
                if (nextRunDateTime <= DateTime.Now)
                {
                    db.Jobs
                        .AsNoTracking()
                        .Include(r => r.JobSteps)
                        .Where(x => x.Id == js.JobId)
                        .ForEach(j =>
                        {
                            js.Job = j;
                            m_jobEngine.Add(j, js);
                        });
                }
            });
        }
        private IEnumerable<Schedule> GetActiveSchedules(IContext db)
        {
            IEnumerable<Schedule> schedules = null;
            try
            {
                schedules = db.Schedules.Where(x => x.Enabled == true && ((int)x.Type & 61) != 0).ToList();
            }
            catch (Exception e)
            {
                m_logger.Error(e);
            }
            return schedules;
        }
        private void QueueStartUpJobs()
        {
            m_logger.Trace("Queueing Startup Jobs");
            using (var db = ContextFactory.CreateContext())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;

                db.Schedules
                    .AsNoTracking()
                    .Where(x => x.Type == FrequencyType.OnStartup)
                    .ToList()
                    .ForEach(s => {
                        ProcessSchedule(s, db);
                    });
            }
        }
        private void Sleep()
        {
            Thread.Sleep(WORKER_DELAY);
        }
    }
}
