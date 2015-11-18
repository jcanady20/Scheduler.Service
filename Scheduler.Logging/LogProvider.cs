using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Scheduler.Logging
{
    public class LogProvider
    {
        private static readonly object m_lockObj = new object();
        private ILogger m_logger = null;
        private static LogProvider m_instance;
        private LogProvider()
        { }
        public static LogProvider Instance
        {
            get
            {
                lock(m_lockObj)
                {
                    if(m_instance == null)
                    {
                        m_instance = new LogProvider();
                    }
                }
                return m_instance;
            }
        }
        public ILogger Logger
        {
            get
            {
                lock(m_lockObj)
                {
                    if(m_logger == null)
                    {
                        m_logger = CreateLogger();
                    }
                }
                return m_logger;
            }
        }
        private ILogger CreateLogger()
        {
            if(m_logger != null)
            {
                return m_logger;
            }
            var logServiceAssemblyName = System.Configuration.ConfigurationManager.AppSettings["log-service-assembly-name"];
            logServiceAssemblyName = logServiceAssemblyName ?? "Scheduler.Logging.NLog";
            var currentLocation = new FileInfo(typeof(LogProvider).Assembly.Location).DirectoryName;
            var filePath = Path.Combine(currentLocation, logServiceAssemblyName);
            if(File.Exists(filePath) == false)
            {
                throw new FileNotFoundException($"Unable to Locate the specified Logger Service Assembly [{logServiceAssemblyName}]");
            }
            //  Load the requested assembly from the file path
            var assembly = Assembly.LoadFile(filePath);
            //  Get the First Class that implements the ILogger Interface
            //  Only support One  ILogger Implementation per Assembly
            var type = assembly.
                GetTypes()
                .Where(x => typeof(ILogger).IsAssignableFrom(x))
                .Where(x => x.IsAbstract == false)
                .Where(x => x.IsInterface == false)
                .FirstOrDefault();
            if(type == null)
            {
                throw new TypeAccessException($"Unable to find a Implementation of the ILogger interface in the specified Assembly [{logServiceAssemblyName}]");
            }
            m_logger = (ILogger)Activator.CreateInstance(type);
            return m_logger;
        }

        public static ILogger LoggerInstance()
        {
            var logger = LogProvider.Instance.CreateLogger();

            return logger;
        }
    }
}
