namespace Build.BuildEngine
{
	public interface ILogger
	{
		void LogFormat(string format, params object[] arguments);
	}
}