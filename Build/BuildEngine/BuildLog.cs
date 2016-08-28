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

		private readonly FileStream _buildLogStream;
		private readonly StreamWriter _writer;
		private int _loggerId;

		public BuildLog(Arguments arguments)
		{
			_arguments = arguments;
			_buildLogStream = File.Open("buildlog.txt", FileMode.OpenOrCreate, FileAccess.Write);
			_buildLogStream.SetLength(0);
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

		public void WriteLine(string format, params object[] arguments)
		{
			try
			{
				var message = string.Format(format, arguments);
				lock (_writer)
				{
					_writer.WriteLine(message);
					Console.WriteLine(message);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void WriteLine(int id, string format, object[] arguments)
		{
			if (_arguments.NoConsoleLogger)
				return;

			try
			{
				var message = string.Format(format, arguments);
				var formatted = string.Format("{0}>{1}", id, message);
				lock (_writer)
				{
					_writer.WriteLine(formatted);
					Console.WriteLine(formatted);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Cauhgt unexpected exception while writing message: {0}", e);
			}
		}

		public void Dispose()
		{
			_writer.Flush();
			_writer.Dispose();
			_buildLogStream.Dispose();
		}
	}
}