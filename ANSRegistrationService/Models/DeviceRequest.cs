using System;
namespace ANSRegistrationService.Models
{
	public class DeviceRequest
	{
		public string Token { get; set; }
		public string Address { get; set; }

		public DeviceRequest() {}

		public DeviceRequest(string token, string address)
		{
			Token = token;
			Address = address;
		}
	}
}
