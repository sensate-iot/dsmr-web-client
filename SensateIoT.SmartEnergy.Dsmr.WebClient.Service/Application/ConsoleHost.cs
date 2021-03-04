/*
 * TriggerService application runner.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;

using log4net;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Services;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Application
{
	public class ConsoleHost
	{
		private static readonly ILog logger = LogManager.GetLogger(nameof(ConsoleHost));

		private readonly ManualResetEvent m_resetEvent;
		private readonly WindowsService m_service;

		public ConsoleHost(WindowsService host)
		{
			this.m_service = host;
			this.m_resetEvent = new ManualResetEvent(false);
			Console.CancelKeyPress += this.CancelEvent_Handler;
		}

		private void CancelEvent_Handler(object sender, ConsoleCancelEventArgs e)
		{
			this.m_resetEvent.Set();
			e.Cancel = true;
		}

		public void Run()
		{
			logger.Info("Starting as console application.");
			this.m_service.StartService();
			this.m_resetEvent.WaitOne();
			this.m_service.StopService();
		}
	}
}
