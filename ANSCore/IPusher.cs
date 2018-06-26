using System;
using System.Collections.Generic;
using Adamant.Models;
using Adamant.NotificationService.Models;

namespace Adamant.NotificationService
{
	public interface IPusher
	{
		void NotifyDevice(Device device, IEnumerable<Transaction> transactions);

		void Start();
		void Stop();

		event InvalidTokenHandler OnInvalidTokenDetected;
	}

	public class InvalidTokenEventArgs: EventArgs
	{
		public string Token { get; }

		public InvalidTokenEventArgs(string token)
		{
			Token = token;
		}
	}

	public delegate void InvalidTokenHandler(IPusher sender, InvalidTokenEventArgs eventArgs);
}
