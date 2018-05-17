using System;
using System.Collections.Generic;
using System.Linq;
using Adamant.NotificationService.DataContext;
using Adamant.NotificationService.Models;
using ANSRegistrationService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Adamant.NotificationService.RegistrationService.Controllers
{
	[Route("api/[controller]")]
	public class DevicesController : Controller
	{
		private readonly DevicesContext _context;

		public DevicesController(DevicesContext context)
		{
			_context = context;
		}

		#if DEBUG

		[HttpGet]
		public IEnumerable<Device> GetRegisteredDevices()
		{
			return _context.Devices.ToList();
		}

		#endif

		[HttpPost("register")]
		public IActionResult RegisterDevice([FromBody] DeviceRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Token) || !AdamantUtilities.IsValidAdamantAddress(request.Address))
				return BadRequest();


			// Drop previous registration, if exist
			var prevDevice = _context.Devices.FirstOrDefault(d => d.Token == request.Token && d.Address == request.Address);
			if (prevDevice != null)
				return Ok();


			// Add new device
			var device = new Device
			{
				Address = request.Address,
				Token = request.Token,
				RegistrationDate = DateTime.Now
			};

			_context.Devices.Add(device);

			// Save changes
			_context.SaveChanges();

			return Ok();
		}

		[HttpPost("drop/{token}")]
		public IActionResult DropDevice(string token)
		{
			if (string.IsNullOrEmpty(token))
				return BadRequest();

			var device = _context.Devices.FirstOrDefault(d => d.Token == token);
			if (device != null)
			{
				_context.Devices.Remove(device);
				_context.SaveChanges();
			}

			return Ok();
		}
	}
}
