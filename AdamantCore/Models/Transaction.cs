using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adamant.Models
{
	public class Transaction
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

        [JsonProperty("blockId")]
        public string BlockId { get; set; }

        [JsonProperty("type")]
		public TransactionType Type { get; set; }

        [JsonProperty("timestamp")]
		public double Timestamp { get; set; }

        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey { get; set; }

		[JsonProperty("requesterPublicKey")]
		public string RequesterPublicKey { get; set; }

        [JsonProperty("senderId")]
        public string SenderId { get; set; }

        [JsonProperty("recipientId")]
        public string RecipientId { get; set; }

        [JsonProperty("recipientPublicKey")]
        public string RecipientPublicKey { get; set; }

        [JsonProperty("amount")]
		public decimal Amount { get; set; }

        [JsonProperty("fee")]
		public decimal Fee { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

		[JsonProperty("signSignature")]
		public string SignSignature { get; set; }

        [JsonProperty("signatures")]
        public List<string> Signatures { get; set; }

        [JsonProperty("confirmations")]
		public UInt32? Confirmations { get; set; }

		[JsonProperty("asset")]
		public TransactionAsset Asset { get; set; }

	}
}

/* Raw JSON
{
    "id": "9356000252801394663",
    "height": 1,
    "blockId": "13096746075322409574",
    "type": 0,
    "timestamp": 0,
    "senderPublicKey": "2efef768fc41949aaf5124d7a3663ae843fec87c930494ce37a54d83383b634d",
    "senderId": "U13113937065479682572",
    "recipientId": "U2065436277795836384",
    "recipientPublicKey": null,
    "amount": 392000000000000,
    "fee": 0,
    "signature": "d61793303db6c1daa813903a4473f10d0b2f5ab965d2da4b6416bfd3a4482777645bdbca04d3ba89e0e21f96bb01bdf443c354b53991c7a1542626e4b345e804",
    "signatures": [
    ],
    "confirmations": 2448056,
    "asset": {
    }
}
*/

/* Chat raw JSON
{
  "id": "8564574931493759812",
  "height": 2738150,
  "blockId": "10228493037174337765",
  "type": 8,
  "timestamp": 20705948,
  "senderPublicKey": "926a4842a7a0cadebcb131b9402f82edff06959c3358441772d9ec53c6f1d03f",
  "requesterPublicKey": null,
  "senderId": "U14399712535055444887",
  "recipientId": "U6714409511125668607",
  "recipientPublicKey": null,
  "amount": 0,
  "fee": 100000,
  "signature": "9e749f704bdeeb041ef89962e488f26b17ea5f2ec087a163f14e3749b742e447ba3486348442422b43905bb05bbcfbb0b36147c92149713194c33e7a80bd220e",
  "signSignature": null,
  "signatures": [
  ],
  "confirmations": null,
  "asset": {
    "chat": {
      "message": "16326d3b48defaa145c02c5c3789b7ac35970648e3a3def4f9462babe766281f05713007f626509bfca40337e0b77a561f089676b694b1c141386ed06214aeb3bfcfd1ea7e6116ad53",
      "own_message": "a3a3a700722cee019f332d00c371d158fe7d260a016aacc3",
      "type": 1
    }
  }
}
*/
