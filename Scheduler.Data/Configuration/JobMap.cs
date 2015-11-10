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

            ToTable("Jobs");
			//MapToStoredProcedures(x => {
			//	x.Insert(i => i.HasName("[dbo].[Job_Insert]"));
			//	x.Delete(d => d.HasName("[dbo].[Job_Delete]"));
			//	x.Update(u => u.HasName("[dbo].[Job_Update]"));
			//});

			Property(x => x.Name).IsRequired();
			Property(x => x.Name).HasMaxLength(60);
			Property(x => x.Description).HasMaxLength(500);
			HasMany(x => x.JobSteps).WithRequired().WillCascadeOnDelete();
            HasMany(x => x.JobHistory).WithRequired().WillCascadeOnDelete();
            HasMany(x => x.JobSchedules).WithRequired().HasForeignKey(x => x.JobId).WillCascadeOnDelete();
		}
	}
}
