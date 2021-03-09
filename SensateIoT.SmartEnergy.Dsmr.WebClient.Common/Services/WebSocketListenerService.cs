using System;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public sealed class WebSocketListenerService : BaseListener, IListener, IDisposable
	{
		private CancellationTokenSource m_ct;
		private Task m_backgroundTask;

		public WebSocketListenerService(Uri remote, ILog logger) : base(remote, logger)
		{
			this.m_backgroundTask = Task.CompletedTask;
		}

		public Task StartAsync(CancellationToken token)
		{
			this.m_ct = CancellationTokenSource.CreateLinkedTokenSource(token);
			this.m_backgroundTask = this.ListenAsync(token);

			return this.m_backgroundTask;
		}

		public async Task StopAsync()
		{
			this.m_ct.Cancel();
			await this.m_backgroundTask.ConfigureAwait(false);
		}


		public void Dispose()
		{
			this.m_socket?.Dispose();
			this.m_ct?.Dispose();
		}
	}
}
