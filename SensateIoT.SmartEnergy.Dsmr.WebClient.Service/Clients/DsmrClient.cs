using System;
using System.Threading;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Settings;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Clients
{
	public sealed class DsmrClient : IDisposable
	{
		private static readonly ILog logger = LogManager.GetLogger(nameof(DsmrClient));

		private readonly IListener m_dsmrClient;
		private readonly PingService m_pingService;
		private readonly IWebSocketEventService m_parser;
		private Thread m_serviceThread;
		private readonly AppSettings m_settings;

		public DsmrClient(AppSettings settings)
		{
            var uri = new Uri($"{settings.Remote.Scheme}://{settings.Remote.Host}:{settings.Remote.Port}{settings.Remote.Path}");
            logger.Info($"Remote address: {uri}");

			this.m_dsmrClient = new WebSocketListenerService(uri, LogManager.GetLogger("DsmrWebSocketClientService"));
			this.m_pingService = new PingService(TimeSpan.FromSeconds(5), this.m_dsmrClient);
			this.m_settings = settings;

			var storageService = new MeasurementStorageService(new AuthorizationService(),
				settings.Remote.StorageUri,
				settings.Listener.ApiKey,
				LogManager.GetLogger(nameof(MeasurementStorageService)));
			this.m_parser = new DsmrParserService(storageService, new ParserSettings {
				ApiKey = settings.Listener.ApiKey,
				Sensors = settings.Listener.Sensors
			}, LogManager.GetLogger("DsmrParserService"));
		}

		public void Start(CancellationToken ct)
		{
			logger.Info("Starting web client service hosting...");

			this.InternalStart();

            var bg = new Thread(() => {
	            try {
		            logger.Info("Web client service started.");
		            this.m_dsmrClient.OnWebSocketMessage += this.m_parser.HandleWebSocketEvent;
		            this.m_dsmrClient.OnWebSocketMessage += this.m_pingService.HandleWebSocketEvent;
		            this.m_pingService.Start();
		            this.m_dsmrClient.StartAsync(ct).GetAwaiter().GetResult();
	            } catch(OperationCanceledException ex) {
					logger.Info("Operation cancelled: stop requested.", ex);
	            }
            }) { IsBackground = true };

            bg.Start();
            this.m_serviceThread = bg;
		}

		public void Stop()
		{
			logger.Warn("Stopping web client service...");
			this.m_pingService.Stop();
			this.m_dsmrClient.StopAsync();
			this.m_serviceThread.Join();
			logger.Warn("Web client service stopped.");
		}

		private void InternalStart()
		{
            this.m_dsmrClient.SetUserData(this.m_settings.Listener.UserId, this.m_settings.Listener.ApiKey);

            foreach(var kvp in this.m_settings.Listener.Sensors) {
	            this.m_dsmrClient.AddSensor(kvp.Key, kvp.Value.Key);
            }

            this.m_dsmrClient.OnWebSocketMessage += WebSocket_Event;
		}

		private static void WebSocket_Event(object sender, WebSocketEventArgs args)
		{
			var service = sender as IListener;

			switch (args.Type)
			{
				case EventType.Rx:
					break;

				case EventType.Connected:
					service?.AuthorizeUserAsync(CancellationToken.None).GetAwaiter().GetResult();
					Thread.Sleep(500);
					service?.AuthorizeSensorsAsync(CancellationToken.None).GetAwaiter().GetResult();
					break;

				case EventType.Ping:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
			this.m_pingService.Dispose();
			this.m_parser.Dispose();
		}
	}
}
