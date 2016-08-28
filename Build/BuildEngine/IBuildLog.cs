namespace Build.BuildEngine
{
	public interface IBuildLog
	{
		/// <summary>
		///     Creates a new logger that is associated with a unique id.
		///     Helps the user to differentiate between messages when multiple <see cref="Builder"/>s are logging
		///     at the same time.
		/// </summary>
		/// <returns></returns>
		ILogger CreateLogger();
	}
}