using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Adamant.NotificationService.Models;
using System.Security.Cryptography;
using Adamant.NotificationService.DataContext;

namespace Adamant.NotificationService.SignalsRegistration
{
	public class SignalsPoller: PollingWorkerBase<Transaction>
	{
		#region Dependencies

		private readonly AdamantApi _adamantApi;
		private readonly DevicesContext _context;

		#endregion

		#region Properties

		public string PrivateKey { get; set; }

		#endregion

		#region Ctor

		protected SignalsPoller(ILogger<PollingWorkerBase<Transaction>> logger, AdamantApi api, DevicesContext context) : base(logger)
		{
			_adamantApi = api;
			_context = context;
		}

		#endregion

		protected override async Task<int> GetCurrentLastHeight()
		{
			var transactions = await _adamantApi.GetChatTransactions(0, 0, ChatType.signal);

			return transactions?.FirstOrDefault()?.Height ?? 0;
		}

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			return await _adamantApi.GetChatTransactions(height, offset, ChatType.signal);
		}

		protected override int ProcessNewTransactions(IEnumerable<Transaction> transactions)
		{
			var count = transactions.Count();
			if (count == 0) {
				Logger.LogWarning("Requested to process 0 transactions");
				return LastHeight;
			}

			Logger.LogInformation("Processing {0} transactions.", count);
			var devices = new List<Device>();

			foreach (var trs in transactions)
			{
				var chat = trs.Asset?.Chat;
				if (chat == null)
					continue;

				String message = null;

				try {
					message = Encryption.DecodeMessage(chat.Message, chat.Nonce, trs.SenderPublicKey, PrivateKey);
					var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(message);
					var device = new Device
					{
						Address = trs.SenderId,
						Token = deviceInfo.Token,
						Provider = deviceInfo.Provider,
						RegistrationDate = DateTime.UtcNow
					};

					devices.Add(device);
				} catch (CryptographicException e) {
					Logger.LogError(e,
					                "Failed to decode message.\nTransactionId: {0}\nMessage: {1}\nNonce: {2}, PublicKey: {3}, SecretKey: {4}",
					                trs.Id,
					                chat.Message,
									chat.Nonce,
									trs.SenderPublicKey,
									"");
					continue;
				} catch (JsonException e) {
					Logger.LogError(e, "Failed to deserialize device info. TransactionId: {0}, message: {1}", trs.Id, message);
					continue;
				} catch (Exception e) {
					Logger.LogError(e, "Failed to read device info from message. TransactionId: {0}, message: {1}", trs.Id, message);
				}
			}

			try {
				_context.AddRange(devices);
				_context.SaveChanges();
			} catch (Exception e) {
				Logger.LogCritical(e, "Failed to save context");
			}

			// Last height
			var newest = transactions.OrderByDescending(t => t.Height).First();
			return newest.Height;
		}
	}
}
