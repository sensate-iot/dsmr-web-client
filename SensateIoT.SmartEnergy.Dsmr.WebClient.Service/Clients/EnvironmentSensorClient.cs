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
	public sealed class EnvironmentSensorClient : IDisposable
	{
		private static readonly ILog logger = LogManager.GetLogger(nameof(DsmrClient));

		private readonly IListener m_listener;
		private readonly PingService m_pingService;
		private readonly IWebSocketEventService m_parser;
		private Thread m_serviceThread;
		private readonly AppSettings m_settings;

		public EnvironmentSensorClient(AppSettings settings)
		{
			var uri = new Uri($"{settings.Remote.Scheme}://{settings.Remote.Host}:{settings.Remote.Port}{settings.Remote.Path}");
			logger.Info($"Remote address: {uri}");

			this.m_listener = new WebSocketListenerService(uri, LogManager.GetLogger("DsmrWebSocketClientService"));
			this.m_pingService = new PingService(TimeSpan.FromSeconds(5), this.m_listener);
			this.m_settings = settings;

			var storageService = new MeasurementStorageService(settings.Remote.StorageUri,
				settings.Listener.ApiKey,
				LogManager.GetLogger(nameof(MeasurementStorageService)));
			this.m_parser = new EnvironmentSensorParserService(storageService, new ParserSettings {
				ApiKey = settings.Listener.ApiKey,
				Sensors = settings.Listener.Sensors
			}, LogManager.GetLogger(nameof(EnvironmentSensorParserService)));
		}

		public void Start(CancellationToken ct)
		{
			logger.Info("Starting web client service hosting...");

			this.InternalStart();

			var bg = new Thread(() => {
				try {
					logger.Info("Web client service started.");
					this.m_listener.OnWebSocketMessage += this.m_parser.HandleWebSocketEvent;
					this.m_listener.OnWebSocketMessage += this.m_pingService.HandleWebSocketEvent;
					this.m_pingService.Start();
					this.m_listener.StartAsync(ct).GetAwaiter().GetResult();
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
			this.m_listener.StopAsync();
			this.m_serviceThread.Join();
			logger.Warn("Web client service stopped.");
		}

		private void InternalStart()
		{
			this.m_listener.SetUserData(this.m_settings.Listener.UserId, this.m_settings.Listener.ApiKey);

			foreach(var kvp in this.m_settings.Listener.Sensors) {
				this.m_listener.AddSensor(kvp.Value.EnvironmentSensor.Id, kvp.Value.EnvironmentSensor.Key);
			}

			this.m_listener.OnWebSocketMessage += WebSocket_Event;
		}

		private static void WebSocket_Event(object sender, WebSocketEventArgs args)
		{
			var service = sender as IListener;

			switch(args.Type) {

			case EventType.Connected:
				service?.AuthorizeUserAsync(CancellationToken.None).GetAwaiter().GetResult();
				Thread.Sleep(500);
				service?.AuthorizeSensorsAsync(CancellationToken.None).GetAwaiter().GetResult();
				break;

			case EventType.Rx:
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
