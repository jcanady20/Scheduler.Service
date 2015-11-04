using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
	public class JobHistoryMap : EntityTypeConfiguration<Entities.JobHistory>
	{
		public JobHistoryMap()
		{
			Map(x => x.ToTable("JobHistory"));
			HasKey(x => x.Id);
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Message).HasMaxLength(1024);
			Property(x => x.StepName).HasMaxLength(128);
			Property(x => x.StepName).IsRequired();
		}
	}
}
