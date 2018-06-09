using System;
using System.Threading;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Logging;

namespace ANSKvsRegistrationService
{
	public class KvsPollingWorker
	{
		#region Dependencies

		private readonly ILogger<KvsPollingWorker> _logger;
		private readonly AdamantApi _adamantApi;
		private readonly DevicesContext _context;

		#endregion

		#region Properties

		private CancellationTokenSource _tokenSource;

		public Task PollingTask { get; private set; }

		public int LastHeight { get; private set; }
		public TimeSpan Delay { get; set; }

		#endregion

		#region Ctor

		public KvsPollingWorker(ILogger<KvsPollingWorker> logger, AdamantApi api, DevicesContext context)
		{
			_logger = logger;
			_adamantApi = api;
			_context = context;
		}

		#endregion

		#region Polling

		public void StartPolling(bool warmup)
		{
			_logger.LogInformation("Start polling");

			_tokenSource = new CancellationTokenSource();
			PollingTask = UpdateTransactionsLoop(warmup, _tokenSource.Token);
		}

		public void StopPolling()
		{
			_logger.LogInformation("Stop polling");
			_tokenSource?.Cancel();
		}

		#endregion

		#region Polling logic

		private async Task UpdateTransactionsLoop(bool warmup, CancellationToken token)
		{
			if (warmup)
			{
				_logger.LogInformation("Warming up, getting current top height.");

				//var transactions await _adamantApi.get
			}
		}

		#endregion
	}
}
