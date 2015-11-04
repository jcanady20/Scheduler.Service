using System;

namespace Scheduler.Logging
{
	public interface ILogger
	{
		void Debug(string message);
		void Debug(string message, params object[] args);
		void Info(string message);
		void Info(string message, params object[] args);
		void Trace(string message);
		void Trace(string message, params object[] args);
		void Warn(string message);
		void Warn(string message, params object[] args);
		void Error(Exception x);
		void Error(string message, params object[] args);
	}
}
