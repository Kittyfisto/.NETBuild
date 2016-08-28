using System;

namespace Build.BuildEngine
{
	/// <summary>
	/// Responsible for adding messages into the build log.
	/// </summary>
	public sealed class Logger
		: ILogger
	{
		private readonly IBuildLog _buildLog;
		private readonly int _id;

		public Logger(IBuildLog buildLog, int id)
		{
			_buildLog = buildLog;
			_id = id;
		}

		public void WriteLine(Verbosity verbosity, string format, params object[] arguments)
		{
			_buildLog.WriteLine(verbosity, _id, format, arguments);
		}

		public void WriteMultiLine(Verbosity verbosity, string message)
		{
			var lines = message.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				WriteLine(verbosity, line);
			}
		}
	}
}