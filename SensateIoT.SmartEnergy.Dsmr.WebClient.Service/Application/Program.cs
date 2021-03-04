using System;
using System.ServiceProcess;

using log4net;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Services;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Application
{
    public class Program
    {
	    private static readonly ILog logger = LogManager.GetLogger("WebClientService");

        public static void Main(string[] args)
        {
            logger.Warn("Starting service!");

            if(Environment.UserInteractive) {
				StartInteractive();
            } else {
				using(var svc = new WindowsService()) {
					ServiceBase.Run(svc);
				}
            }

			logger.Warn("Service stopped.");
        }

        private static void StartInteractive()
        {
			var svc = new WindowsService();
			var host = new ConsoleHost(svc);

			host.Run();
			Console.WriteLine("Press <ENTER> to exit.");
			Console.ReadLine();
        }
    }
}
