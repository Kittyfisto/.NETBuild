using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class Target
		: Node
	{
		private readonly List<Node> _children;

		public Target()
		{
			_children = new List<Node>();
		}

		public string DependsOnTargets { get; set; }

		public string Name { get; set; }

		public string Inputs { get; set; }

		public string Output { get; set; }

		public List<Node> Children
		{
			get { return _children; }
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