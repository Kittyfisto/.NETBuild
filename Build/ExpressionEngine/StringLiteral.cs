using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class StringLiteral
		: IExpression
	{
		public readonly string Value;

		public static readonly StringLiteral Empty = new StringLiteral(string.Empty);

		public StringLiteral(string value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value;
		}

		[Pure]
		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return Value;
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return Value;
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item)
		{
			return Value;
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			var item = fileSystem.CreateProjectItem("None", Value, null, environment);
			items.Add(item);
		}

		private bool Equals(StringLiteral other)
		{
			return string.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is StringLiteral && Equals((StringLiteral) obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}
	}
}