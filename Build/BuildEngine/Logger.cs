﻿namespace Build.BuildEngine
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

		public void LogFormat(string format, params object[] arguments)
		{
			_buildLog.LogFormat(_id, format, arguments);
		}
	}
}