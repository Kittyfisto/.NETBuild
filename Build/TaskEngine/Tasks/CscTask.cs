using System;
using System.IO;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	public class CscTask : ITaskRunner
	{
		private const string CompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Csc.exe";

		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public CscTask(ExpressionEngine.ExpressionEngine expressionEngine,
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
			var csc = (Csc) task;
			var arguments = new ArgumentBuilder();

			BuildCommandLineArguments(environment, csc, arguments);

			logger.WriteLine(Verbosity.Normal, "  {0} {1}", CompilerPath, arguments);

			CreateDirectories(environment, csc);

			var rootPath = environment.Properties[Properties.MSBuildProjectDirectory];
			string output;
			var exitCode = ProcessEx.Run(CompilerPath, rootPath, arguments, out output);
			logger.WriteMultiLine(Verbosity.Minimal, output, interpretLines: true);

			if (exitCode != 0)
				throw new BuildException(string.Format("csc returned {0}", exitCode));
		}

		private void CreateDirectories(BuildEnvironment environment, Csc csc)
		{
			var root = environment.Properties[Properties.MSBuildProjectDirectory];

			var outputAssembly = _expressionEngine.EvaluateExpression(csc.OutputAssembly, environment);
			var assemblyOutputPath = Path.MakeAbsolute(root, Path.GetDirectory(outputAssembly));
			_fileSystem.CreateDirectory(assemblyOutputPath);

			var outputPdb = _expressionEngine.EvaluateExpression(csc.PdbFile, environment);
			var pdbPath = Path.MakeAbsolute(root, Path.GetDirectory(outputPdb));
			_fileSystem.CreateDirectory(pdbPath);
		}

		private void BuildCommandLineArguments(BuildEnvironment environment, Csc csc, ArgumentBuilder arguments)
		{
			if (AllowUnsafeBlocks(environment, csc))
				arguments.Flag("unsafe");

			arguments.Add("debug", GetDebugType(environment, csc));
			if (EmitDebugInformation(environment, csc))
				arguments.Flag("debug+");

			if (!Optimize(environment, csc))
				arguments.Flag("optimize-");

			var baseAddress = GetBaseAddress(environment, csc);
			if (!string.IsNullOrEmpty(baseAddress))
				arguments.Add("baseaddress", baseAddress);

			arguments.Add("define", GetDefines(environment, csc));
			arguments.Add("nowarn", GetDisabledWarnings(environment, csc));

			if (GetNoLogo(environment, csc))
				arguments.Flag("nologo");

			arguments.Add("target", GetTarget(environment, csc));
			arguments.Add("errorreport", GetErrorReport(environment, csc));
			arguments.Add("platform", GetPlatform(environment, csc));

			if (ErrorEndLocation(environment, csc))
				arguments.Flag("errorendlocation");

			arguments.Add("filealign", GetFileAlignment(environment, csc));

			if (HighEntropyVA(environment, csc))
				arguments.Flag("highentropyva+");

			arguments.Add("out", GetOutputAssembly(environment, csc));

			arguments.Flag("nostdlib+");

			var rootPath = environment.Properties[Properties.MSBuildProjectDirectory];
			var references = _expressionEngine.EvaluateItemList(csc.References, environment);
			foreach (var reference in references)
			{
				AddReference(arguments, reference, rootPath);
			}

			var resources = _expressionEngine.EvaluateItemList(csc.Resources, environment);
			foreach (var resource in resources)
			{
				var include = resource.Include;
				var resourceName = string.Format("EmbeddedResource.{0}", include.Replace('\\', '.'));
				arguments.Add("resource", resource.Include, resourceName);
			}

			var sources = _expressionEngine.EvaluateItemList(csc.Sources, environment);
			foreach (var source in sources)
			{
				arguments.Add(source.Include);
			}

			string outputPath = environment.Properties[Properties.OutputPath];
			_fileSystem.CreateDirectory(outputPath);

			// http://stackoverflow.com/questions/19306194/process-launch-exception-filename-or-extension-is-too-long-can-it-be-caused-b
			int commandLineLength = arguments.Length + CompilerPath.Length + 1;
			if (commandLineLength > 32000) //< The maximum length seems to be 32768, but this one's close enough
			{
				// In order to not exceed the limit we'll have to write the arguments
				// to a file that is then passes as an argument to csc (so that's why that option exists).
				var filename = Path.Combine(outputPath, "csc_arguments.txt");
				File.WriteAllText(filename, arguments.ToString());
				arguments.Clear();

				arguments.Add(string.Format("\"@{0}\"", filename));
			}

			// without this, csc automatically references a bunch of assemblies and then gives us shit
			// that we've included a double reference...
			arguments.Flag("noconfig");
		}

		private void AddReference(ArgumentBuilder arguments, ProjectItem reference, string rootPath)
		{
			string referencePath = reference.Include;
			arguments.Add("reference", referencePath);
		}

		private string GetPlatform(BuildEnvironment environment, Csc csc)
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
						return "anycpu";
					}

					return prefer32Bit
							    ? "anycpu32bitpreferred"
							    : "anycpu";

				case x86:
					return "x86";

				case x64:
					return "x64";

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

		private string GetBaseAddress(BuildEnvironment environment, Csc csc)
		{
			var address = csc.BaseAddress;
			var evaluated = _expressionEngine.EvaluateExpression(address, environment);
			return evaluated;
		}

		private string GetOutputAssembly(BuildEnvironment environment, Csc csc)
		{
			var asm = csc.OutputAssembly;
			var evaluated = _expressionEngine.EvaluateExpression(asm, environment);
			return evaluated;
		}

		private bool HighEntropyVA(BuildEnvironment environment, Csc csc)
		{
			var entropy = csc.HighEntropyVA;
			var evaluated = _expressionEngine.EvaluateCondition(entropy, environment);
			return evaluated;
		}

		private string GetFileAlignment(BuildEnvironment environment, Csc csc)
		{
			var alignment = csc.FileAlignment;
			var evaluated = _expressionEngine.EvaluateExpression(alignment, environment);
			return evaluated;
		}

		private bool ErrorEndLocation(BuildEnvironment environment, Csc csc)
		{
			var end = csc.ErrorEndLocation;
			return _expressionEngine.EvaluateCondition(end, environment);
		}

		private string GetErrorReport(BuildEnvironment environment, Csc csc)
		{
			var error = csc.ErrorReport;
			var evaluated = _expressionEngine.EvaluateExpression(error, environment);
			return evaluated;
		}

		private string GetDisabledWarnings(BuildEnvironment environment, Csc csc)
		{
			var disabled = csc.DisabledWarnings;
			var evaluated = _expressionEngine.EvaluateExpression(disabled, environment);
			return evaluated;
		}

		private string GetDefines(BuildEnvironment environment, Csc csc)
		{
			var defines = csc.DefineConstants;
			var evaluated = _expressionEngine.EvaluateExpression(defines, environment);
			return evaluated;
		}

		private string GetDebugType(BuildEnvironment environment, Csc csc)
		{
			var debugType = csc.DebugType;
			var evaluated = _expressionEngine.EvaluateExpression(debugType, environment);
			return evaluated;
		}

		private bool EmitDebugInformation(BuildEnvironment environment, Csc csc)
		{
			var emit = csc.EmitDebugInformation;
			return _expressionEngine.EvaluateCondition(emit, environment);
		}

		private bool Optimize(BuildEnvironment environment, Csc csc)
		{
			var optimize = csc.Optimize;
			return _expressionEngine.EvaluateCondition(optimize, environment);
		}

		private bool AllowUnsafeBlocks(BuildEnvironment environment, Csc csc)
		{
			var allow = csc.AllowUnsafeBlocks;
			return _expressionEngine.EvaluateCondition(allow, environment);
		}

		private bool GetNoLogo(BuildEnvironment environment, Csc csc)
		{
			var target = csc.NoLogo;
			var evaluated = _expressionEngine.EvaluateCondition(target, environment);
			return evaluated;
		}

		private string GetTarget(BuildEnvironment environment, Csc csc)
		{
			var target = csc.TargetType;
			var evaluated = _expressionEngine.EvaluateExpression(target, environment);

			switch (evaluated)
			{
				case "Library":
					return "library";

				case "Exe":
					return "exe";

				case "WinExe":
					return "winexe";

				default:
					throw new Exception(string.Format("Unknown output type: '{0}'", evaluated));
			}
		}
	}
}