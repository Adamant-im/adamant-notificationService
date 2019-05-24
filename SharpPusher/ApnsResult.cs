namespace SharpPusher
{
	public enum ApnsResult
	{
		/// <summary>
		/// Unknown error.
		/// </summary>
		UnknownError = 0,

		/// <summary>
		/// Success.
		/// </summary>
		Success = 200,

		/// <summary>
		/// Bad request.
		/// </summary>
		BadRequest = 400,

		/// <summary>
		/// There was an error with the certificate or with the provider authentication token.
		/// </summary>
		NotAuthorized = 403,

		/// <summary>
		/// The request used a bad :method value. Only POST requests are supported.
		/// </summary>
		BadMethod = 405,

		/// <summary>
		/// The device token is no longer active for the topic.
		/// </summary>
		TokenExpired = 410,

		/// <summary>
		/// The notification payload was too large.
		/// </summary>
		PayloadTooLarge = 413,

		/// <summary>
		/// The server received too many requests for the same device token.
		/// </summary>
		TooManyRequests = 429,

		/// <summary>
		/// Internal server error.
		/// </summary>
		InternalServerError = 500,

		/// <summary>
		/// The server is shutting down and unavailable.
		/// </summary>
		ServerShuttingDown = 503
	}
}
