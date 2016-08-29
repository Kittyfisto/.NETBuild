using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Build.BuildEngine.Tasks
{
	public sealed class DeleteFileTask
	{
		private readonly ILogger _logger;
		private readonly string _directory;
		private readonly string _file;

		public DeleteFileTask(ILogger logger,
		                string directory,
		                string file)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (file == null)
				throw new ArgumentNullException("file");

			_directory = directory;
			_logger = logger;
			_file = file;
		}

		public void Run()
		{
			var relativeFile = Path.MakeRelative(_directory, _file);
			_logger.WriteLine(Verbosity.Normal, "  Delete file from \"{0}\".", relativeFile);

			try
			{
				if (File.Exists(_file))
				{
					File.Delete(_file);
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
			var message = string.Format("Unable to delete file from \"{0}\"", _file);
			return new BuildException(message, inner);
		}
	}
}