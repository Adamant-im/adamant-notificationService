using System;
using System.Collections.Generic;
using Adamant.Models;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Configuration;

namespace Adamant.NotificationService
{
	public interface IPusher
	{
		IConfiguration Configuration { get; set; }

		void NotifyDevice(Device device, IEnumerable<Transaction> transactions);

		void Start();
		void Stop();
	}
}
