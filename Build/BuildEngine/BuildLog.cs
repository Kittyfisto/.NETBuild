using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for writing messages into a <see cref="Stream" />.
	/// </summary>
	public sealed class BuildLog
		: IBuildLog
	{
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

		public void Log(int id, string message)
		{
			_writer.WriteLine("{0}>{1}", id, message);
		}

		public void Flush()
		{
			_writer.Flush();
		}
	}
}