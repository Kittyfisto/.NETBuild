namespace Build.BuildEngine
{
	public interface ILogger
	{
		/// <summary>
		///     Tests if any error has been logged already.
		/// </summary>
		bool HasErrors { get; }

		void WriteLine(Verbosity verbosity, string format, params object[] arguments);
		void WriteMultiLine(Verbosity verbosity, string message);

		void WriteWarning(string format, params object[] arguments);
		void WriteError(string foramt, params object[] arguments);
	}
}