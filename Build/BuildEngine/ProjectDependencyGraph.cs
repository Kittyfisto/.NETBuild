using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	public sealed class ProjectDependencyGraph
	{
		private readonly HashSet<CSharpProject> _failed;
		private readonly ManualResetEventSlim _finishedEvent;
		private readonly int _projectCount;
		private readonly HashSet<CSharpProject> _succeeded;
		private readonly object _syncRoot;

		// TODO: obviously the wrong data structure...
		private readonly Dictionary<CSharpProject, BuildEnvironment> _todo;

		public ProjectDependencyGraph(IReadOnlyDictionary<CSharpProject, BuildEnvironment> projects)
		{
			_todo = new Dictionary<CSharpProject, BuildEnvironment>(projects.Count);
			foreach (var pair in projects)
			{
				_todo.Add(pair.Key, pair.Value);
			}

			_finishedEvent = new ManualResetEventSlim(false);
			_projectCount = _todo.Count;
			_syncRoot = new object();
			var comparer = new ProjectEqualityComparer();
			_succeeded = new HashSet<CSharpProject>(comparer);
			_failed = new HashSet<CSharpProject>(comparer);
		}

		public ManualResetEventSlim FinishedEvent
		{
			get { return _finishedEvent; }
		}

		public bool TryGetNextProject(out CSharpProject project, out BuildEnvironment environment)
		{
			lock (_todo)
			{
				if (_todo.Count == 0)
				{
					project = null;
					environment = null;
					return false;
				}

				KeyValuePair<CSharpProject, BuildEnvironment> pair = _todo.First();
				project = pair.Key;
				environment = pair.Value;
				_todo.Remove(project);
				return true;
			}
		}

		public void Failed(CSharpProject project)
		{
			lock (_syncRoot)
			{
				_failed.Add(project);

				TetIfFinished();
			}
		}

		public void Succeeded(CSharpProject project)
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
			: IEqualityComparer<CSharpProject>
		{
			public bool Equals(CSharpProject x, CSharpProject y)
			{
				string xName = x.Filename;
				string yName = y.Filename;

				return string.Equals(xName, yName);
			}

			public int GetHashCode(CSharpProject obj)
			{
				string name = obj.Filename;
				return name.GetHashCode();
			}
		}
	}
}