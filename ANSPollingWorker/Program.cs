using System;
using System.Linq;
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

			// API
			var api = new AdamantApi(configuration);

			#endregion

			#region DI & NLog

			var nLogConfig = configuration["PollingWorker:NlogConfig"];
			if (String.IsNullOrEmpty(nLogConfig))
				nLogConfig = "nlog.config";
			else
				nLogConfig = Utilities.HandleUnixHomeDirectory(nLogConfig);

			_logger = NLog.LogManager.LoadConfiguration(nLogConfig).GetCurrentClassLogger();

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
			services.AddSingleton<ILoggerFactory, LoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

			var serviceProvider = services.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

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

			Task.WaitAll(chatWorker.PollingTask, transferWorker.PollingTask);
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
				_logger.Info("Removing invalid/expired token: {0}", eventArgs.Token);
				_context.Devices.Remove(device);
				_context.SaveChanges();
			}
		}

	}
}
