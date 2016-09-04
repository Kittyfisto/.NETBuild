using System.Text;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;

namespace Build.TaskEngine.Tasks
{
	public sealed class ResolveProjectReferenceTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public ResolveProjectReferenceTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem)
		{
			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var t = (ResolveProjectReference) task;
			var items = _expressionEngine.EvaluateItemList(t.ProjectReferences, environment);
			var fileNames = ResolveFileNames(items, environment);
			var projects = graph.TryGetAll(fileNames);
			var assemblies = ResolveOutputAssemblies(projects);
			var builder = new StringBuilder();
			for (int i = 0; i < assemblies.Length; ++i)
			{
				var name = assemblies[i];
				if (name != null)
				{
					builder.Append(name);
					builder.Append(Tokenizer.ItemListSeparator);
				}
			}

			if (builder.Length != 0)
				builder.Remove(builder.Length - 1, 1);

			environment.Output["ResolvedFiles"] = builder.ToString();
		}

		private string[] ResolveFileNames(ProjectItem[] items, BuildEnvironment environment)
		{
			var fileNames = new string[items.Length];
			for (int i = 0; i < items.Length; ++i)
			{
				var fileName = items[i].Include;
				if (!Path.IsPathRooted(fileName))
					fileName = Path.Combine(environment.Properties[Properties.MSBuildProjectDirectory], fileName);
				fileName = Path.Normalize(fileName);
				fileNames[i] = fileName;
			}
			return fileNames;
		}

		private string[] ResolveOutputAssemblies(ProjectAndEnvironment[] pairs)
		{
			var ret = new string[pairs.Length];
			for (int i = 0; i < pairs.Length; ++i)
			{
				var pair = pairs[i];
				if (pair.Project != null)
				{
					var environment = pair.Environment;
					var outputAssembly = environment.Properties["OutputAssembly"];
					if (!Path.IsPathRooted(outputAssembly))
					{
						var dir = environment.Properties[Properties.MSBuildProjectDirectory];
						outputAssembly = Path.Combine(dir, outputAssembly);
					}

					ret[i] = Path.Normalize(outputAssembly);
				}
			}
			return ret;
		}
	}
}