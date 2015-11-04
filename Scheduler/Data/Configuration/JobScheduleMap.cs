using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
	public class JobScheduleMap : EntityTypeConfiguration<Entities.JobSchedule>
	{
		public JobScheduleMap()
		{
			HasKey(x => x.Id);
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Name).IsRequired();
			Property(x => x.Name).HasMaxLength(60);
		}
	}
}
