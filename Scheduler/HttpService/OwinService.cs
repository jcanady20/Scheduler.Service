using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.Owin.Hosting;
using System.Threading.Tasks;

using Scheduler.Logging;

namespace Scheduler.HttpService
{
	public class OwinService
	{
		private ILogger m_logger;
		private static readonly object m_lockObj = new object();
		private static OwinService m_instance;
		private IDisposable m_owin;
		private OwinService()
		{
			m_logger = new NLogger();
			BaseAddress = ServiceUrl.GetServiceUrl();
		}
		public static OwinService Instance
		{
			get
			{
				lock(m_lockObj)
				{
					if(m_instance == null)
					{
						m_instance = new OwinService();
					}
				}
				return m_instance;
			}
		}
		public string BaseAddress { get; private set; }
		public void Start()
		{
			m_logger.Info("Started :: Starting OwinServices");
			if(!this.Enabled)
			{
				m_logger.Warn("Owin Services are disabled in the Application Configuration File");
			}
			if (m_owin == null)
			{
				m_owin = WebApp.Start<App_Start.Startup>(url: BaseAddress);
			}
			m_logger.Info("Completed :: Starting OwinServices");
		}
		public void Stop()
		{
			m_logger.Info("Started :: Stopping OwinServices");
			if (m_owin != null)
			{
				m_owin.Dispose();
				m_owin = null;
			}
			m_logger.Info("Completed :: Stopping OwinServices");
		}
		public bool Enabled { get { return isEnabled(); } }
		private bool isEnabled()
		{
			var result = true;
			var tmp = ConfigurationManager.AppSettings["owinService"];
			if(String.IsNullOrEmpty(tmp))
			{
				return result;
			}
			Boolean.TryParse(tmp, out result);
			return result;
		}

	}
}
