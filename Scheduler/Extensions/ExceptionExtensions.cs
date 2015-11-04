using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Threading;

namespace Scheduler.Extensions
{
	public static class ExceptionExtensions
	{
		public static string BuildExceptionMessage(this Exception e)
		{
			var msg = new StringBuilder();
			msg.AppendLine(e.Message);
			if (e.InnerException != null) { msg.Append(e.InnerException.BuildExceptionMessage()); }
			return msg.ToString();
		}

		public static bool IsCatchableExceptionType(this Exception e)
		{

			// a 'catchable' exception is defined by what it is not.
			var type = e.GetType();

			return ((type != typeof(StackOverflowException)) &&
					(type != typeof(OutOfMemoryException)) &&
					(type != typeof(ThreadAbortException)) &&
					(type != typeof(NullReferenceException)) &&
					(type != typeof(AccessViolationException)) &&
					!typeof(SecurityException).IsAssignableFrom(type));
		}
	}
}
