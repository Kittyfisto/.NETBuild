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
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly StreamWriter _writer;
		private int _loggerId;

		public BuildLog(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_writer = new StreamWriter(stream, Encoding.UTF8);
		}

		public ILogger CreateLogger()
		{
			return new Logger(this, Interlocked.Increment(ref _loggerId));
		}

		public void LogFormat(int id, string format, object[] arguments)
		{
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

		public void Flush()
		{
			_writer.Flush();
		}
	}
}