using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace Scheduler.Data.Context
{
	public interface IContext : IDisposable
	{
		
		DbSet<Entities.Job> Jobs { get; set; }
		DbSet<Entities.JobHistory> JobHistory { get; set; }
		DbSet<Entities.JobStep> JobSteps { get; set; }
		DbSet<Entities.JobSchedule> JobSchedules { get; set; }
		DbSet<Entities.Activity> Activity { get; set; }
		DbSet<Entities.JobActivity> JobActivity { get; set; }

		DbContextConfiguration Configuration { get; }
		Database Database { get; }
		void SetModified(object entity);
		DbEntityValidationResult GetValidationResult(object entity);
		int ExecuteSqlCommand(string sql, params object[] parameters);
		IEnumerable<T> SqlQuery<T>(string sql, params object[] parameters);
		int SaveChanges();
		Task<int> SaveChangesAsync();
		Task<int> SaveChangesAsync(CancellationToken cancellationToken);

	}
}
