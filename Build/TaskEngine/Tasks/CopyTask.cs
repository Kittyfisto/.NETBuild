using System;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	internal sealed class CopyTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public CopyTask(ExpressionEngine.ExpressionEngine expressionEngine,
		                IFileSystem fileSystem)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var copy = (Copy) task;

			ProjectItem[] sourceFiles = _expressionEngine.EvaluateItemList(copy.SourceFiles, environment);
			ProjectItem[] destinationFiles = _expressionEngine.EvaluateItemList(copy.DestinationFiles, environment);

			if (sourceFiles.Length != destinationFiles.Length)
				throw new BuildException(
					string.Format("SourceFiles and DestinationFiles must be of the same length, but \"{0}\" and \"{1}\" are not",
					              sourceFiles.Length,
					              destinationFiles.Length));

			string directory = environment.Properties[Properties.MSBuildProjectDirectory];
			var copied = new ProjectItem[sourceFiles.Length];
			for (int i = 0; i < sourceFiles.Length; ++i)
			{
				ProjectItem source = sourceFiles[i];
				ProjectItem destination = destinationFiles[i];

				if (Copy(directory, source, destination, logger))
				{
					copied[i] = destination;
				}
			}
		}

		private bool Copy(string directory,
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
				_fileSystem.CopyFile(absoluteSource, absoluteDestination, true);
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