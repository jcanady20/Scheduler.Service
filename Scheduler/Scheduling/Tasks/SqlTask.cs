using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Scheduler.Tasks;

namespace Scheduler.Scheduling.Tasks
{
    [DisplayName("Transact-SQL script")]
	[Description("Transact-SQL script")]
	public class SqlTask : BaseTask
	{
		public SqlTask()
		{
		}

		public override void OnExecute()
		{
			if(String.IsNullOrEmpty(m_taskStep.Command))
			{
				throw new Exception("Aborting Command Execution, the Command was Empty");
			}
            ExecuteSqlCommand();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private async void ExecuteSqlCommand()
        {
            using (var conn = CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = m_taskStep.Command;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        private DbConnection CreateConnection()
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            return factory.CreateConnection();
        }
		private string GetConnectionString()
		{
			var csb = new SqlConnectionStringBuilder();
            if(!String.IsNullOrEmpty(m_taskStep.DataSource))
            {
                csb.DataSource = m_taskStep.DataSource;
            }
            if (!String.IsNullOrEmpty(m_taskStep.DatabaseName))
            {
                csb.InitialCatalog = m_taskStep.DatabaseName;
            }
            if (String.IsNullOrEmpty(m_taskStep.UserName) == false && m_taskStep.Password != null)
			{
				csb.UserID = m_taskStep.UserName;
				csb.Password = Encoding.UTF8.GetString(m_taskStep.Password);
			}
            else
            {
                csb.IntegratedSecurity = true;
            }
			return csb.ConnectionString;
		}
	}
}
