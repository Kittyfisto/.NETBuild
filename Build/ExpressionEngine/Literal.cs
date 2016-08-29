using System.Diagnostics.Contracts;
using Build.BuildEngine;

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