using System.Collections.Generic;
using Adamant.Models;
using Adamant.NotificationService.Models;

namespace Adamant.NotificationService
{
	public interface IPusher
	{
		void NotifyDevice(Device device, IEnumerable<Transaction> transactions);

		void Start();
		void Stop();
	}
}
