using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;

namespace Adamant.NotificationService.PollingWorker
{
	public class AdamantPollingWorker
	{
		#region Dependencies

		public AdamantApi AdamantApi { get; set; }

		public IPusher Pusher { get; set; }

		public DevicesContext Context { get; set; }

		#endregion

		#region Properties

		private CancellationTokenSource _tokenSource;
		public Task PollingTask { get; private set; }

		public int LastHeight { get; private set; }
		public TimeSpan Delay { get; set; }

		#endregion

		#region Polling

		public void StartPolling(bool warmup)
		{
			Console.WriteLine("Start polling");

			_tokenSource = new CancellationTokenSource();
			PollingTask = UpdateTransactionsLoop(warmup, _tokenSource.Token);
		}

		public void StopPolling()
		{
			Console.WriteLine("Stop polling");
			_tokenSource?.Cancel();
		}

		#endregion

		#region Polling logic

		private async Task UpdateTransactionsLoop(bool warmup, CancellationToken token)
		{
			if (warmup)
			{
				Console.WriteLine("Warming up, getting current top height.");

				var transactions = await AdamantApi.GetChatTransactions(0, 0);

				var newest = transactions.FirstOrDefault();

				if (newest != null)
				{
					LastHeight = newest.Height;
					Console.WriteLine("Received last height: {0}", LastHeight);
				}
				else
					Console.WriteLine("No transactions received, starting from height 0");
			}

			while (!token.IsCancellationRequested)
			{
				Console.WriteLine("Updating... Last height: {0}", LastHeight);
				var transactions = await AdamantApi.GetChatTransactions(0, LastHeight);

				if (transactions != null && transactions.Any())
				{
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
				
				var registeredDevices = Context.Devices.Where(d => d.Address.Equals(recipient.Key));


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
				var task = new Task(() => Pusher.NotifyDevice(device.Key, device.Value));
				task.Start();
			}
		}

		#endregion
	}
}
