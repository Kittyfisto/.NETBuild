using System;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal sealed class CopyTask
		: ITaskRunner
	{
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;

		public CopyTask(ExpressionEngine.ExpressionEngine expressionEngine,
		                IFileSystem fileSystem,
		                ILogger logger,
		                BuildEnvironment environment)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (logger == null)
				throw new ArgumentNullException("logger");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
			_logger = logger;
			_environment = environment;
		}

		public void Run(Node task)
		{
			var copy = (Copy) task;

			ProjectItem[] sourceFiles = _expressionEngine.EvaluateItemList(copy.SourceFiles, _environment);
			ProjectItem[] destinationFiles = _expressionEngine.EvaluateItemList(copy.DestinationFiles, _environment);

			if (sourceFiles.Length != destinationFiles.Length)
				throw new BuildException(
					string.Format("SourceFiles and DestinationFiles must be of the same length, but \"{0}\" and \"{1}\" are not",
					              sourceFiles.Length,
					              destinationFiles.Length));

			string directory = _environment.Properties[Properties.MSBuildProjectDirectory];
			var copied = new ProjectItem[sourceFiles.Length];
			for (int i = 0; i < sourceFiles.Length; ++i)
			{
				ProjectItem source = sourceFiles[i];
				ProjectItem destination = destinationFiles[i];

				if (Copy(directory, source, destination))
				{
					copied[i] = destination;
				}
			}
		}

		private bool Copy(string directory,
		                  ProjectItem source,
		                  ProjectItem destination)
		{
			string absoluteSource = source[Metadatas.FullPath];
			string absoluteDestination = destination[Metadatas.FullPath];

			string relativeSource = Path.MakeRelative(directory, absoluteSource);
			string relativeDestination = Path.MakeRelative(directory, absoluteDestination);

			_logger.WriteLine(Verbosity.Normal, "  Copying file from \"{0}\" to \"{1}\".",
			                  relativeSource,
			                  relativeDestination);

			try
			{
				_fileSystem.CopyFile(absoluteSource, absoluteDestination);
				return true;
			}
			catch (UnauthorizedAccessException e)
			{
				LogError(relativeSource, relativeDestination, e);
				return false;
			}
			catch (IOException e)
			{
				LogError(relativeSource, relativeDestination, e);
				return false;
			}
		}

		private void LogError(
			string relativeSource,
			string relativeDestination,
			Exception inner)
		{
			_logger.WriteError("Unable to copy file from '\"{0}\" to \"{1}\": {2}",
			                   relativeSource,
			                   relativeDestination,
			                   inner.Message);
		}
	}
}