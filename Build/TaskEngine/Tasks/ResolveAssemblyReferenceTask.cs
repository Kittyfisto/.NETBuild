using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;

namespace Build.TaskEngine.Tasks
{
	public sealed class ResolveAssemblyReferenceTask
		: ITaskRunner
	{
		private const string DotNetPath =
			   @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\";

		private static readonly string[] AssemblyExtensions = new[] { ".exe", ".dll" };

		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public ResolveAssemblyReferenceTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem)
		{
			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var resolve = (ResolveAssemblyReference) task;
			var assemblies = _expressionEngine.EvaluateItemList(resolve.Assemblies, environment);

			var rootPath = environment.Properties[Properties.MSBuildProjectDirectory];
			var builder = new StringBuilder();
			for (int i = 0; i < assemblies.Length; ++i)
			{
				var fileName = ResolveReference(assemblies[i], rootPath);
				if (fileName != null)
				{
					builder.Append(fileName);
					builder.Append(Tokenizer.ItemListSeparator);
				}
			}

			if (builder.Length > 0)
				builder.Remove(builder.Length - 1, 1);

			environment.Output["ResolvedFiles"] = builder.ToString();
		}

		public string ResolveReference(ProjectItem reference, string rootPath)
		{
			var hintPathProperty = reference[Metadatas.HintPath];
			//var hintPath = _expressionEngine.Evaluate(hintPathProperty, new BuildEnvironment());
			var hintPath = hintPathProperty;

			string path;
			if (!string.IsNullOrEmpty(hintPath))
			{
				path = Resolve(rootPath, hintPath);
				if (File.Exists(path))
				{
					return path;
				}
			}

			path = Path.Combine(DotNetPath, reference.Include);
			string actualPath;
			if (!TryResolveReference(path, out actualPath))
				return null;

			return actualPath;
		}

		private bool TryResolveReference(string absolutePath, out string actualPath)
		{
			// TODO: This is actually a lot more complicated because absolutePath might end with an assembly qualified name.
			//       If it does, then we need to look through all possible include folders and find a match for this name

			if (EndsWithExtension(absolutePath))
			{
				if (File.Exists(absolutePath))
				{
					actualPath = absolutePath;
					return true;
				}

				actualPath = null;
				return false;
			}

			foreach (string extension in AssemblyExtensions)
			{
				string path = absolutePath + extension;
				if (File.Exists(path))
				{
					actualPath = path;
					return true;
				}
			}

			actualPath = null;
			return false;
		}

		[Pure]
		private static bool EndsWithExtension(string absolutePath)
		{
			foreach (string extension in AssemblyExtensions)
			{
				if (absolutePath.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private string Resolve(string rootPath, string relativeOrAbsolutePath)
		{
			if (Path.IsPathRooted(relativeOrAbsolutePath))
			{
				return relativeOrAbsolutePath;
			}

			string path = Path.Combine(rootPath, relativeOrAbsolutePath);
			return path;
		}
	}
}