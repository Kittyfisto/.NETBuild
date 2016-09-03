﻿using System;

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
		private int _errorCount;
		private int _warningCount;

		public Logger(IBuildLog buildLog, int id)
		{
			_buildLog = buildLog;
			_id = id;
		}

		public bool HasErrors
		{
			get { return _errorCount > 0; }
		}

		public void WriteLine(Verbosity verbosity, string format, params object[] arguments)
		{
			_buildLog.WriteLine(verbosity, _id, format, arguments);
		}

		public void WriteMultiLine(Verbosity verbosity, string message, bool interpretLines)
		{
			var lines = message.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				if (line.ToLower().Contains("error"))
				{
					WriteError(line);
				}
				else if (line.ToLower().Contains("warning"))
				{
					WriteWarning(line);
				}
				else
				{
					WriteLine(verbosity, line);
				}
			}
		}

		public void WriteWarning(string format, params object[] arguments)
		{
			var message = string.Format(format, arguments);
			++_warningCount;
			_buildLog.WriteWarning(message);
		}

		public void WriteError(string format, params object[] arguments)
		{
			var message = string.Format(format, arguments);
			++_errorCount;
			_buildLog.WriteError(message);
		}
	}
}