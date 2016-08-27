namespace Build.BuildEngine
{
	/// <summary>
	/// Responsible for adding messages into the build log.
	/// </summary>
	public sealed class Logger
		: ILogger
	{
		private readonly BuildLog _buildLog;
		private readonly int _id;

		public Logger(BuildLog buildLog, int id)
		{
			_buildLog = buildLog;
			_id = id;
		}

		public void Log(string message)
		{
			_buildLog.Log(_id, message);
		}
	}
}