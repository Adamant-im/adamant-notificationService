using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public class TransferPollingWorker: TransactionsPollingWorkerBase
	{
		public TransferPollingWorker(ILogger<TransferPollingWorker> logger,
		                             AdamantApi api,
		                             IPusher pusher,
		                             DevicesContext context) : base(logger, api, pusher, context)
		{
		}

		protected override async Task<int> GetCurrentLastHeight()
		{
			var transactions = await _adamantApi.GetTransactions(0, 0, TransactionType.Send);

			var latest = transactions?.FirstOrDefault();
			if (latest != null)
				return latest.Height + 1;
			else
				return 0;
		}

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			return await _adamantApi.GetTransactions(height, offset, TransactionType.Send);
		}

		protected override int GetLastHeight(IEnumerable<Transaction> transactions)
		{
			// Last height. API returns transactions with height >= lastHeight. So +1.
			return transactions.OrderByDescending(t => t.Height).First().Height + 1;
		}
	}
}
