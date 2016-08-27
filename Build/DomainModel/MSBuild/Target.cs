using System;
using System.Linq;

namespace Build.DomainModel.MSBuild
{
	public sealed class Target
	{
		private readonly string _name;

		public Target(string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (name.Any(char.IsWhiteSpace))
				throw new ArgumentException("name must not contain whitespace", "name");

			_name = name;
		}

		public override string ToString()
		{
			return string.Format("<Target Name=\"{0}\" />", _name);
		}

		public string Name
		{
			get { return _name; }
		}

		private bool Equals(Target other)
		{
			return string.Equals(_name, other._name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Target && Equals((Target) obj);
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}
	}
}