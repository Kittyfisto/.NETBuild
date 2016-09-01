using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	public class CscTask : ITaskRunner
	{
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;

		public CscTask(ExpressionEngine.ExpressionEngine expressionEngine,
		               IFileSystem fileSystem,
		               ILogger logger,
		               BuildEnvironment environment)
		{
			if (environment == null)
				throw new ArgumentNullException("environment");
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
			var csc = (Csc) task;
			var arguments = new ArgumentBuilder();

			if (AllowUnsafeBlocks(csc))
				arguments.Flag("unsafe");

			arguments.Add("debug", GetDebugType(csc));
			if (EmitDebugInformation(csc))
				arguments.Flag("debug+");

			if (!Optimize(csc))
				arguments.Flag("optimize-");

			arguments.Add("baseaddress", GetBaseAddress(csc));
			arguments.Add("define", GetDefines(csc));
			arguments.Add("nowarn", GetDisabledWarnings(csc));
			arguments.Add("nologo", GetNoLogo(csc));
			arguments.Add("target", GetTarget(csc));
			arguments.Add("errorreport", GetErrorReport(csc));
			arguments.Add("platform", GetPlatform(csc));

			if (ErrorEndLocation(csc))
				arguments.Flag("errorendlocation");

			arguments.Add("filealign", GetFileAlignment(csc));

			if (HighEntropyVA(csc))
				arguments.Flag("highentropyva+");

			arguments.Add("out", GetOutputAssembly(csc));

			arguments.Flag("nostdlib+");
		}

		private string GetPlatform(Csc csc)
		{
			string platform = _expressionEngine.EvaluateExpression(csc.Platform, _environment);
			var prefer32Bit = _expressionEngine.EvaluateCondition(csc.Prefer32Bit, _environment);

			const string anyCpu = "AnyCPU";
			const string x86 = "x86";
			const string x64 = "x64";

			switch (platform)
			{
				case anyCpu:
					if (_environment.Properties[Properties.OutputType] == "Library")
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

		private string GetBaseAddress(Csc csc)
		{
			var address = csc.BaseAddress;
			var evaluated = _expressionEngine.EvaluateExpression(address, _environment);
			return evaluated;
		}

		private string GetOutputAssembly(Csc csc)
		{
			var asm = csc.OutputAssembly;
			var evaluated = _expressionEngine.EvaluateExpression(asm, _environment);
			return evaluated;
		}

		private bool HighEntropyVA(Csc csc)
		{
			var entropy = csc.HighEntropyVA;
			var evaluated = _expressionEngine.EvaluateCondition(entropy, _environment);
			return evaluated;
		}

		private string GetFileAlignment(Csc csc)
		{
			var alignment = csc.FileAlignment;
			var evaluated = _expressionEngine.EvaluateExpression(alignment, _environment);
			return evaluated;
		}

		private bool ErrorEndLocation(Csc csc)
		{
			var end = csc.ErrorEndLocation;
			return _expressionEngine.EvaluateCondition(end, _environment);
		}

		private string GetErrorReport(Csc csc)
		{
			var error = csc.ErrorReport;
			var evaluated = _expressionEngine.EvaluateExpression(error, _environment);
			return evaluated;
		}

		private string GetDisabledWarnings(Csc csc)
		{
			var disabled = csc.DisabledWarnings;
			var evaluated = _expressionEngine.EvaluateExpression(disabled, _environment);
			return evaluated;
		}

		private string GetDefines(Csc csc)
		{
			var defines = csc.DefineConstants;
			var evaluated = _expressionEngine.EvaluateExpression(defines, _environment);
			return evaluated;
		}

		private string GetDebugType(Csc csc)
		{
			var debugType = csc.DebugType;
			var evaluated = _expressionEngine.EvaluateExpression(debugType, _environment);
			return evaluated;
		}

		private bool EmitDebugInformation(Csc csc)
		{
			var emit = csc.EmitDebugInformation;
			return _expressionEngine.EvaluateCondition(emit, _environment);
		}

		private bool Optimize(Csc csc)
		{
			var optimize = csc.Optimize;
			return _expressionEngine.EvaluateCondition(optimize, _environment);
		}

		private bool AllowUnsafeBlocks(Csc csc)
		{
			var allow = csc.AllowUnsafeBlocks;
			return _expressionEngine.EvaluateCondition(allow, _environment);
		}

		private string GetNoLogo(Csc csc)
		{
			var target = csc.NoLogo;
			var evaluated = _expressionEngine.EvaluateCondition(target, _environment);
			return evaluated ? "true" : "false";
		}

		private string GetTarget(Csc csc)
		{
			var target = csc.TargetType;
			var evaluated = _expressionEngine.EvaluateExpression(target, _environment);

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