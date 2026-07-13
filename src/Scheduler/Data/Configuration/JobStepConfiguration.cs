using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

class JobStepConfiguration : IEntityTypeConfiguration<Entities.JobStep>
{
  public void Configure(EntityTypeBuilder<JobStep> builder)
  {
    builder.ToTable("JobSteps");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedOnAdd();
    builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
    builder.Property(x => x.SubSystem).IsRequired().HasMaxLength(40);
    builder.Property(x => x.DatabaseName).HasMaxLength(128);
  }
}
