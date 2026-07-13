using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class ScheduleConfiguration : IEntityTypeConfiguration<Entities.Schedule>
{
  public void Configure(EntityTypeBuilder<Schedule> builder)
  {
    builder.ToTable("Schedules");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedOnAdd();
    builder.HasMany(x => x.JobSchedules)
      .WithOne()
      .HasForeignKey(x => x.ScheduleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
