using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.ApplePusher;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
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
		private static ANSContext _context;

		#endregion

		static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += Global_UnhandledException;

			#region Config

			var configuration = ConfigurationLoader.GetConfiguration();

			var provider = configuration["Database:Provider"];
			var connectionName = configuration["Database:ConnectionString"] ?? "devices";
			var connectionString = configuration.GetConnectionString(connectionName);

			if (!int.TryParse(configuration["PollingWorker:Delay"], out int delay))
				delay = 2000;
			
			if (!Enum.TryParse(configuration["PollingWorker:Startup"], out StartupMode startupMode))
				startupMode = StartupMode.database;

			#endregion

			#region Services

			// Data context
			_context = new ANSContext(connectionString, provider);
			_context.Database.Migrate();

			#endregion

			#region DI & NLog

			var nLogConfig = configuration["PollingWorker:NlogConfig"];
			if (String.IsNullOrEmpty(nLogConfig))
				nLogConfig = "nlog.config";
			else
				nLogConfig = Utilities.HandleUnixHomeDirectory(nLogConfig);

			_logger = NLog.LogManager.LoadConfiguration(nLogConfig).GetCurrentClassLogger();
			_logger.Info("ANS Polling Worker version {0}", Assembly.GetExecutingAssembly().GetName().Version);

			var services = new ServiceCollection();

			// Application services

			services.AddSingleton<IConfiguration>(configuration);
			services.AddSingleton<AdamantApi>();
			services.AddSingleton(typeof(IPusher), typeof(ApplePusher.ApplePusher));
			services.AddSingleton(_context);

			// Polling workers
			services.AddSingleton<ChatPollingWorker>();
			services.AddSingleton<TransferPollingWorker>();

			// Other
			services.AddLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Trace);
				builder.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
			});

			var serviceProvider = services.BuildServiceProvider();

			#endregion

			var totalDevices = _context.Devices.Count();
			_logger.Info("Database initialized. Total devices in db: {0}", totalDevices);
			_logger.Info("Starting polling. Delay: {0}ms.", delay);

			var applePusher = serviceProvider.GetRequiredService<IPusher>();
			applePusher.OnInvalidToken += ApplePusher_OnInvalidToken;
			applePusher.Start();

			var chatWorker = serviceProvider.GetRequiredService<ChatPollingWorker>();
			chatWorker.Delay = TimeSpan.FromMilliseconds(delay);
			chatWorker.StartPolling(startupMode);

			var transferWorker = serviceProvider.GetRequiredService<TransferPollingWorker>();
			transferWorker.Delay = TimeSpan.FromMilliseconds(delay);
			transferWorker.StartPolling(startupMode);

			// Docker/systemd stop the container with SIGTERM before killing it; the .NET runtime
			// turns that into ProcessExit. Without observing it, the workers get killed mid-cycle
			// and the container always reports a non-zero exit, even on an intentional stop.
			AppDomain.CurrentDomain.ProcessExit += (s, e) => StopWorkers(chatWorker, transferWorker);
			Console.CancelKeyPress += (s, e) =>
			{
				e.Cancel = true;
				StopWorkers(chatWorker, transferWorker);
			};

			Task.WaitAll(chatWorker.PollingTask, transferWorker.PollingTask);
		}

		static void StopWorkers(ChatPollingWorker chatWorker, TransferPollingWorker transferWorker)
		{
			_logger.Info("Shutdown requested, stopping workers...");
			chatWorker.StopPolling();
			transferWorker.StopPolling();
			Task.WaitAll(new[] { chatWorker.PollingTask, transferWorker.PollingTask }, TimeSpan.FromSeconds(10));
		}

		// Log all unhandled exceptions
		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal(e.ExceptionObject);
		}

		static void ApplePusher_OnInvalidToken(IPusher sender, InvalidTokenEventArgs eventArgs)
		{
			var device = _context.Devices.FirstOrDefault(d => d.Token.Equals(eventArgs.Token));

			if (device != null)
			{
				// Never log the device token itself — see AGENTS.md's privacy rules.
				_logger.Info("Removing invalid/expired token for device {0}", device.ID);
				_context.Devices.Remove(device);
				_context.SaveChanges();
			}
		}

	}
}
