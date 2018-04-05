using System.Collections.Generic;

namespace Scheduler.Data
{
	public class PagedList<T>
	{
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public IEnumerable<T> Entities { get; set; }
	}
}
