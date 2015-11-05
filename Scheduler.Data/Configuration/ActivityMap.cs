using System.Data.Entity.ModelConfiguration;

namespace Scheduler.Data.Configuration
{
	public class ActivityMap : EntityTypeConfiguration<Entities.Activity>
	{
		public ActivityMap()
		{
			Map(x => x.ToTable("Activity"));
			HasKey(x => x.JobId);
			HasRequired(x => x.Job).WithOptional();
		}
	}
}
