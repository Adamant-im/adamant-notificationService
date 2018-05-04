using System;
using System.Collections.Generic;
using Adamant.Models;
using Newtonsoft.Json;

namespace Adamant.Api.Responses
{
	internal class TransactionsResponse
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("transactions")]
		public IEnumerable<Transaction> Transactions { get; set; }

		[JsonProperty("count")]
		public string Count { get; set; }
	}
}
