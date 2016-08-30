using System;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.TaskEngine
{
	internal static class CopyTask
	{
		public static ProjectItem[] Run(IFileSystem fileSystem,
		                                BuildEnvironment environment,
		                                ProjectItem[] sourceFiles,
		                                ProjectItem[] destinationFiles,
		                                ILogger logger)
		{
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (sourceFiles == null)
				throw new ArgumentNullException("sourceFiles");
			if (destinationFiles == null)
				throw new ArgumentNullException("destinationFiles");
			if (logger == null)
				throw new ArgumentNullException("logger");

			if (sourceFiles.Length != destinationFiles.Length)
				throw new BuildException(
					string.Format("SourceFiles and DestinationFiles must be of the same length, but \"{0}\" and \"{1}\" are given",
					              sourceFiles.Length,
					              destinationFiles.Length));

			string directory = environment.Properties[Properties.MSBuildProjectDirectory];
			var copied = new ProjectItem[sourceFiles.Length];
			for (int i = 0; i < sourceFiles.Length; ++i)
			{
				ProjectItem source = sourceFiles[i];
				ProjectItem destination = destinationFiles[i];

				if (Copy(fileSystem, directory, source, destination, logger))
				{
					copied[i] = destination;
				}
			}

			return copied;
		}

		private static bool Copy(IFileSystem fileSystem, string directory,
		                         ProjectItem source,
		                         ProjectItem destination,
		                         ILogger logger)
		{
			string absoluteSource = source[Metadatas.FullPath];
			string absoluteDestination = destination[Metadatas.FullPath];

			string relativeSource = Path.MakeRelative(directory, absoluteSource);
			string relativeDestination = Path.MakeRelative(directory, absoluteDestination);

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