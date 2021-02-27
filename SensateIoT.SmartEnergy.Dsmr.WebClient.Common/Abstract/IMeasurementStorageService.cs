using System;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract
{
	public interface IMeasurementStorageService : IDisposable
	{
		Task StoreAsync(Measurement measurement, CancellationToken ct = default);
	}
}
