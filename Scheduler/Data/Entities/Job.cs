﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Scheduler.Data.Entities
{
	public class Job
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		[JsonIgnore]
		public virtual ICollection<JobSchedule> JobSchedules { get; set; }
		[JsonIgnore]
		public virtual ICollection<JobStep> JobSteps { get; set; }
        [JsonIgnore]
        public virtual ICollection<JobHistory> JobHistory { get; set; }
	}
}
