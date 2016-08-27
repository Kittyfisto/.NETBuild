namespace Build.BuildEngine
{
	public interface IBuildLog
	{
		/// <summary>
		///     Creates a new logger that is associated with an id.
		///     Helps the user to differentiate between messages when multiple actions are logging
		///     at the same time.
		/// </summary>
		/// <returns></returns>
		ILogger CreateLogger();
	}
}