using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Adamant.Api.Responses;
using Adamant.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Adamant.Api
{
	public class AdamantApi
	{
		#region Properties

		public IConfiguration Configuration { get; }

		public ServerDescription CurrentServer { get; private set; }
		public IEnumerable<ServerDescription> Servers { get => ServersList; }
		private List<ServerDescription> ServersList;

		private static string getTransactions = "api/transactions";
		private static string getChatTransactions = "api/chats/get";

		private static Random rnd = new Random();

		private readonly ILogger<AdamantApi> _logger;

		// Shared across the process lifetime: HttpClient is designed to be reused, and creating
		// one per request (as this used to do) leaks a socket into CLOSE-WAIT on every call, since
		// the underlying connection isn't released until the client itself is disposed/collected.
		private static readonly HttpClient httpClient = CreateHttpClient();

		#endregion

		#region ctor

		public AdamantApi(IConfiguration configuration, ILogger<AdamantApi> logger)
		{
			Configuration = configuration;
			_logger = logger;
			var endpointsRaw = configuration.GetSection("Api").GetSection("Server").GetChildren();
			var endpoints = new List<ServerDescription>();

			foreach (var raw in endpointsRaw)
			{
				int? port = null;
				if (int.TryParse(raw["port"], out var p))
					port = p;

				endpoints.Add(new ServerDescription(raw["ip"], raw["protocol"], port));
			}

			ServersList = endpoints;

			// GetChildren() never returns null, only an empty sequence, so this is the only
			// reliable place to catch a missing/empty Api:Server config section — otherwise
			// CurrentServer silently stays null and the first request throws deep inside BuildEndpoint.
			if (ServersList.Count == 0)
				throw new Exception("Can't get endpoint addresses from config");

			SelectNextEndpoint();
		}

		private static HttpClient CreateHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return client;
		}

		#endregion

		#region Public Methods

		public async Task<IEnumerable<Transaction>> GetTransactions(int height, int offset, TransactionType? type, int? limit = null)
		{
			var query = new Dictionary<string, string>
			{
				{ "orderBy", "timestamp:desc" }
			};
			
			if (type.HasValue)
				query.Add("type", ((int)type.Value).ToString());
			
			if (height > 0)
				query.Add("and:fromHeight", height.ToString());

			if (offset > 0)
				query.Add("offset", offset.ToString());

			if (limit.HasValue)
				query.Add("limit", limit.Value.ToString());

			var results = await GetResponse<TransactionsResponse>(getTransactions, query);
			return results.Transactions;
		}

		public async Task<IEnumerable<Transaction>> GetChatTransactions(int height, int offset, ChatType? chatType = null, string address = null)
		{
			var query = new Dictionary<string, string>
			{
				{ "orderBy", "timestamp:desc" }
			};

			if (!string.IsNullOrEmpty(address))
				query.Add("isIn", address);

			if (chatType.HasValue)
				query.Add("type", ((int)chatType.Value).ToString());
			
			if (height > 0)
				query.Add("fromHeight", height.ToString());

			if (offset > 0)
				query.Add("offset", offset.ToString());

			var results = await GetResponse<TransactionsResponse>(getChatTransactions, query);
			return results.Transactions;
		}

		#endregion

		#region Internal logic

		/// <summary>
		/// Selects a different server than <see cref="CurrentServer"/> when more than one is
		/// configured, so a retry after a failed request always targets a fresh node instead of
		/// re-rolling the one that just failed.
		/// </summary>
		public void SelectNextEndpoint()
		{
			if (ServersList == null || ServersList.Count == 0)
				return;

			if (ServersList.Count == 1)
			{
				CurrentServer = ServersList[0];
				return;
			}

			ServerDescription next;
			do
			{
				next = ServersList[rnd.Next(ServersList.Count)];
			} while (next.Equals(CurrentServer));

			CurrentServer = next;
		}

		public static string BuildEndpoint(ServerDescription server, string function, Dictionary<string, string> query)
		{
			var builder = new StringBuilder(server.Url);
			builder.Append('/');
			builder.Append(function);

			if (query != null && query.Count > 0)
			{
				var enumerator = query.GetEnumerator();
				if (enumerator.MoveNext())
					builder.AppendFormat("?{0}={1}", enumerator.Current.Key, enumerator.Current.Value);

				while (enumerator.MoveNext())
					builder.AppendFormat("&{0}={1}", enumerator.Current.Key, enumerator.Current.Value);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Requests <paramref name="function"/> from the current node and deserializes the result.
		/// On a transport or malformed-response failure, retries against a different configured
		/// node (up to once per configured node) instead of failing the whole polling loop — a
		/// single bad or unreachable node must not be able to take the process down.
		/// </summary>
		private async Task<T> GetResponse<T>(string function, Dictionary<string, string> query)
		{
			if (string.IsNullOrEmpty(function))
				throw new ArgumentException(nameof(function));

			var attempts = Math.Max(ServersList?.Count ?? 1, 1);
			Exception lastError = null;

			for (var attempt = 1; attempt <= attempts; attempt++)
			{
				var endpoint = BuildEndpoint(CurrentServer, function, query);

				try
				{
					var raw = await httpClient.GetStringAsync(endpoint);
					return JsonConvert.DeserializeObject<T>(raw);
				}
				catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException)
				{
					lastError = ex;
					_logger?.LogWarning(ex, "Request to {Endpoint} failed (attempt {Attempt}/{Attempts})", endpoint, attempt, attempts);

					if (attempt < attempts)
						SelectNextEndpoint();
				}
			}

			_logger?.LogError(lastError, "All {Attempts} configured node(s) failed to respond", attempts);
			throw lastError;
		}

		#endregion
	}
}
