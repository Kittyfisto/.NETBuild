using System.Collections.Generic;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class Variable
		: IExpression
	{
		public readonly string Name;

		public Variable(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return string.Format("$({0})", Name);
		}

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return ToString(fileSystem, environment);
		}

		public bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var value = ToString(fileSystem, environment);
			return string.Equals(value, "true");
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return environment.Properties[Name];
		}

		public List<ProjectItem> ToItemList(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new System.NotImplementedException();
		}

		private bool Equals(Variable other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Variable && Equals((Variable) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}