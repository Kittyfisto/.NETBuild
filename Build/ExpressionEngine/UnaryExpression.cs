using System.ComponentModel;
using System.Diagnostics.Contracts;
using Build.BuildEngine;

namespace Build.ExpressionEngine
{
	public sealed class UnaryExpression
		: IExpression
	{
		public readonly IExpression Expression;
		public readonly UnaryOperation Operation;

		public UnaryExpression(UnaryOperation operation, IExpression expression)
		{
			Operation = operation;
			Expression = expression;
		}

		public override string ToString()
		{
			return string.Format("!{0}", Expression);
		}

		[Pure]
		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			object value = Expression.Evaluate(fileSystem, environment);
			switch (Operation)
			{
				case UnaryOperation.Not:
					if (value == null)
						return true;

					bool booleanValue;
					if (!(value is bool))
					{
						if (!bool.TryParse(value.ToString(), out booleanValue))
						{
							// TODO: Add warning that we're negating a non boolean value
							return false;
						}
					}
					else
					{
						booleanValue = (bool) value;
					}

					return !booleanValue;

				default:
					throw new InvalidEnumArgumentException("Operation", (int) Operation, typeof (UnaryOperation));
			}
		}

		private bool Equals(UnaryExpression other)
		{
			return Operation == other.Operation && Expression.Equals(other.Expression);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is UnaryExpression && Equals((UnaryExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Operation*397) ^ Expression.GetHashCode();
			}
		}
	}
}