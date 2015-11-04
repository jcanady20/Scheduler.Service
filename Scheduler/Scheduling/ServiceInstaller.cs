using System.ComponentModel;
using System.ServiceProcess;


namespace Scheduler.Scheduling
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
			serviceAdmin.ServiceName = "Visualutions Inc. Scheduler Service";
			serviceAdmin.DisplayName = "Visualutions Inc. Scheduler Service";
			serviceAdmin.Description = "Visualutions Inc. Scheduler Service is used to manage events across Visualutions Applications";

			Installers.Add(process);
			Installers.Add(serviceAdmin);

		}
	}
}
