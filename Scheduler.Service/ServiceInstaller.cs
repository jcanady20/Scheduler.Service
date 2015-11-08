using System.ComponentModel;
using System.ServiceProcess;


namespace Scheduler.Service
{
    [RunInstaller(true)]
	public class ServiceInstall : System.Configuration.Install.Installer
	{
		public ServiceInstall()
		{
			ServiceProcessInstaller process = new ServiceProcessInstaller();
			ServiceInstaller serviceAdmin = new ServiceInstaller();

			process.Account = ServiceAccount.LocalSystem;

			serviceAdmin.StartType = ServiceStartMode.Automatic;
			serviceAdmin.ServiceName = "Scheduler Service";
			serviceAdmin.DisplayName = "Scheduler Service";
			serviceAdmin.Description = "Scheduler Service is used to scheudle events across Applications";

			Installers.Add(process);
			Installers.Add(serviceAdmin);
		}
	}
}
