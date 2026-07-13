using Microsoft.EntityFrameworkCore;

namespace Scheduler.Data.Context;

public class ScheduleContext : DbContext
{
  public ScheduleContext() : base()
  { }
  public ScheduleContext(DbContextOptions options) : base(options)
  { }
  
  public DbSet<Entities.Job> Jobs { get; set; }
  public DbSet<Entities.JobHistory> JobHistory { get; set; }
  public DbSet<Entities.JobStep> JobSteps { get; set; }
  public DbSet<Entities.JobSchedule> JobSchedules { get; set; }
  public DbSet<Entities.Activity> Activity { get; set; }
  public DbSet<Entities.JobActivity> JobActivity { get; set; }
  public DbSet<Entities.Schedule> Schedules { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScheduleContext).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}
