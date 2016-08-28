using System;
using System.Reflection;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build.BuildEngine
{
	/// <summary>
	///     
	/// </summary>
	public sealed class ProjectBuilder
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogger _logger;
		private readonly CSharpProject _project;
		private readonly BuildEnvironment _environment;
		private readonly string _target;

		public ProjectBuilder(ILogger logger, CSharpProject project, BuildEnvironment environment, string target)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (target == null)
				throw new ArgumentNullException("target");

			_logger = logger;
			_project = project;
			_environment = environment;
			_target = target;
		}

		public void Run()
		{
			_logger.LogFormat(Verbosity.Quiet, "------ Build started: Project: {0}, Configuration: {1} {2} ------",
					   _environment[Properties.MSBuildProjectName],
					   _environment[Properties.Configuration],
					   _environment[Properties.PlatformTarget]
				);

			var started = DateTime.Now;
			_logger.LogFormat(Verbosity.Normal, "Build started {0}.", started);

			switch (_target)
			{
				case Targets.Clean:
					break;

				case Targets.Build:

					break;

				default:
					throw new ArgumentException(string.Format("Unknown build target: {0}", _target));
			}

			_logger.LogFormat(Verbosity.Normal, "Build succeeded.");
			var elapsed = DateTime.Now - started;
			_logger.LogFormat(Verbosity.Normal, "Time Elapsed {0}", elapsed);
		}
	}
}