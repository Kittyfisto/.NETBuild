using System;
using System.Collections.Generic;

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
		private readonly List<string> _warnings;
		private readonly List<string> _errors;

		public Logger(IBuildLog buildLog, int id)
		{
			_buildLog = buildLog;
			_id = id;

			// What do we do with warnings?
			_warnings = new List<string>();
			_errors = new List<string>();
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

		public void WriteWarning(string format, params object[] arguments)
		{
			var message = string.Format(format, arguments);
			_warnings.Add(message);
			WriteLine(Verbosity.Quiet, message);
		}

		public void WriteError(string format, params object[] arguments)
		{
			var message = string.Format(format, arguments);
			_errors.Add(message);
			WriteLine(Verbosity.Quiet, message);
		}
	}
}