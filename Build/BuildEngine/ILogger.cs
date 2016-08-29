namespace Build.BuildEngine
{
	public interface ILogger
	{
		void WriteLine(Verbosity verbosity, string format, params object[] arguments);
		void WriteMultiLine(Verbosity verbosity, string message);

		void WriteWarning(string message);
		void WriteError(string message);
	}
}