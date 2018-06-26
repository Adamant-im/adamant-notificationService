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
