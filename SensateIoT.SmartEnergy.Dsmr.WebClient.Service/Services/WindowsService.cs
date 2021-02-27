using System;
using System.ServiceProcess;
using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Application;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Services
{
	public class WindowsService : ServiceBase
	{
	    private static readonly ILog logger = LogManager.GetLogger("WindowsService");

	    private WebClientService m_client;

		protected override void OnStart(string[] args)
		{
			this.StartService();
		}

		protected override void OnStop()
		{
			this.StopService();
		}

		public void StartService()
		{
            AppSettings settings;

            try {
	            settings = ConfigurationLoader.BuildAppSettings();
            } catch(Exception ex) {
                logger.Fatal("Unable to parse configuration file. Fatalling.", ex);
                throw ex;
            }
            
            this.m_client = new WebClientService(settings);
            this.m_client.Start();
		}

		public void StopService()
		{
			this.m_client.Stop();
		}
	}
}
