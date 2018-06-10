using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public class ChatPollingWorker: PollingWorkerBase<Transaction>
	{
		#region Dependencies

		private readonly AdamantApi _adamantApi;
		private readonly DevicesContext _context;
		private readonly IPusher _pusher;

		#endregion

		protected ChatPollingWorker(ILogger<PollingWorkerBase<Transaction>> logger, AdamantApi api, IPusher pusher, DevicesContext context) : base(logger)
		{
			_adamantApi = api;
			_context = context;
			_pusher = pusher;
		}
		
		protected override async Task<int> GetCurrentLastHeight()
		{
			var transactions = await _adamantApi.GetChatTransactions(0, 0);
			return transactions?.FirstOrDefault()?.Height ?? 0;
		}

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			return await _adamantApi.GetChatTransactions(height, offset);
		}

		protected override int ProcessNewTransactions(IEnumerable<Transaction> transactions)
		{
			var count = transactions.Count();
			if (count == 0)
			{
				Logger.LogWarning("Requested to process 0 transactions");
				return LastHeight;
			}

			Logger.LogInformation("Processing {0} transactions.", count);

			var recipients = transactions.GroupBy(t => t.RecipientId);
			var devicesToNotify = new List<(Device device, IEnumerable<Transaction> transactions)>();

			foreach (var recipient in recipients)
			{
				var address = recipient.Key;

				if (string.IsNullOrEmpty(address))
					continue;

				var registeredDevices = _context.Devices.Where(d => d.Address.Equals(address));

				foreach (var device in registeredDevices)
					devicesToNotify.Add((device, recipient.AsEnumerable()));
			}

			Logger.LogInformation("Notifying {0} devices.", devicesToNotify.Count);

			foreach (var d in devicesToNotify)
			{
				_pusher.NotifyDevice(d.device, d.transactions);
			}

			// Last height
			var newest = transactions.OrderByDescending(t => t.Height).First();
			return newest.Height;
		}
	}
}
