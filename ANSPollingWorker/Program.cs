﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Adamant.Api;
using Adamant.NotificationService.ApplePusher;
using Adamant.NotificationService.DataContext;
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

			if (!Boolean.TryParse(configuration["PollingWorker:Warmup"], out bool warmup))
				warmup = true;

			#endregion

			#region Services

			// Data context
			var context = new DevicesContext(connectionString, provider);
			context.Database.Migrate();

			// API
			var api = new AdamantApi(configuration);

			#endregion

			#region DI & NLog

			var nLogConfig = Utilities.HandleUnixHomeDirectory(configuration["PollingWorker:NlogConfig"]);
			_logger = NLog.LogManager.LoadConfiguration(nLogConfig).GetCurrentClassLogger();

			var services = new ServiceCollection();

			// Application services

			services.AddSingleton<IConfiguration>(configuration);
			services.AddSingleton<AdamantApi>();
			services.AddSingleton(typeof(IPusher), typeof(Pusher));
			services.AddSingleton(context);

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

			var totalDevices = context.Devices.Count();
			_logger.Info("Database initialized. Total devices in db: {0}", totalDevices);
			_logger.Info("Starting polling. Delay: {0}ms.", delay);

			var applePusher = serviceProvider.GetRequiredService<IPusher>();
			applePusher.Start();

			var chatWorker = serviceProvider.GetRequiredService<ChatPollingWorker>();
			chatWorker.Delay = TimeSpan.FromMilliseconds(delay);
			chatWorker.StartPolling(warmup);

			var transferWorker = serviceProvider.GetRequiredService<TransferPollingWorker>();
			transferWorker.Delay = TimeSpan.FromMilliseconds(delay);
			transferWorker.StartPolling(warmup);

			Task.WaitAll(chatWorker.PollingTask, transferWorker.PollingTask);
		}

		// Log all unhandled exceptions
		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal(e.ExceptionObject);
		}
	}
}