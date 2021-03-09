using System;
using System.Threading;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Clients;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Services
{
	public sealed class WebClientService : IDisposable
	{
		private static readonly ILog logger = LogManager.GetLogger(nameof(WebClientService));

		private readonly DsmrClient m_client;
		private readonly EnvironmentSensorClient m_environmentalClient;
		private readonly CancellationTokenSource m_source;

		public WebClientService(AppSettings settings)
		{
			this.m_source = new CancellationTokenSource();

			/* Start/add other services */
			this.m_client = new DsmrClient(settings);
			this.m_environmentalClient = new EnvironmentSensorClient(settings);
		}

		public void Start()
		{
			logger.Info("Starting web client service hosting...");
			this.m_client.Start(this.m_source.Token);
			this.m_environmentalClient.Start(this.m_source.Token);
		}

		public void Stop()
		{
			logger.Warn("Stopping web client service...");
			this.m_source.Cancel();
			this.m_client.Stop();
			this.m_environmentalClient.Stop();
			logger.Warn("Web client service stopped.");
		}

		public void Dispose()
		{
			this.m_source.Dispose();
		}
	}
}
