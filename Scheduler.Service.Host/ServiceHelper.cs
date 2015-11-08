using System;
using System.ComponentModel;
using System.Diagnostics;
using Scheduler.Service;

namespace Scheduler.Service
{
    public partial class Program
    {
        [Description("Install Service")]
        static void InstallService()
        {
            try
            {
                // "/LogFile=" - to suppress install log creation
                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/LogFile=", System.Reflection.Assembly.GetExecutingAssembly().Location });
            }
            catch { }
        }

        [Description("Uninstall Service")]
        static void UninstallService()
        {
            try
            {
                // "/LogFile=" - to suppress uninstall log creation
                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", System.Reflection.Assembly.GetExecutingAssembly().Location });
            }
            catch { }
        }

        [Description("Start Service Interactively")]
        static void StartService()
        {
            Console.WriteLine("Attempting to start Api Service Host.");
            m_service = new SchedulerService();
            m_service.Start();
            Console.WriteLine("Started Api Service Host: {0}", m_service.BaseAddress);
            Console.WriteLine("Use StopService Command to stop the service...");
        }

        [Description("Stops the Service")]
        static void StopService()
        {
            if (m_service != null && m_service.CanStop)
                m_service.Stop();
        }

        [Description("Create the Eventlog used by the service")]
        static void CreateEventLog()
        {
            var eventLogSource = SchedulerService.EVENTLOGSOURCE;
            var eventLogName = SchedulerService.EVENTLOGNAME;
            if (!EventLog.SourceExists(eventLogSource))
            {
                EventLog.CreateEventSource(eventLogSource, eventLogName);
            }
        }
    }
}
