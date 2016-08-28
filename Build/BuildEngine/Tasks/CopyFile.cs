using System;
using System.IO;

namespace Build.BuildEngine.Tasks
{
	public sealed class CopyFile
	{
		private readonly string _destination;
		private readonly ILogger _logger;
		private readonly string _directory;
		private readonly string _source;
		private readonly Copy _type;

		public CopyFile(ILogger logger,
		                string directory,
		                string source,
		                string destination,
		                Copy type)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");

			_directory = directory;
			_logger = logger;
			_source = source;
			_destination = destination;
			_type = type;
		}

		public void Run()
		{
			var relativeSource = Path.MakeRelative(_directory, _source);
			var relativeDestination = Path.MakeRelative(_directory, _destination);
			_logger.WriteLine(Verbosity.Normal, "  Copying file from \"{0}\" to \"{1}\".",
			                  relativeSource,
			                  relativeDestination);

			if (_type == Copy.Always)
			{
				CopyAlways();
			}
			else
			{
				CopyIfNewer();
			}
		}

		private void CopyAlways()
		{
			File.Copy(_source, _destination, true);
		}

		private void CopyIfNewer()
		{
			if (File.Exists(_destination))
			{
				DateTime sourceWritten = File.GetLastWriteTime(_source);
				DateTime destinationWritte = File.GetLastWriteTime(_destination);

				if (sourceWritten <= destinationWritte)
					return;
			}

			CopyAlways();
		}
	}
}