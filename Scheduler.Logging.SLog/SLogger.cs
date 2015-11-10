using System;
using System.Collections.Generic;
using Scheduler.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Scheduler.Logging.SLog
{
    public class SLogger : ILogger
    {
        private Serilog.ILogger m_logger;
        public SLogger()
        {
            m_logger = new Serilog.LoggerConfiguration()
                 .ReadFrom.AppSettings()
                 .CreateLogger();
        }

        public void Debug(string message)
        {
            m_logger.Debug(message);
        }
        public void Debug(string message, params object[] args)
        {
            m_logger.Debug(message, args);
        }

        public void Error(Exception exception, string message)
        {
            m_logger.Error(exception, message);
        }
        public void Error(Exception exception, string message, params object[] args)
        {
            m_logger.Error(exception, message, args);
        }

        public void Info(string message)
        {
            m_logger.Information(message);
        }
        public void Info(string message, params object[] args)
        {
            m_logger.Information(message, args);
        }

        public void Trace(string message)
        {
            m_logger.Verbose(message);
        }
        public void Trace(string message, params object[] args)
        {
            m_logger.Verbose(message, args);
        }

        public void Warn(string message)
        {
            m_logger.Warning(message);
        }
        public void Warn(string message, params object[] args)
        {
            m_logger.Warning(message, args);
        }
    }
}
