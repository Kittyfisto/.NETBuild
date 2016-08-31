using System;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal sealed class DeleteTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;
		private readonly BuildEnvironment _environment;

		public DeleteTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem, ILogger logger, BuildEnvironment environment)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (environment == null)
				throw new ArgumentNullException("environment");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
			_logger = logger;
			_environment = environment;
		}

		public void Run(Node task)
		{
			var delete = (Delete) task;
			var files = _expressionEngine.EvaluateItemList(delete.Files, _environment);

			string directory = _environment.Properties[Properties.MSBuildProjectDirectory];
			var deleted = new ProjectItem[files.Length];
			for (int i = 0; i < files.Length; ++i)
			{
				ProjectItem file = files[i];

				if (Delete(directory, file))
				{
					deleted[i] = file;
				}
			}
		}

		private bool Delete(string directory, ProjectItem file)
		{
			string absoluteFile = file[Metadatas.FullPath];
			string relativeFile = Path.MakeRelative(directory, absoluteFile);

			_logger.WriteLine(Verbosity.Normal, "  Deleting file \"{0}\".",
			                  relativeFile);

			try
			{
				_fileSystem.DeleteFile(absoluteFile);
				return true;
			}
			catch (UnauthorizedAccessException e)
			{
				LogError(relativeFile, e);
				return false;
			}
			catch (IOException e)
			{
				LogError(relativeFile, e);
				return false;
			}
		}

		private void LogError(
			string relativeFile,
			Exception inner)
		{
			_logger.WriteError("Unable to delete file '\"{0}\": {1}",
							   relativeFile,
							   inner.Message);
		}
	}
}