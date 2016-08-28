using System;
using System.Reflection;
using System.Threading;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for building one project at a time.
	/// </summary>
	public sealed class Node
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IBuildLog _buildLog;
		private readonly ProjectDependencyGraph _graph;
		private readonly string _name;
		private readonly AssemblyResolver _resolver;
		private readonly string _target;
		private readonly Thread _thread;
		private bool _isFinished;

		public Node(ProjectDependencyGraph graph,
		            AssemblyResolver resolver,
		            IBuildLog buildLog,
		            string name,
		            string target)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");
			if (resolver == null)
				throw new ArgumentNullException("resolver");
			if (buildLog == null)
				throw new ArgumentNullException("buildLog");
			if (name == null)
				throw new ArgumentNullException("name");
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("A non-empty/whitespace name must be given", "name");
			if (target == null)
				throw new ArgumentNullException("target");

			_graph = graph;
			_resolver = resolver;
			_buildLog = buildLog;
			_name = name;
			_target = target;
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

		public void Stop()
		{
			_thread.Join();
		}

		private void Run()
		{
			try
			{
				while (true)
				{
					CSharpProject project;
					BuildEnvironment environment;
					if (_graph.TryGetNextProject(out project, out environment))
					{
						ILogger logger = _buildLog.CreateLogger();
						try
						{
							var builder = new ProjectBuilder(logger, _resolver, project, environment, _target);
							builder.Run();
						}
						catch (Exception e)
						{
							logger.WriteLine(Verbosity.Quiet, "error: Internal build error: {0}", e);

							Log.ErrorFormat("Cauhgt unexpected exception while building project '{0}': {1}",
							                project.Filename,
							                e);

							_graph.Failed(project);
						}

						_graph.Succeeded(project);
					}
					else
					{
						if (_graph.FinishedEvent.Wait(TimeSpan.FromSeconds(100)))
							break;
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
	}
}