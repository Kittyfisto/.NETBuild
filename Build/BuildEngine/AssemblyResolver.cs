using System;
using System.Diagnostics.Contracts;
using System.IO;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for resolving assemblies on the local system.
	/// </summary>
	public sealed class AssemblyResolver
	{
		private const string DotNetPath =
			@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\";

		private static readonly string[] AssemblyExtensions = new[] {".exe", ".dll"};
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public AssemblyResolver(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");

			_expressionEngine = expressionEngine;
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
				throw new NotImplementedException();

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