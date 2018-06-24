using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Adamant.Api.Responses;
using Adamant.Models;
using Microsoft.Extensions.Configuration;
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

		#endregion

		#region ctor

		public AdamantApi(IConfiguration configuration)
		{
			Configuration = configuration;
			var endpointsRaw = configuration.GetSection("Api").GetSection("Server").GetChildren();

			if (endpointsRaw != null)
			{
				var endpoints = new List<ServerDescription>();

				foreach (var raw in endpointsRaw)
				{
					int? port = null;
					if (int.TryParse(raw["port"], out var p))
						port = p;

				   endpoints.Add(new ServerDescription(raw["ip"], raw["protocol"], port));
				}

				ServersList = endpoints;
			}

			if (Servers == null)
				throw new Exception("Can't get endpoint addresses from config");

			SelectNextEndpoint();
		}

		#endregion

		#region Public Methods

		public async Task<IEnumerable<Transaction>> GetTransactions(int height, int offset, TransactionType? type)
		{
			var query = new Dictionary<string, string>
			{
				{ "orderBy", "timestamp:desc" },
				{ "type", "0" }
			};

			if (offset > 0)
				query.Add("offset", offset.ToString());

			if (height > 0)
				query.Add("fromHeight", height.ToString());

			if (type.HasValue)
				query.Add("type", type.Value.ToString("0"));

			var endpoint = BuildEndpoint(CurrentServer, getTransactions, query);

			var results = await GetResponse<TransactionsResponse>(endpoint);
			return results.Transactions;
		}

		public async Task<IEnumerable<Transaction>> GetChatTransactions(int height, int offset, ChatType? chatType = null)
		{
			var query = new Dictionary<string, string>
			{
				{ "orderBy", "timestamp:desc" }
			};

			if (offset > 0)
				query.Add("offset", offset.ToString());

			if (height > 0)
				query.Add("fromHeight", height.ToString());

			if (chatType.HasValue)
				query.Add("type", chatType.Value.ToString());

			var endpoint = BuildEndpoint(CurrentServer, getChatTransactions, query);

			var results = await GetResponse<TransactionsResponse>(endpoint);
			return results.Transactions;
		}

		#endregion

		#region Internal logic

		public void SelectNextEndpoint()
		{
			if (ServersList == null)
				return;

			CurrentServer = ServersList[rnd.Next(ServersList.Count)];
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

		private string BuildEndpointt(string url, int? type, int offset, int height)
		{
			var builder = new StringBuilder(url);
			builder.Append("?orderBy=timestamp:desc");

			if (type.HasValue)
				builder.AppendFormat("&type={0}", type.Value);

			if (offset > 0)
				builder.AppendFormat("&offset={0}", offset);

			if (height > 0)
				builder.AppendFormat("&fromHeight={0}", height);

			return builder.ToString();
		}

		private async Task<T> GetResponse<T>(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint))
			{
				throw new ArgumentException("endpoint");
			}

			try
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var raw = await client.GetStringAsync(endpoint);
				return JsonConvert.DeserializeObject<T>(raw);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw ex;
			}
		}

		#endregion
	}
}
