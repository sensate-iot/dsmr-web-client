using System;
using System.Threading;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public class PingService : IDisposable
	{
		private static ILog logger = LogManager.GetLogger("PingService");

		private readonly TimeSpan m_interval;
		private readonly TimeSpan m_startDelay;
		private readonly IWebSocketClientService m_client;

		private int m_unansweredPings;
		private Timer m_timer;

		public PingService(TimeSpan interval, IWebSocketClientService client)
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

		private async void InvokeAsync(object arg)
		{
			Interlocked.Increment(ref this.m_unansweredPings);
			await this.m_client.PingAsync(CancellationToken.None);
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
					Interlocked.Decrement(ref this.m_unansweredPings);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
			this.m_timer?.Dispose();
		}
	}
}