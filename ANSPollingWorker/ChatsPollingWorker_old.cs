using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	public class ChatsPollingWorker_old
	{
		#region Dependencies

		private readonly ILogger<ChatsPollingWorker_old> _logger;
		private readonly AdamantApi _adamantApi;
		private readonly DevicesContext _context;
		private readonly IPusher _pusher;

		#endregion

		#region Properties

		private CancellationTokenSource _tokenSource;

		public Task PollingTask { get; private set; }

		public int LastHeight { get; private set; }
		public TimeSpan Delay { get; set; }

		#endregion

		#region Ctor

		public ChatsPollingWorker_old(ILogger<ChatsPollingWorker_old> logger, AdamantApi api, IPusher pusher, DevicesContext context)
		{
			_logger = logger;
			_adamantApi = api;
			_context = context;
			_pusher = pusher;
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

				var transactions = await _adamantApi.GetChatTransactions(0, 0);

				var newest = transactions.FirstOrDefault();

				if (newest != null)
				{
					LastHeight = newest.Height;
					_logger.LogInformation("Received last height: {0}", LastHeight);
				}
				else
					_logger.LogInformation("No transactions received, starting from height 0");
			}

			_logger.LogInformation("Begin polling from {0}", LastHeight);

			while (!token.IsCancellationRequested)
			{
				_logger.LogDebug("Updating... Last height: {0}", LastHeight);
				var transactions = await _adamantApi.GetChatTransactions(0, LastHeight);

				if (transactions != null && transactions.Any())
				{
					#if DEBUG
					_logger.LogDebug("Got {0} new transactions", transactions.Count());
					#else
					_logger.LogDebug("Got new transactions");
					#endif

					var maxHeight = transactions.Max(t => t.Height);
					if (LastHeight < maxHeight)
						LastHeight = maxHeight;

					var task = new Task(() => HandleNewTransactions(transactions));
					task.Start();
				}

				await Task.Delay(Delay);
			}

			token.ThrowIfCancellationRequested();
		}

		private void HandleNewTransactions(IEnumerable<Transaction> transactions)
		{
			var recipients = transactions.GroupBy(t => t.RecipientId);
			var devicesToNotify = new Dictionary<Device, IEnumerable<Transaction>>();

			foreach (var recipient in recipients)
			{
				var address = recipient.Key;

				if (string.IsNullOrWhiteSpace(address))
					continue;
				
				var registeredDevices = _context.Devices.Where(d => d.Address.Equals(recipient.Key));


				// TODO: Проверять на дубликаты, добавлять транзакции в имеющиеся коллекции
				if (registeredDevices != null && registeredDevices.Any())
				{
					foreach (var device in registeredDevices)
					{
						devicesToNotify.Add(device, recipient.AsEnumerable());
					}
				}
			}

			foreach (var device in devicesToNotify)
			{
				var task = new Task(() => _pusher.NotifyDevice(device.Key, device.Value));
				task.Start();
			}
		}

		#endregion
	}
}
