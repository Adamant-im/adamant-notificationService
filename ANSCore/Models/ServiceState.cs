using System;
namespace Adamant.NotificationService.Models
{
	public class ServiceState
	{
		public int ID { get; set; }
		public string Service { get; set; }
		public int LastHeight { get; set; }
		public DateTime Date { get; set; }
	}
}
