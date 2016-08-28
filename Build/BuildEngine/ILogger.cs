namespace Build.BuildEngine
{
	public interface ILogger
	{
		void LogFormat(Verbosity verbosity, string format, params object[] arguments);
	}
}