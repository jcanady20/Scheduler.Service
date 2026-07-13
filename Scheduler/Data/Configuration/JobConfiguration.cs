using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Configuration;

public class JobConfiguration : IEntityTypeConfiguration<Entities.Job>
{
  public void Configure(EntityTypeBuilder<Job> builder)
  {
    builder.ToTable("Jobs");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedOnAdd();
    builder.Property(x => x.Name).IsRequired().HasMaxLength(60);
    builder.Property(x => x.Description).HasMaxLength(500);
    builder.HasMany(x => x.JobSteps).WithOne().OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(x => x.JobSchedules).WithOne().OnDelete(DeleteBehavior.Cascade);
  }
}
