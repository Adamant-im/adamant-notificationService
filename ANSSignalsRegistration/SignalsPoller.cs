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
		#region Properties

		public override string ServiceName { get; } = "SignalsPoller";

		public string Address { get; set; }
		public string PrivateKey { get; set; }

		#endregion

		#region Ctor

		public SignalsPoller(ILogger<PollingWorkerBase<Transaction>> logger,
		                     AdamantApi api,
		                     ANSContext context) : base(api, context, logger)
		{
		}

		#endregion

		protected override async Task<IEnumerable<Transaction>> GetNewTransactions(int height, int offset = 0)
		{
			var transactions = await Api.GetChatTransactions(height, offset, ChatType.signal, Address);
			return transactions.Where(t => t.RecipientId.Equals(Address));
		}

		protected override void ProcessNewTransactions(IEnumerable<Transaction> transactions)
		{
			if (transactions == null)
			{
				Logger.LogError("Requested to process null transactions");
				return;
			}

			var count = transactions.Count();
			if (count == 0) {
				Logger.LogWarning("Requested to process 0 transactions");
				return;
			}

			var devices = new List<Device>();

			foreach (var trs in transactions)
			{
				var chat = trs.Asset?.Chat;
				if (chat == null || chat.Type != ChatType.signal)
				{
					Logger.LogError("Processing: got transaction with wrong ChatAsset. TransactionId: {0}", trs.Id);
					continue;
				}

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
					continue;
				}
			}

			Logger.LogInformation("Processed {0} devices.", devices.Count);

			var duplicates = new List<Device>();
			foreach (var device in devices) {
				var baseDevice = Context.Devices.FirstOrDefault(d => d.Token == device.Token &&
				                                                 d.Address == device.Address &&
				                                                 d.Provider == device.Provider);

				if (baseDevice != null)
					duplicates.Add(device);
			}

			if (duplicates.Count > 0) {

				duplicates.ForEach(d => devices.Remove(d));

				if (devices.Count == 0) {
					Logger.LogInformation("Found {0} duplicates, nothing to save.", duplicates.Count);
					return;
				}
			}

			try {
				Context.AddRange(devices);
				Context.SaveChanges();
			} catch (Exception e) {
				Logger.LogCritical(e, "Failed to save context");
			}
		}
	}
}
