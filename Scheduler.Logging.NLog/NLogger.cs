﻿using System;
using NLog;
using Scheduler.Extensions;

namespace Scheduler.Logging.NLog
{
    public class NLogger : ILogger
	{
		private Logger _logger;

		public NLogger()
		{
			_logger = LogManager.GetCurrentClassLogger();
		}
		public NLogger(string name)
		{
			_logger = LogManager.GetLogger(name);
		}
		public void Trace(string message)
		{
			_logger.Trace(message);
		}
		public void Trace(string message, params object[] args)
		{
			_logger.Trace(message, args);
		}
		public void Info(string message)
		{
			_logger.Info(message);
		}
		public void Info(string message, params object[] args)
		{
			_logger.Info(message, args);
		}
		public void Debug(string message)
		{
			_logger.Debug(message);
		}
		public void Debug(string message, params object[] args)
		{
			_logger.Debug(message, args);
		}
		public void Warn(string message)
		{
			_logger.Warn(message);
		}
		public void Warn(string message, params object[] args)
		{
			_logger.Warn(message, args);
		}
		public void Error(Exception exception, string message = null)
		{
            if(String.IsNullOrEmpty(message))
            {
                message = exception.BuildExceptionMessage();
            }
			_logger.Error(exception, message);
		}
		public void Error(Exception exception, string message, params object[] args)
		{
			_logger.Error(exception, message, args);
		}
		public void Fatal(Exception x)
		{
			_logger.Fatal(x, x.BuildExceptionMessage());
		}
		public void Log(LogLevel logLevel, string message, params object[] args)
		{
			_logger.Log(logLevel, message, args);
		}
	}
}
