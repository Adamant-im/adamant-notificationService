using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Adamant.NotificationService.DataContext
{
	class Program
	{
		static void Main(string[] args)
		{
			var connectionString = "";

			var optionsBuilder = new DbContextOptionsBuilder<ANSContext>();
			optionsBuilder.UseMySQL(connectionString);

			var context = new ANSContext(optionsBuilder.Options);
			context.Database.Migrate();
			Console.WriteLine("Total registered devices: {0}", context.Devices.Count());
		}
	}
}
