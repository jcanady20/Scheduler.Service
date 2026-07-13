using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class JobScheduleConfiguration : IEntityTypeConfiguration<Entities.JobSchedule>
{
  public void Configure(EntityTypeBuilder<JobSchedule> builder)
  {
    builder.ToTable("JobSchedules");
    builder.HasKey(x => new { x.JobId, x.ScheduleId });
  }
}
