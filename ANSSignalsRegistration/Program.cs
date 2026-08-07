using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Adamant.NotificationService.SignalsRegistration
{
	class Program
	{
		#region Properties

		private static NLog.ILogger _logger;

		#endregion

		static async Task Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += Global_UnhandledException;

			#region Config

			var configuration = ConfigurationLoader.GetConfiguration();

			var provider = configuration["Database:Provider"];
			var connectionName = configuration["Database:ConnectionString"] ?? "devices";
			var connectionString = configuration.GetConnectionString(connectionName);

			if (!int.TryParse(configuration["SignalsRegistration:Delay"], out int delay))
				delay = 2000;

			if (!Enum.TryParse(configuration["SignalsRegistration:Startup"], out StartupMode startupMode))
				startupMode = StartupMode.database;
			
			#endregion

			#region Services

			// Data context
			var context = new ANSContext(connectionString, provider);
			context.Database.Migrate();

			#endregion

			#region DI & NLog

			var nLogConfig = configuration["SignalsRegistration:NlogConfig"];
			if (String.IsNullOrEmpty(nLogConfig))
				nLogConfig = "nlog.config";
			
			else
				nLogConfig = Utilities.HandleUnixHomeDirectory(nLogConfig);

			_logger = NLog.LogManager.LoadConfiguration(nLogConfig).GetCurrentClassLogger();
			_logger.Info("ANS Signals Registration version {0}", Assembly.GetExecutingAssembly().GetName().Version);

			var services = new ServiceCollection();

			// Application services

			services.AddSingleton<IConfiguration>(configuration);
			services.AddSingleton<AdamantApi>();
			services.AddSingleton(context);

			services.AddSingleton<SignalsPoller>();

			// Other
			services.AddLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Trace);
				builder.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
			});

			var serviceProvider = services.BuildServiceProvider();

			#endregion

			var totalDevices = context.Devices.Count();
			_logger.Info("Database initialized. Total devices in db: {0}", totalDevices);
			_logger.Info("Starting polling. Delay: {0}ms.", delay);

			var address = configuration["SignalsRegistration:Address"];
			if (string.IsNullOrEmpty(address))
				throw new Exception("ANS account address is required");

			var privateKey = configuration["SignalsRegistration:PrivateKey"];
			if (string.IsNullOrEmpty(privateKey))
				throw new Exception("ANS account private key is required");

			var worker = serviceProvider.GetRequiredService<SignalsPoller>();
			worker.Delay = TimeSpan.FromMilliseconds(delay);
			worker.Address = address;
			worker.PrivateKey = privateKey;
			worker.StartPolling(startupMode);

			// Docker/systemd stop the container with SIGTERM before killing it; the .NET runtime
			// turns that into ProcessExit. Without observing it, the worker gets killed mid-cycle
			// and the container always reports a non-zero exit, even on an intentional stop.
			AppDomain.CurrentDomain.ProcessExit += (s, e) => StopWorker(worker);
			Console.CancelKeyPress += (s, e) =>
			{
				e.Cancel = true;
				StopWorker(worker);
			};

			if (worker.PollingTask != null)
			{
				await worker.PollingTask;
			}
			else
			{
				throw new Exception("Can't await worker");
			}
		}

		static void StopWorker(SignalsPoller worker)
		{
			_logger.Info("Shutdown requested, stopping worker...");
			worker.StopPolling();
			worker.PollingTask?.Wait(TimeSpan.FromSeconds(10));
		}

		// Log all unhandled exceptions
		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal(e.ExceptionObject);
		}
	}
}
