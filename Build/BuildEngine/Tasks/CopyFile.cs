using System;
using System.Diagnostics.Contracts;
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
		private readonly string _relativeSource;
		private readonly string _relativeDestination;

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

			_relativeSource = Path.MakeRelative(_directory, _source);
			_relativeDestination = Path.MakeRelative(_directory, _destination);
		}

		public void Run()
		{
			_logger.WriteLine(Verbosity.Normal, "  Copying file from \"{0}\" to \"{1}\".",
			                  _relativeSource,
			                  _relativeDestination);

			try
			{
				if (_type == Copy.Always)
				{
					CopyAlways();
				}
				else
				{
					CopyIfNewer();
				}
			}
			catch (UnauthorizedAccessException e)
			{
				throw CreateBuildException(e);
			}
			catch (IOException e)
			{
				throw CreateBuildException(e);
			}
		}

		[Pure]
		private BuildException CreateBuildException(Exception inner)
		{
			var message = string.Format("Unable to copy file from '\"{0}\" to \"{1}\"",
											_relativeSource,
											_relativeDestination);
			return new BuildException(message, inner);
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