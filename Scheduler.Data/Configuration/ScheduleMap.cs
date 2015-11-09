using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
    public class ScheduleMap : EntityTypeConfiguration<Entities.Schedule>
    {
        public ScheduleMap()
        {
            ToTable("Schedules");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
