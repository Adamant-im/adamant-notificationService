using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Adamant.Api;
using Adamant.NotificationService.ApplePusher;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Configuration;

namespace Adamant.NotificationService.PollingWorker
{
	class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");

			var context = DevicesContext.CreateContextWithSQLite(connectionString);

			Console.WriteLine("Total registered devices: {0}", context.Devices.Count());

			var applePusher = new Pusher { Configuration = configuration };
			var api = new AdamantApi(configuration);
			var worker = new AdamantPollingWorker
			{
				Delay = TimeSpan.FromSeconds(2),
				Pusher = applePusher,
				AdamantApi = api,
				Context = context
			};

			if (!Boolean.TryParse(configuration["PollingOptions:Warmup"], out bool warmup))
				warmup = true;

			Console.WriteLine("Any key to stop...");

			applePusher.Start();

			worker.StartPolling(warmup);

			Console.ReadKey();
			applePusher.Stop();
			worker.StopPolling();
        }
    }
}
