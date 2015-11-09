using System;
using System.Collections.Generic;
using Scheduler.Extensions;
using System.Linq;
using System.Text;
using Serilog.Core;
using Serilog.Configuration;
using System.Threading.Tasks;

namespace Scheduler.Logging.SLog
{
    public class SLogger : ILogger
    {
        private Serilog.ILogger m_logger;
        public SLogger()
        {
            CreateLogger();
        }

        private void CreateLogger()
        {
            m_logger = new Serilog.LoggerConfiguration()
                .ReadFrom.KeyValuePairs(GetApplicationSettings())
                .CreateLogger();
        }

        public void Debug(string message)
        {
            throw new NotImplementedException();
        }
        public void Debug(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception x)
        {
            throw new NotImplementedException();
        }
        public void Error(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }
        public void Info(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message)
        {
            throw new NotImplementedException();
        }
        public void Trace(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }
        public void Warn(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<KeyValuePair<string, string>> GetApplicationSettings()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            foreach(var key in appSettings.Keys)
            {
                foreach(var val in appSettings.GetValues(key.ToString()))
                {
                    yield return new KeyValuePair<string, string>(key.ToString(), val);
                }
            }
        }
    }
}
