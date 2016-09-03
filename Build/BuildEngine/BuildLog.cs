using System;
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
		private readonly Arguments _arguments;
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Stream _buildLogStream;
		private readonly StreamWriter _writer;
		private int _loggerId;
		private readonly bool _disposeStream;

		public BuildLog()
			: this(new Arguments())
		{}

		public BuildLog(Arguments arguments)
			: this(arguments, File.Open("buildlog.txt", FileMode.OpenOrCreate, FileAccess.Write), true)
		{}

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
		}

		public ILogger CreateLogger()
		{
			return new Logger(this, Interlocked.Increment(ref _loggerId));
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

		public void WriteLine(Verbosity verbosity, string format, params object[] arguments)
		{
			if (_arguments.Verbosity < verbosity)
				return;

			try
			{
				var message = string.Format(format, arguments);
				WriteLine(message);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void WriteLine(Verbosity verbosity, int id, string format, object[] arguments)
		{
			if (_arguments.Verbosity < verbosity)
				return;

			try
			{
				var message = string.Format(format, arguments);
				var formatted = string.Format("{0}>{1}", id, message);
				WriteLine(formatted);
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

		public void Dispose()
		{
			_writer.Flush();
			_writer.Dispose();

			if (_disposeStream)
				_buildLogStream.Dispose();
		}
	}
}