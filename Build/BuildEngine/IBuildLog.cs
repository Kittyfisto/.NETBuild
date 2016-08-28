namespace Build.BuildEngine
{
	public interface IBuildLog
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="verbosity"></param>
		/// <param name="id"></param>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		void WriteLine(Verbosity verbosity, int id, string format, object[] arguments);

		/// <summary>
		///     Creates a new logger that is associated with a unique id.
		///     Helps the user to differentiate between messages when multiple <see cref="ProjectBuilder"/>s are logging
		///     at the same time.
		/// </summary>
		/// <returns></returns>
		ILogger CreateLogger();
	}
}