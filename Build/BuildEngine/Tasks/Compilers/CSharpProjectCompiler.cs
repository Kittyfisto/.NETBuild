using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine.Tasks.Compilers
{
	/// <summary>
	///     Responsible for invoking the C# compiler (csc.exe).
	/// </summary>
	public sealed class CSharpProjectCompiler
		: IProjectCompiler
	{
		private const string CompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Csc.exe";
		private static readonly ProjectItem MsCoreLibrary = new ProjectItem(Items.Reference, "mscorlib");
		private static readonly BuildEnvironment DefaultValues;

		private readonly ArgumentBuilder _arguments;
		private readonly ILogger _logger;
		private readonly string _outputFilePath;
		private readonly List<string> _additionalOutputFiles;
		private readonly CSharpProject _project;
		private readonly BuildEnvironment _projectEnvironment;
		private readonly AssemblyResolver _resolver;
		private readonly string _rootPath;

		private int _exitCode;

		static CSharpProjectCompiler()
		{
			DefaultValues = new BuildEnvironment
				{
					{Properties.SubsystemVersion, "6.00"},
					{Properties.Prefer32Bit, "true"}
				};
		}

		public CSharpProjectCompiler(AssemblyResolver assemblyResolver,
		                             ILogger logger,
		                             CSharpProject project,
		                             BuildEnvironment projectEnvironment)
		{
			if (assemblyResolver == null)
				throw new ArgumentNullException("assemblyResolver");
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (project == null)
				throw new ArgumentNullException("project");
			if (projectEnvironment == null)
				throw new ArgumentNullException("projectEnvironment");

			_project = project;
			_logger = logger;
			_projectEnvironment = new BuildEnvironment(projectEnvironment, DefaultValues);
			_rootPath = Path.GetDirectoryName(_project.Filename);
			_resolver = assemblyResolver;
			_arguments = new ArgumentBuilder();

			_outputFilePath = CreateOutputFilePath(_projectEnvironment);
			_additionalOutputFiles = new List<string>
				{
					CreateOutputPdbFilePath(_projectEnvironment)
				};
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

		private string Platform
		{
			get
			{
				string platform = _projectEnvironment[Properties.Platform];
				string prefer32Bit = _projectEnvironment[Properties.Prefer32Bit];

				switch (platform)
				{
					case "AnyCPU":
						if (_projectEnvironment[Properties.OutputType] == "Library")
						{
							return "anycpu";
						}

						return prefer32Bit == "true"
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

		public string OutputFilePath
		{
			get { return _outputFilePath; }
		}

		public IEnumerable<string> AdditionalOutputFiles
		{
			get { return _additionalOutputFiles; }
		}

		public void Run()
		{
			if (_projectEnvironment[Properties.DebugSymbols] == "true")
				_arguments.Flag("debug+");

			_arguments.Add("debug", _projectEnvironment[Properties.DebugType]);
			if (_projectEnvironment[Properties.Optimize] == "False")
				_arguments.Flag("optmize-");

			_arguments.Add("target", Target);

			_arguments.Flag("nologo");
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

			if (_projectEnvironment[Properties.Utf8Output] == "true")
				_arguments.Flag("utf8output");

			if (_projectEnvironment[Properties.AllowUnsafeBlocks] == "true")
				_arguments.Flag("unsafe");

			_arguments.Add("out", _outputFilePath);

			List<ProjectItem> items = _project.ItemGroups.SelectMany(x => x).ToList();
			IEnumerable<ProjectItem> referenceItems = items.Where(x => x.Type == Items.Reference);
			foreach (ProjectItem reference in referenceItems)
			{
				AddReference(reference);
			}
			AddReference(MsCoreLibrary);

			IEnumerable<ProjectItem> resourceItems = items.Where(x => x.Type == Items.EmbeddedResource);
			foreach(ProjectItem resource in resourceItems)
			{
				AddResource(resource);
			}

			IEnumerable<ProjectItem> compileItems = items.Where(x => x.Type == Items.Compile);
			foreach (ProjectItem compile in compileItems)
			{
				AddCompile(compile);
			}

			string fullOutputPath = Path.Combine(_rootPath, _projectEnvironment[Properties.OutputPath]);
			if (!Directory.Exists(fullOutputPath))
				Directory.CreateDirectory(fullOutputPath);

			_logger.WriteLine(Verbosity.Normal, "  {0} {1}", CompilerPath, _arguments);

			// http://stackoverflow.com/questions/19306194/process-launch-exception-filename-or-extension-is-too-long-can-it-be-caused-b
			int commandLineLength = _arguments.Length + CompilerPath.Length + 1;
			if (commandLineLength > 32000) //< The maximum length seems to be 32768, but this one's close enough
			{
				// In order to not exceed the limit we'll have to write the arguments
				// to a file that is then passes as an argument to csc (so that's why that option exists).
				var filename = Path.Combine(fullOutputPath, "csc_arguments.txt");
				File.WriteAllText(filename, _arguments.ToString());
				_arguments.Clear();

				_arguments.Add(string.Format("\"@{0}\"", filename));
			}

			// without this, csc automatically references a bunch of assemblies and then gives us shit
			// that we've included a double reference...
			_arguments.Flag("noconfig");

			string output;
			_exitCode = ProcessEx.Run(CompilerPath, _rootPath, _arguments, out output);
			_logger.WriteMultiLine(Verbosity.Minimal, output);

			if (_exitCode != 0)
				throw new BuildException(string.Format("csc returned {0}", _exitCode));
		}

		private static string CreateOutputPdbFilePath(BuildEnvironment environment)
		{
			string path = environment[Properties.OutputPath];
			string assemblyName = environment[Properties.AssemblyName];
			string fileName = string.Format("{0}.pdb", assemblyName);
			string fullPath = Path.Combine(path, fileName);
			return fullPath;
		}

		private static string CreateOutputFilePath(BuildEnvironment environment)
		{
			string outputType = environment[Properties.OutputType];
			string fileEnding;
			switch (outputType)
			{
				case "Library":
					fileEnding = "dll";
					break;

				case "Exe":
				case "WinExe":
					fileEnding = "exe";
					break;

				default:
					throw new Exception(string.Format("Unknown output type: '{0}'", outputType));
			}

			string path = environment[Properties.OutputPath];
			string assemblyName = environment[Properties.AssemblyName];
			string fileName = string.Format("{0}.{1}", assemblyName, fileEnding);
			string fullPath = Path.Combine(path, fileName);
			return fullPath;
		}

		private void AddCompile(ProjectItem compile)
		{
			_arguments.Add(compile.Include);
		}

		private void AddReference(ProjectItem reference)
		{
			string referencePath = _resolver.ResolveReference(reference, _rootPath);
			_arguments.Add("reference", referencePath);
		}

		private void AddResource(ProjectItem resource)
		{
			var include = resource.Include;
			var resourceName = string.Format("EmbeddedResource.{0}", include.Replace('\\', '.'));

			_arguments.Add("resource", resource.Include, resourceName);
		}
	}
}