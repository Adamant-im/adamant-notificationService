using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Adamant.NotificationService
{
	public abstract class PollingWorkerBase<T>
	{
		#region Dependencies

		protected ILogger<PollingWorkerBase<T>> Logger { get; }

		#endregion

		#region Properties

		private CancellationTokenSource _tokenSource;

		public Task PollingTask { get; private set; }

		public string PrivateKey { get; set; }

		public int LastHeight { get; private set; }
		public int TransactionsLimit { get; set; }
		public TimeSpan Delay { get; set; }

		public bool IsWorking { get; private set; }


		#endregion

		#region Ctor

		protected PollingWorkerBase(ILogger<PollingWorkerBase<T>> logger)
		{
			Logger = logger;
		}

		#endregion

		#region Public API

		public void StartPolling(bool warmup)
		{
			if (IsWorking)
				return;

			IsWorking = true;

			Logger.LogInformation("Start polling");

			_tokenSource = new CancellationTokenSource();
			PollingTask = UpdateTransactionsLoop(warmup, _tokenSource.Token);
		}

		public void StopPolling()
		{
			if (!IsWorking)
				return;

			Logger.LogInformation("Stop polling");
			_tokenSource?.Cancel();

			IsWorking = false;
		}

		#endregion

		#region Logic

		private async Task UpdateTransactionsLoop(bool warmup, CancellationToken token)
		{
			if (warmup)
			{
				Logger.LogInformation("Warming up, getting current top height.");
				LastHeight = await Warmup();
			}

			Logger.LogInformation("Begin polling from {0}", LastHeight);

			while (!token.IsCancellationRequested)
			{
				Logger.LogDebug("Updating from height: {0}", LastHeight);

				var transactions = await GetTransactions(LastHeight);

				if (transactions == null || transactions.Count == 0) {
					Logger.LogDebug("No new transactions");
					break;
				}

				Logger.LogDebug("Got {0} new transactions, processing", transactions.Count);

				ProcessNewTransactions(transactions);

				await Task.Delay(Delay);
			}
		}

		private async Task<List<T>> GetTransactions(int height, int offset = 0)
		{
			Logger.LogDebug("Getting transaction, height: {0}, offset: {1}", height, offset);
			var transactions = await GetNewTransactions(0, height);
			var list = new List<T>(transactions);

			var count = transactions.Count();
			if (count >= TransactionsLimit)
			{
				Logger.LogDebug("Received {0} transactions. (h: {1}, o: {2})", count, height, offset);
				var more = await GetTransactions(height, offset + TransactionsLimit);
				list.AddRange(more);
			}

			return list;
		}

		/// <summary>
		/// Get current latest height.
		/// </summary>
		protected abstract Task<int> Warmup();

		/// <summary>
		/// Processes received new transactions.
		/// </summary>
		/// <param name="transactions">New transactions.</param>
		protected abstract void ProcessNewTransactions(IEnumerable<T> transactions);

		/// <summary>
		/// Gets the new transactions from API.
		/// </summary>
		/// <returns>New transactions.</returns>
		/// <param name="height">Height.</param>
		/// <param name="offset">Offset.</param>
		protected abstract Task<IEnumerable<T>> GetNewTransactions(int height, int offset = 0);

		#endregion
	}
}
