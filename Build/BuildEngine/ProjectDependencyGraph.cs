using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	public sealed class ProjectDependencyGraph
	{
		private readonly HashSet<Project> _failed;
		private readonly ManualResetEventSlim _finishedEvent;
		private readonly int _projectCount;
		private readonly HashSet<Project> _succeeded;
		private readonly object _syncRoot;

		// TODO: obviously the wrong data structure...
		private readonly Dictionary<Project, BuildEnvironment> _todo;

		public ProjectDependencyGraph(IReadOnlyDictionary<Project, BuildEnvironment> projects)
		{
			_todo = new Dictionary<Project, BuildEnvironment>(projects.Count);
			foreach (var pair in projects)
			{
				_todo.Add(pair.Key, pair.Value);
			}

			_finishedEvent = new ManualResetEventSlim(false);
			_projectCount = _todo.Count;
			_syncRoot = new object();
			var comparer = new ProjectEqualityComparer();
			_succeeded = new HashSet<Project>(comparer);
			_failed = new HashSet<Project>(comparer);
		}

		public ManualResetEventSlim FinishedEvent
		{
			get { return _finishedEvent; }
		}

		public bool TryGetNextProject(out Project project, out BuildEnvironment environment)
		{
			lock (_todo)
			{
				if (_todo.Count == 0)
				{
					project = null;
					environment = null;
					return false;
				}

				KeyValuePair<Project, BuildEnvironment> pair = _todo.First();
				project = pair.Key;
				environment = pair.Value;
				_todo.Remove(project);
				return true;
			}
		}

		public void Failed(Project project)
		{
			lock (_syncRoot)
			{
				_failed.Add(project);

				TetIfFinished();
			}
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

		public void Succeeded(Project project)
		{
			lock (_syncRoot)
			{
				_succeeded.Add(project);

				TetIfFinished();
			}
		}

		private void TetIfFinished()
		{
			int finished = _succeeded.Count + _failed.Count;
			if (finished == _projectCount)
			{
				_finishedEvent.Set();
			}
		}

		internal sealed class ProjectEqualityComparer
			: IEqualityComparer<Project>
		{
			public bool Equals(Project x, Project y)
			{
				string xName = x.Filename;
				string yName = y.Filename;

				return string.Equals(xName, yName);
			}

			public int GetHashCode(Project obj)
			{
				string name = obj.Filename;
				return name.GetHashCode();
			}
		}
	}
}