using System;
using System.Linq;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	public sealed class CSharpProjectCompiler
	{
		private const string CompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Csc.exe";
		private static readonly ProjectItem MsCoreLibrary = new ProjectItem(Items.Reference, "mscorlib");
		private static readonly BuildEnvironment DefaultValues;

		private readonly AssemblyResolver _resolver;
		private readonly CSharpProject _project;
		private readonly BuildEnvironment _projectEnvironment;
		private readonly ArgumentBuilder _arguments;
		private readonly string _rootPath;

		private int _exitCode;
		private string _output;

		static CSharpProjectCompiler()
		{
			DefaultValues = new BuildEnvironment
				{
					{Properties.SubsystemVersion, "6.00"},
					{Properties.Prefer32Bit, "True"}
				};
		}

		public CSharpProjectCompiler(AssemblyResolver assemblyResolver,
			CSharpProject project,
			BuildEnvironment projectEnvironment)
		{
			if (assemblyResolver == null)
				throw new ArgumentNullException("assemblyResolver");
			if (project == null)
				throw new ArgumentNullException("project");
			if (projectEnvironment == null)
				throw new ArgumentNullException("projectEnvironment");

			_project = project;
			_projectEnvironment = new BuildEnvironment(projectEnvironment, DefaultValues);
			_rootPath = Path.GetDirectoryName(_project.Filename);
			_resolver = assemblyResolver;
			_arguments = new ArgumentBuilder();
		}

		public string Output
		{
			get { return _output; }
		}

		private string OutputPath
		{
			get
			{
				var path = _projectEnvironment[Properties.OutputPath];
				var assemblyName = _projectEnvironment[Properties.AssemblyName];
				var fileName = string.Format("{0}.{1}", assemblyName, FileEnding);
				var fullPath = Path.Combine(path, fileName);
				return fullPath;
			}
		}

		private string FileEnding
		{
			get
			{
				string outputType = _projectEnvironment[Properties.OutputType];
				switch (outputType)
				{
					case "Library":
						return "dll";

					case "Exe":
					case "WinExe":
						return "exe";

					default:
						throw new Exception(string.Format("Unknown output type: '{0}'", outputType));
				}
			}
		}

		private string Target
		{
			get
			{
				string outputType = _projectEnvironment[Properties.OutputType];
				switch (outputType)
				{
					case "Library":
						return "library";

					case "Exe":
						return "exe";

					case "WinExe":
						return "winexe";

					default:
						throw new Exception(string.Format("Unknown output type: '{0}'", outputType));
				}
			}
		}

		public int Run()
		{
			if (_projectEnvironment[Properties.DebugSymbols] == "True")
				_arguments.Flag("debug+");

			_arguments.Add("debug", _projectEnvironment[Properties.DebugType]);
			if (_projectEnvironment[Properties.Optimize] == "False")
				_arguments.Flag("optmize-");

			_arguments.Add("target", Target);

			_arguments.Flag("noconfig");
			_arguments.Flag("nowarn:1701,1702,2008");
			_arguments.Flag("nostdlib+");
			_arguments.Add("platform", Platform);
			_arguments.Add("warn", _projectEnvironment[Properties.WarningLevel]);
			_arguments.Add("define", _projectEnvironment[Properties.DefineConstants]);
			_arguments.Add("filealign", _projectEnvironment[Properties.FileAlignment]);
			_arguments.Add("errorreport", _projectEnvironment[Properties.ErrorReport]);
			_arguments.Flag("errorendlocation");
			_arguments.Flag("preferreduilang:en-US");
			_arguments.Flag("highentropyva+");

			if (_projectEnvironment[Properties.Utf8Output] == "True")
				_arguments.Flag("utf8output");

			_arguments.Add("out", OutputPath);

			var items = _project.ItemGroups.SelectMany(x => x).ToList();
			var referenceItems = items.Where(x => x.Type == Items.Reference);
			foreach (var reference in referenceItems)
			{
				AddReference(reference);
			}
			AddReference(MsCoreLibrary);

			var compileItems = items.Where(x => x.Type == Items.Compile);
			foreach (var compile in compileItems)
			{
				AddCompile(compile);
			}

			_exitCode = ProcessEx.Run(CompilerPath, _rootPath, _arguments, out _output);

			return _exitCode;
		}

		private string Platform
		{
			get
			{
				var platform = _projectEnvironment[Properties.Platform];
				var prefer32Bit = _projectEnvironment[Properties.Prefer32Bit];

				switch (platform)
				{
					case "AnyCPU":
						return prefer32Bit == "True"
							       ? "anycpu32bitpreferred"
							       : "anycpu";

					case "x86":
						return "x86";

					case "x64":
						return "x64";

					default:
						throw new Exception(string.Format("Unknown platform: {0}", platform));
				}
			}
		}

		private void AddCompile(ProjectItem compile)
		{
			_arguments.Add(compile.Include);
		}

		private void AddReference(ProjectItem reference)
		{
			var referencePath = _resolver.ResolveReference(reference, _rootPath);
			_arguments.Add("reference", referencePath);
		}
	}
}