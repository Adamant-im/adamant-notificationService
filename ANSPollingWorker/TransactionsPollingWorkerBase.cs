﻿using System.Collections.Generic;
using System.Linq;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public abstract class TransactionsPollingWorkerBase : PollingWorkerBase<Transaction>
	{
		#region Dependencies

		protected readonly IPusher _pusher;

		#endregion

		public TransactionsPollingWorkerBase(ILogger<PollingWorkerBase<Transaction>> logger,
		                                     AdamantApi api,
		                                     IPusher pusher, 
		                                     ANSContext context) : base(api, context, logger)
		{
			_pusher = pusher;
		}

		protected override void ProcessNewTransactions(IEnumerable<Transaction> transactions)
		{
			var count = transactions.Count();
			if (count == 0)
			{
				Logger.LogWarning("Requested to process 0 transactions");
				return;
			}

			Logger.LogInformation("Processing {0} transactions.", count);

			var recipients = transactions.GroupBy(t => t.RecipientId);
			var devicesToNotify = new List<(Device device, IEnumerable<Transaction> transactions)>();

			foreach (var recipient in recipients)
			{
				var address = recipient.Key;

				if (string.IsNullOrEmpty(address))
					continue;

				var registeredDevices = Context.Devices.Where(d => d.Address.Equals(address));

				foreach (var device in registeredDevices)
					devicesToNotify.Add((device, recipient.AsEnumerable()));
			}

			Logger.LogInformation("Notifying {0} devices.", devicesToNotify.Count);

			foreach (var d in devicesToNotify)
			{
				_pusher.NotifyDevice(d.device, d.transactions);
			}
		}
	}
}
