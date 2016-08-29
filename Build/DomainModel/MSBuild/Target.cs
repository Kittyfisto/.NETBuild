using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class Target
	{
		private readonly List<Task> _tasks;

		public Target()
		{
			_tasks = new List<Task>();
		}

		public string Name { get; set; }

		public List<Task> Tasks
		{
			get { return _tasks; }
		}

		public override string ToString()
		{
			return string.Format("<Target Name=\"{0}\" />", Name);
		}

		private bool Equals(Target other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Target && Equals((Target) obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}