using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.ApplePusher;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Configuration;

namespace Adamant.NotificationService.PollingWorker
{
	class Program
	{
		private static readonly HttpClient client = new HttpClient();

		static async Task Main()
		{
			AppDomain.CurrentDomain.UnhandledException += Global_UnhandledException;

			#region Config

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");
			var provider = configuration["Database:Provider"];

			if (!int.TryParse(configuration["PollingOptions:Delay"], out int delay))
				delay = 2000;

			if (!Boolean.TryParse(configuration["PollingOptions:Warmup"], out bool warmup))
				warmup = true;

			#endregion

			#region DataContext

			var context = new DevicesContext(connectionString, provider);
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

			Console.WriteLine("Starting polling. Delay: {0}ms.", delay);

			applePusher.Start();

			worker.StartPolling(warmup);

			if (worker.PollingTask != null)
			{
				await worker.PollingTask;
			}
			else
			{
				throw new Exception("Can't await worker");
			}
		}

		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.WriteLine("Fatal error: Unhadled exception: {0}", e.ExceptionObject);
		}

	}
}
