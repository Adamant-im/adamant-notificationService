using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Adamant.Api.Tests
{
	public class AdamantApiTests
	{
		private static AdamantApi BuildApi(params (string ip, string protocol)[] servers)
		{
			var settings = new Dictionary<string, string>();

			for (var i = 0; i < servers.Length; i++)
			{
				settings[$"Api:Server:{i}:ip"] = servers[i].ip;
				settings[$"Api:Server:{i}:protocol"] = servers[i].protocol;
			}

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();

			return new AdamantApi(configuration, NullLogger<AdamantApi>.Instance);
		}

		[Fact]
		public void Constructor_WithNoServersConfigured_Throws()
		{
			Assert.Throws<System.Exception>(() => BuildApi());
		}

		[Fact]
		public void Constructor_SelectsAServerFromConfig()
		{
			var api = BuildApi(("node1.example.com", "https"), ("node2.example.com", "https"));

			Assert.NotNull(api.CurrentServer);
			Assert.Contains(api.CurrentServer.Ip, new[] { "node1.example.com", "node2.example.com" });
		}

		[Fact]
		public void SelectNextEndpoint_WithSingleServer_StaysOnIt()
		{
			var api = BuildApi(("only-node.example.com", "https"));

			for (var i = 0; i < 5; i++)
				api.SelectNextEndpoint();

			Assert.Equal("only-node.example.com", api.CurrentServer.Ip);
		}

		[Fact]
		public void SelectNextEndpoint_WithMultipleServers_NeverRepeatsCurrent()
		{
			// This is what makes failover actually happen: a retry after a failed request must
			// land on a different node instead of possibly re-rolling the one that just failed.
			var api = BuildApi(("node1.example.com", "https"), ("node2.example.com", "https"), ("node3.example.com", "https"));

			for (var i = 0; i < 20; i++)
			{
				var previous = api.CurrentServer;
				api.SelectNextEndpoint();
				Assert.NotEqual(previous.Ip, api.CurrentServer.Ip);
			}
		}

		[Fact]
		public void SelectNextEndpoint_WithAllServersDuplicatingCurrent_DoesNotHang()
		{
			// A misconfigured Api:Server with the same node listed twice must not hang the
			// process looking for a "different" server that doesn't exist.
			var api = BuildApi(("only-node.example.com", "https"), ("only-node.example.com", "https"));

			for (var i = 0; i < 5; i++)
				api.SelectNextEndpoint();

			Assert.Equal("only-node.example.com", api.CurrentServer.Ip);
		}

		[Fact]
		public void SelectNextEndpoint_WithMultipleServers_EventuallyVisitsAllOfThem()
		{
			var api = BuildApi(("node1.example.com", "https"), ("node2.example.com", "https"), ("node3.example.com", "https"));

			var visited = new HashSet<string> { api.CurrentServer.Ip };
			for (var i = 0; i < 20; i++)
			{
				api.SelectNextEndpoint();
				visited.Add(api.CurrentServer.Ip);
			}

			Assert.Equal(3, visited.Count);
		}

		[Fact]
		public void BuildEndpoint_AppendsQueryStringInOrder()
		{
			var server = new ServerDescription("node.example.com", "https", null);
			var query = new Dictionary<string, string>
			{
				{ "orderBy", "timestamp:desc" },
				{ "offset", "10" }
			};

			var endpoint = AdamantApi.BuildEndpoint(server, "api/transactions", query);

			Assert.Equal("https://node.example.com/api/transactions?orderBy=timestamp:desc&offset=10", endpoint);
		}
	}
}
