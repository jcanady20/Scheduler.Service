using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class ActivityConfiguration : IEntityTypeConfiguration<Entities.Activity>
{
  public void Configure(EntityTypeBuilder<Activity> builder)
  {
    builder.ToTable("Activity");
    builder.HasKey(x => x.JobId);
  }
}
