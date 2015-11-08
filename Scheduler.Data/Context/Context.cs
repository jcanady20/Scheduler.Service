using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity.Validation;

namespace Scheduler.Data.Context
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class Context : DbContext, IContext
	{
		public Context() : base() { }
		public Context(string nameOfConnectionString) : base(nameOfConnectionString)
		{
			Database.SetInitializer<Context>(null);
		}
		
		#region
		public DbSet<Entities.Job> Jobs { get; set; }
		public DbSet<Entities.JobHistory> JobHistory { get; set; }
		public DbSet<Entities.JobStep> JobSteps { get; set; }
		public DbSet<Entities.JobSchedule> JobSchedules { get; set; }
		public DbSet<Entities.Activity> Activity { get; set; }
		public DbSet<Entities.JobActivity> JobActivity { get; set; }
        public DbSet<Entities.Schedule> Schedules { get; set; }
        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.AddFromAssembly(typeof(Context).Assembly);
			base.OnModelCreating(modelBuilder);
		}

		public void SetModified(object entity)
		{
			Entry(entity).State = EntityState.Modified;
		}

		public DbEntityValidationResult GetValidationResult(object entity)
		{
			return Entry(entity).GetValidationResult();
		}

		public int ExecuteSqlCommand(string sql, params object[] parameters)
		{
			return this.Database.ExecuteSqlCommand(sql, parameters);
		}

		public IEnumerable<T> SqlQuery<T>(string sql, params object[] parameters)
		{
			return this.Database.SqlQuery<T>(sql, parameters);
		}

		internal static IContext Create()
		{
			return new Context("SqlServer");
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
