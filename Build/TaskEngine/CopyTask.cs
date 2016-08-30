using System;
using System.IO;
using System.Text;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.TaskEngine
{
	internal static class CopyTask
	{
		private static readonly char[] Separator = new[] {';'};

		public static void Run(IFileSystem fileSystem,
		                       BuildEnvironment environment,
		                       Copy task,
		                       ILogger logger)
		{
			if (task.SourceFiles == null)
				throw new BuildException();
			if (task.DestinationFiles == null)
				throw new BuildException();

			string[] input = task.SourceFiles.Split(Separator);
			string[] output = task.DestinationFiles.Split(Separator);
			var copied = new StringBuilder();

			if (input.Length != output.Length)
				throw new BuildException();

			for (int i = 0; i < input.Length; ++i)
			{
				var directory = environment.Properties[Properties.MSBuildProjectDirectory];
				string source = input[i];
				var destination = output[i];

				if (Copy(fileSystem, directory, source, destination, logger))
				{
					copied.Append(destination);
					copied.Append(';');
				}
			}

			if (copied.Length > 0)
				copied.Remove(copied.Length - 1, 1);
			task.CopiedFiles = copied.ToString();
		}

		private static bool Copy(IFileSystem fileSystem, string directory, string source, string destination, ILogger logger)
		{
			var relativeSource = Path.MakeRelative(directory, source);
			var absoluteSource = Path.MakeAbsolute(directory, source);

			var relativeDestination = Path.MakeRelative(directory, destination);
			var absoluteDestination = Path.MakeAbsolute(directory, destination);

			logger.WriteLine(Verbosity.Normal, "  Copying file from \"{0}\" to \"{1}\".",
			                 relativeSource,
			                 relativeDestination);

			try
			{
				fileSystem.Copy(absoluteSource, absoluteDestination);
				return true;
			}
			catch (UnauthorizedAccessException e)
			{
				LogError(logger, relativeSource, relativeDestination, e);
				return false;
			}
			catch (IOException e)
			{
				LogError(logger, relativeSource, relativeDestination, e);
				return false;
			}
		}

		private static void LogError(
			ILogger logger,
			string relativeSource,
			string relativeDestination,
			Exception inner)
		{
			logger.WriteError("Unable to copy file from '\"{0}\" to \"{1}\": {2}",
			                  relativeSource,
			                  relativeDestination,
			                  inner.Message);
		}
	}
}