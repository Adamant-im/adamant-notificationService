using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Adamant.NotificationService.RegistrationService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
		           .UseStartup<Startup>()
		           .UseKestrel(options => options.ConfigureEndpoints())
		           .Build();
	}
}
