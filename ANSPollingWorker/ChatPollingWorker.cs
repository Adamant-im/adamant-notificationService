using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public class ChatPollingWorker: TransactionsPollingWorkerBase
	{
		public override string ServiceName { get; } = "ChatPoller";

		public ChatPollingWorker(ILogger<ChatPollingWorker> logger,
		                         AdamantApi api,
		                         IPusher pusher,
		                         ANSContext context) : base(logger, api, pusher, context)
		{
		}

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			return await Api.GetChatTransactions(height, offset);
		}
	}
}
