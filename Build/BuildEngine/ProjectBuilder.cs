using System;
using System.Collections.Generic;
using System.Reflection;
using Build.BuildEngine.Tasks;
using Build.BuildEngine.Tasks.Compilers;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build.BuildEngine
{
	/// <summary>
	/// </summary>
	public sealed class ProjectBuilder
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogger _logger;
		private readonly string _target;
		private readonly BuildEnvironment _environment;
		private AssemblyResolver _resolver;
		private CSharpProject _project;

		public ProjectBuilder(ILogger logger,
		                      AssemblyResolver resolver,
		                      CSharpProject project,
		                      BuildEnvironment environment,
		                      string target)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (target == null)
				throw new ArgumentNullException("target");

			// TODO: Support different compilers
			_resolver = resolver;
			_project = project;
			_environment = environment;
			_logger = logger;
			_target = target;
		}

		public void Run()
		{
			_logger.WriteLine(Verbosity.Quiet, "------ Build started: Project: {0}, Configuration: {1} {2} ------",
			                  _environment[Properties.MSBuildProjectName],
			                  _environment[Properties.Configuration],
			                  _environment[Properties.PlatformTarget]
				);

			DateTime started = DateTime.Now;
			_logger.WriteLine(Verbosity.Normal, "Build started {0}.", started);

			switch (_target)
			{
				case Targets.Clean:
					Clean();
					break;

				case Targets.Build:
					Build();
					break;

				default:
					throw new ArgumentException(string.Format("Unknown build target: {0}", _target));
			}

			_logger.WriteLine(Verbosity.Normal, "Build succeeded.");
			TimeSpan elapsed = DateTime.Now - started;
			_logger.WriteLine(Verbosity.Normal, "Time Elapsed {0}", elapsed);
		}

		private void Clean()
		{
			
		}

		private void Build()
		{
			string assembly;
			IEnumerable<string> additionalFiles;
			Compile(out assembly, out additionalFiles);
			CopyToOutputPath(assembly, additionalFiles);
			CopyDependencies();
		}

		private void Compile(out string outputFile, out IEnumerable<string> additionalFiles)
		{
			var compileEnvironment = new BuildEnvironment(_environment, name: "Compiler Environment");
			var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
			compileEnvironment[Properties.OutputPath] = Path.Combine(projectDirectory,
			                                                         "obj",
			                                                         _environment[Properties.Configuration],
			                                                         _environment[Properties.Platform]);
			var compiler = CreateCompiler(compileEnvironment);
			compiler.Run();
			outputFile = compiler.OutputFilePath;
			additionalFiles = compiler.AdditionalOutputFiles;
		}

		private void CopyToOutputPath(string outputFile, IEnumerable<string> additionalOutputFiles)
		{
			var destinationFile = CopyToOutputPath(outputFile);
			var projectName = _environment[Properties.MSBuildProjectName];
			_logger.WriteLine(Verbosity.Minimal, "  {0} -> {1}", projectName, destinationFile);

			foreach (var file in additionalOutputFiles)
			{
				CopyToOutputPath(file);
			}
		}

		private string CopyToOutputPath(string inputFilePath)
		{
			var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
			var outputPath = Path.MakeAbsolute(projectDirectory,
											   _environment[Properties.OutputPath]);
			var fileName = Path.GetFilename(inputFilePath);
			var outputFilePath = Path.Combine(outputPath, fileName);
			var task = new CopyFile(_logger, projectDirectory, inputFilePath, outputFilePath, Copy.IfNewer);
			task.Run();
			return outputFilePath;
		}

		private void CopyDependencies()
		{
			
		}

		private IProjectCompiler CreateCompiler(BuildEnvironment environment)
		{
			var compiler = new CSharpProjectCompiler(_resolver, _logger, _project, environment);
			return compiler;
		}
	}
}