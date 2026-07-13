using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class JobHistoryConfiguration : IEntityTypeConfiguration<Entities.JobHistory>
{
  public void Configure(EntityTypeBuilder<JobHistory> builder)
  {
    builder.ToTable("JobHistory");
    builder.HasKey(X => X.Id);
    builder.Property(x => x.Id).ValueGeneratedOnAdd();
    builder.Property(x => x.Message).HasMaxLength(1024);
    builder.Property(x => x.StepName).HasMaxLength(128);
    builder.Property(x => x.StepId).IsRequired();
  }
}
