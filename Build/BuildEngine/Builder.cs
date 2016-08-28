using System;
using System.Reflection;
using System.Threading;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build.BuildEngine
{
	/// <summary>
	///     A builder is responsible for executing one build task at a time.
	///     A builder can execute multiple tasks is succession.
	/// </summary>
	public sealed class Builder
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ProjectDependencyGraph _graph;
		private readonly IBuildLog _log;
		private readonly string _name;
		private readonly Thread _thread;
		private bool _isFinished;
		private bool _isDisposed;

		public Builder(ProjectDependencyGraph graph,
		               IBuildLog log,
		               string name)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");
			if (log == null)
				throw new ArgumentNullException("log");
			if (name == null)
				throw new ArgumentNullException("name");
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("A non-empty/whitespace name must be given", "name");

			_graph = graph;
			_log = log;
			_name = name;
			_thread = new Thread(Run)
				{
					IsBackground = true,
					Name = name
				};
			_thread.Start();
		}

		public bool IsFinished
		{
			get { return _isFinished; }
		}

		public string Name
		{
			get { return _name; }
		}

		public void Dispose()
		{
			_isDisposed = true;
			_thread.Join();
		}

		private void Run()
		{
			try
			{
				while (!_isDisposed)
				{
					CSharpProject project;
					BuildEnvironment environment;
					if (_graph.TryGetNextProject(out project, out environment))
					{
						var logger = _log.CreateLogger();
						try
						{
							Run(logger, project, environment);
						}
						catch (Exception e)
						{
							logger.LogFormat("error: Internal build error: {0}", e);
							Log.ErrorFormat("Cauhgt unexpected exception while building project '{0}': {1}",
							                project.Filename,
							                e);

							_graph.Failed(project);
						}

						_graph.Succeeded(project);
					}
					else
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(100));
					}
				}
			}
			catch (Exception e)
			{
				Log.FatalFormat("Caught unexpected exception: {0}", e);
			}
			finally
			{
				_isFinished = true;
			}
		}

		private void Run(ILogger logger, CSharpProject project, BuildEnvironment environment)
		{
			logger.LogFormat("------ Build started: Project: {0}, Configuration: {1} {2} ------",
					   environment[Properties.MSBuildProjectName],
					   environment[Properties.Configuration],
					   environment[Properties.PlatformTarget]
				);

			var target = environment[Properties.DotNetBuildTarget];
			switch (target)
			{
				case Targets.Clean:
					break;

				case Targets.Build:
					break;

				default:
					throw new ArgumentException(string.Format("Unknown build target: {0}", target));
			}
		}
	}
}