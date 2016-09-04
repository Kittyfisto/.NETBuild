using System;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	internal sealed class DeleteTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public DeleteTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem)
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
			var delete = (Delete) task;
			ProjectItem[] files = _expressionEngine.EvaluateItemList(delete.Files, environment);

			string directory = environment.Properties[Properties.MSBuildProjectDirectory];
			var deleted = new ProjectItem[files.Length];
			for (int i = 0; i < files.Length; ++i)
			{
				ProjectItem file = files[i];

				if (Delete(directory, file, logger))
				{
					deleted[i] = file;
				}
			}
		}

		private bool Delete(string directory, ProjectItem file, ILogger logger)
		{
			string absoluteFile = file[Metadatas.FullPath];
			string relativeFile = Path.MakeRelative(directory, absoluteFile);

			logger.WriteLine(Verbosity.Normal, "  Deleting file \"{0}\".",
			                 relativeFile);

			try
			{
				_fileSystem.DeleteFile(absoluteFile);
				return true;
			}
			catch (UnauthorizedAccessException e)
			{
				LogError(logger, relativeFile, e);
				return false;
			}
			catch (IOException e)
			{
				LogError(logger, relativeFile, e);
				return false;
			}
		}

		private void LogError(
			ILogger logger,
			string relativeFile,
			Exception inner)
		{
			logger.WriteError("Unable to delete file '\"{0}\": {1}",
			                  relativeFile,
			                  inner.Message);
		}
	}
}