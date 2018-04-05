using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Logging
{
    public class LogProvider
    {
        private static readonly object _lockObj = new object();
        private static LogProvider _instance;
        private ILogger _logger;
        private LogProvider()
        { }
        public static LogProvider Instance
        {
            get
            {
                lock(_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new LogProvider();
                    }
                }
                return _instance;
            }
        }

        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new NLogger();
                }
                return _logger;
            }
        }
    }
}
