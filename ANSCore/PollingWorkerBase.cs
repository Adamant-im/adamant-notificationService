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

		public int LastHeight { get; private set; }

		/// <summary>
		/// Amount of transactions for every request. Default is 100.
		/// 
		/// Setter is not yet impletented by API.
		/// </summary>
		public int TransactionsLimit { get; /*set;*/ } = 100;
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
				LastHeight = await GetCurrentLastHeight();
			}

			Logger.LogInformation("Begin polling from {0}", LastHeight);

			while (!token.IsCancellationRequested)
			{
				Logger.LogDebug("Updating from height: {0}", LastHeight);

				var transactions = await GetTransactions(LastHeight);

				if (transactions != null && transactions.Count > 0)
				{
					Logger.LogInformation("Got {0} new transactions, processing", transactions.Count);
					LastHeight = ProcessNewTransactions(transactions);
					Logger.LogInformation("New lastHeight: {0}", LastHeight);
				}
				else
					Logger.LogDebug("Got 0, delay.");

				await Task.Delay(Delay);
			}
		}

		private async Task<List<T>> GetTransactions(int height, int offset = 0)
		{
			var transactions = await GetNewTransactions(height, offset);
			var list = new List<T>(transactions);

			var count = transactions.Count();
			if (count >= TransactionsLimit)
			{
				Logger.LogDebug("Received {0} transactions. Requesting more. (h: {1}, o: {2})", count, height, offset);
				var more = await GetTransactions(height, offset + TransactionsLimit);
				list.AddRange(more);
			}

			return list;
		}

		#endregion

		#region Abstract

		/// <summary>
		/// Get current latest height.
		/// </summary>
		protected abstract Task<int> GetCurrentLastHeight();

		/// <summary>
		/// Processes received new transactions. This method should return new last height.
		/// </summary>
		/// <param name="transactions">New transactions.</param>
		/// <returns>Last height</returns>
		protected abstract int ProcessNewTransactions(IEnumerable<T> transactions);

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
