using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Build.TaskEngine.Tasks
{
	/// <summary>
	///     Alternate CSC task implementation.
	///     Uses the Roslyn platform.
	/// </summary>
	public sealed class RoslynTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public RoslynTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException(nameof(expressionEngine));
			if (fileSystem == null)
				throw new ArgumentNullException(nameof(fileSystem));

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var csc = (Csc) task;
			var compilation = Compile(environment, csc);

			var outputAssembly = GetOutputAssembly(environment, csc);
			var rootPath = environment.Properties[Properties.MSBuildProjectDirectory];
			var fullAssemblyFileName = Path.MakeAbsolute(rootPath, outputAssembly);
			var pdbName = string.Format("{0}.pdb", Path.GetFilenameWithoutExtension(outputAssembly));
			var outputPdb = Path.Combine(Path.GetDirectory(outputAssembly), pdbName);
			var fullPdbFileName = Path.MakeAbsolute(rootPath, outputPdb);

			var manifestResources = GetManifestResources(environment, csc);

			var directory = Path.GetDirectory(fullAssemblyFileName);
			_fileSystem.CreateDirectory(directory);

			using (var assemblyStream = _fileSystem.OpenWrite(fullAssemblyFileName))
			using (var pdbStream = _fileSystem.OpenWrite(fullPdbFileName))
			{
				var result = compilation.Emit(assemblyStream, pdbStream, manifestResources: manifestResources);
				if (!result.Success)
				{
					foreach(var diagnostic in result.Diagnostics)
					{
						logger.WriteMultiLine(Verbosity.Minimal, diagnostic.ToString(), true);
					}

					throw new BuildException("csc returned: -1");
				}
			}
		}

		private List<ResourceDescription> GetManifestResources(BuildEnvironment environment, Csc csc)
		{
			var ret = new List<ResourceDescription>();

			var rootPath = environment.Properties[Properties.MSBuildProjectDirectory];
			var resources = _expressionEngine.EvaluateItemList(csc.Resources, environment);
			foreach (var resource in resources)
			{
				var include = resource.Include;
				var fileName = Path.MakeAbsolute(rootPath, include);
				var resourceName = string.Format("EmbeddedResource.{0}", include.Replace('\\', '.'));
				var description = new ResourceDescription(resourceName, () => _fileSystem.OpenRead(fileName), true);

				ret.Add(description);
			}

			return ret;
		}

		private CSharpCompilation Compile(BuildEnvironment environment, Csc csc)
		{
			var syntaxTrees = ParseSources(environment, csc);
			var references = GetReferences(environment, csc);
			var options = CreateOptions(environment, csc);
			var outputAssembly = GetOutputAssembly(environment, csc);
			var assemblyName = Path.GetFilename(outputAssembly);
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);
			return compilation;
		}

		private IEnumerable<MetadataReference> GetReferences(BuildEnvironment environment, Csc csc)
		{
			var ret = new List<MetadataReference>();
			var references = _expressionEngine.EvaluateItemList(csc.References, environment);
			foreach (var reference in references)
			{
				var referencePath = reference.Include;
				ret.Add(MetadataReference.CreateFromFile(referencePath));
			}
			return ret;
		}

		private List<SyntaxTree> ParseSources(BuildEnvironment environment, Csc csc)
		{
			var syntaxTrees = new List<SyntaxTree>();

			var sources = _expressionEngine.EvaluateItemList(csc.Sources, environment);
			foreach (var sourceFile in sources)
			{
				var sourceCode = _fileSystem.ReadAllText(sourceFile[Metadatas.FullPath]);
				var tree = CSharpSyntaxTree.ParseText(sourceCode);
				syntaxTrees.Add(tree);
			}

			return syntaxTrees;
		}

		private CSharpCompilationOptions CreateOptions(BuildEnvironment environment, Csc csc)
		{
			var options = new CSharpCompilationOptions(
				GetTarget(environment, csc),
				allowUnsafe: AllowUnsafeBlocks(environment, csc),
				platform: GetPlatform(environment, csc),
				optimizationLevel: GetOpimizationLevel(environment, csc)
			);
			return options;
		}

		private bool AllowUnsafeBlocks(BuildEnvironment environment, Csc csc)
		{
			var allow = csc.AllowUnsafeBlocks;
			return _expressionEngine.EvaluateCondition(allow, environment);
		}

		private Platform GetPlatform(BuildEnvironment environment, Csc csc)
		{
			string platform = _expressionEngine.EvaluateExpression(csc.Platform, environment);
			var prefer32Bit = _expressionEngine.EvaluateCondition(csc.Prefer32Bit, environment);

			const string anyCpu = "AnyCPU";
			const string x86 = "x86";
			const string x64 = "x64";

			switch (platform)
			{
				case anyCpu:
					if (environment.Properties[Properties.OutputType] == "Library")
					{
						return Platform.AnyCpu;
					}

					return prefer32Bit
								? Platform.AnyCpu32BitPreferred
								: Platform.AnyCpu;

				case x86:
					return Platform.X86;

				case x64:
					return Platform.X64;

				default:
					throw new EvaluationException(
						string.Format(
							"Specified condition \"{0}\" evaluates to \"{1}\" instead of any of the allow values \"{2}\", \"{3}\" or \"{4}\".",
							csc.Platform,
							platform,
							anyCpu, x86, x64)
						);
			}
		}

		private OptimizationLevel GetOpimizationLevel(BuildEnvironment environment, Csc csc)
		{
			var optimize = csc.Optimize;
			return _expressionEngine.EvaluateCondition(optimize, environment)
				? OptimizationLevel.Release
				: OptimizationLevel.Debug;
		}

		private OutputKind GetTarget(BuildEnvironment environment, Csc csc)
		{
			var target = csc.TargetType;
			var evaluated = _expressionEngine.EvaluateExpression(target, environment);

			switch (evaluated)
			{
				case "Library":
					return OutputKind.DynamicallyLinkedLibrary;

				case "Exe":
					return OutputKind.ConsoleApplication;

				case "WinExe":
					return OutputKind.WindowsApplication;

				default:
					throw new Exception(string.Format("Unknown output type: '{0}'", evaluated));
			}
		}

		private string GetOutputAssembly(BuildEnvironment environment, Csc csc)
		{
			var asm = csc.OutputAssembly;
			var evaluated = _expressionEngine.EvaluateExpression(asm, environment);
			return evaluated;
		}
	}
}