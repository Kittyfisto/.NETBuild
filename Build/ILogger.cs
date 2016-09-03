namespace Build
{
	public interface ILogger
	{
		/// <summary>
		///     Tests if any error has been logged already.
		/// </summary>
		bool HasErrors { get; }

		void WriteLine(Verbosity verbosity, string format, params object[] arguments);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="verbosity"></param>
		/// <param name="message"></param>
		/// <param name="interpretLines">when set to true, then each line will be pattern matched for errors and warnings</param>
		void WriteMultiLine(Verbosity verbosity, string message, bool interpretLines);

		void WriteWarning(string format, params object[] arguments);
		void WriteError(string foramt, params object[] arguments);
	}
}