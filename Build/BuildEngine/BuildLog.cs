using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for writing messages into a <see cref="Stream" />.
	/// </summary>
	public sealed class BuildLog
		: IBuildLog
		  , IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Arguments _arguments;

		private readonly Stream _buildLogStream;
		private readonly bool _disposeStream;
		private readonly List<string> _errors;
		private readonly List<string> _warnings;
		private readonly StreamWriter _writer;

		private int _loggerId;
		
		public BuildLog(Arguments arguments, Stream stream, bool disposeStream = false)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");
			if (stream == null)
				throw new ArgumentNullException("stream");

			_arguments = arguments;
			_buildLogStream = stream;
			_buildLogStream.SetLength(0);
			_disposeStream = disposeStream;
			_writer = new StreamWriter(_buildLogStream, Encoding.UTF8);

			_warnings = new List<string>();
			_errors = new List<string>();
		}

		public IEnumerable<string> Errors
		{
			get { return _errors; }
		}

		public IEnumerable<string> Warnings
		{
			get { return _warnings; }
		}

		public ILogger CreateLogger()
		{
			return new Logger(this, Interlocked.Increment(ref _loggerId));
		}

		public void WriteLine(Verbosity verbosity, int id, string format, object[] arguments)
		{
			if (_arguments.Verbosity < verbosity)
				return;

			try
			{
				string message = string.Format(format, arguments);
				string formatted = string.Format("{0}>{1}", id, message);
				WriteLine(formatted);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void WriteWarning(string message)
		{
			_warnings.Add(message);
			FormatLine(Verbosity.Quiet, message);
		}

		public void WriteError(string message)
		{
			_errors.Add(message);
			FormatLine(Verbosity.Quiet, message);
		}

		public void Dispose()
		{
			_writer.Flush();
			_writer.Dispose();

			if (_disposeStream)
				_buildLogStream.Dispose();
		}

		public void WriteLine()
		{
			try
			{
				lock (_writer)
				{
					_writer.WriteLine();
					Console.WriteLine();
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void FormatLine(Verbosity verbosity, string format, params object[] arguments)
		{
			if (_arguments.Verbosity < verbosity)
				return;

			try
			{
				string message;
				if (arguments == null || arguments.Length == 0)
					message = format;
				else
					message = string.Format(format, arguments);
				WriteLine(message);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void WriteLine(string message)
		{
			lock (_writer)
			{
				_writer.WriteLine(message);
				if (!_arguments.NoConsoleLogger)
					Console.WriteLine(message);
			}
		}
	}
}