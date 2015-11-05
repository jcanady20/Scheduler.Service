using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Scheduler.Data.Entities
{
	public class Job
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		[JsonIgnore]
		public virtual ICollection<JobSchedule> JobSchedules { get; set; }
		[JsonIgnore]
		public ICollection<JobStep> JobSteps { get; set; }
	}
}
