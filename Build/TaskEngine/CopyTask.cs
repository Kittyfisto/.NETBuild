using System;
using System.Diagnostics.Contracts;
using System.IO;
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

			if (input.Length != output.Length)
				throw new BuildException();

			for (int i = 0; i < input.Length; ++i)
			{
				var directory = environment[Properties.MSBuildProjectDirectory];
				string source = input[i];
				var destination = output[i];

				Copy(fileSystem, directory, source, destination, logger);
			}
		}

		private static void Copy(IFileSystem fileSystem, string directory, string source, string destination, ILogger logger)
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
			}
			catch (UnauthorizedAccessException e)
			{
				throw CreateBuildException(relativeSource, relativeDestination, e);
			}
			catch (IOException e)
			{
				throw CreateBuildException(relativeSource, relativeDestination, e);
			}
		}

		[Pure]
		private static BuildException CreateBuildException(
			string relativeSource,
			string relativeDestination,
			Exception inner)
		{
			var message = string.Format("Unable to copy file from '\"{0}\" to \"{1}\"",
			                            relativeSource,
			                            relativeDestination);
			return new BuildException(message, inner);
		}
	}
}