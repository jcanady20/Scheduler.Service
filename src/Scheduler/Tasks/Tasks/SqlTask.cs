using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Scheduler.Tasks;

[DisplayName("Transact-SQL script")]
[Description("Transact-SQL script")]
public class SqlTask : JobTaskBase
{
  public SqlTask()
  {
  }

  public override void OnExecute()
  {
    if(String.IsNullOrEmpty(_taskStep.Command))
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
                cmd.CommandText = _taskStep.Command;
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
    var csb = new DbConnectionStringBuilder();
        if(!String.IsNullOrEmpty(_taskStep.DataSource))
        {
            csb["DataSource"] = _taskStep.DataSource;
        }
        if (!String.IsNullOrEmpty(_taskStep.DatabaseName))
        {
            csb["InitialCatalog"] = _taskStep.DatabaseName;
        }
        if (String.IsNullOrEmpty(_taskStep.UserName) == false && _taskStep.Password != null)
    {
      csb["UserID"] = _taskStep.UserName;
      csb["Password"] = Encoding.UTF8.GetString(_taskStep.Password);
    }
        else
        {
            csb["IntegratedSecurity"] = true;
        }
    return csb.ConnectionString;
  }
}
