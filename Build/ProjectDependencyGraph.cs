using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build
{
	public sealed class ProjectDependencyGraph
		: IProjectDependencyGraph
	{
		private readonly Dictionary<string, ProjectAndEnvironment> _all;
		private readonly Dictionary<string, ProjectAndEnvironment> _todo;
		private readonly Dictionary<Project, HashSet<Project>> _dependencies;
		private readonly HashSet<Project> _failed;
		private readonly ManualResetEventSlim _finishedEvent;
		private readonly HashSet<Project> _succeeded;
		private readonly object _syncRoot;

		private int _projectCount;

		public ProjectDependencyGraph()
		{
			_all = new Dictionary<string, ProjectAndEnvironment>(new FilenameComparer());
			_todo = new Dictionary<string, ProjectAndEnvironment>(new FilenameComparer());
			_dependencies = new Dictionary<Project, HashSet<Project>>();
			_succeeded = new HashSet<Project>();
			_failed = new HashSet<Project>();

			_finishedEvent = new ManualResetEventSlim(false);
			_projectCount = _todo.Count;
			_syncRoot = new object();
		}

		public ManualResetEventSlim FinishedEvent
		{
			get { return _finishedEvent; }
		}

		public int SucceededCount
		{
			get
			{
				lock (_syncRoot)
				{
					return _succeeded.Count;
				}
			}
		}

		public int FailedCount
		{
			get
			{
				lock (_syncRoot)
				{
					return _failed.Count;
				}
			}
		}

		private void Finish()
		{
			_finishedEvent.Set();
		}

		public void Add(Project project, BuildEnvironment environment)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (project.Filename == null)
				throw new ArgumentNullException("project.Filename");
			if (!Path.IsPathRooted(project.Filename))
				throw new ArgumentException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");

			var env = new ProjectAndEnvironment
				{
					Project = project,
					Environment = environment
				};
			_all.Add(project.Filename,env);
			_todo.Add(project.Filename, env);
			var dependencies = new HashSet<Project>();
			_dependencies.Add(project, dependencies);
			++_projectCount;
		}

		[Pure]
		public bool Contains(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			return _todo.ContainsKey(path);
		}

		public void AddDependency(Project project, Project dependency)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (dependency == null)
				throw new ArgumentNullException("dependency");

			HashSet<Project> dependencies = _dependencies[project];
			dependencies.Add(dependency);
		}

		public bool TryGetNextProject(out Project project, out BuildEnvironment environment)
		{
			lock (_syncRoot)
			{
				// The goal is simple: Find the first project that doesn't have any 
				// unfinished dependencies anymore.
				foreach (var pair in _todo.Values)
				{
					var possibleProject = pair.Project;
					var dependencies = _dependencies[possibleProject];
					if (dependencies.Count == 0)
					{
						var fileName = possibleProject.Filename;
						project = possibleProject;
						environment = pair.Environment;
						_todo.Remove(fileName);
						return true;
					}
				}

				project = null;
				environment = null;
				return false;
			}
		}

		public void MarkAsFailed(Project project)
		{
			lock (_syncRoot)
			{
				_failed.Add(project);

				foreach (var pair in _dependencies)
				{
					if (pair.Value.Contains(project))
					{
						var dependantProject = pair.Key;
						_todo.Remove(dependantProject.Filename);
						--_projectCount;
					}
				}

				TetIfFinished();
			}
		}

		public void MarkAsSuccess(Project project)
		{
			lock (_syncRoot)
			{
				_succeeded.Add(project);
				foreach (var pair in _dependencies)
				{
					var dependencies = pair.Value;
					dependencies.Remove(project);
				}

				TetIfFinished();
			}
		}

		public bool IsFinished
		{
			get
			{
				lock (_syncRoot)
				{
					int finished = _succeeded.Count + _failed.Count;
					if (finished >= _projectCount)
						return true;
				}

				return false;
			}
		}

		private void TetIfFinished()
		{
			int finished = _succeeded.Count + _failed.Count;
			if (finished == _projectCount)
			{
				Finish();
			}
		}

		public ProjectAndEnvironment this[string fileName]
		{
			get
			{
				lock (_syncRoot)
				{
					return _all[fileName];
				}
			}
		}

		public ProjectAndEnvironment[] TryGetAll(string[] fileNames)
		{
			if (fileNames == null)
				throw new ArgumentNullException("fileNames");

			lock (_syncRoot)
			{
				var ret = new ProjectAndEnvironment[fileNames.Length];
				for (int i = 0; i < fileNames.Length; ++i)
				{
					ProjectAndEnvironment tmp;
					if (_all.TryGetValue(fileNames[i], out tmp))
					{
						ret[i] = tmp;
					}
				}
				return ret;
			}
		}
	}
}