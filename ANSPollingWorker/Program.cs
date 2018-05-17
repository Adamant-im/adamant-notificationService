using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.ApplePusher;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Adamant.NotificationService.PollingWorker
{
	class Program
	{
		#region Properties

		private static NLog.ILogger _logger;
		private static readonly HttpClient client = new HttpClient();

		#endregion

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

			#region Services

			// Data context
			var context = new DevicesContext(connectionString, provider);

			// API
			var api = new AdamantApi(configuration);

			#endregion

			#region DI & NLog

			_logger = NLog.LogManager.LoadConfiguration("nlog.config").GetCurrentClassLogger();

			var services = new ServiceCollection();

			// Application services

			services.AddSingleton<IConfiguration>(configuration);
			services.AddSingleton<AdamantApi>();
			services.AddSingleton(typeof(IPusher), typeof(Pusher));
			services.AddSingleton(context);

			services.AddSingleton<AdamantPollingWorker>();

			// Other
			services.AddSingleton<ILoggerFactory, LoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

			var serviceProvider = services.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

			#endregion

			var totalDevices = context.Devices.Count();
			_logger.Info("Database initialized. Total devices in db: {0}", totalDevices);
			_logger.Info("Starting polling. Delay: {0}ms.", delay);

			var applePusher = serviceProvider.GetRequiredService<IPusher>();
			var worker = serviceProvider.GetRequiredService<AdamantPollingWorker>();
			applePusher.Start();
			worker.Delay = TimeSpan.FromMilliseconds(delay);
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

		// Log all unhandled exceptions
		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal(e.ExceptionObject);
		}
	}
}
