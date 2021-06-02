using System;
using System.Net.WebSockets;
using System.ServiceModel;
using System.Threading;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public sealed class PingService : IDisposable
	{
		private static ILog logger = LogManager.GetLogger("PingService");

		private readonly TimeSpan m_interval;
		private readonly TimeSpan m_startDelay;
		private readonly IListener m_client;

		private int m_unansweredPings;
		private Timer m_timer;

		public PingService(TimeSpan interval, IListener client)
		{
			this.m_interval = interval;
			this.m_startDelay = TimeSpan.FromSeconds(5);
			this.m_client = client;
			this.m_unansweredPings = 0;
		}

		public void Start()
		{
			this.m_timer = new Timer(this.InvokeAsync, null, this.m_startDelay, this.m_interval);
		}

		public void Stop()
		{
			this.m_timer?.Change(Timeout.Infinite, Timeout.Infinite);
		}

		private void ValidatePingResponses()
		{
			if(this.m_unansweredPings >= 5) {
				throw new EndpointNotFoundException($"Remote server not responding: {this.m_unansweredPings} pings unanswered!");
			}
		}

		private async void InvokeAsync(object arg)
		{
			try {
				this.ValidatePingResponses();
				await this.m_client.PingAsync(CancellationToken.None);

				Interlocked.Increment(ref this.m_unansweredPings);
			} catch(InvalidOperationException) {
				logger.Warn("Unable to write PING request to server.");
			} catch(WebSocketException) {
				logger.Warn("Unable to write PING request to server.");
			} catch(Exception ex) {
				logger.Fatal("Unable to ping remote server!", ex);
				Environment.Exit(1);
			}
		}

		public void HandleWebSocketEvent(object sender, WebSocketEventArgs args)
		{
			switch (args.Type)
			{
				case EventType.Rx:
				case EventType.Connected:
					break;

				case EventType.Ping:
					logger.Info("PONG received.");
					Interlocked.Exchange(ref this.m_unansweredPings, 0);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
			this.m_timer.Dispose();
		}
	}
}