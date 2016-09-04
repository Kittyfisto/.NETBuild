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
	public sealed class BuildNode
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ProjectDependencyGraph _graph;
		private readonly string _name;
		private readonly string _target;
		private readonly TaskEngine.TaskEngine _taskEngine;
		private readonly IBuildLog _buildLog;
		private readonly Thread _thread;
		private bool _isFinished;

		public BuildNode(ProjectDependencyGraph graph,
		                 TaskEngine.TaskEngine taskEngine,
		                 IBuildLog buildLog,
		                 string name,
		                 string target)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");
			if (taskEngine == null)
				throw new ArgumentNullException("taskEngine");
			if (buildLog == null)
				throw new ArgumentNullException("buildLog");
			if (target == null)
				throw new ArgumentNullException("target");
			if (name == null)
				throw new ArgumentNullException("name");
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("A non-empty/whitespace name must be given", "name");
			if (target == null)
				throw new ArgumentNullException("target");

			_graph = graph;
			_taskEngine = taskEngine;
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
					Project project;
					BuildEnvironment environment;
					if (_graph.TryGetNextProject(out project, out environment))
					{
						var logger = _buildLog.CreateLogger();
						try
						{
							_taskEngine.Run(project,
							                _target,
							                environment,
							                _graph,
							                logger);

							if (logger.HasErrors)
							{
								_graph.MarkAsFailed(project);
							}
							else
							{
								_graph.MarkAsSuccess(project);
							}
						}
						catch (BuildException e)
						{
							logger.WriteLine(Verbosity.Quiet, "error: {0}", e.Message);

							Log.ErrorFormat("Cauhgt exception while building project '{0}': {1}",
							                project.Filename,
							                e);

							_graph.MarkAsFailed(project);
						}
						catch (Exception e)
						{
							logger.WriteLine(Verbosity.Quiet, "error: Internal build error: {0}", e);

							Log.ErrorFormat("Cauhgt unexpected exception while building project '{0}': {1}",
							                project.Filename,
							                e);

							_graph.MarkAsFailed(project);
						}
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