using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class Literal
		: IExpression
	{
		public readonly string Value;

		public Literal(string value)
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

		public bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return Expression.IsTrue(Value);
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return Value;
		}

		public List<ProjectItem> ToItemList(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new System.NotImplementedException();
		}

		private bool Equals(Literal other)
		{
			return string.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Literal && Equals((Literal) obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}
	}
}