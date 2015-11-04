using System;

namespace Scheduler.Data.Models
{
    public class JobHistorySearch
	{
		public JobHistorySearch()
		{
			Page = 1;
			PageSize = 10;
		}
		public Guid JobId { get; set; }
		public string Term { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	}
}
