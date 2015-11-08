using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Scheduler.Data.Entities
{
    public class JobSchedule
    {
        public JobSchedule()
        {
            LastRunDateTime = DateTime.MinValue;
        }
        public Guid JobId { get; set; }
        public Guid ScheduleId { get; set; }
        public DateTime LastRunDateTime { get; set; }

        public virtual Job Job { get; set; }
        public virtual Schedule Schedule { get; set; }
    }
}
