using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class JobActivityConfiguration : IEntityTypeConfiguration<Entities.JobActivity>
{
  public void Configure(EntityTypeBuilder<JobActivity> builder)
  {
    builder.ToTable("JobActivity");
    builder.HasKey(x => x.Id);
  }
}
