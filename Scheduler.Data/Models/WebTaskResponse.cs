using System;
using System.Collections.Generic;

namespace Scheduler.Data.Models
{
    public class WebTaskResponse
	{
		public WebTaskResponse()
		{
			OutCome = JobStepOutCome.Unknown;
			Errors = new List<Exception>();
		}
		public Guid RequestId { get; set; }
		public JobStepOutCome OutCome { get; set; }
		public string Message { get; set; }
		public Nullable<TimeSpan> Duration { get; set; }
		public ICollection<Exception> Errors { get; set; }
	}
}
