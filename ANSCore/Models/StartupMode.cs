namespace Adamant.NotificationService.Models
{
	/// <summary>
	/// Startup mode
	/// </summary>
	public enum StartupMode
	{
		/// <summary>
		/// Start from 0.
		/// </summary>
		initial,

		/// <summary>
		/// Get current network height. If fails - start from 0.
		/// </summary>
		network,

		/// <summary>
		/// Get saved last height from database. Otherwise - warmup.
		/// </summary>
		database
	}
}
