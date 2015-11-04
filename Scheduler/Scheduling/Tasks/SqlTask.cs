using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Scheduler.Logging;

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
			using(var conn = CreateConnection())
			{
                conn.ConnectionString = GetConnectionString();
				conn.Open();
				ExecuteCommand(conn);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private void ExecuteCommand(IDbConnection connection)
		{
			using(var cmd = connection.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = m_taskStep.Command;
				cmd.ExecuteNonQuery();
			}
		}

        private IDbConnection CreateConnection()
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            return factory.CreateConnection();
        }


		private string GetConnectionString()
		{
			var cs = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
			var csb = new SqlConnectionStringBuilder(cs);
			if(String.IsNullOrEmpty(m_taskStep.UserName) == false && m_taskStep.Password != null)
			{
				csb.UserID = m_taskStep.UserName;
				csb.Password = Encoding.UTF8.GetString(m_taskStep.Password);
			}
			if (!String.IsNullOrEmpty(m_taskStep.DatabaseName))
			{
				csb.InitialCatalog = m_taskStep.DatabaseName;
			}
			return csb.ConnectionString;
		}
	}
}
