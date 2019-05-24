using Newtonsoft.Json;
using SharpPusher;

namespace Adamant.NotificationService.ApplePusher
{
    internal class AdamantApnsNotification: ApnsNotification
    {
        [JsonProperty("recipient-address")]
        public string RecipientAddress { get; set; }

        [JsonProperty("txn-id")]
        public string TxnId { get; set; }
    }
}
