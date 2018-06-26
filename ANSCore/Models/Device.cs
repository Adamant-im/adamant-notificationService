using System;

namespace Adamant.NotificationService.Models
{
	public class Device
	{
		public int ID { get; set; }
		public string Address { get; set; }
		public string Token { get; set; }
		public string Provider { get; set; }
		public DateTime TransactionDate { get; set; }
		public DateTime RegistrationDate { get; set; }
	}
}
