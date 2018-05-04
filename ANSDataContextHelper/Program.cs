using System;
using System.IO;
using System.Linq;
using Adamant.NotificationService.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ANSDataContextHelper
{
	class Program
	{
		static void Main(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();
			optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly("ANSDataContextHelper"));

			var context = new DevicesContext(optionsBuilder.Options);

			Console.WriteLine("Total devices in DB: {0}", context.Devices.Count());
		}
	}
}
