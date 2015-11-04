using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
	public class JobMap : EntityTypeConfiguration<Entities.Job>
	{
		public JobMap()
		{
			HasKey(x => x.Id);
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

			MapToStoredProcedures(x => {
				x.Insert(i => i.HasName("[Scheduler].[Job_Insert]"));
				x.Delete(d => d.HasName("[Scheduler].[Job_Delete]"));
				x.Update(u => u.HasName("[Scheduler].[Job_Update]"));
			});

			Property(x => x.Name).IsRequired();
			Property(x => x.Name).HasMaxLength(60);
			Property(x => x.Description).HasMaxLength(500);
			HasMany(x => x.JobSteps).WithRequired().WillCascadeOnDelete();
		}
	}
}
