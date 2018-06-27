using System.Collections.Generic;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public class TransferPollingWorker: TransactionsPollingWorkerBase
	{
		public override string ServiceName { get; } = "TransferPoller";

		public TransferPollingWorker(ILogger<TransferPollingWorker> logger,
		                             AdamantApi api,
		                             IPusher pusher,
		                             ANSContext context) : base(logger, api, pusher, context)
		{
		}

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			return await Api.GetTransactions(height, offset, TransactionType.Send);
		}

		protected override int GetLastHeight(IEnumerable<Transaction> transactions)
		{
			// Last height. API returns transactions with height >= lastHeight. So +1.
			return base.GetLastHeight(transactions) + 1;
		}
	}
}
