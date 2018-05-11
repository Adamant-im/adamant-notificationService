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
			#region Config

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			var configuration = builder.Build();
			
			var connectionString = configuration.GetConnectionString("Devices");
			
			if (!int.TryParse(configuration["PollingOptions:Delay"], out int delay))
				delay = 2000;
			
			if (!Boolean.TryParse(configuration["PollingOptions:Warmup"], out bool warmup))
				warmup = true;

			#endregion

			#region DataContext

			var context = new DevicesContext(connectionString);
			Console.WriteLine("Total registered devices: {0}", context.Devices.Count());

			#endregion

			#region Initializing worker

			var applePusher = new Pusher { Configuration = configuration };
			var api = new AdamantApi(configuration);
			var worker = new AdamantPollingWorker
			{
				Delay = TimeSpan.FromMilliseconds(delay),
				Pusher = applePusher,
				AdamantApi = api,
				Context = context
			};

			#endregion

			Console.WriteLine("Starting polling. Delay: {0}ms.\nAny key to stop...", delay);

			applePusher.Start();

			worker.StartPolling(warmup);

			Console.ReadKey();
			applePusher.Stop();
			worker.StopPolling();
        }
    }
}
