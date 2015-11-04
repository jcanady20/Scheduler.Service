using System.Data;
using System.Data.Common;

namespace Scheduler.Extensions
{
	public static class IDbCommandExtensions
	{
		public static IDataParameter Add(this IDbCommand cmd, string parameterName, DbType dbType, int length)
		{
			var param = cmd.CreateParameter();
			param.ParameterName = parameterName;
			param.DbType = dbType;
			param.Size = length;
			cmd.Parameters.Add(param);
			return param;
		}

		public static IDataParameter AddWithValue(this IDbCommand cmd, string parameterName, object value)
		{
			var param = cmd.CreateParameter();
			param.ParameterName = parameterName;
			param.Value = value;
			cmd.Parameters.Add(param);
			return param;
		}

		public static IDataParameter AddWithValue(this IDbCommand cmd, string parameterName, object value, DbType dbType)
		{
			var param = cmd.CreateParameter();
			param.Value = value;
			param.ParameterName = parameterName;
			param.DbType = dbType;
			cmd.Parameters.Add(param);
			return param;
		}

		public static IDataParameter AddWithValue(this IDbCommand cmd, string parameterName, object value, DbType dbType, int length)
		{
			var param = cmd.CreateParameter();
			param.Value = value;
			param.ParameterName = parameterName;
			param.DbType = dbType;
			param.Size = length;
			cmd.Parameters.Add(param);
			return param;
		}

		public static IDataReader ExecuteReaderWithRetry(this IDbCommand cmd, int maxRetries, int retryDelay, int retryCount = 0)
		{
			return cmd.ExecuteReaderWithRetry(CommandBehavior.Default, maxRetries, retryDelay, retryCount);
		}

		public static IDataReader ExecuteReaderWithRetry(this IDbCommand cmd, CommandBehavior behaviors, int maxRetries, int retryDelay, int retryCount = 0)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Current Retry Count {0}", retryCount));
				return cmd.ExecuteReader(behaviors);
			}
			catch (DbException)
			{
				retryCount += 1;
				if (retryCount < maxRetries)
				{

					System.Threading.Thread.Sleep(retryDelay);
					return cmd.ExecuteReaderWithRetry(behaviors, maxRetries, retryDelay, retryCount);
				}
				else
				{
					throw;
				}
			}
		}
	}
}
