using System;
using System.IO;
using Adamant.Api;
using Adamant.NotificationService.DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ANSKvsRegistrationService
{
	class Program
	{
		#region Properties

		private static NLog.ILogger _logger;

		#endregion
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += Global_UnhandledException;

			#region Config

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");
			var provider = configuration["Database:Provider"];

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
			services.AddSingleton(context);

			// Other
			services.AddSingleton<ILoggerFactory, LoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

			var serviceProvider = services.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

			#endregion
		}

		// Log all unhandled exceptions
		static void Global_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal(e.ExceptionObject);
		}
	}
}
