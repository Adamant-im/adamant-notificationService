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

namespace Adamant.NotificationService
{
	public abstract class PollingWorkerBase<T> where T: Transaction
	{
		#region Dependencies

		protected ILogger<PollingWorkerBase<T>> Logger { get; }
		protected ANSContext Context { get; }
		protected AdamantApi Api { get; }

		#endregion

		#region Properties

		public abstract string ServiceName { get; }

		private CancellationTokenSource _tokenSource;

		public Task PollingTask { get; private set; }

		public int LastHeight { get; private set; }

		/// <summary>
		/// Amount of transactions for every request. Not all API's provides support for this param.
		/// </summary>
		public int TransactionsLimit { get; } = 100;
		public TimeSpan Delay { get; set; }

		public bool IsWorking { get; private set; }

		#endregion

		#region Ctor

		protected PollingWorkerBase(AdamantApi api, ANSContext context, ILogger<PollingWorkerBase<T>> logger)
		{
			Api = api;
			Context = context;
			Logger = logger;
		}

		#endregion

		#region Public API

		public void StartPolling(StartupMode startupMode)
		{
			if (IsWorking)
				return;

			IsWorking = true;

			Logger.LogInformation("Start polling");

			_tokenSource = new CancellationTokenSource();
			PollingTask = UpdateTransactionsLoop(startupMode, _tokenSource.Token);
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

		private async Task UpdateTransactionsLoop(StartupMode startupMode, CancellationToken token)
		{
			switch (startupMode)
			{
				case StartupMode.database:
					Logger.LogInformation("Getting stored height from databse...");
					LastHeight = GetStoredLastHeight();

					if (LastHeight == 0)
					{
						Logger.LogInformation("No stored height, warming up from network.");
						LastHeight = await GetNetworkCurrentLastHeight();
					}

					break;
					
				case StartupMode.network:
					Logger.LogInformation("Warming up, getting current top height.");
					LastHeight = await GetNetworkCurrentLastHeight();
					break;

				case StartupMode.initial:
					break;
			}

			Logger.LogInformation("Begin polling from {0}", LastHeight);

			while (!token.IsCancellationRequested)
			{
				Logger.LogDebug("Updating from height: {0}", LastHeight);

				var transactions = await GetTransactions(LastHeight);

				if (transactions != null && transactions.Count > 0)
				{
					Logger.LogInformation("Got {0} new transactions, processing", transactions.Count);
					ProcessNewTransactions(transactions);
					LastHeight = GetLastHeight(transactions);
					Logger.LogInformation("New lastHeight: {0}", LastHeight);
					StoreLastHeight();
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
		/// Processes received new transactions. This method should return new last height.
		/// </summary>
		/// <param name="transactions">New transactions.</param>
		/// <returns>Last height</returns>
		protected abstract void ProcessNewTransactions(IEnumerable<T> transactions);

		/// <summary>
		/// Gets the new transactions from API.
		/// </summary>
		/// <returns>New transactions.</returns>
		/// <param name="height">Height.</param>
		/// <param name="offset">Offset.</param>
		protected abstract Task<IEnumerable<T>> GetNewTransactions(int height, int offset = 0);

		/// <summary>
		/// Get current Blockchain latest height.
		/// </summary>
		protected virtual async Task<int> GetNetworkCurrentLastHeight()
		{
			var transactions = await Api.GetTransactions(0, 0, null, 1);
			return transactions?.FirstOrDefault()?.Height ?? 0;
		}

		/// <summary>
		/// Gets stored last height from dabatase.
		/// </summary>
		protected virtual int GetStoredLastHeight()
		{
			return Context.ServiceStates.FirstOrDefault(ss => ss.Service.Equals(ServiceName))?.LastHeight ?? 0;
		}

		protected virtual void StoreLastHeight()
		{
			var serviceState = Context.ServiceStates.FirstOrDefault(ss => ss.Service.Equals(ServiceName));
			if (serviceState == null)
			{
				serviceState = new ServiceState { Service = ServiceName };
				Context.ServiceStates.Add(serviceState);
			}

			serviceState.LastHeight = LastHeight;
			serviceState.Date = DateTime.UtcNow;
			Context.SaveChanges();
		}

		/// <summary>
		/// Calculate last height
		/// </summary>
		/// <returns>Last height</returns>
		protected virtual int GetLastHeight(IEnumerable<T> transactions)
		{
			return transactions.OrderByDescending(t => t.Height).First().Height;
		}

		#endregion
	}
}
