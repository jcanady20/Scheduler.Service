using System.Data.Entity.ModelConfiguration;

namespace Scheduler.Data.Configuration
{
	public class JobActivityMap : EntityTypeConfiguration<Entities.JobActivity>
	{
		public JobActivityMap()
		{
			Map(x => x.ToTable("JobActivity"));
			HasKey(x => x.Id);
		}
	}
}
