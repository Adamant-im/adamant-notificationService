using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Adamant.NotificationService.DataContext
{
	class Program
	{
		static void Main(string[] args)
		{
			var connectionString = "Data Source=database.db";

			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();
			optionsBuilder.UseSqlite(connectionString);

			var context = new DevicesContext(optionsBuilder.Options);
			context.Database.Migrate();
			Console.WriteLine("Total registered devices: {0}", context.Devices.Count());
		}
	}
}
