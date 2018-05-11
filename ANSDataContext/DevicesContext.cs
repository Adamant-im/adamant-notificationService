using System.IO;
using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Adamant.NotificationService.DataContext
{
	public class DevicesContext: DbContext
	{
		public DevicesContext(DbContextOptions<DevicesContext> options) : base(options)
		{
		}

		public DevicesContext() : base(OptionsWithConnectionString("appsettings.json"))
		{
		}

		public DbSet<Device> Devices { get; set; }

		private static DbContextOptions<DevicesContext> OptionsWithConnectionString(string json) {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(json, optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();
			optionsBuilder.UseSqlite(connectionString);
			return optionsBuilder.Options;
		}
	}
}
