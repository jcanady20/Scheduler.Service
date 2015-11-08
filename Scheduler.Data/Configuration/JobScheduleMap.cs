using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
	public class JobScheduleMap : EntityTypeConfiguration<Entities.JobSchedule>
	{
		public JobScheduleMap()
		{
			HasKey(x => new { x.JobId, x.ScheduleId });
		}
	}
}
